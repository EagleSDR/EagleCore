using EagleWeb.Core.Auth;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EagleWeb.Core.Web.Services
{
    public class LoginService : IEagleWebServerService
    {
        public LoginService(EagleContext ctx)
        {
            this.ctx = ctx;
        }

        private EagleContext ctx;

        public async Task HandleRequest(HttpContext e)
        {
            //Decode request
            IFormCollection form;
            try
            {
                form = await e.Request.ReadFormAsync();
            } catch
            {
                e.Response.StatusCode = 400;
                return;
            }

            //Get parts
            string username;
            string password;
            if (!form.TryGetString("username", out username) || !form.TryGetString("password", out password))
            {
                e.Response.StatusCode = 400;
                return;
            }

            //Attempt to authenticate account
            bool success = ctx.Auth.Authenticate(username, password, out EagleAccount account);
            string token = success ? ctx.Sessions.CreateSession(account) : null;

            //Create response
            JObject response = new JObject();
            response["success"] = success;
            response["token"] = token;

            //Send
            await e.Response.WriteJsonAsync(response);
        }
    }
}
