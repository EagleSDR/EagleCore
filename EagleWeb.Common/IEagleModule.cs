using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common
{
    /// <summary>
    /// External DLLs will extend this.
    /// </summary>
    public interface IEagleModule
    {
        void Init(IEagleComponent context);
    }
}
