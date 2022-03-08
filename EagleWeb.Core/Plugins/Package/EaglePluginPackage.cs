using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace EagleWeb.Core.Plugins.Package
{
    class EaglePluginPackage : IDisposable
    {
        public EaglePluginPackage(string filename)
        {
            //Set
            this.filename = filename;

            //Open file and archive
            file = new FileStream(filename, FileMode.Open, FileAccess.Read);
            archive = new ZipArchive(file, ZipArchiveMode.Read, true);

            //Read manifest
            using (Stream manifestStream = archive.GetEntry("eagle-manifest.json").Open())
            using (StreamReader manifestReader = new StreamReader(manifestStream))
                manifest = JsonConvert.DeserializeObject<PluginManifest>(manifestReader.ReadToEnd());

            //Wrap all assets
            List<ZipArchiveEntry> assetFiles = archive.FindEntries("assets/");
            assets = new PluginAsset[assetFiles.Count];
            for (int i = 0; i < assets.Length; i++)
                assets[i] = new PluginAsset(assetFiles[i]);
        }

        private readonly string filename;
        private readonly FileStream file;
        private readonly ZipArchive archive;
        private readonly PluginManifest manifest;
        private readonly PluginAsset[] assets;

        public string PluginName => manifest.plugin_name;
        public string DeveloperName => manifest.developer_name;
        public Version PluginVersion => new Version(manifest.version_major, manifest.version_minor, manifest.version_build);
        public Version SdkVersion => new Version(manifest.sdk_version_major, manifest.sdk_version_minor);
        public IEaglePluginPackageModule[] Modules => manifest.modules;
        public IEaglePluginPackageAsset[] Assets => assets;
        public IEaglePluginPackageDependency[] Dependencies => manifest.dependencies;

        public EaglePluginPackageNatives GetNatives(string platform)
        {
            return new EaglePluginPackageNatives(archive, $"native/{platform}/bin/");
        }

        public EaglePluginPackageNatives GetCurrentNatives()
        {
            return GetNatives(PluginUtil.IdentifyPlatform());
        }

        public bool TryGetManagedFile(string filename, out Stream file)
        {
            //Create path
            string path = "managed/" + filename;

            //Check if it exists
            ZipArchiveEntry entry = archive.GetEntry(path);
            if (entry == null)
            {
                file = null;
                return false;
            }

            //Open
            file = entry.OpenFixed();
            return true;
        }

        public void Dispose()
        {
            archive.Dispose();
            file.Dispose();
        }

        /* MISC */

        class PluginAsset : IEaglePluginPackageAsset
        {
            public PluginAsset(ZipArchiveEntry entry)
            {
                this.entry = entry;
            }

            private readonly ZipArchiveEntry entry;
            private string cachedHash;

            public string FileName => entry.Name;

            public string Hash
            {
                get
                {
                    //Create the cached hash if one is not currently available
                    if (cachedHash == null)
                    {
                        using (SHA256 sha = SHA256.Create())
                        using (Stream src = Open())
                            cachedHash = BitConverter.ToString(sha.ComputeHash(src)).Replace("-", "");
                    }

                    return cachedHash;
                }
            }

            public Stream Open()
            {
                return entry.OpenFixed();
            }
        }

        /* MANIFEST */

        class PluginManifest
        {
            public string developer_name;
            public string plugin_name;
            public int version_major;
            public int version_minor;
            public int version_build;
            public int sdk_version_major;
            public int sdk_version_minor;
            public DateTime build_at;
            public string build_platform;
            public PluginManifestNativeObject[] objects_native;
            public PluginManifestModules[] modules;
            public PluginManifestDepencency[] dependencies = new PluginManifestDepencency[0];

            public class PluginManifestDepencency : IEaglePluginPackageDependency
            {
                public string plugin_name;
                public string developer_name;
                public int min_version_major;
                public int min_version_minor;

                [JsonIgnore]
                public string PluginName => plugin_name;

                [JsonIgnore]
                public string DeveloperName => developer_name;

                [JsonIgnore]
                public Version MinVersion => new Version(min_version_major, min_version_minor);
            }

            public class PluginManifestNativeObject
            {
                public string name;
                public string[] platforms;
            }

            public class PluginManifestModules : IEaglePluginPackageModule
            {
                public string dll;
                public string classname;

                [JsonIgnore]
                public string DllName => dll;

                [JsonIgnore]
                public string ClassName => classname;
            }
        }
    }
}
