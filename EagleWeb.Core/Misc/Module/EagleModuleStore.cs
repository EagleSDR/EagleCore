using EagleWeb.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Misc.Module
{
    /// <summary>
    /// Holds applications which create an instance whenever we request a new instance.
    /// </summary>
    public class EagleModuleStore<THost, TApplicationBase> where TApplicationBase : IEagleDestructable
    {
        private List<IEagleModuleApplication<THost, TApplicationBase>> applications = new List<IEagleModuleApplication<THost, TApplicationBase>>();

        public void RegisterApplication(string id, Func<THost, TApplicationBase> constructor)
        {
            RegisterApplication(new BasicApplication(id, constructor));
        }

        public void RegisterApplication(IEagleModuleApplication<THost, TApplicationBase> application)
        {
            applications.Add(application);
        }

        public IEagleModuleInstance<THost, TApplicationBase> CreateInstance(THost context)
        {
            return new EagleModuleInstance(this, context);
        }

        class BasicApplication : IEagleModuleApplication<THost, TApplicationBase>
        {
            public BasicApplication(string id, Func<THost, TApplicationBase> constructor)
            {
                this.id = id;
                this.constructor = constructor;
            }

            private readonly string id;
            private readonly Func<THost, TApplicationBase> constructor;

            public string Id => id;

            public TApplicationBase SpawnModule(THost context)
            {
                return constructor(context);
            }
        }

        /// <summary>
        /// The instance with each application in the store added
        /// </summary>
        class EagleModuleInstance : IEagleModuleInstance<THost, TApplicationBase>
        {
            public EagleModuleInstance(EagleModuleStore<THost, TApplicationBase> store, THost context)
            {
                //Set
                this.store = store;
                this.context = context;

                //Construct each application
                modules = new List<IEagleModuleInstanceApp<TApplicationBase>>();
                foreach (var app in store.applications)
                {
                    //Create
                    TApplicationBase mod;
                    try
                    {
                        mod = app.SpawnModule(context);
                    } catch
                    {
                        mod = default(TApplicationBase);
                    }

                    //Add
                    modules.Add(new AppInstance(app, mod));
                }
            }

            private readonly EagleModuleStore<THost, TApplicationBase> store;
            private readonly THost context;
            private readonly List<IEagleModuleInstanceApp<TApplicationBase>> modules;

            public IEnumerable<IEagleModuleInstanceApp<TApplicationBase>> Modules => modules;

            public void Destroy()
            {
                //Destroy all modules
                foreach (var m in modules)
                    m.Module.Destroy();
                modules.Clear();
            }

            class AppInstance : IEagleModuleInstanceApp<TApplicationBase>
            {
                public AppInstance(IEagleModuleApplication<THost, TApplicationBase> app, TApplicationBase module)
                {
                    this.app = app;
                    this.module = module;
                }

                private readonly IEagleModuleApplication<THost, TApplicationBase> app;
                private readonly TApplicationBase module;

                public string Id => app.Id;
                public TApplicationBase Module => module;
            }
        }
    }
}
