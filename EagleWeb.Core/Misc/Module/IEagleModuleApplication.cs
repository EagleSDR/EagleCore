using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Misc.Module
{
    public interface IEagleModuleApplication<THost, TApplicationBase>
    {
        string Id { get; }
        TApplicationBase SpawnModule(THost context);
    }
}
