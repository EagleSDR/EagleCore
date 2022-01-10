using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO.FileSystem
{
    /// <summary>
    /// Thrown when the SYSTEM ITSELF isn't allowing us to access a file. This indicates a bug.
    /// </summary>
    public class WebFsSystemAccessDeniedException : Exception
    {
        public WebFsSystemAccessDeniedException() : base("The operating system denied access to this file. This is a problem with your EagleSDR installation!")
        {

        }
    }
}
