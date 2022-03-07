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

        private JObject CreateResponsePluginAssets(EaglePluginContext plugin)
        {
            JObject response = new JObject();
            foreach (var a in plugin.Package.Assets)
                response.Add(a.FileName, a.Hash);
            return response;
        }

        private JObject CreateResponsePlugin(EaglePluginContext plugin)
        {
            JObject response = new JObject();
            response["id"] = plugin.PluginId;
            response["plugin_name"] = plugin.Package.PluginName;
            response["developer_name"] = plugin.Package.DeveloperName;
            response["version_major"] = plugin.Package.PluginVersion.Major;
            response["version_minor"] = plugin.Package.PluginVersion.Minor;
            response["version_build"] = plugin.Package.PluginVersion.Build;
            response["assets"] = CreateResponsePluginAssets(plugin);
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
            response["sockets"] = JObject.FromObject(ctx.Sockets.GetNameMap());
            return response;
        }

        public async Task HandleRequest(HttpContext e)
        {
            await e.Response.WriteJsonAsync(CreateResponse());
        }
    }
}
