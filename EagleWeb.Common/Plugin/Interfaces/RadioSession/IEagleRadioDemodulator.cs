using EagleWeb.Common.Radio;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Plugin.Interfaces.RadioSession
{
    public interface IEagleRadioDemodulator : IEagleObject, IEagleRadioSessionModule
    {
        string DisplayName { get; }
        string DisplayNameShort { get; }

        /// <summary>
        /// Configures this demodulator and returns the output sample rate.
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        float Configure(float sampleRate, int bufferSize);

        /// <summary>
        /// Processes the incoming IQ into audio.
        /// </summary>
        /// <param name="input">Incoming IQ.</param>
        /// <param name="count">Incoming IQ count.</param>
        /// <param name="audioL">Output L buffer.</param>
        /// <param name="audioR">Output R buffer.</param>
        /// <returns>Count of output per channel.</returns>
        unsafe int Process(EagleComplex* input, int count, float* audioL, float* audioR);
    }
}
