using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Plugins.Package
{
    interface IEaglePluginPackageDependency
    {
        string PluginName { get; }
        string DeveloperName { get; }
        Version MinVersion { get; }
    }
}
