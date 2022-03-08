using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Core.Radio
{
    public interface IEagleNativeRadioFactory
    {
        IEagleNativeRadio CreateNativeRadio(int bufferSize);
    }
}
