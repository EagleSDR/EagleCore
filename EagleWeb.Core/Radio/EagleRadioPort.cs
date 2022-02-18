using EagleWeb.Common.Radio;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Radio
{
    class EagleRadioPort<T> : IEagleRadioPort<T> where T : unmanaged
    {
        public EagleRadioPort(string name, float sampleRate = 0)
        {
            this.name = name;
            this.sampleRate = sampleRate;
        }

        private string name;
        private float sampleRate;

        public string Name => name;
        public float SampleRate
        {
            get => sampleRate;
            set
            {
                //Set
                sampleRate = value;

                //Fire event
                OnSampleRateChanged?.Invoke(this, value);
            }
        }

        public event IEagleRadioPort_SampleRateChanged<T> OnSampleRateChanged;
        public event IEagleRadioPort_Output<T> OnOutput;

        public unsafe void Output(T* buffer, int count)
        {
            OnOutput?.Invoke(this, buffer, count);
        }
    }
}
