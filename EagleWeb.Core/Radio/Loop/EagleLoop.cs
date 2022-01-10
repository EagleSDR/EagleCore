using EagleWeb.Common;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.NetObjects.IO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EagleWeb.Core.Radio.Loop
{
    /// <summary>
    /// Represents a radio object that runs in a loop.
    /// </summary>
    public abstract class EagleLoop : EagleObject
    {
        protected EagleLoop(IEagleObjectManagerLink link, JObject info = null) : base(link, info)
        {
        }

        public IEaglePortProperty<bool> Enabled => portEnabled;

        private IEaglePortProperty<bool> portEnabled;
        private IEaglePortEventDispatcher portOnError;
        private bool stale = true;
        private List<IEagleLoopPropertyInternal> properties = new List<IEagleLoopPropertyInternal>();

        protected override void ConfigureObject(IEagleObjectConfigureContext context)
        {
            base.ConfigureObject(context);

            //Create an "enabled" port, as it'll be treated a bit specially
            portEnabled = context.CreatePropertyBool("IsEnabled")
                .MakeWebEditable();

            //Create a port for sending errors
            portOnError = context.CreateEventDispatcher("OnError");
        }

        /// <summary>
        /// Creates a property that'll be updated in a thread-safe manner each time the loop is run.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected IEagleLoopProperty<T> CreateLoopProperty<T>()
        {
            EagleLoopProperty<T> prop = new EagleLoopProperty<T>();
            properties.Add(prop);
            return prop;
        }

        /// <summary>
        /// Creates a property that'll be updated in a thread-safe manner each time the loop is run, based on a web port. Will be automatically updated from the web.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseProperty"></param>
        /// <returns></returns>
        protected IEagleLoopPortProperty<T> CreateLoopProperty<T>(IEaglePortProperty<T> baseProperty)
        {
            EagleLoopPortProperty<T> prop = new EagleLoopPortProperty<T>(baseProperty);
            properties.Add(prop);
            return prop;
        }

        /// <summary>
        /// Manually queues this for reconfiguration.
        /// </summary>
        protected void MakeStale()
        {
            stale = true;
        }

        /// <summary>
        /// Processes a cycle of the loop. If the loop is disabled, this will wait (on this thread) until it is reenabled
        /// </summary>
        public void ProcessWait(params object[] args)
        {
            while (!Process(args))
                Thread.Sleep(200);
        }

        /// <summary>
        /// Processes a cycle of the loop. Returns true if it succeeded, otherwise false if it is DISABLED.
        /// </summary>
        public bool Process(params object[] args)
        {
            //Check if we're disabled
            if (!portEnabled.Value)
                return false;

            //Check if any changes have been made
            stale = IsUpdated() || stale;

            //Enter exception catching...
            try
            {
                //If we're stale, apply settings
                if (stale)
                {
                    ConfigureInternal();
                    stale = false;
                }

                //Process main body
                ProcessInternal(args);
            } catch (Exception ex)
            {
                EnterErrorState(ex.Message);
            }

            return true;
        }

        /// <summary>
        /// Checks if there are any updates required that would make this item stale.
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsUpdated()
        {
            //Check properties
            bool stale = false;
            foreach (var p in properties)
                stale = p.Apply() || stale;

            return stale;
        }

        /// <summary>
        /// Reconfigures the object when it's stale. Throw an exception here to put the object into an error-state until it's made stale again.
        /// </summary>
        protected abstract void ConfigureInternal();

        /// <summary>
        /// Processes each loop after it's configured. Throw an exception here to put the object into an error-state until it's made stale again.
        /// </summary>
        protected abstract void ProcessInternal(params object[] args);

        /// <summary>
        /// Enters the error state, sending an event in the process.
        /// </summary>
        private void EnterErrorState(string message)
        {
            //Create the message to dispatch
            JObject msg = new JObject();
            msg["message"] = message;

            //Send
            portOnError.Push(msg);

            //Go into disabled state
            EnterDisabledState();
        }

        /// <summary>
        /// Disables the loop until the user manually reenables it.
        /// </summary>
        private void EnterDisabledState()
        {
            if (portEnabled.Value)
                portEnabled.Value = false;
        }

        interface IEagleLoopPropertyInternal
        {
            bool Apply();
        }

        class EagleLoopProperty<T> : IEagleLoopProperty<T>, IEagleLoopPropertyInternal
        {
            public EagleLoopProperty()
            {

            }

            private T value;
            private T pendingValue;
            private bool isStale;

            public T Value => value;

            /// <summary>
            /// Sets the value as stale and to be updated next time the loop runs.
            /// </summary>
            /// <param name="value"></param>
            public void ScheduleUpdate(T value)
            {
                pendingValue = value;
                isStale = true;
            }

            /// <summary>
            /// Applies this property if an update was scheduled. Returns if it was updated or not.
            /// </summary>
            /// <returns></returns>
            public bool Apply()
            {
                if (isStale)
                {
                    value = pendingValue;
                    isStale = false;
                    return true;
                } else
                {
                    return false;
                }
            }
        }

        class EagleLoopPortProperty<T> : EagleLoopProperty<T>, IEagleLoopPortProperty<T>
        {
            public EagleLoopPortProperty(IEaglePortProperty<T> port)
            {
                this.port = port;
                port.OnChanged += Property_OnChanged;
                ScheduleUpdate(port.Value);
            }

            private IEaglePortProperty<T> port;

            public IEaglePortProperty<T> Port => port;

            private void Property_OnChanged(IEaglePortPropertySetArgs<T> args)
            {
                ScheduleUpdate(args.Value);
            }
        }
    }    
}
