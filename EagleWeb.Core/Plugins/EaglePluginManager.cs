using EagleWeb.Common;
using EagleWeb.Common.Plugin;
using EagleWeb.Core.Misc;
using EagleWeb.Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Core.Plugins
{
    public class EaglePluginManager
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

        public bool TryGetAsset(string hash, out Stream s)
        {
            //Get the filename
            string filename = pluginDir.CreateSubdirectory("assets").FullName + Path.DirectorySeparatorChar + hash + ".asset";
            if (File.Exists(filename))
            {
                s = new FileStream(filename, FileMode.Open, FileAccess.Read);
                return true;
            } else
            {
                s = null;
                return false;
            }
        }

        public void Init()
        {
            //Construct each plugin
            Log(EagleLogLevel.INFO, $"Loading plugins...");
            foreach (var p in db.Data)
                InitPlugin(p);

            //Initialize all
            Log(EagleLogLevel.INFO, $"Initializing plugins...");
            foreach (var p in loaded)
                p.Init();

            //Log
            Log(EagleLogLevel.INFO, $"Successfully loaded and initialized {db.Data.Count} plugins!");
        }

        private void InitPlugin(EaglePluginInfo p)
        {
            //Create the context for it
            EagleLoadedPlugin plugin = new EagleLoadedPlugin(p, this);
            loaded.Add(plugin);

            //Go through each module
            foreach (var m in p.modules)
            {
                //First, load the assembly
                Assembly asm = plugin.GetAssembly(m.dll);

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
                Log(EagleLogLevel.DEBUG, $"Constructing class \"{m.classname}\" from plugin {p.developer_name}.{p.plugin_name}");
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

                //Sanity check
                if (!(item is EagleObjectPlugin))
                {
                    Log(EagleLogLevel.ERROR, $"Failed to load {p.developer_name}.{p.plugin_name}: \"{m.classname}\" must be of type {typeof(EagleObjectPlugin).Name}, but it is instead {item.GetType().Name}.");
                    continue;
                }

                //Add
                plugin.RegisterModule(m.classname, item as EagleObjectPlugin);
            }
        }
    }
}
