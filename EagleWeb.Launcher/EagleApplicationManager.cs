using EagleWeb.Core.Plugins;
using EagleWeb.Launcher.Misc;
using EagleWeb.Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EagleWeb.Launcher
{
    /// <summary>
    /// A temporary object allowing you to edit settings for an application.
    /// </summary>
    class EagleApplicationManager : IDisposable
    {
        public EagleApplicationManager(EagleApplication app, Action release)
        {
            //Set
            this.app = app;
            this.release = release;

            //Open config files
            plugins = new DataFile<List<EaglePluginInfo>>(app.EagleDirectory + Path.DirectorySeparatorChar + "plugins.json", new List<EaglePluginInfo>());
        }

        private EagleApplication app;
        private Action release;

        private DataFile<List<EaglePluginInfo>> plugins;

        public EaglePluginInfo InstallPlugin(FileInfo filename)
        {
            return InstallPlugin(filename.FullName);
        }

        public EaglePluginInfo InstallPlugin(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                return InstallPlugin(fs);
        }

        public EaglePluginInfo InstallPlugin(Stream file)
        {
            //Open the plugin
            EaglePackageReader reader;
            try
            {
                reader = new EaglePackageReader(file);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to open package: {ex.Message}");
            }

            //Check to make sure this platform supports all items in this
            string platform = PluginUtil.IdentifyPlatform();
            foreach (var o in reader.Manifest.objects_native)
            {
                if (!o.platforms.Contains(platform))
                    throw new Exception($"Sorry, this package does not appear to support the current platform ({platform}). Installation was aborted.");
            }

            //Remove the plugin if it already exists
            TryRemovePlugin(reader.Id);

            //Copy all libraries
            reader.CopyLibs(platform, app.ExecutableDirectory);

            //Copy all web assets
            var assets = reader.CopyWebAssets(new DirectoryInfo(app.EagleDirectory).CreateSubdirectory("plugins").CreateSubdirectory("assets"));

            //Add it's data to the db
            EaglePluginInfo info = new EaglePluginInfo
            {
                plugin_name = reader.Manifest.plugin_name,
                developer_name = reader.Manifest.developer_name,
                version_major = reader.Manifest.version_major,
                version_minor = reader.Manifest.version_minor,
                version_build = reader.Manifest.version_build,
                built_at = reader.Manifest.build_at,
                installed_at = DateTime.UtcNow,
                modules = reader.Manifest.modules,
                assets = assets
            };
            plugins.Data.Add(info);

            //Log
            Console.WriteLine($"### Installed plugin \"{info.plugin_name}\" by \"{info.developer_name}\" at v{info.version_major}.{info.version_minor}.{info.version_build}.");

            return info;
        }

        public bool TryRemovePlugin(string id)
        {
            //Attempt to locate
            if (!TryFindPlugin(id, out EaglePluginInfo info))
                return false;

            //Remove from list
            plugins.Data.Remove(info);

            return true;
        }

        private bool TryFindPlugin(string id, out EaglePluginInfo info)
        {
            foreach (var p in plugins.Data)
            {
                if (p.developer_name + "." + p.plugin_name == id)
                {
                    info = p;
                    return true;
                }
            }
            info = null;
            return false;
        }

        public void Dispose()
        {
            //Save files
            plugins.Save();

            //Release lock
            release();
        }
    }
}
