using EagleWeb.Core.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace EagleWeb.Core.Web.WS
{
    public abstract class EagleWsConnectionService2 : IEagleWebServerService
    {
        public EagleWsConnectionService2(EagleContext ctx)
        {
            this.ctx = ctx;
        }

        private readonly EagleContext ctx;

        public EagleContext Ctx => ctx;

        protected abstract bool CreateConnection(HttpContext e, EagleAccount account, out EagleBaseConnection connection);

        public async Task HandleRequest(HttpContext e)
        {
            //Make sure this is ACTUALLY a WebSocket request
            if (!e.WebSockets.IsWebSocketRequest)
            {
                e.Response.StatusCode = 404;
                return;
            }

            //Get access token
            string token;
            if (!e.Request.Query.TryGetString("access_token", out token))
            {
                e.Response.StatusCode = 401;
                return;
            }

            //Authenticate access token
            EagleAccount account;
            if (!ctx.Sessions.Authenticate(token, out account))
            {
                e.Response.StatusCode = 403;
                return;
            }

            //Create the connection
            if (!CreateConnection(e, account, out EagleBaseConnection connection))
                return;

            //Do additional steps
            if (!connection.Authenticate(e))
                return;

            //Open this as a websocket
            WebSocket sock = await e.WebSockets.AcceptWebSocketAsync();

            //Run
            await connection.RunAsync(sock);
        }
    }
}
