using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Package.Manifest
{
    public class EagleManifest
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

        public EagleManifestObjectNative[] objects_native;
        public EagleManifestModule[] modules;
    }
}
