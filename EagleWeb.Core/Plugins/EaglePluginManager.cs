using EagleWeb.Common;
using EagleWeb.Common.Plugin;
using EagleWeb.Core.Misc;
using EagleWeb.Core.Plugins.Loader;
using EagleWeb.Core.Plugins.Package;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;

namespace EagleWeb.Core.Plugins
{
    class EaglePluginManager
    {
        public EaglePluginManager(EagleContext ctx, DirectoryInfo pluginDir)
        {
            //Set
            this.ctx = ctx;
            this.pluginDir = pluginDir;

            //Create and activate cache
            cache = new EagleUnmanagedCache(ctx, pluginDir.CreateSubdirectory("cache"));
            cache.Activate();
        }

        private readonly EagleContext ctx;
        private readonly DirectoryInfo pluginDir;
        private readonly EagleUnmanagedCache cache;
        private readonly List<IEaglePluginPackageAsset> assets = new List<IEaglePluginPackageAsset>();
        private readonly List<EaglePluginContext> plugins = new List<EaglePluginContext>();

        public List<EaglePluginContext> LoadedPlugins => plugins;
        public int PluginsCount => plugins.Count;

        public void LoadPlugins()
        {
            //Find, load, and unpack all packages
            List<EagleInternalLoadedPlugin> newPlugins = ScanPlugins();
            plugins.AddRange(newPlugins);

            //Clean the cache since all required files have been extracted
            cache.Clean();

            //Validate dependencies
            foreach (var p in newPlugins)
                p.ValidateDependencies();

            //Add all assets
            foreach (var p in newPlugins)
                assets.AddRange(p.Package.Assets);

            //Load all modules
            foreach (var p in newPlugins)
                p.LoadModules();

            //Create all modules
            foreach (var p in newPlugins)
                p.InitializeModules();

            //Send out init events
            foreach (var p in newPlugins)
                p.Init();

            //Count up successfully loaded plugins
            int loadedOk = 0;
            foreach (var p in newPlugins)
                loadedOk += p.LoadedSuccessfully ? 1 : 0;

            //Log
            Log(EagleLogLevel.INFO, $"Finished loading plugins. {loadedOk} of {newPlugins.Count} plugins were successfully loaded.");
            if (plugins.Count == 0)
                Log(EagleLogLevel.WARN, "No plugins are loaded. This application will be essentially useless until plugins are installed!");
        }

        private List<EagleInternalLoadedPlugin> ScanPlugins()
        {
            //Get plugin list
            FileInfo[] pluginFiles = pluginDir.GetFiles("*.egk");

            //Load each
            List<EagleInternalLoadedPlugin> newPlugins = new List<EagleInternalLoadedPlugin>();
            foreach (var p in pluginFiles)
                newPlugins.Add(new EagleInternalLoadedPlugin(p.FullName, this));

            return newPlugins;
        }

        public bool TryFindPluginByName(string developerName, string pluginName, out EaglePluginContext plugin)
        {
            //Search
            foreach (var p in plugins)
            {
                if (p.Package.DeveloperName == developerName && p.Package.PluginName == pluginName)
                {
                    plugin = p;
                    return true;
                }
            }

            //Failed
            plugin = null;
            return false;
        }

        public bool TryGetAsset(string hash, out IEaglePluginPackageAsset asset)
        {
            //Search
            foreach (var a in assets)
            {
                if (a.Hash == hash)
                {
                    asset = a;
                    return true;
                }
            }

            //Failed
            asset = null;
            return false;
        }

        private void Log(EagleLogLevel level, string message)
        {
            ctx.Log(level, "EaglePluginManager", message);
        }

        class EagleInternalLoadedPlugin : EaglePluginContext
        {
            public EagleInternalLoadedPlugin(string packageFilename, EaglePluginManager manager) : base(new EaglePluginPackage(packageFilename), manager.ctx)
            {
                //Set
                this.manager = manager;

                //Create the loader context that'll handle loading
                loader = new EaglePluginPackageLoader(PluginId, manager.cache);

                //Load the package in. This'll automatically unpack unmanaged files
                loader.AddPackage(Package);

                //Create modules (but don't load em)
                modules = new EagleInternalLoadedPluginModule[Package.Modules.Length];
                for (int i = 0; i < modules.Length; i++)
                    modules[i] = new EagleInternalLoadedPluginModule(this, Package.Modules[i]);
            }

            private readonly EaglePluginManager manager;
            private readonly EaglePluginPackageLoader loader;
            private readonly EagleInternalLoadedPluginModule[] modules;

            public bool LoadedSuccessfully
            {
                get
                {
                    bool ok = true;
                    foreach (var m in modules)
                        ok = ok && m.LoadedSuccessfully;
                    return ok;
                }
            }

