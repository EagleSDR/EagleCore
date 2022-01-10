using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EagleWeb.Common.IO.Streams
{
    /// <summary>
    /// Implemented by the client. This describes how a stream source should behave.
    /// </summary>
    public interface IEagleStreamServer
    {
        /// <summary>
        /// Called when a new stream is created.
        /// </summary>
        /// <param name="stream">The actual IO stream.</param>
        void HandleClientConnect(IEagleStreamClient stream);

        /// <summary>
        /// Called when a stream is destroyed.
        /// </summary>
        /// <param name="stream">The actual IO stream.</param>
        void HandleClientDisconnect(IEagleStreamClient stream);
    }
}
