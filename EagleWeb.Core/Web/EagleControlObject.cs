using EagleWeb.Common;
using EagleWeb.Common.Auth;
using EagleWeb.Common.NetObjects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Web
{
    internal class EagleControlObject : EagleObject
    {
        public EagleControlObject(IEagleObjectContext context, EagleContext ctx) : base(context)
        {
            this.ctx = ctx;

            //Configure
            context.CreatePortApi("GetComponents")
                .Bind(GetComponentsHandler);
            context.CreatePortApi("GetPluginModules")
                .Bind(GetPluginModulesHandler);
        }

        private EagleContext ctx;

        private JObject GetComponentsHandler(IEagleAccount account, JObject request)
        {
            JObject response = new JObject();
            response["radio"] = ctx.Radio.Guid;
            response["file_manager"] = ctx.FileManager.Guid;
            return response;
        }

        private JObject GetPluginModulesHandler(IEagleAccount account, JObject request)
        {
            //Enumerate plugins
            JObject plugins = new JObject();
            foreach (var p in ctx.PluginManager.LoadedPlugins)
            {
                //Enumerate static modules
                JObject modules = new JObject();
                foreach (var m in p.StaticObjects)
                    modules.Add(m.Key, m.Value.Guid);

                //Add
                plugins.Add(p.PluginId, modules);
            }

            //Build response
            JObject response = new JObject();
            response["plugins"] = plugins;
            return response;
        }
    }
}
