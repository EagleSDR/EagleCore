using EagleWeb.Common.NetObjects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Radio.Modules
{
    /// <summary>
    /// Represents a demodulator instance.
    /// </summary>
    public abstract class EagleModuleDemodulator : EagleObject
    {
        protected EagleModuleDemodulator(IEagleObjectManagerLink link, JObject info = null) : base(link, info)
        {
        }

        /// <summary>
        /// Configures this demodulator and returns the output sample rate.
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        public abstract float Configure(float sampleRate);

        /// <summary>
        /// Processes the incoming IQ into audio.
        /// </summary>
        /// <param name="input">Incoming IQ.</param>
        /// <param name="count">Incoming IQ count.</param>
        /// <param name="audioL">Output L buffer.</param>
        /// <param name="audioR">Output R buffer.</param>
        /// <returns>Count of output per channel.</returns>
        public abstract unsafe int Process(EagleComplex* input, int count, float* audioL, float* audioR);
    }
}
