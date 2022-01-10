using EagleWeb.Common.IO.DataProperty;
using EagleWeb.Common.NetObjects.IO;
using EagleWeb.Common.Radio.Modules;
using EagleWeb.Common.Radio.RDS;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Radio
{
    public interface IEagleRadio
    {
        IEaglePortProperty<bool> Enabled { get; }
        IEaglePortProperty<EagleModuleSource> Source { get; }
    }
}
