using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Core.Radio.Components.FilterBuilder
{
    internal class EagleFilterBuilderLowpass : EagleFilterBuilderBase
    {
        public EagleFilterBuilderLowpass(float sampleRate, float cutoffFreq, float transitionWidth, float attenuation = 60) : base(glue_builder_create_lowpass(sampleRate, cutoffFreq), transitionWidth, attenuation)
        {
        }

        [DllImport(EagleComponentNative.DLL_NAME)]
        private static extern IntPtr glue_builder_create_lowpass(float sampleRate, float cutoffFreq);
    }
}
