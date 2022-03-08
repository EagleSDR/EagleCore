using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Radio
{
    public interface IEagleRadioAudioOutput : IEagleRadioPort<EagleStereoPair>, IDisposable
    {
    }
}
