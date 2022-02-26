using EagleWeb.Common.Radio;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Plugin.Interfaces.Radio
{
    public interface IEagleRadioSource : IEagleObject, IEagleRadioModule
    {
        /// <summary>
        /// Opens the source, returning the output sample rate.
        /// </summary>
        /// <returns></returns>
        float Open(int bufferSize);

        /// <summary>
        /// Gets the current center frequency.
        /// </summary>
        long CenterFrequency { get; set; }

        /// <summary>
        /// Reads a block of samples from the source and returns the amount read.
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="count"></param>
        unsafe int Read(EagleComplex* ptr, int count);

        /// <summary>
        /// Closes this source.
        /// </summary>
        void Close();
    }
}
