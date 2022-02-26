using EagleWeb.Common;
using EagleWeb.Common.IO.Sockets;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.Plugin;
using EagleWeb.Common.Plugin.Interfaces.Radio;
using EagleWeb.Common.Plugin.Interfaces.RadioSession;
using EagleWeb.Common.Radio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace EagleWeb.Core.Plugins
{
    class EagleLoadedPlugin : IEaglePluginContext
    {
        public EagleLoadedPlugin(EaglePluginInfo info, EaglePluginManager manager)
        {
            this.info = info;
            this.manager = manager;
        }

        private readonly EaglePluginInfo info;
        private readonly EaglePluginManager manager;
        private readonly Dictionary<string, IEagleObject> staticObjects = new Dictionary<string, IEagleObject>();
        private readonly Dictionary<string, IEagleSocketServer> sockets = new Dictionary<string, IEagleSocketServer>();

        public event IEaglePluginContext_OnInitEventArgs OnInit;

        public EaglePluginInfo Info => info;
        public string PluginId => GeneratePluginId(info);
        public IEagleContext Context => manager.Ctx;
        public Dictionary<string, IEagleObject> StaticObjects => staticObjects;

        public void Init()
        {
            OnInit?.Invoke();
        }

        private static string GeneratePluginId(EaglePluginInfo info)
        {
            return info.developer_name + "." + info.plugin_name;
        }

        /* API */

        public T CreateObject<T>(Func<IEagleObjectContext, T> creator) where T : IEagleObject
        {
            return manager.Ctx.CreateObject(creator);
        }

        public T CreateStaticObject<T>(string key, Func<IEagleObjectContext, T> creator) where T : IEagleObject
        {
            //Ensure this doesn't already exist
            if (staticObjects.ContainsKey(key))
                throw new Exception($"Attempted to add static object \"{key}\" when that key was already in use! Keys must be unique to the plugin.");

            //Create as normal
            T obj = CreateObject(creator);

            //Add to the list of static items
            staticObjects.Add(key, obj);

            return obj;
        }

        public IEagleSocketServer RegisterSocketServer(string friendlyName, IEagleSocketHandler handler)
        {
            //Wrap the friendly name
            friendlyName = PluginId + "." + friendlyName;

            //Run normally
            IEagleSocketServer server = manager.Ctx.RegisterSocketServer(friendlyName, handler);

            //Add
            lock (sockets)
                sockets.Add(friendlyName, server);

            return server;
        }

        public void RegisterModuleRadio(string id, Func<IEagleRadio, IEagleRadioModule> module)
        {
            manager.Ctx.RadioModules.RegisterApplication(PluginId + "." + id, module);
        }

        public void RegisterModuleRadioSession(string id, Func<IEagleRadioSession, IEagleRadioSessionModule> module)
        {
            manager.Ctx.RadioSessionModules.RegisterApplication(PluginId + "." + id, module);
        }

        public void Log(EagleLogLevel level, string topic, string message)
        {
            manager.Ctx.Log(level, "EagleLoadedPlugin-" + PluginId, message);
        }
    }
}
