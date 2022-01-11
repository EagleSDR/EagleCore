using EagleWeb.Common.NetObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common
{
    public interface IEagleObject
    {
        IEagleObjectManager ObjectManager { get; }
        string Guid { get; }
    }
}
