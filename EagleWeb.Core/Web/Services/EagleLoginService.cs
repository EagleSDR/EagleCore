using EagleWeb.Core.Auth;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EagleWeb.Core.Web.Services
{
    class EagleLoginService : IEagleWebServerService
    {
        public EagleLoginService(EagleContext ctx)
        {
            this.ctx = ctx;
        }

        private EagleContext ctx;

        public async Task HandleRequest(HttpContext e)
        {
            //Decode request
            LoginRequest request;
            try
            {
                request = await e.Request.ReadAsJsonAsync<LoginRequest>();
            } catch
            {
                e.Response.StatusCode = 400;
                return;
            }

            //Validate
            if (request.username == null || request.password == null)
            {
                e.Response.StatusCode = 400;
                return;
            }

            //Attempt to authenticate account
            bool success = ctx.Auth.Authenticate(request.username, request.password, out EagleAccount account);
            string token = success ? ctx.Sessions.CreateSession(account) : null;

            //Create response
            JObject response = new JObject();
            response["success"] = success;
            response["token"] = token;

            //Send
            await e.Response.WriteJsonAsync(response);
        }

        class LoginRequest
        {
            public string username;
            public string password;
        }
    }
}
