using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.NetObjects
{
    public interface IEagleObjectPort
    {
        string Guid { get; }
        string Name { get; }
    }
}
