using EagleWeb.Core.Plugins.Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace EagleWeb.Core.Plugins.Loader
{
    class EaglePluginPackageLoader : AssemblyLoadContext
    {
        public EaglePluginPackageLoader(string name, EagleUnmanagedCache cache) : base(name)
        {
            this.cache = cache;
        }

        private readonly EagleUnmanagedCache cache;
        private readonly List<EaglePluginPackage> packages = new List<EaglePluginPackage>();

        public void AddPackage(EaglePluginPackage package)
        {
            //Add to package list
            packages.Add(package);

            //Unpack to cache
            package.GetCurrentNatives().Unpack(cache.CopyTo);
        }

        public bool TryLoadFromPackageName(string name, out Assembly asm)
        {
            foreach (var p in packages)
            {
                if (p.TryGetManagedFile(name, out Stream file))
                {
                    try
                    {
                        asm = LoadFromStream(file);
                    }
                    finally
                    {
                        file.Close();
                    }
                    return true;
                }
            }
            asm = null;
            return false;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            //Try to locate the assembly within the packages
            if (TryLoadFromPackageName(assemblyName.Name, out Assembly asm))
                return asm;

            //Attempt to load normally
            return base.Load(assemblyName);
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            //Determine if this is a relative or absolute path
            bool absolute = unmanagedDllName.Contains('/') || unmanagedDllName.Contains('\\');

            //If this is a relative filename, attempt to locate it locally in the cache first
            if (!absolute && cache.TryLocateFile(unmanagedDllName, out string locatedFile))
                unmanagedDllName = locatedFile;

            //Load as usual
            return base.LoadUnmanagedDll(unmanagedDllName);
        }
    }
}
