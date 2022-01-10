using EagleWeb.Common.Auth;
using EagleWeb.Common.IO.Streams;
using EagleWeb.Core.Auth;
using EagleWeb.Core.Web.WS;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace EagleWeb.Core.Web.IO.Streams
{
    public class EagleStreamClient : EagleBaseConnection, IEagleStreamClient
    {
        public EagleStreamClient(EagleStreamService ctx, EagleAccount account) : base(ctx, account)
        {
            streamCtx = ctx;
        }

        private EagleStreamService streamCtx;
        private EagleStreamServer server;

        public event IEagleStreamClient_ReceiveBinaryEventArgs OnReceiveBinary;
        public event IEagleStreamClient_ReceiveTextEventArgs OnReceiveText;
        public event IEagleStreamClient_CloseEventArgs OnClose;

        IEagleAccount IEagleStreamClient.Account => Account;

        public object UserData { get; set; }

        public override bool Authenticate(HttpContext e)
        {
            if (!base.Authenticate(e))
                return false;

            //Make sure this stream ID is valid and get the server
            if (!e.Request.Query.TryGetString("stream", out string streamId) || !streamCtx.TryGetServer(streamId, out server))
            {
                e.Response.StatusCode = 404;
                return false;
            }

            return true;
        }

        protected override void ClientReady()
        {
            //Send event to server
            server.Server.HandleClientConnect(this);
        }

        protected override void ClientReceive(byte[] data, int count, bool asText)
        {
            if (asText)
                OnReceiveText?.Invoke(this, Encoding.UTF8.GetString(data, 0, count));
            else
                OnReceiveBinary?.Invoke(this, data, count);
        }

        protected override void ClientClosed()
        {
            //Send event
            OnClose?.Invoke(this);

            //Send event to server
            server.Server.HandleClientDisconnect(this);
        }
    }
}
