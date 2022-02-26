using EagleWeb.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Misc.Module
{
    public interface IEagleModuleInstance<THost, TApplicationBase> where TApplicationBase : IEagleDestructable
    {
        IEnumerable<IEagleModuleInstanceApp<TApplicationBase>> Modules { get; }
        void Destroy();
    }
}
