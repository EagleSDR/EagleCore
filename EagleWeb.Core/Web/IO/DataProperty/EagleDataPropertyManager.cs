using EagleWeb.Common.IO;
using EagleWeb.Common.IO.DataProperty;
using EagleWeb.Core.Web.IO.DataProperty.Impl;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Web.IO.DataProperty
{
    public class EagleDataPropertyManager
    {
        public EagleDataPropertyManager(IEaglePortDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        private IEaglePortDispatcher dispatcher;
        private List<IEagleDataPropertyImpl> properties = new List<IEagleDataPropertyImpl>();

        public IEagleDataProperty<T> CreatePropertyReadOnly<T>(string id, T defaultValue)
        {
            EagleDataProperty<T> prop = new EagleDataProperty<T>(id, dispatcher.CreatePortDispatcher(id), defaultValue);
            properties.Add(prop);
            return prop;
        }

        public IEagleDataPropertyWritable<T> CreatePropertyWritable<T>(string id, T defaultValue)
        {
            EagleDataPropertyWritable<T> prop = new EagleDataPropertyWritable<T>(id, dispatcher.CreatePortDispatcher(id), defaultValue);
            properties.Add(prop);
            return prop;
        }

        public JArray CreatePropertyDefinitions()
        {
            JArray arr = new JArray();
            foreach (var p in properties)
            {
                JObject o = new JObject();
                o["opcode"] = p.Id;
                o["type"] = p.Type.FullName;
                o["write_allowed"] = p.WebWritable;
                o["write_required_permissions"] = new JArray();
                foreach (var perm in p.WebRequiredPermissions)
                    (o["write_required_permissions"] as JArray).Add(perm);
                arr.Add(o);
            }
            return arr;
        }
    }
}
