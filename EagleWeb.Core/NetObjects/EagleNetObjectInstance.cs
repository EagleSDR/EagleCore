using EagleWeb.Common;
using EagleWeb.Common.IO;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.NetObjects.Interfaces;
using EagleWeb.Common.NetObjects.IO;
using EagleWeb.Core.NetObjects.Enums;
using EagleWeb.Core.NetObjects.Misc;
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
    class EagleNetObjectInstance : IEagleNetObjectInternalIO, IEagleObjectContext
    {
        public EagleNetObjectInstance(string guid, EagleNetObjectManager manager, IEagleFilteredTarget outputTarget)
        {
            this.guid = guid;
            this.manager = manager;
            this.outputTarget = outputTarget;
        }

        private const long PING_INTERVAL = 30 * 1000; //How often the client should be sending a ping
        private const long PING_TIMEOUT = PING_INTERVAL * 3; //Max time to wait before triggering a timeout

        private readonly string guid;
        private readonly EagleNetObjectManager manager;
        private readonly IEagleFilteredTarget outputTarget;

        private List<EagleNetObjectPort> ports = new List<EagleNetObjectPort>();
        private JObject extras = new JObject();
        private IEagleObject user;
        private bool isDestroying = false;
        private bool webDestructionEnabled = false;

        private Timer pingTimeout;
        private IEagleObjectPingExpiredHandler pingTimeoutHandler;

        public event IEagleObjectConfigureContext_OnDestroyedEventArgs OnDestroyed;

        public string Guid => guid;
        public IEagleContext Context => manager.Ctx;
        public string LoggableId => $"{(user == null ? "UNKNOWN" : user.GetType().FullName)} @ {Guid}";
        public EagleNetObjectManager Manager => manager;
        public IEagleNetObjectTarget TargetAll => outputTarget;
        public IEagleObject User
        {
            get
            {
                if (user == null)
                    throw new Exception("User object has not yet been set!");
                return user;
            }
        }

        /* INTERNAL API */

        public void Finalize(IEagleObject user)
        {
            //Set user code
            this.user = user;

            //Send create message to all
            SendCreateMessage(TargetAll);
        }

        /* INTERNAL METHODS */

        private void SendCreateMessage(IEagleNetObjectTarget target)
        {
            //Create type list
            JArray typeList;
            {
                typeList = new JArray();
                Type type = user.GetType();
                while (typeof(EagleObject).IsAssignableFrom(type))
                {
                    typeList.Add(type.FullName.Split('`')[0]);
                    type = type.BaseType;
                }
            }

            //Create port list
            JArray portList;
            {
                portList = new JArray();
                foreach (var p in ports)
                    portList.Add(p.CreateInfo());
            }

            //Make and send create message
            {
                JObject msg = new JObject();
                msg["guid"] = guid;
                msg["types"] = typeList;
                msg["ports"] = portList;
                msg["extras"] = extras;
                msg["ping_interval"] = pingTimeout == null ? 0 : PING_INTERVAL;
                msg["web_destruction_allowed"] = webDestructionEnabled;
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
                    if (webDestructionEnabled)
                        Destroy();
                    break;
            }
        }

        private void LogInternal(EagleLogLevel level, string message)
        {
            Manager.Ctx.Log(level, GetType().Name, message);
        }

        private void PingTimeout_Elapsed(object sender, ElapsedEventArgs e)
        {
            //A ping has elapsed! Call the handler to see if we're going to cancel the timeout...
            if (pingTimeoutHandler != null && pingTimeoutHandler.WebPingTimeout())
                return;

            //Dispose the object by calling user code
            Destroy();
        }

        private void EnsureNotInitialized()
        {
            if (user != null)
                throw new Exception("Settings on EagleObjects can't be changed once the object is fully constructed! Consider moving this code into the constructor.");
        }

        private T CreateAndAddPort<T>(T port) where T : EagleNetObjectPort
        {
            EnsureNotInitialized();
            ports.Add(port);
            return port;
        }

        /* CONFIGURE CONTEXT API */

        public T CreateChildObject<T>(Func<IEagleObjectContext, T> creator) where T : IEagleObject
        {
            return manager.CreateObject(creator, outputTarget.Clone());
        }

        public void RequireKeepAlivePings(IEagleObjectPingExpiredHandler handler = null)
        {
            //Validate
            EnsureNotInitialized();

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

        public void AllowWebDeletion()
        {
            EnsureNotInitialized();
            webDestructionEnabled = true;
        }

        public void AddExtra(string key, JObject value)
        {
            //Validate
            EnsureNotInitialized();

            //Add usual
            AddExtraPost(key, value);
        }

        public void AddExtraPost(string key, JObject value)
        {
            //Make sure it doesn't already exist
            if (extras.ContainsKey(key))
                throw new Exception("The specified extra key already exists in the data!");

            //Add
            extras.Add(key, value);
        }

        public IEaglePortApi CreatePortApi(string name)
        {
            return CreateAndAddPort(new EaglePortApi(this, name));
        }

        public IEaglePortEventDispatcher CreateEventDispatcher(string name)
        {
            return CreateAndAddPort(new EaglePortEventDispatcher(this, name));
        }

        public IEaglePortProperty<bool> CreatePropertyBool(string name)
        {
            return CreateAndAddPort(new EaglePortPropertyPrimitive<bool>(this, name));
        }

        public IEaglePortProperty<string> CreatePropertyString(string name)
        {
            return CreateAndAddPort(new EaglePortPropertyPrimitive<string>(this, name));
        }

        public IEaglePortProperty<long> CreatePropertyLong(string name)
        {
            return CreateAndAddPort(new EaglePortPropertyPrimitive<long>(this, name));
        }

        public IEaglePortProperty<int> CreatePropertyInt(string name)
        {
            return CreateAndAddPort(new EaglePortPropertyPrimitive<int>(this, name));
        }

        public IEaglePortProperty<float> CreatePropertyFloat(string name)
        {
            return CreateAndAddPort(new EaglePortPropertyPrimitive<float>(this, name));
        }

        public IEaglePortProperty<T> CreatePropertyObject<T>(string name) where T : IEagleObject
        {
            return CreateAndAddPort(new EaglePortPropertyObject<T>(this, name));
        }

        public void Log(EagleLogLevel level, string topic, string message)
        {
            //Get the type name
            string typeName = user == null ? GetType().Name : user.GetType().Name;

            //Create new topic
            string extTopic = typeName;
            if (topic != null && topic.Length > 0)
                extTopic += "-" + topic;

            //Log
            Manager.Ctx.Log(level, extTopic, message);
        }

        public void Destroy()
        {
            //Make sure we aren't already being destroyed, as that'll cause a stack overflow
            if (isDestroying)
                return;

            //Set flag
            isDestroying = true;

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
            SendDestroyMessage(TargetAll);

            //Raise event
            try
            {
                OnDestroyed?.Invoke();
            }
            catch (Exception ex)
            {
                LogInternal(EagleLogLevel.ERROR, $"Error raised in user destroy event for NetObject \"{LoggableId}\": {ex.Message}{ex.StackTrace}");
            }
        }
    }
}
