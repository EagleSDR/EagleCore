using EagleWeb.Common;
using EagleWeb.Common.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Web.IO.Rpc
{
    public class EagleRpcManager : IEagleTarget
    {
        public EagleRpcManager(EagleContext ctx)
        {
            this.ctx = ctx;
        }

        private readonly EagleContext ctx;
        private readonly List<IEagleClient> clients = new List<IEagleClient>();

        public IEagleTarget TargetAll => this;

        public event EaglePortIO_OnReceiveEventArgs OnReceive;
        public event EaglePortIO_OnConnectEventArgs OnClientConnect;
        public event EaglePortIO_OnConnectEventArgs OnClientDisconnect;

        public void Deliver(byte[] payload, int count)
        {
            //Send to all connected clients...
            lock(clients)
            {
                foreach (var c in clients)
                    c.Deliver(payload, count);
            }
        }

        public void Send(JObject payload, IEagleTarget target)
        {
            byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
            target.Deliver(data, data.Length);
        }

        public void AddClient(IEagleClient client)
        {
            lock (clients)
                clients.Add(client);
            OnClientConnect?.Invoke(client);
        }

        public void RemoveClient(IEagleClient client)
        {
            lock (clients)
                clients.Remove(client);
            OnClientDisconnect?.Invoke(client);
        }

        public void ClientReceive(byte[] data, int count, bool asText, IEagleClient client)
        {
            //If it isn't text, ignore
            if (!asText)
                return;

            //Decode as JSON
            JObject payload;
            try
            {
                payload = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(data, 0, count));
            }
            catch
            {
                return;
            }

            //Send IO event
            OnReceive?.Invoke(client, payload);
        }
    }
}
