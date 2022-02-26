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
    public class EagleObject : IEagleObject
    {
        public EagleObject(IEagleObjectContext context)
        {
            this.context = context;
            context.OnDestroyed += Destroy;
        }

        private readonly IEagleObjectContext context;

        public string Guid => context.Guid;
        public IEagleContext SystemContext => context.Context;

        public T CreateChildObject<T>(Func<IEagleObjectContext, T> creator) where T : IEagleObject
        {
            return context.CreateChildObject(creator);
        }

        public void Log(EagleLogLevel level, string topic, string message)
        {
            context.Log(level, topic, message);
        }

        protected void Log(EagleLogLevel level, string message)
        {
            Log(level, null, message);
        }

        public virtual void Destroy()
        {
            context.Destroy();
        }
    }
}
