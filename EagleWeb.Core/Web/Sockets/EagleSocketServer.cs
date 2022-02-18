using EagleWeb.Common;
using EagleWeb.Common.IO.Sockets;
using EagleWeb.Core.Auth;
using EagleWeb.Core.Web.WS;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Web.Sockets
{
    class EagleSocketServer : IEagleSocketServer
    {
        public EagleSocketServer(EagleContext ctx, Guid id, string friendlyName, IEagleSocketHandler handler)
        {
            this.ctx = ctx;
            this.id = id;
            this.friendlyName = friendlyName;
            this.handler = handler;
        }

        private readonly EagleContext ctx;
        private readonly Guid id;
        private readonly string friendlyName;
        private readonly IEagleSocketHandler handler;

        private List<EagleSocketClient> connected = new List<EagleSocketClient>();

        public string Id => id.ToString();
        public string FriendlyName => friendlyName;
        public IEagleContext Context => ctx;

        public void AddClient(EagleSocketClient client)
        {
            //Add to list of clients
            lock (connected)
                connected.Add(client);

            //Invoke handler
            handler.OnClientConnect(client);
        }

        public void RemoveClient(EagleSocketClient client)
        {
            //Remove from list of clients
            lock (connected)
                connected.Remove(client);

            //Invoke handler
            handler.OnClientDisconnect(client);
        }

        public void SendAll(byte[] data, int offset, int count, bool asText)
        {
            //Enumerate
            lock (connected)
            {
                foreach (var conn in connected)
                    conn.Send(data, offset, count, asText);
            }
        }
    }
}
