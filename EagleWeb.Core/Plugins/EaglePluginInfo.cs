using EagleWeb.Package.Manifest;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Plugins
{
    public class EaglePluginInfo
    {
        public string developer_name;
        public string plugin_name;

        public int version_major;
        public int version_minor;
        public int version_build;

        public DateTime built_at;
        public DateTime installed_at;

        public EagleManifestModule[] modules = new EagleManifestModule[0];
        public Dictionary<string, string> assets = new Dictionary<string, string>();
    }
}
