using EagleWeb.Common;
using EagleWeb.Common.Auth;
using EagleWeb.Common.IO;
using EagleWeb.Core.Auth;
using EagleWeb.Core.NetObjects.Enums;
using EagleWeb.Core.Web.WS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.NetObjects
{
    /// <summary>
    /// Represents a WebSocket client.
    /// </summary>
    class EagleNetObjectClient : EagleBaseConnection, IEagleNetObjectTarget
    {
        public EagleNetObjectClient(EagleNetObjectManager manager, EagleAccount account) : base(account)
        {
            this.manager = manager;
        }

        private readonly EagleNetObjectManager manager;

        protected override void ClientReady()
        {
            //Dispatch
            manager.Collection.Enumerate((IEagleNetObjectInternalIO o) => o.OnClientConnect(this));

            //Notify about the control object. The control object is a "static" object that clients are told about. It's used to get everything else.
            if (manager.Control != null)
                SendMessage(EagleNetObjectOpcode.SET_CONTROL_OBJECT, manager.Control.Guid, new JObject());
        }

        public void SendMessage(EagleNetObjectOpcode opcode, string guid, JObject payload)
        {
            byte[] data = EagleNetObjectManager.EncodeMessage(opcode, guid, payload);
            Send(data, 0, data.Length, true);
        }

        protected override void ClientReceive(byte[] data, int count, bool asText)
        {
            //Drop non-text messages
            if (!asText)
            {
                Log(EagleLogLevel.DEBUG, "Client attempted to send non-text message. Dropping...");
                return;
            }

            //Decode JSON
            JObject msg;
            try
            {
                msg = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(data, 0, count));
            } catch
            {
                Log(EagleLogLevel.INFO, "Client sent invalid JSON! Dropping...");
                return;
            }

            //Read parameters
            int opcode;
            string guid;
            JObject payload;
            if (!msg.TryGetInt("o", out opcode) || !msg.TryGetString("g", out guid) || !msg.TryGetObject("p", out payload))
                return;

            //Look for this object
            IEagleNetObjectInternalIO target;
            if (!manager.Collection.TryGetItemByGuid(guid, out target))
            {
                Log(EagleLogLevel.DEBUG, $"Client attempted to send request to unknown NetObject [{guid}]. Ignoring...");
                return;
            }

            //Send
            target.OnClientMessage(this, (EagleNetObjectOpcode)opcode, payload);
        }

        protected override void ClientClosed()
        {
            
        }

        private void Log(EagleLogLevel log, string msg)
        {
            manager.Ctx.Log(log, GetType().Name, msg);
        }
    }
}
