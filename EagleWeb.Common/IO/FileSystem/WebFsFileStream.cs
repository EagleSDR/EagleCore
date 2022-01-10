using EagleWeb.Common.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EagleWeb.Common.IO.FileSystem
{
    /// <summary>
    /// Represents a file opened by a web user
    /// </summary>
    public abstract class WebFsFileStream : Stream
    {
        public abstract string FileName { get; }
        public abstract string Token { get; }
        public abstract IEagleAccount Account { get; }
        public abstract FileStream Underlying { get; }
    }
}
