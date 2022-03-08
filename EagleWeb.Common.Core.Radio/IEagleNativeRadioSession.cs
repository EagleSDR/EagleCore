using EagleWeb.Common.Plugin.Interfaces.RadioSession;
using EagleWeb.Common.Radio;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Core.Radio
{
    public interface IEagleNativeRadioSession : IDisposable
    {
        IEagleRadioPort<EagleComplex> PortInput { get; }
        IEagleRadioPort<EagleComplex> PortVFO { get; }
        IEagleRadioPort<EagleComplex> PortIF { get; }
        IEagleRadioPort<EagleStereoPair> PortAudio { get; }

        void SetDemodulator(IEagleRadioDemodulator newDemodulator);
        void SetBandwidth(float bandwidth);
        void SetFrequencyOffset(float frequencyOffset);
        IEagleRadioAudioOutput GetResampledAudioOutput(float outputSampleRate);
    }
}
