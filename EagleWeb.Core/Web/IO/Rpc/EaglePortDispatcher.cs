using EagleWeb.Common;
using EagleWeb.Common.IO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Web.IO.Rpc
{
    public class EaglePortDispatcher : IEaglePortDispatcher
    {
        public EaglePortDispatcher(IEaglePortIO parent)
        {
            this.parent = parent;
            parent.OnReceive += Parent_OnReceive;
        }

        private IEaglePortIO parent;
        private Dictionary<string, DispatcherIO> outlets = new Dictionary<string, DispatcherIO>();

        public string Id => parent.Id;

        private void Parent_OnReceive(IEagleClient client, JObject payload)
        {
            //Get the parts
            if (!payload.TryGetValue("opcode", out JToken opcodeToken) || opcodeToken.Type != JTokenType.String)
                return;
            if (!payload.TryGetValue("payload", out JToken payloadToken) || payloadToken.Type != JTokenType.Object)
                return;

            //Find an outlet with this opcode
            if (outlets.TryGetValue((string)opcodeToken, out DispatcherIO port))
            {
                port.Receive(client, (JObject)payloadToken);
            }
        }

        private void SendOutgoing(string opcode, JObject msg, IEagleTarget target)
        {
            //Create outgoing payload
            JObject payload = new JObject();
            payload["opcode"] = opcode;
            payload["payload"] = msg;

            //Send
            parent.Send(payload, target);
        }

        public IEaglePortDispatcher CreatePortDispatcher(string opcode)
        {
            return new EaglePortDispatcher(CreatePort(opcode));
        }

        public IEaglePortIO CreatePort(string opcode)
        {
            DispatcherIO io = new DispatcherIO(this, opcode);
            outlets.Add(opcode, io);
            return io;
        }

        class DispatcherIO : IEaglePortIO
        {
            public DispatcherIO(EaglePortDispatcher dispatcher, string opcode)
            {
                this.dispatcher = dispatcher;
                this.opcode = opcode;
                id = dispatcher.Id + "." + opcode;
                dispatcher.parent.OnClientConnect += Parent_OnClientConnect;
                dispatcher.parent.OnClientDisconnect += Parent_OnClientDisconnect;
            }

            private EaglePortDispatcher dispatcher;
            private string opcode;
            private string id;

            public IEagleTarget TargetAll => dispatcher.parent.TargetAll;

            public string Id => id;

            public event EaglePortIO_OnReceiveEventArgs OnReceive;
            public event EaglePortIO_OnConnectEventArgs OnClientConnect;
            public event EaglePortIO_OnConnectEventArgs OnClientDisconnect;

            private void Parent_OnClientDisconnect(IEagleClient client)
            {
                OnClientDisconnect?.Invoke(client);
            }

            private void Parent_OnClientConnect(IEagleClient client)
            {
                OnClientConnect?.Invoke(client);
            }

            public void Receive(IEagleClient client, JObject payload)
            {
                OnReceive?.Invoke(client, payload);
            }

            public void Send(JObject payload, IEagleTarget target)
            {
                dispatcher.SendOutgoing(opcode, payload, target);
            }
        }
    }
}
