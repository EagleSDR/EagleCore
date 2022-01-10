using EagleWeb.Core.Misc;
using EagleWeb.Core.Plugins;
using EagleWeb.Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EagleWeb.PluginInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            //Parse args
            ParseArgs(args);

            //Open database
            db = new DataFile<List<EaglePluginInfo>>(eagleDir + Path.DirectorySeparatorChar + "plugins.json", new List<EaglePluginInfo>());

            //Run CLI
            RunCLI();
        }

        static string eagleDir;
        static string libDir;

        static DataFile<List<EaglePluginInfo>> db;

        static void ParseArgs(string[] args)
        {
            //Loop through pairs
            for (int i = 0; i < args.Length; i += 2)
            {
                switch (args[i])
                {
                    case "-e": eagleDir = args[i + 1]; break;
                    case "-o": libDir = args[i + 1]; break;
                }
            }

            //If invoked with nothing, show help
            if (args.Length == 0)
            {
                Console.WriteLine("    -e : Eagle install directory.");
                Console.WriteLine("    -o : Library install directory.");
            }

            //Make sure required items are set
            if (eagleDir == null)
                throw new Exception("Eagle install directory must be set.");
            if (libDir == null)
                throw new Exception("Library install directory must be set.");
        }

        static void RunCLI()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(">");
                Console.ForegroundColor = ConsoleColor.White;
                string[] cmd = Console.ReadLine().Split(' ');
                switch (cmd[0])
                {
                    case "install":
                        if (TryGetArg(cmd, 1, out string installPath))
                            InstallPlugin(installPath);
                        else
                            Console.WriteLine("Invalid usage: install [path]");
                        break;
                    case "remove":
                        if (TryGetArg(cmd, 1, out string removeId))
                        {
                            if (TryFindPlugin(removeId, out EaglePluginInfo found))
                                RemovePlugin(found);
                            else
                                Console.WriteLine("Plugin with that ID isn't installed.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid usage: remove [id]");
                        }
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unknown command.");
                        break;
                }
            }
        }

        static bool TryGetArg(string[] cmd, int index, out string data)
        {
            if (index >= cmd.Length)
            {
                data = null;
                return false;
            }
            else
            {
                data = cmd[index];
                return true;
            }
        }

        static bool InstallPlugin(string filename)
        {
            //If this is a directory, check if it has a single file
            if ((File.GetAttributes(filename) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                //It is a directory. Query files...
                FileInfo[] files = new DirectoryInfo(filename).GetFiles();
                if (files.Length != 1)
                {
                    Console.WriteLine($"Specified a directory, but there were {(files.Length == 0 ? "no files" : "more than one files")} to pick from.");
                    return false;
                }

                //Update
                filename = files[0].FullName;
            }

            //Open
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                return InstallPlugin(fs);
        }

        static bool InstallPlugin(Stream file)
        {
            //Open the plugin
            EaglePackageReader reader;
            try
            {
                reader = new EaglePackageReader(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open package: {ex.Message}");
                return false;
            }

            //Log package info
            Console.WriteLine($"Installing package {reader.Id} at v{reader.Version}...");

            //Remove it if it already exists
            if (TryFindPlugin(reader.Id, out EaglePluginInfo info))
            {
                Console.WriteLine($"Package is already installed. Removing existing package...");
                RemovePlugin(info);
            }

            //Check to make sure this platform supports all items in this
            string platform = PluginUtil.IdentifyPlatform();
            bool supported = true;
            foreach (var o in reader.Manifest.objects_native)
                supported = supported && o.platforms.Contains(platform);
            if (!supported)
            {
                Console.WriteLine($"Sorry, this package does not appear to support the current platform ({platform}). Installation was aborted.");
                return false;
            }

            //Copy all libraries
            reader.CopyLibs(platform, new DirectoryInfo(libDir));

            //Copy all web assets
            var assets = reader.CopyWebAssets(new DirectoryInfo(eagleDir).CreateSubdirectory("plugins").CreateSubdirectory("assets"));

            //Add it's data to the db
            info = new EaglePluginInfo
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
            db.Data.Add(info);
            db.Save();

            //Log
            Console.WriteLine($"Package successfully installed!");
            return true;
        }

        static bool TryFindPlugin(string id, out EaglePluginInfo info)
        {
            foreach (var p in db.Data)
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

        static void RemovePlugin(EaglePluginInfo info)
        {
            //Remove
            db.Data.Remove(info);
            db.Save();
        }
    }
}
