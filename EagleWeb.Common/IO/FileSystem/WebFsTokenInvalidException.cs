using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO.FileSystem
{
    public class WebFsTokenInvalidException : Exception
    {
        public WebFsTokenInvalidException() : base("This file token is invalid. This is a bug in the plugin. Please try again.")
        {

        }
    }
}
