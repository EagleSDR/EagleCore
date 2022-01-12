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
        private List<EagleRadioSession> uninitializedSessions = new List<EagleRadioSession>();

        //PIPES
        private EagleRadioPort<EagleComplex> pipeOutput = new EagleRadioPort<EagleComplex>("Output");

        //PORTS
        private IEaglePortApi portCreateSession;
        private IEagleLoopPortProperty<EagleModuleSource> propSource;

        public event IEagleRadio_SessionEventArgs OnSessionCreated;
        public event IEagleRadio_SessionEventArgs OnSessionRemoved;

        //GETTERS
        public IEaglePortProperty<EagleModuleSource> Source => propSource.Port;
        public IEagleRadioPort<EagleComplex> PortInput => pipeOutput;

        protected override void ConfigureObject(IEagleObjectConfigureContext context)
        {
            base.ConfigureObject(context);

            //Set up the loop props
            Enabled.RequirePermission(EaglePermissions.PERMISSION_POWER);

            //Create ports
            portCreateSession = context.CreatePortApi("CreateSession")
                .Bind(ApiCreateSession);

            //Create props
            propSource = CreateLoopProperty(
                context.CreatePropertyObject<EagleModuleSource>("Source")
                .MakeWebEditable()
                .RequirePermission(EaglePermissions.PERMISSION_CHANGE_SOURCE)
            );
        }

        private JObject ApiCreateSession(IEagleAccount client, JObject message)
        {
            //Create the session
            EagleRadioSession session = new EagleRadioSession(this);

            //Add
            lock (uninitializedSessions)
                uninitializedSessions.Add(session);

            //Set as stale so it gets set up
            MakeStale();

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
            //Check if any sessions stale
            PruneSessions();

            //Read from radio
            int count = propSource.Value.Read((EagleComplex*)bufferIq.Pointer, BUFFER_SIZE);

            //Send out
            pipeOutput.Output((EagleComplex*)bufferIq.Pointer, count);

            //Dispatch
            foreach (var s in sessions)
                s.Process(bufferIq, count);
        }

        private void PruneSessions()
        {
            //Check if any are stale
            for (int i = 0; i < sessions.Count; i++)
            {
                if (sessions[i].IsExpired())
                {
                    sessions[i].Dispose();
                    sessions.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
