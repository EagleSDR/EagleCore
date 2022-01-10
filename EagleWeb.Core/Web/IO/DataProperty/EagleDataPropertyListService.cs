using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EagleWeb.Core.Web.IO.DataProperty
{
    public class EagleDataPropertyListService : IEagleWebServerService
    {
        public EagleDataPropertyListService(EagleDataPropertyManager manager)
        {
            this.manager = manager;
        }

        private readonly EagleDataPropertyManager manager;

        public async Task HandleRequest(HttpContext e)
        {
            //Create
            JObject payload = new JObject();
            payload["properties"] = manager.CreatePropertyDefinitions();

            //Send
            await e.Response.WriteJsonAsync(payload);
        }
    }
}
