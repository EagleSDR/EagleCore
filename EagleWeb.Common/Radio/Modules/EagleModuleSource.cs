using EagleWeb.Common.NetObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Radio.Modules
{
    public abstract class EagleModuleSource : EagleObject
    {
        protected EagleModuleSource(IEagleObjectManagerLink link) : base(link)
        {
        }

        /// <summary>
        /// Returns if this is ready to be read yet or not.
        /// </summary>
        public abstract bool IsReady { get; }

        /// <summary>
        /// Returns the current output sample rate, assuming this is ready.
        /// </summary>
        public abstract float SampleRate { get; }

        /// <summary>
        /// The center frequency of the source.
        /// </summary>
        public abstract long CenterFrequency { get; set; }

        /// <summary>
        /// Reads a block of samples from the source and returns the amount read.
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="count"></param>
        public abstract unsafe int Read(EagleComplex* ptr, int count);

        /// <summary>
        /// Closes this source.
        /// </summary>
        public abstract void Close();
    }
}
