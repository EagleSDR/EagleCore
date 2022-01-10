using EagleWeb.Common;
using EagleWeb.Common.NetObjects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Web
{
    internal class EagleControlObject : EagleObject
    {
        public EagleControlObject(EagleContext ctx) : base(ctx, null)
        {
            this.ctx = ctx;
        }

        private EagleContext ctx;

        protected override void ConfigureObject(IEagleObjectConfigureContext context)
        {
            base.ConfigureObject(context);
            context.CreatePortApi("GetComponents")
                .Bind(GetComponentsHandler);
            context.CreatePortApi("GetPluginModules")
                .Bind(GetPluginModulesHandler);
        }

        private JObject GetComponentsHandler(IEagleClient client, JObject request)
        {
            JObject response = new JObject();
            response["radio"] = ctx.Radio.Guid;
            response["file_manager"] = ctx.FileManager.Guid;
            return response;
        }

        private JObject GetPluginModulesHandler(IEagleClient client, JObject request)
        {
            //Enumerate plugins
            JObject plugins = new JObject();
            foreach (var p in ctx.PluginManager.LoadedPlugins)
            {
                //Enumerate modules
                JObject modules = new JObject();
                foreach (var m in p.Modules)
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
