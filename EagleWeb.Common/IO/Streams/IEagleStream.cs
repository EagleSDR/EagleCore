using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EagleWeb.Common.IO.Streams
{
    /// <summary>
    /// Defines a created stream source (not the actual stream itself). Streams allow web clients to send arbitrary binary data.
    /// </summary>
    public interface IEagleStream
    {
        string Id { get; }
    }
}
