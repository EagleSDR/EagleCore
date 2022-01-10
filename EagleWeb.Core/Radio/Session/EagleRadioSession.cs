using EagleWeb.Common;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.NetObjects.IO;
using EagleWeb.Common.Radio;
using EagleWeb.Common.Radio.Components;
using EagleWeb.Common.Radio.Modules;
using EagleWeb.Core.Misc;
using EagleWeb.Core.Radio.Components;
using EagleWeb.Core.Radio.Components.FilterBuilder;
using EagleWeb.Core.Radio.Components.Filters;
using EagleWeb.Core.Radio.Loop;
using Newtonsoft.Json.Linq;
using RaptorDspNet;
using RaptorDspNet.raptordsp.analog;
using RaptorDspNet.raptordsp.filter.builder;
using RaptorDspNet.raptordsp.filter.fir;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Radio.Session
{
    /// <summary>
    /// Represents a VFO.
    /// </summary>
    internal class EagleRadioSession : EagleLoop
    {
        public EagleRadioSession(EagleRadio radio) : base(radio)
        {
            this.radio = radio;

            //Create buffers
            bufferIq = new RaptorBuffer<raptor_complex>(EagleRadio.BUFFER_SIZE);
            bufferAudioL = new RaptorBuffer<float>(EagleRadio.BUFFER_SIZE);
            bufferAudioR = new RaptorBuffer<float>(EagleRadio.BUFFER_SIZE);

            //Create components
            rotator = RaptorActivator.MakeRaptorRotator();
            filterIf = RaptorActivator.MakeRaptorFilter<raptor_complex, raptor_complex, float>();
        }

        protected override void ConfigureObject(IEagleObjectConfigureContext context)
        {
            base.ConfigureObject(context);

            //Create ports
            portDelete = context.CreatePortApi("Delete").Bind((IEagleClient client, JObject message) =>
            {
                //Set
                userRequestedRemoval = true;

                //Create response
                JObject response = new JObject();
                response["ok"] = true;
                return response;
            });

            //Create properties
            propFrequencyOffset = CreateLoopProperty(
                context.CreatePropertyLong("FrequencyOffset")
                .MakeWebEditable()
            );
            propBandwidth = CreateLoopProperty(
                context.CreatePropertyFloat("Bandwidth")
                .MakeWebEditable()
            );
            propDemodulator = CreateLoopProperty(
                context.CreatePropertyObject<EagleModuleDemodulator>("Demodulator")
                .MakeWebEditable()
            );
        }

        //MISC
        private EagleRadio radio;
        private volatile bool userRequestedRemoval = false;
        private float inputSampleRate;

        //NATIVE COMPONENTS
        private IRaptorRotator rotator;
        private IRaptorFilter<raptor_complex, raptor_complex, float> filterIf;  

        //BUFFERS
        private RaptorBuffer<raptor_complex> bufferIq;
        private RaptorBuffer<float> bufferAudioL;
        private RaptorBuffer<float> bufferAudioR;

        //PORTS
        private IEaglePortApi portDelete;
        private IEagleLoopPortProperty<long> propFrequencyOffset;
        private IEagleLoopPortProperty<float> propBandwidth;
        private IEagleLoopPortProperty<EagleModuleDemodulator> propDemodulator;

        /// <summary>
        /// Sets settings to be applied.
        /// </summary>
        /// <param name="inputSampleRate"></param>
        public void Configure(float inputSampleRate)
        {
            //Set
            this.inputSampleRate = inputSampleRate;

            //Make stale so we reconfigure with it
            MakeStale();
        }

        /// <summary>
        /// Returns true if it's time to remove this item.
        /// </summary>
        /// <returns></returns>
        public bool IsExpired()
        {
            //If the user requested removal, immediately do
            if (userRequestedRemoval)
                return true;

            return false;
        }

        protected unsafe override void ConfigureInternal()
        {
            //Make sure we have all parts
            if (propDemodulator.Value == null)
                throw new Exception("No demodulator was set.");

            //Validate settings
            if (propBandwidth.Value >= inputSampleRate || propBandwidth.Value <= 0)
                throw new Exception($"Bandwidth {propBandwidth.Value} is invalid.");

            //Configure rotator
            rotator.SetSampleRate(inputSampleRate);
            rotator.SetFreqOffset(propFrequencyOffset.Value);

            //Configure IF filter
            float decimatedSampleRate;
            using (IRaptorFilterBuilderLowpass builder = RaptorActivator.MakeRaptorFilterBuilderLowpass(inputSampleRate, propBandwidth.Value * 0.5f))
            {
                builder.AutomaticTapCount(propBandwidth.Value * 0.1f, 60);
                using (var taps = builder.BuildTapsReal())
                    filterIf.Configure(taps, builder.CalculateDecimation(&decimatedSampleRate));
            }

            //Configure demodulator
            float audioSampleRate = propDemodulator.Value.Configure(decimatedSampleRate);

            //Log
            Log(EagleLogLevel.DEBUG, $"Reconfigured session: {inputSampleRate} -> [bw={propBandwidth.Value}] -> {decimatedSampleRate} -> [demod={propDemodulator.Value.GetType().FullName}] -> {audioSampleRate}");
        }

        protected override void ProcessInternal(params object[] args)
        {
            ProcessMain((RaptorBuffer<raptor_complex>)args[0], (int)args[1]);
        }

        private unsafe void ProcessMain(RaptorBuffer<raptor_complex> inBuffer, int count)
        {
            //Rotate into our buffer
            rotator.Process(inBuffer, bufferIq, count);

            //Filter
            count = filterIf.Process(bufferIq, bufferIq, count);
        }

        public override void Dispose()
        {
            base.Dispose();

            //Clean up
            rotator.Dispose();
            filterIf.Dispose();
            bufferIq.Dispose();
            bufferAudioL.Dispose();
            bufferAudioR.Dispose();
        }
    }
}
