using EagleWeb.Common;
using EagleWeb.Common.Auth;
using EagleWeb.Common.IO.Sockets;
using EagleWeb.Core.Auth;
using EagleWeb.Core.Web.WS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Web.Sockets
{
    class EagleSocketClient : EagleBaseConnection, IEagleSocketClient
    {
        public EagleSocketClient(EagleSocketServer server, EagleAccount account) : base(account)
        {
            this.server = server;
        }

        private readonly EagleSocketServer server;

        IEagleAccount IEagleSocketClient.Account => Account;
        public object Custom { get; set; }
        public IEagleContext Context => server.Context;

        public event IEagleSocketClient_OnReceiveJsonArgs OnReceiveJson;
        public event IEagleSocketClient_OnReceiveArgs OnReceive;
        public event IEagleSocketClient_OnCloseArgs OnClose;

        protected override void ClientReady()
        {
            //Fire in parent
            server.AddClient(this);
        }

        protected override void ClientReceive(byte[] data, int count, bool asText)
        {
            //Fire event
            OnReceive?.Invoke(this, data, count, asText);

            //Decode as JSON if bound
            if (asText && OnReceiveJson != null)
            {
                //Read as text
                string dataString = Encoding.UTF8.GetString(data, 0, count);

                //Attempt to decode
                JObject dataJson;
                try
                {
                    dataJson = JsonConvert.DeserializeObject<JObject>(dataString);
                } catch
                {
                    return;
                }

                //Fire event
                OnReceiveJson?.Invoke(this, dataJson);
            }
        }

        protected override void ClientClosed()
        {
            //Fire in parent
            server.RemoveClient(this);

            //Fire event
            OnClose?.Invoke(this);
        }
    }
}