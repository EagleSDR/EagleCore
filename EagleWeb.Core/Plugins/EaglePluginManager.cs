using EagleWeb.Common;
using EagleWeb.Common.Plugin;
using EagleWeb.Core.Misc;
using EagleWeb.Package;
using EagleWeb.Package.Data;
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

            //Load database
            db = new DataFile<List<EaglePluginInfo>>(DbPathname, new List<EaglePluginInfo>());
        }

        private EagleContext ctx;
        private DataFile<List<EaglePluginInfo>> db;
        private List<EagleLoadedPlugin> loaded = new List<EagleLoadedPlugin>();
        private DirectoryInfo pluginDir;

        public EagleContext Ctx => ctx;
        public DirectoryInfo PluginInstallPath => new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory); //wish this could be somewhere else...
        private string DbPathname => ctx.Root.FullName + Path.DirectorySeparatorChar + "plugins.json";
        public List<EagleLoadedPlugin> LoadedPlugins => loaded;

        private void Log(EagleLogLevel level, string message)
        {
            ctx.Log(level, "EaglePluginManager", message);
        }

        public bool TryGetAsset(string hash, out EaglePluginAssetInfo info, out Stream s)
        {
            //Set defaults
            s = null;
            info = null;

            //Get the filename and check if it exists
            string filename = pluginDir.CreateSubdirectory("assets").FullName + Path.DirectorySeparatorChar + hash + ".asset";
            if (!File.Exists(filename))
                return false;

            //Read the plugin info
            string infoFilename = filename + "info";
            if (!File.Exists(infoFilename))
            {
                Log(EagleLogLevel.WARN, "Attempted to load asset without asset info! Asset file exists, but the info file does not. This plugin may not have been correctly installed.");
                return false;
            }

            //Read info
            info = JsonConvert.DeserializeObject<EaglePluginAssetInfo>(File.ReadAllText(infoFilename));

            //Open file
            s = new FileStream(filename, FileMode.Open, FileAccess.Read);
            return true;
        }
        
        public void CreateAll()
        {
            //Construct each plugin
            Log(EagleLogLevel.INFO, $"Loading plugins...");
            foreach (var p in db.Data)
                ConstructPlugin(p);
        }

        public void InitAll()
        {
            //Initialize all
            Log(EagleLogLevel.INFO, $"Initializing plugins...");
            foreach (var p in loaded)
                p.Init();
        }

        private void ConstructPlugin(EaglePluginInfo p)
        {
            //Create the context for it
            EagleInternalLoadedPlugin plugin = new EagleInternalLoadedPlugin(p, this);
            loaded.Add(plugin);

            //Go through each module
            foreach (var m in p.modules)
            {
                //First, load the assembly
                Assembly asm;
                Log(EagleLogLevel.DEBUG, $"Loading managed library \"{m.dll}\" for {p.developer_name}.{p.plugin_name}...");
                try
                {
                    asm = plugin.LoadAssembly(PluginInstallPath.FullName + Path.DirectorySeparatorChar + m.dll);
                } catch (Exception ex)
                {
                    Log(EagleLogLevel.ERROR, $"Failed to load {p.developer_name}.{p.plugin_name}: Managed library \"{m.dll}\" could not be loaded: {ex.Message}{ex.StackTrace}");
                    continue;
                }

                //Get the class
                Type type;
                try
                {
                    type = asm.GetType(m.classname);
                    if (type == null)
                        throw new Exception("Unable to find type.");
                } catch
                {
                    Log(EagleLogLevel.ERROR, $"Failed to load {p.developer_name}.{p.plugin_name}: \"{m.classname}\" was not a valid type in {m.dll}.");
                    continue;
                }

                //Construct
                Log(EagleLogLevel.DEBUG, $"Constructing class \"{m.classname}\" from plugin {p.developer_name}.{p.plugin_name}...");
                object item;
                try
                {
                    item = Activator.CreateInstance(type, plugin);
                }
                catch (Exception ex)
                {
                    Log(EagleLogLevel.ERROR, $"Failed to load {p.developer_name}.{p.plugin_name}: Construction of \"{m.classname}\" failed: {ex.Message}{ex.StackTrace}");
                    continue;
                }

                //Add
                plugin.AddLoadedModule(item);
            }
        }

        class EagleInternalLoadedPlugin : EagleLoadedPlugin
        {
            public EagleInternalLoadedPlugin(EaglePluginInfo info, EaglePluginManager manager) : base(info, manager)
            {
                loader = new AssemblyLoadContext(PluginId);
            }

            private readonly AssemblyLoadContext loader;
            private readonly List<object> loadedModules = new List<object>();

            public Assembly LoadAssembly(string fullname)
            {
                return loader.LoadFromAssemblyPath(fullname);
            }

            public void AddLoadedModule(object module)
            {
                loadedModules.Add(module);
            }
        }
    }
}
