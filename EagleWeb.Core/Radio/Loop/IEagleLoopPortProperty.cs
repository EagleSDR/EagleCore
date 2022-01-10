using EagleWeb.Common.NetObjects.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Radio.Loop
{
    public interface IEagleLoopPortProperty<T> : IEagleLoopProperty<T>
    {
        public IEaglePortProperty<T> Port { get; }
    }
}
