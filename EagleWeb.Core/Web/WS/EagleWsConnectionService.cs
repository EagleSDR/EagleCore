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
    public delegate EagleBaseConnection EagleWsConnectionService_CreateConnection(EagleWsConnectionService ctx, EagleAccount account);

    public class EagleWsConnectionService : IEagleWebServerService
    {
        public EagleWsConnectionService(EagleContext ctx, EagleWsConnectionService_CreateConnection constructor)
        {
            this.ctx = ctx;
            this.constructor = constructor;
        }

        private readonly EagleContext ctx;
        private readonly EagleWsConnectionService_CreateConnection constructor;

        public EagleContext Ctx => ctx;

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
            EagleBaseConnection connection = constructor(this, account);

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
