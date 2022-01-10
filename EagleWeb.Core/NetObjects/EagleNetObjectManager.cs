using EagleWeb.Common;
using EagleWeb.Common.IO;
using EagleWeb.Common.NetObjects;
using EagleWeb.Core.NetObjects.Enums;
using EagleWeb.Core.Web.IO.Rpc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.NetObjects
{
    class EagleNetObjectManager : IEagleObjectManager
    {
        public EagleNetObjectManager(IEagleLogger logger, EagleRpcManager io)
        {
            //Set
            this.logger = logger;

            //Set up IO
            this.io = io;
            io.OnClientConnect += Io_OnClientConnect;
            io.OnReceive += Io_OnReceive;
        }

        public IEagleTarget TargetAll => io.TargetAll;
        public EagleGuidObjectCollection Collection => collection;
        public IEagleLogger Logger => logger;

        private IEagleLogger logger;
        private EagleRpcManager io;
        private EagleGuidObjectCollection collection = new EagleGuidObjectCollection();
        private EagleObject control;

        public void SetControlObject(EagleObject control)
        {
            this.control = control;
        }

        public IEagleObjectInternalContext CreateObject(EagleObject ctx, JObject constructorInfo)
        {
            //Generate a unique GUID
            string guid = collection.ReserveGuid();

            //If the constructor info is null, create a filler value
            if (constructorInfo == null)
                constructorInfo = new JObject();

            //Create instance
            return new EagleNetObjectInstance(guid, ctx, constructorInfo);
        }

        public void SendMessage(IEagleTarget target, EagleNetObjectOpcode opcode, string guid, JObject payload)
        {
            //Wrap
            JObject msg = new JObject();
            msg["o"] = (int)opcode;
            msg["g"] = guid;
            msg["p"] = payload;

            //Send
            io.Send(msg, target);
        }

        private void Io_OnClientConnect(IEagleClient client)
        {
            //Dispatch
            collection.Enumerate((IEagleNetObjectInternalIO o) => o.OnClientConnect(client));

            //Notify about the control object. The control object is a "static" object that clients are told about. It's used to get everything else.
            if (control != null)
                SendMessage(client, EagleNetObjectOpcode.SET_CONTROL_OBJECT, control.Guid, new JObject());
        }

        private void Io_OnReceive(IEagleClient client, JObject msg)
        {
            //Read parameters
            int opcode;
            string guid;
            JObject payload;
            if (!msg.TryGetInt("o", out opcode) || !msg.TryGetString("g", out guid) || !msg.TryGetObject("p", out payload))
                return;

            //Look for this object
            IEagleNetObjectInternalIO target;
            if (!collection.TryGetItemByGuid(guid, out target))
            {
                Log(EagleLogLevel.DEBUG, $"Client attempted to send request to unknown NetObject [{guid}]. Ignoring...");
                return;
            }

            //Send
            target.OnClientMessage(client, (EagleNetObjectOpcode)opcode, payload);
        }

        private void Log(EagleLogLevel log, string msg)
        {
            Logger.Log(log, GetType().Name, msg);
        }
    }
}
