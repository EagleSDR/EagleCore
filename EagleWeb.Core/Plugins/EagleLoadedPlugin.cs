using EagleWeb.Common;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace EagleWeb.Core.Plugins
{
    public class EagleLoadedPlugin : IEagleObjectPluginContext
    {
        public EagleLoadedPlugin(EaglePluginInfo info, EaglePluginManager manager)
        {
            this.info = info;
            this.manager = manager;
            loader = new AssemblyLoadContext(PluginId);
        }

        private readonly EaglePluginInfo info;
        private readonly EaglePluginManager manager;
        private readonly AssemblyLoadContext loader;
        private readonly Dictionary<string, EagleObjectPlugin> modules = new Dictionary<string, EagleObjectPlugin>();

        public EaglePluginInfo Info => info;
        public string PluginId => GeneratePluginId(info);
        public IEagleContext Context => manager.Ctx;
        public IEagleObjectManager ObjectManager => manager.Ctx.ObjectManager;
        public Dictionary<string, EagleObjectPlugin> Modules => modules;

        private void Log(EagleLogLevel level, string message)
        {
            manager.Ctx.Log(level, "EagleLoadedPlugin-" + PluginId, message);
        }

        public Assembly GetAssembly(string dll)
        {
            Log(EagleLogLevel.DEBUG, $"Loading assembly \"{dll}\"...");
            string path = manager.PluginInstallPath.FullName + Path.DirectorySeparatorChar + dll;
            return loader.LoadFromAssemblyPath(path);
        }

        public void RegisterModule(string name, EagleObjectPlugin component)
        {
            modules.Add(name, component);
        }

        public void Init()
        {
            foreach (var c in modules)
            {
                c.Value.PluginInit();
            }
        }

        private static string GeneratePluginId(EaglePluginInfo info)
        {
            return info.developer_name + "." + info.plugin_name;
        }
    }
}
