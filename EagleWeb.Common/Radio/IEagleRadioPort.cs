using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Radio
{
    /// <summary>
    /// Outputs samples to plugins, allowing them to subscribe to it.
    /// </summary>
    public interface IEagleRadioPort<T> where T : unmanaged
    {
        string Name { get; }
        float SampleRate { get; }
        event IEagleRadioPort_SampleRateChanged<T> OnSampleRateChanged;
        event IEagleRadioPort_Output<T> OnOutput;
    }

    public delegate void IEagleRadioPort_SampleRateChanged<T>(IEagleRadioPort<T> port, float sampleRate) where T : unmanaged;
    public unsafe delegate void IEagleRadioPort_Output<T>(IEagleRadioPort<T> port, T* buffer, int count) where T : unmanaged;
}
