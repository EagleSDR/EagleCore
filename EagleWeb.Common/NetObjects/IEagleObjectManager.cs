using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.NetObjects
{
    public interface IEagleObjectManager
    {
        /// <summary>
        /// Begins the task of creating an object.
        /// </summary>
        /// <returns></returns>
        IEagleObjectInternalContext CreateObject(EagleObject ctx, JObject constructorInfo);
    }
}
