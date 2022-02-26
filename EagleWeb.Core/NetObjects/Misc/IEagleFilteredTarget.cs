using EagleWeb.Common.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.NetObjects.Misc
{
    interface IEagleFilteredTarget : IEagleNetObjectTarget
    {
        void AddAccountFilter(IEagleAccount account);
        IEagleFilteredTarget Clone();
    }
}
