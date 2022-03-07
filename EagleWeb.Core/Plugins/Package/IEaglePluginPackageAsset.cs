using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EagleWeb.Core.Plugins.Package
{
    interface IEaglePluginPackageAsset
    {
        string FileName { get; }
        string Hash { get; }
        Stream Open();
    }
}
