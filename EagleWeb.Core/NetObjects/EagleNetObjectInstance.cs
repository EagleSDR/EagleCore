using EagleWeb.Common;
using EagleWeb.Common.IO;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.NetObjects.Interfaces;
using EagleWeb.Common.NetObjects.IO;
using EagleWeb.Core.NetObjects.Enums;
using EagleWeb.Core.NetObjects.Ports;
using EagleWeb.Core.NetObjects.Ports.Property;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Timers;

namespace EagleWeb.Core.NetObjects
{
    class EagleNetObjectInstance : IEagleObjectInternalContext, IEagleNetObjectInternalIO
    {
        public EagleNetObjectInstance(string guid, EagleObject ctx, JObject constructorInfo)
        {
            this.guid = guid;
            this.ctx = ctx;
            this.constructorInfo = constructorInfo;
        }

        private const long PING_INTERVAL = 30 * 1000; //How often the client should be sending a ping
        private const long PING_TIMEOUT = PING_INTERVAL * 3; //Max time to wait before triggering a timeout

        private readonly string guid;
        private readonly EagleObject ctx;
        private readonly JObject constructorInfo;

        private List<EagleNetObjectPort> ports = new List<EagleNetObjectPort>();

        private Timer pingTimeout;
        private IEagleObjectPingExpiredHandler pingTimeoutHandler;

        public string Guid => guid;
        public string LoggableId => $"{Ctx.GetType().FullName} @ {Guid}";
        public List<EagleNetObjectPort> Ports => ports;
        public EagleNetObjectManager Manager => (EagleNetObjectManager)ctx.ObjectManager;
        public IEagleObject Ctx => ctx;
        public bool WebDeletionAllowed { get; set; } = false;

        /* INTERNAL API */

        public void RequirePings(IEagleObjectPingExpiredHandler handler)
        {
            //Ensure we haven't already set this
            if (pingTimeout != null)
                throw new Exception("This object has already been configured to require pings!");

            //Set handler
            pingTimeoutHandler = handler;

            //Create timeout
            pingTimeout = new Timer(PING_TIMEOUT);
            pingTimeout.Elapsed += PingTimeout_Elapsed;
            pingTimeout.AutoReset = true;
            pingTimeout.Start();
        }

        /* INTERNAL METHODS */

        private JArray CreateTypeTree()
        {
            JArray arr = new JArray();
            Type type = ctx.GetType();
            while (typeof(EagleObject).IsAssignableFrom(type))
            {
                arr.Add(type.FullName.Split('`')[0]);
                type = type.BaseType;
            }
            return arr;
        }

        private JArray CreatePortsList()
        {
            JArray msgPorts = new JArray();
            foreach (var p in ports)
                msgPorts.Add(p.CreateInfo());
            return msgPorts;
        }

        private void SendCreateMessage(IEagleNetObjectTarget target)
        {
            //Make and send create message
            {
                JObject msg = new JObject();
                msg["guid"] = guid;
                msg["extra"] = constructorInfo;
                msg["types"] = CreateTypeTree();
                msg["ports"] = CreatePortsList();
                msg["ping_interval"] = pingTimeout == null ? 0 : PING_INTERVAL;
                msg["web_destruction_allowed"] = WebDeletionAllowed;
                target.SendMessage(EagleNetObjectOpcode.OBJECT_CREATE, guid, msg);
            }

            //Send info for all ports
            foreach (var p in ports)
                p.OnClientConnect(target);

            //Make and send object ready message
            {
                JObject msg = new JObject();
                target.SendMessage(EagleNetObjectOpcode.OBJECT_READY, guid, msg);
            }
        }

        private void SendDestroyMessage(IEagleNetObjectTarget target)
        {
            //Make and send destroy message
            JObject msg = new JObject();
            target.SendMessage(EagleNetObjectOpcode.OBJECT_DESTROY, guid, msg);
        }

        public void OnClientConnect(IEagleNetObjectTarget target)
        {
            //Tell client about us
            SendCreateMessage(target);
        }

        public void OnClientMessage(EagleNetObjectClient client, EagleNetObjectOpcode opcode, JObject message)
        {
            switch (opcode)
            {
                case EagleNetObjectOpcode.OBJECT_PING:
                    //Pinged! Reset timer.
                    if (pingTimeout != null)
                    {
                        pingTimeout.Stop();
                        pingTimeout.Start();
                    }
                    break;
                case EagleNetObjectOpcode.OBJECT_REQUEST_DESTRUCTION:
                    if (WebDeletionAllowed)
                        ctx.Destroy();
                    break;
            }
        }

        public IEagleObjectConfigureContext BeginCreate()
        {
            return new EagleNetObjectConfigureContext(this);
        }

        public void EndCreate(IEagleObjectConfigureContext ctx)
        {
            //Actually register it
            Manager.Collection.ActivateGuid(this);

            //Notify all clients of it's creation
            SendCreateMessage(Manager);
        }

        public void Destroy()
        {
            //Log
            LogInternal(EagleLogLevel.DEBUG, $"Disposing object {LoggableId}...");

            //Stop timeout
            if (pingTimeout != null)
            {
                pingTimeout.Stop();
                pingTimeout.Dispose();
                pingTimeout = null;
            }

            //Unregister it
            Manager.Collection.DeactivateGuid(this);

            //Notify all clients of it's destruction
            SendDestroyMessage(Manager);
        }

        public void Log(EagleLogLevel level, string topic, string message)
        {
            //Create new topic
            string extTopic = ctx.GetType().Name;
            if (topic != null && topic.Length > 0)
                extTopic += "-" + topic;

            //Log
            Manager.Ctx.Log(level, extTopic, message);
        }

        public void LogInternal(EagleLogLevel level, string message)
        {
            Manager.Ctx.Log(level, GetType().Name, message);
        }

        private void PingTimeout_Elapsed(object sender, ElapsedEventArgs e)
        {
            //A ping has elapsed! Call the handler to see if we're going to cancel the timeout...
            if (pingTimeoutHandler != null && pingTimeoutHandler.WebPingTimeout())
                return;

            //Dispose the object by calling user code
            ctx.Destroy();
        }
    }
}
