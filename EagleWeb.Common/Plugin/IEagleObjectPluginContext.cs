using EagleWeb.Common.NetObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Plugin
{
    public interface IEagleObjectPluginContext : IEagleObjectManagerLink
    {
        IEagleContext Context { get; }
    }
}