            public void ValidateDependencies()
            {
                foreach (var d in Package.Dependencies)
                {
                    //Search for the plugin
                    EaglePluginContext depend;
                    if (manager.TryFindPluginByName(d.DeveloperName, d.PluginName, out depend))
                    {
                        //Found; Validate the version
                        if (depend.Package.PluginVersion.Major > d.MinVersion.Major || (depend.Package.PluginVersion.Major == d.MinVersion.Major && depend.Package.PluginVersion.Minor >= d.MinVersion.Minor))
                        {
                            //OK!
                            continue;
                        } else
                        {
                            //Plugin is too old!
                            manager.Log(EagleLogLevel.FATAL, $"Can't use plugin \"{PluginId}\": Depends on plugin \"{d.DeveloperName}.{d.PluginName}\" (>= v{d.MinVersion.Major}.{d.MinVersion.Minor}) which is installed, but out of date (currently v{depend.Package.PluginVersion.Major}.{depend.Package.PluginVersion.Minor}).");
                        }
                    } else
                    {
                        //Plugin is not installed!
                        manager.Log(EagleLogLevel.FATAL, $"Can't use plugin \"{PluginId}\": Depends on plugin \"{d.DeveloperName}.{d.PluginName}\" (>= v{d.MinVersion.Major}.{d.MinVersion.Minor}) which is not installed.");
                    }

                    //Print more info and abort
                    manager.Log(EagleLogLevel.FATAL, $"The application will now quit. Either install \"{d.DeveloperName}.{d.PluginName}\" (>= v{d.MinVersion.Major}.{d.MinVersion.Minor}) or remove \"{PluginId}\" and restart EagleSDR.");
                    throw new Exception("Dependency mismatch.");
                }
            }

            public void LoadModules()
            {
                foreach (var m in modules)
                    m.Load();
            }

            public void InitializeModules()
            {
                foreach (var m in modules)
                    m.Initialize();
            }

            public override bool TryFindModuleByClassnameAny(string classname, out object module)
            {
                //Search
                foreach (var m in modules)
                {
                    if (m.Info.ClassName == classname)
                    {
                        //Found! Make sure it is loaded correctly
                        if (m.LoadedSuccessfully)
                        {
                            //Set result
                            module = m.Module;
                            return true;
                        } else
                        {
                            //Failed. Warn the user
                            manager.Log(EagleLogLevel.WARN, $"Attempted to locate module \"{classname}\" in plugin \"{PluginId}\", but returning failure. The module exists, but did not load correctly!");
                            break;
                        }
                    }
                }

                //Fail
                module = null;
                return false;
            }

            class EagleInternalLoadedPluginModule
            {
                public EagleInternalLoadedPluginModule(EagleInternalLoadedPlugin plugin, IEaglePluginPackageModule info)
                {
                    this.plugin = plugin;
                    this.info = info;
                }

                private readonly EagleInternalLoadedPlugin plugin;
                private readonly IEaglePluginPackageModule info;

                private Assembly assembly;
                private object module;

                public IEaglePluginPackageModule Info => info;
                public bool LoadedSuccessfully => module != null;
                public object Module
                {
                    get
                    {
                        if (module == null)
                            throw new Exception("The module has not yet loaded (or was not loaded successfully!)");
                        return module;
                    }
                }

                public void Load()
                {
                    plugin.manager.Log(EagleLogLevel.DEBUG, $"Loading managed library \"{info.DllName}\" for {plugin.PluginId}...");
                    try
                    {
                        if (!plugin.loader.TryLoadFromPackageName(info.DllName, out assembly))
                        {
                            assembly = null;
                            plugin.manager.Log(EagleLogLevel.ERROR, $"Failed to load {plugin.PluginId}: Managed library \"{info.DllName}\" could not be found within the package. The package is misconfigured.");
                        }
                    }
                    catch (Exception ex)
                    {
                        plugin.manager.Log(EagleLogLevel.ERROR, $"Failed to load {plugin.PluginId}: Managed library \"{info.DllName}\" could not be loaded: {ex.Message}{ex.StackTrace}");
                    }
                }

                public void Initialize()
                {
                    if (assembly != null)
                    {
                        //Get the class
                        Type type;
                        try
                        {
                            type = assembly.GetType(info.ClassName);
                            if (type == null)
                                throw new Exception("Unable to find type.");
                        }
                        catch
                        {
                            plugin.manager.Log(EagleLogLevel.ERROR, $"Failed to load {plugin.PluginId}: \"{info.ClassName}\" was not a valid type in {info.DllName}.");
                            return;
                        }

                        //Construct
                        plugin.manager.Log(EagleLogLevel.DEBUG, $"Constructing class \"{type.FullName}\" from plugin {plugin.PluginId}...");
                        try
                        {
                            module = Activator.CreateInstance(type, plugin);
                        }
                        catch (Exception ex)
                        {
                            plugin.manager.Log(EagleLogLevel.ERROR, $"Failed to load {plugin.PluginId}: Construction of \"{type.FullName}\" failed: {ex.Message}{ex.StackTrace}");
                            return;
                        }
                    }
                }
            }
        }
    }
}
