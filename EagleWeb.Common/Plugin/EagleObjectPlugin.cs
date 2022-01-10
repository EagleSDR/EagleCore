using EagleWeb.Common.NetObjects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Plugin
{
    public abstract class EagleObjectPlugin : EagleObject
    {
        protected EagleObjectPlugin(IEagleObjectPluginContext context, JObject info = null) : base(context, info)
        {
            this.context = context;
        }

        private IEagleObjectPluginContext context;

        protected IEagleContext Context => context.Context;

        public abstract void PluginInit();
    }
}
