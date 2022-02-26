using EagleWeb.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Misc.Module
{
    public interface IEagleModuleInstanceApp<TApplicationBase> where TApplicationBase : IEagleDestructable
    {
        string Id { get; }
        TApplicationBase Module { get; }
    }
}
