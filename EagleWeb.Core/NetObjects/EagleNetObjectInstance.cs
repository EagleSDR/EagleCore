using EagleWeb.Common;
using EagleWeb.Common.IO;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.NetObjects.IO;
using EagleWeb.Core.NetObjects.Enums;
using EagleWeb.Core.NetObjects.Ports;
using EagleWeb.Core.NetObjects.Ports.Property;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

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

        private readonly string guid;
        private readonly EagleObject ctx;
        private readonly JObject constructorInfo;

        private List<EagleNetObjectPort> ports = new List<EagleNetObjectPort>();

        public string Guid => guid;
        public string LoggableId => $"{Ctx.GetType().FullName} @ {Guid}";
        public List<EagleNetObjectPort> Ports => ports;
        public EagleNetObjectManager Manager => (EagleNetObjectManager)ctx.ObjectManager;
        public EagleObject Ctx => ctx;

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

        private void SendCreateMessage(IEagleTarget target)
        {
            //Make
            JObject msg = new JObject();
            msg["guid"] = guid;
            msg["extra"] = constructorInfo;
            msg["types"] = CreateTypeTree();
            msg["ports"] = CreatePortsList();
            
            //Send
            Manager.SendMessage(target, EagleNetObjectOpcode.OBJECT_CREATE, guid, msg);
        }

        public void OnClientConnect(IEagleTarget target)
        {
            //Tell client about us
            SendCreateMessage(target);
        }

        public void OnClientMessage(IEagleClient client, EagleNetObjectOpcode opcode, JObject message)
        {
            //Do nothing...
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
            SendCreateMessage(Manager.TargetAll);
        }

        public void Destroy()
        {
            //Log
            LogInternal(EagleLogLevel.DEBUG, $"Disposing object {LoggableId}...");

            //Unregister it
            Manager.Collection.DeactivateGuid(this);

            //Notify all clients of it's destruction
            SendCreateMessage(Manager.TargetAll);
        }

        public void Log(EagleLogLevel level, string topic, string message)
        {
            //Create new topic
            string extTopic = ctx.GetType().Name;
            if (topic != null && topic.Length > 0)
                extTopic += "-" + topic;

            //Log
            Manager.Logger.Log(level, extTopic, message);
        }

        public void LogInternal(EagleLogLevel level, string message)
        {
            Manager.Logger.Log(level, GetType().Name, message);
        }
    }
}
