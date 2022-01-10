using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.NetObjects
{
    public interface IEagleObjectManagerLink
    {
        IEagleObjectManager ObjectManager { get; }
    }
}
