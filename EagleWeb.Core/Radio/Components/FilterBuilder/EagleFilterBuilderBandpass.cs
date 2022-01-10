using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Core.Radio.Components.FilterBuilder
{
    internal class EagleFilterBuilderBandpass : EagleFilterBuilderBase
    {
        public EagleFilterBuilderBandpass(float sampleRate, float cutoffLow, float cutoffHigh, float transitionWidth, float attenuation = 60) : base(glue_builder_create_bandpass(sampleRate, cutoffLow, cutoffHigh), transitionWidth, attenuation)
        {
        }

        [DllImport(EagleComponentNative.DLL_NAME)]
        private static extern IntPtr glue_builder_create_bandpass(float sampleRate, float cutoffLow, float cutoffHigh);
    }
}
