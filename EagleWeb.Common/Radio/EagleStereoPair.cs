using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Radio
{
    public struct EagleStereoPair
    {
        public float left;
        public float right;

        public EagleStereoPair(float left, float right)
        {
            this.left = left;
            this.right = right;
        }

        public float Average => (left + right) / 2;
    }
}
