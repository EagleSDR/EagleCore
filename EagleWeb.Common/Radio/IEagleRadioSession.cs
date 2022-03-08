using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Radio
{
    public interface IEagleRadioSession : IEagleObject
    {
        /// <summary>
        /// Output for samples after they've been offset.
        /// </summary>
        IEagleRadioPort<EagleComplex> PortVFO { get; }

        /// <summary>
        /// Output for the filtered+decimated demodulator input.
        /// </summary>
        IEagleRadioPort<EagleComplex> PortIF { get; }

        /// <summary>
        /// Output for the raw input IQ from the source.
        /// </summary>
        IEagleRadioPort<EagleStereoPair> PortAudio { get; }

        /// <summary>
        /// Creates a resampled audio output to the desired sample rate.
        /// </summary>
        /// <param name="outputSampleRate"></param>
        /// <returns></returns>
        IEagleRadioAudioOutput CreateResampledOutput(float outputSampleRate);
    }
}
