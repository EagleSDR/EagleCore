using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO.FileSystem
{
    public class WebFsAccessDeniedException : Exception
    {
        public WebFsAccessDeniedException() : base("Your account doesn't have access to perform this operation, or the file wasn't opened with the correct parameters.")
        {

        }
    }
}
