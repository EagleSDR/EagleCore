using EagleWeb.Common.NetObjects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common
{
    /// <summary>
    /// An object replicated both on the server and on clients
    /// </summary>
    public class EagleObject : IEagleObject, IEagleObjectManagerLink, IDisposable
    {
        public EagleObject(IEagleObjectManagerLink link, JObject info = null)
        {
            //Get the manager
            manager = link.ObjectManager;

            //Request a context
            ctx = manager.CreateObject(this, info);

            //Get the creation context, configure, and apply
            IEagleObjectConfigureContext configure = ctx.BeginCreate();
            ConfigureObject(configure);
            ctx.EndCreate(configure);
        }

        private readonly IEagleObjectManager manager;
        private readonly IEagleObjectInternalContext ctx;

        public IEagleObjectManager ObjectManager => manager;
        public string Guid => ctx.Guid;

        protected virtual void ConfigureObject(IEagleObjectConfigureContext context)
        {
            //Users will implement this themselves...
        }

        public void Log(EagleLogLevel level, string topic, string message)
        {
            ctx.Log(level, topic, message);
        }

        protected void Log(EagleLogLevel level, string message)
        {
            Log(level, null, message);
        }

        public virtual void Dispose()
        {
            ctx.Destroy();
        }
    }
}
