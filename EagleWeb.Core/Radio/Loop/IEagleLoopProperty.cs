using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Radio.Loop
{
    public interface IEagleLoopProperty<T>
    {
        public T Value { get; }
    }
}
