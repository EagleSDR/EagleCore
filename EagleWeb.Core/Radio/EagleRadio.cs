using EagleWeb.Common;
using EagleWeb.Common.IO;
using EagleWeb.Common.Radio;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EagleWeb.Common.Radio.RDS;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.NetObjects.IO;
using EagleWeb.Core.Misc;
using RaptorDspNet;
using EagleWeb.Common.Auth;
using System.Collections.Concurrent;
using EagleWeb.Core.Misc.Module;
using EagleWeb.Core.Radio.Native;
using EagleWeb.Common.Plugin.Interfaces.Radio;

namespace EagleWeb.Core.Radio
{
    class EagleRadio : EagleObject, IEagleRadio
    {
        public EagleRadio(IEagleObjectContext context, EagleContext ctx) : base(context)
        {
            //Set
            this.context = ctx;

            //Create modules and add them as an extra when we finish initializing
            //Adding the extra after it leaves the constructor is *technically* unsupported but it should be fine since nobody will have requested the object yet
            ctx.OnPluginsLoaded += () =>
            {
                modules = ctx.RadioModules.CreateInstance(this);
                (context as NetObjects.EagleNetObjectInstance).AddExtraPost("modules", modules.CreateEagleObjectMap());
            };            

            //Create error
            portOnError = context.CreateEventDispatcher("OnError");

            //Create ports
            portCreateSession = context.CreatePortApi("CreateSession")
                .Bind(ApiCreateSession);

            //Create props
            portEnabled = context.CreatePropertyBool("IsEnabled")
                .RequirePermission(EaglePermissions.PERMISSION_POWER)
                .MakeWebEditable()
                .BindOnChanged(OnEnabledChanged);
            propSource = context.CreatePropertyObject<IEagleRadioSource>("Source")
                .MakeWebEditable()
                .RequirePermission(EaglePermissions.PERMISSION_CHANGE_SOURCE)
                .BindOnChanged(OnSourceChanged);
            propCenterFreq = context.CreatePropertyLong("CenterFrequency")
                .MakeWebEditable()
                .RequirePermission(EaglePermissions.PERMISSION_TUNE)
                .BindOnChanged(OnCenterFreqChanged);

            //Create radio
            radio = new EagleNativeRadio(BUFFER_SIZE);
            radio.StartWorker();
            radio.OnError += Radio_OnError;
        }

        public const int BUFFER_SIZE = 65536;

        private EagleContext context;
        private EagleNativeRadio radio;

        private IEagleModuleInstance<EagleRadio, IEagleRadioModule> modules;

        private IEaglePortEventDispatcher portOnError;
        private IEaglePortProperty<bool> portEnabled;
        private IEaglePortApi portCreateSession;
        private IEaglePortProperty<IEagleRadioSource> propSource;
        private IEaglePortProperty<long> propCenterFreq;

        private IEagleRadioSource source;

        public event IEagleRadio_SessionEventArgs OnSessionCreated;
        public event IEagleRadio_SessionEventArgs OnSessionRemoved;

        public EagleContext Context => context;
        public EagleNativeRadio Radio => radio;

        private void OnEnabledChanged(IEaglePortPropertySetArgs<bool> args)
        {
            if (args.Value)
                radio.Unsuspend();
            else
                radio.Suspend();
        }

        private void OnSourceChanged(IEaglePortPropertySetArgs<IEagleRadioSource> args)
        {
            //Set on the radio
            radio.SetSource(args.Value);

            //Set
            source = args.Value;

            //Attempt to update the center frequency of this to that of the current center frequency
            if (args.Value != null)
            {
                try
                {
                    //Set
                    args.Value.CenterFrequency = propCenterFreq.Value;
                }
                catch
                {
                    //Failed. Instead, set the center frequency port's value to this.
                    propCenterFreq.Value = args.Value.CenterFrequency;
                }
            }
        }

        private void OnCenterFreqChanged(IEaglePortPropertySetArgs<long> args)
        {
            //Read the source
            IEagleRadioSource source = this.source;

            //Set
            if (source != null)
                source.CenterFrequency = args.Value;
        }

        private JObject ApiCreateSession(IEagleAccount client, JObject message)
        {
            //Create the session
            EagleRadioSession session = CreateChildObject((IEagleObjectContext context) =>
            {
                return new EagleRadioSession(context, this, radio.CreateSession());
            });

            //Send event
            OnSessionCreated?.Invoke(this, session);

            //Create response
            JObject msg = new JObject();
            msg["guid"] = session.Guid;
            return msg;
        }

        private void Radio_OnError(EagleNativeRadio radio, string message)
        {
            //Update "running"
            portEnabled.Value = false;

            //Create the message to dispatch
            JObject msg = new JObject();
            msg["message"] = message;

            //Send
            portOnError.Push(msg);
        }
    }
}
