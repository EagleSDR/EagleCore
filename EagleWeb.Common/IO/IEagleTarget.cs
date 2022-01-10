using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO
{
    /// <summary>
    /// Defines a target for an outgoing message.
    /// </summary>
    public interface IEagleTarget
    {
        /// <summary>
        /// Sends raw data to this target.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="count"></param>
        void Deliver(byte[] payload, int count);
    }
}
