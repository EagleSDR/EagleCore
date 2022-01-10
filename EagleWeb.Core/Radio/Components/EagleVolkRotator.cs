using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Common.Radio.Components
{
    public class EagleVolkRotator
    {
        public EagleVolkRotator()
        {
            phase = new EagleComplex(1, 0);
            inc = new EagleComplex(0, 0);
            enabled = false;
        }

        private EagleComplex phase;
        private EagleComplex inc;
        private bool enabled;

        public void Configure(double sampleRate, double freqOffset)
        {
            if (sampleRate == 0 || freqOffset == 0)
            {
                enabled = false;
            }
            else
            {
                double angle = 2.0 * Math.PI * freqOffset / sampleRate;
                inc = new EagleComplex((float)Math.Cos(angle), (float)Math.Sin(angle));
                enabled = true;
            }
        }

        public unsafe void Process(EagleComplex* input, EagleComplex* output, int count)
        {
            if (enabled)
            {
                EagleComplex phase = this.phase;
                volk_32fc_s32fc_x2_rotator_32fc(output, input, inc, &phase, count);
                this.phase = phase;
            }
        }

        [DllImport("libvolk")]
        private static unsafe extern void volk_32fc_s32fc_x2_rotator_32fc(EagleComplex* output, EagleComplex* input, EagleComplex inc, EagleComplex* phase, int count);
    }
}
