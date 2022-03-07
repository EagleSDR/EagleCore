using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Plugins.Package
{
    interface IEaglePluginPackageModule
    {
        string DllName { get; }
        string ClassName { get; }
    }
}
