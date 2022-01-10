using EagleWeb.Common;
using EagleWeb.Common.Auth;
using EagleWeb.Common.IO;
using EagleWeb.Core.Auth;
using EagleWeb.Core.Web.WS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace EagleWeb.Core.Web.IO.Rpc
{
    public class EagleRpcConnection : EagleBaseConnection, IEagleClient
    {
        public EagleRpcConnection(EagleWsConnectionService ctx, EagleRpcManager manager, EagleAccount account) : base(ctx, account)
        {
            this.manager = manager;
        }

        private EagleRpcManager manager;

        IEagleAccount IEagleClient.Account => Account;

        protected override void ClientReady()
        {
            manager.AddClient(this);
        }

        protected override void ClientReceive(byte[] data, int count, bool asText)
        {
            manager.ClientReceive(data, count, asText, this);
        }

        protected override void ClientClosed()
        {
            manager.RemoveClient(this);
        }

        public void Deliver(byte[] data, int len)
        {
            Send(data, 0, len, true);
        }
    }
}
