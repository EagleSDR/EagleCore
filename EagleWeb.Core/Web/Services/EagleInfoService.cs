using EagleWeb.Core.Plugins;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EagleWeb.Core.Web.Services
{
    /// <summary>
    /// Provides info about this server.
    /// </summary>
    class EagleInfoService : IEagleWebServerService
    {
        public EagleInfoService(EagleContext ctx)
        {
            this.ctx = ctx;
        }

        private EagleContext ctx;

        private JObject CreateResponsePlugin(EagleLoadedPlugin plugin)
        {
            JObject response = new JObject();
            response["id"] = plugin.PluginId;
            response["plugin_name"] = plugin.Info.plugin_name;
            response["developer_name"] = plugin.Info.developer_name;
            response["version_major"] = plugin.Info.version_major;
            response["version_minor"] = plugin.Info.version_minor;
            response["version_build"] = plugin.Info.version_build;
            response["assets"] = JToken.FromObject(plugin.Info.assets);
            return response;
        }

        private JArray CreateResponsePlugins()
        {
            JArray arr = new JArray();
            foreach (var p in ctx.PluginManager.LoadedPlugins)
                arr.Add(CreateResponsePlugin(p));
            return arr;
        }

        private JObject CreateResponse()
        {
            JObject response = new JObject();
            response["plugins"] = CreateResponsePlugins();
            return response;
        }

        public async Task HandleRequest(HttpContext e)
        {
            await e.Response.WriteJsonAsync(CreateResponse());
        }
    }
}
