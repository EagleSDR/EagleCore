using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Core.Plugins
{
    public static class PluginUtil
    {
        public static string IdentifyPlatform()
        {
            //Determine os;
            string os;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                os = "win";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                os = "linux";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                os = "mac";
            else
                throw new Exception("Could not determine current OS!");

            //Determine arch
            string arch;
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X64: arch = "x64"; break;
                case Architecture.X86: arch = "x86"; break;
                case Architecture.Arm: arch = "arm"; break;
                case Architecture.Arm64: arch = "arm64"; break;
                default: throw new Exception("Could not determine current architecture!");
            }

            return os + "-" + arch;
        }
    }
}
