using EagleWeb.Common;
using EagleWeb.Common.IO.Sockets;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.Plugin;
using EagleWeb.Common.Plugin.Interfaces.Radio;
using EagleWeb.Common.Plugin.Interfaces.RadioSession;
using EagleWeb.Common.Radio;
using EagleWeb.Core.Plugins.Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace EagleWeb.Core.Plugins
{
    class EaglePluginContext : IEaglePluginContext
    {
        public EaglePluginContext(EaglePluginPackage package, EagleContext context)
        {
            this.package = package;
            this.context = context;
        }

        private readonly EaglePluginPackage package;
        private readonly EagleContext context;
        private readonly Dictionary<string, IEagleObject> staticObjects = new Dictionary<string, IEagleObject>();
        private readonly Dictionary<string, IEagleSocketServer> sockets = new Dictionary<string, IEagleSocketServer>();

        public event IEaglePluginContext_OnInitEventArgs OnInit;

        public EaglePluginPackage Package => package;
        public string PluginId => GeneratePluginId(package);
        public IEagleContext Context => context;
        public Dictionary<string, IEagleObject> StaticObjects => staticObjects;

        public void Init()
        {
            OnInit?.Invoke();
        }

        private static string GeneratePluginId(EaglePluginPackage package)
        {
            return package.DeveloperName + "." + package.PluginName;
        }

        /* API */

        public T CreateObject<T>(Func<IEagleObjectContext, T> creator) where T : IEagleObject
        {
            return context.CreateObject(creator);
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
            IEagleSocketServer server = context.RegisterSocketServer(friendlyName, handler);

            //Add
            lock (sockets)
                sockets.Add(friendlyName, server);

            return server;
        }

        public void RegisterModuleRadio(string id, Func<IEagleRadio, IEagleRadioModule> module)
        {
            context.RadioModules.RegisterApplication(PluginId + "." + id, module);
        }

        public void RegisterModuleRadioSession(string id, Func<IEagleRadioSession, IEagleRadioSessionModule> module)
        {
            context.RadioSessionModules.RegisterApplication(PluginId + "." + id, module);
        }

        public void Log(EagleLogLevel level, string topic, string message)
        {
            context.Log(level, "EagleLoadedPlugin-" + PluginId, message);
        }
    }
}
