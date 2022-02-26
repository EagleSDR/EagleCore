using EagleWeb.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Misc.Module
{
    public static class EagleModuleHelpers
    {
        /// <summary>
        /// Creates a "lookup table" that can be used to turn IDs into GUIDs of EagleObjects
        /// </summary>
        /// <typeparam name="THost"></typeparam>
        /// <typeparam name="TApplicationBase"></typeparam>
        /// <param name="instance"></param>
        public static JObject CreateEagleObjectMap<THost, TApplicationBase>(this IEagleModuleInstance<THost, TApplicationBase> instance) where TApplicationBase : IEagleDestructable
        {
            JObject result = new JObject();
            foreach (var app in instance.Modules)
            {
                if (app.Module is IEagleObject obj) //Get the module as an EagleObject if it is one
                {
                    if (result.ContainsKey(app.Id))
                        result[app.Id] = "CONFLICT"; //Multiple items! This shouldn't be possible, but if it is add a special case we can scan for
                    else
                        result.Add(app.Id, obj.Guid); //Add GUID
                }
            }
            return result;
        }
    }
}
