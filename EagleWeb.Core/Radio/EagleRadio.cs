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
using EagleWeb.Common.Radio.Modules;
using EagleWeb.Core.Misc;
using EagleWeb.Core.Radio.Loop;
using RaptorDspNet;
using EagleWeb.Common.Auth;
using System.Collections.Concurrent;

namespace EagleWeb.Core.Radio
{
    class EagleRadio : EagleLoopThread, IEagleRadio
    {
        public EagleRadio(EagleContext ctx) : base(ctx)
        {
            //Create buffers
            bufferIq = new RaptorBuffer<raptor_complex>(BUFFER_SIZE);

            //Start worker
            StartWorkerThread("Main Radio Worker");
        }

        public const int BUFFER_SIZE = 65536;

        //BUFFERS
        private RaptorBuffer<raptor_complex> bufferIq;

        //MISC
        private List<EagleRadioSession> sessions = new List<EagleRadioSession>();
        private ConcurrentQueue<EagleRadioSession> uninitializedSessions = new ConcurrentQueue<EagleRadioSession>();

        //PIPES
        private EagleRadioPort<EagleComplex> pipeOutput = new EagleRadioPort<EagleComplex>("Output");

        //PORTS
        private IEagleLoopPortProperty<bool> portEnabled;
        private IEaglePortApi portCreateSession;
        private IEagleLoopPortProperty<EagleModuleSource> propSource;
        private IEaglePortProperty<long> propCenterFreq;

        //EVENTS
        public event IEagleRadio_SessionEventArgs OnSessionCreated;
        public event IEagleRadio_SessionEventArgs OnSessionRemoved;

        //GETTERS
        protected override bool Enabled => portEnabled.Value;
        public IEaglePortProperty<EagleModuleSource> Source => propSource.Port;
        public IEagleRadioPort<EagleComplex> PortInput => pipeOutput;

        protected override void ConfigureObject(IEagleObjectConfigureContext context)
        {
            base.ConfigureObject(context);

            //Create an "enabled" port, as it'll be treated a bit specially
            portEnabled = CreateLoopProperty(
                context.CreatePropertyBool("IsEnabled")
                .RequirePermission(EaglePermissions.PERMISSION_POWER)
                .MakeWebEditable()
            ); 

            //Create ports
            portCreateSession = context.CreatePortApi("CreateSession")
                .Bind(ApiCreateSession);

            //Create props
            propSource = CreateLoopProperty(
                context.CreatePropertyObject<EagleModuleSource>("Source")
                .MakeWebEditable()
                .RequirePermission(EaglePermissions.PERMISSION_CHANGE_SOURCE)
                .BindOnChanged(OnSourceChanged)
            );
            propCenterFreq = context.CreatePropertyLong("CenterFrequency")
                .MakeWebEditable()
                .RequirePermission(EaglePermissions.PERMISSION_TUNE)
                .BindOnChanged(OnCenterFreqChanged);
        }

        private void OnSourceChanged(IEaglePortPropertySetArgs<EagleModuleSource> args)
        {
            if (args.Value != null)
            {
                //Attempt to update the center frequency of this to that of the current center frequency
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
            //Attempt to update on the source, if there's one set
            propSource.Lock((EagleModuleSource source) =>
            {
                if (source != null)
                    source.CenterFrequency = args.Value;
            });
        }

        private JObject ApiCreateSession(IEagleAccount client, JObject message)
        {
            //Create the session
            EagleRadioSession session = new EagleRadioSession(this);

            //Queue session for initialization
            uninitializedSessions.Enqueue(session);

            //Send event
            OnSessionCreated?.Invoke(this, session);

            //Create response
            JObject msg = new JObject();
            msg["guid"] = session.Guid;
            return msg;
        }

        protected override void ConfigureInternal()
        {
            //Make sure the source is ready, then query it
            if (propSource.Value == null)
                throw new Exception("No source is set.");
            if (!propSource.Value.IsReady)
                throw new Exception("Source is not yet ready.");
            pipeOutput.SampleRate = propSource.Value.SampleRate;
            if (pipeOutput.SampleRate <= 0)
                throw new Exception("Source is producing an invalid sample rate: " + pipeOutput.SampleRate);

            //Set up sessions
            foreach (var s in sessions)
                s.Configure(pipeOutput.SampleRate);
        }

        protected override unsafe void ProcessInternal(params object[] args)
        {
            //Add/remove nessessary sessions
            AdmitSessions();
            PruneSessions();

            //Read from radio
            int count = propSource.Value.Read((EagleComplex*)bufferIq.Pointer, BUFFER_SIZE);

            //If we read no samples, assume this is the end of stream
            if (count == 0)
            {
                Log(EagleLogLevel.INFO, "Stopping radio: Source returned no samples, assuming end of stream...");
                portEnabled.Port.Value = false;
                return;
            }

            //Send out
            pipeOutput.Output((EagleComplex*)bufferIq.Pointer, count);

            //Dispatch
            foreach (var s in sessions)
                s.Process(bufferIq, count);
        }

        private void AdmitSessions()
        {
            //Dequeue sessions
            while (uninitializedSessions.TryDequeue(out EagleRadioSession session))
            {
                //Configure
                session.Configure(pipeOutput.SampleRate);

                //Move to the main queue
                sessions.Add(session);
            }
        }

        private void PruneSessions()
        {
            //Check if any are stale
            for (int i = 0; i < sessions.Count; i++)
            {
                if (sessions[i].IsExpired())
                {
                    OnSessionRemoved?.Invoke(this, sessions[i]);
                    sessions[i].Dispose();
                    sessions.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
