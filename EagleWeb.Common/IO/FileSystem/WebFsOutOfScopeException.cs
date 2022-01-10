using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO.FileSystem
{
    public class WebFsOutOfScopeException : Exception
    {
        public WebFsOutOfScopeException() : base("Access to out-of-scope file denied. This incident will be reported.")
        {

        }
    }
}
