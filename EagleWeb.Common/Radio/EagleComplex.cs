using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Radio
{
    public struct EagleComplex
    {
        public float real;
        public float imag;

        public EagleComplex(float real, float imag)
        {
            this.real = real;
            this.imag = imag;
        }
    }
}
