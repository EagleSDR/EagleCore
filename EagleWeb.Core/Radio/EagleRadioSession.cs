using EagleWeb.Common;
using EagleWeb.Common.Auth;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.NetObjects.IO;
using EagleWeb.Common.Radio;
using EagleWeb.Common.Radio.Modules;
using EagleWeb.Core.Misc;
using EagleWeb.Core.Radio.Components;
using EagleWeb.Core.Radio.Loop;
using Newtonsoft.Json.Linq;
using RaptorDspNet;
using RaptorDspNet.raptordsp.analog;
using RaptorDspNet.raptordsp.filter.builder;
using RaptorDspNet.raptordsp.filter.fir;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Radio
{
    /// <summary>
    /// Represents a VFO.
    /// </summary>
    internal class EagleRadioSession : EagleLoop, IEagleRadioSession
    {
        public EagleRadioSession(EagleRadio radio) : base(radio)
        {
            this.radio = radio;

            //Create buffers
            bufferIq = new RaptorBuffer<raptor_complex>(EagleRadio.BUFFER_SIZE);
            bufferAudioL = new RaptorBuffer<float>(EagleRadio.BUFFER_SIZE);
            bufferAudioR = new RaptorBuffer<float>(EagleRadio.BUFFER_SIZE);
            bufferAudioResampledL = new RaptorBuffer<float>(EagleRadio.BUFFER_SIZE);
            bufferAudioResampledR = new RaptorBuffer<float>(EagleRadio.BUFFER_SIZE);

            //Create components
            rotator = RaptorActivator.MakeRaptorRotator();
            filterIf = RaptorActivator.MakeRaptorFilter<raptor_complex, raptor_complex, float>();
            audioResampler = new EagleMultichannelResampler(2, 1, 1, true);
        }

        protected override void ConfigureObject(IEagleObjectConfigureContext context)
        {
            base.ConfigureObject(context);

            //Create ports
            portDelete = context.CreatePortApi("Delete").Bind((IEagleAccount account, JObject message) =>
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

        const int OUTPUT_AUDIO_RATE = 48000;

        //MISC
        private EagleRadio radio;
        private volatile bool userRequestedRemoval = false;
        private float inputSampleRate;
        private double audioResamplingRate;
        private bool demodulationEnabled;

        //NATIVE COMPONENTS
        private IRaptorRotator rotator;
        private IRaptorFilter<raptor_complex, raptor_complex, float> filterIf;
        private EagleMultichannelResampler audioResampler;

        //BUFFERS
        private RaptorBuffer<raptor_complex> bufferIq;
        private RaptorBuffer<float> bufferAudioL;
        private RaptorBuffer<float> bufferAudioR;
        private RaptorBuffer<float> bufferAudioResampledL;
        private RaptorBuffer<float> bufferAudioResampledR;

        //PORTS
        private IEaglePortApi portDelete;
        private IEagleLoopPortProperty<long> propFrequencyOffset;
        private IEagleLoopPortProperty<float> propBandwidth;
        private IEagleLoopPortProperty<EagleModuleDemodulator> propDemodulator;

        //PIPES
        private EagleRadioPort<EagleComplex> portVfo = new EagleRadioPort<EagleComplex>("VFO");
        private EagleRadioPort<EagleComplex> portIf = new EagleRadioPort<EagleComplex>("IF");
        private EagleRadioPort<EagleStereoPair> portAudio = new EagleRadioPort<EagleStereoPair>("Audio", OUTPUT_AUDIO_RATE);

        //GETTERS
        protected override bool Enabled => true;
        public IEagleRadioPort<EagleComplex> PortVFO => portVfo;
        public IEagleRadioPort<EagleComplex> PortIF => portIf;
        public IEagleRadioPort<EagleStereoPair> PortAudio => portAudio;

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
            //Validate settings
            /*if (propBandwidth.Value >= inputSampleRate || propBandwidth.Value <= 0)
                throw new Exception($"Bandwidth {propBandwidth.Value} is invalid.");*/

            //Configure pipe
            portVfo.SampleRate = inputSampleRate;

            //Configure rotator
            rotator.SetSampleRate(inputSampleRate);
            rotator.SetFreqOffset(propFrequencyOffset.Value);

            //Configure IF and demodulator only if they are set
            demodulationEnabled = propBandwidth.Value > 0 && propDemodulator.Value != null;
            if (demodulationEnabled)
            {
                //Configure IF filter
                float decimatedSampleRate;
                using (IRaptorFilterBuilderLowpass builder = RaptorActivator.MakeRaptorFilterBuilderLowpass(inputSampleRate, propBandwidth.Value * 0.5f))
                {
                    builder.AutomaticTapCount(propBandwidth.Value * 0.1f, 60);
                    filterIf.Configure(builder, builder.CalculateDecimation(&decimatedSampleRate));
                }
                portIf.SampleRate = decimatedSampleRate;

                //Configure demodulator
                float audioSampleRate = propDemodulator.Value.Configure(decimatedSampleRate);

                //Recalculate the resampling factor
                audioResamplingRate = EagleResampler.CalculateFactor(audioSampleRate, OUTPUT_AUDIO_RATE);
                audioResampler.Reconfigure(audioResamplingRate, audioResamplingRate, true);

                //Log
                Log(EagleLogLevel.DEBUG, $"Reconfigured session: {inputSampleRate} -> [bw={propBandwidth.Value}] -> {decimatedSampleRate} -> [demod={propDemodulator.Value.GetType().FullName}] -> {audioSampleRate} -> [factor={audioResamplingRate}] -> {OUTPUT_AUDIO_RATE}");
            }
        }

        protected override void ProcessInternal(params object[] args)
        {
            ProcessMain((RaptorBuffer<raptor_complex>)args[0], (int)args[1]);
        }

        private static System.IO.FileStream test = new System.IO.FileStream("C:\\Users\\Roman\\Desktop\\test.bin", System.IO.FileMode.Create);

        private unsafe void ProcessMain(RaptorBuffer<raptor_complex> inBuffer, int count)
        {
            //Rotate into our buffer
            rotator.Process(inBuffer, bufferIq, count);

            //Send out
            portVfo.Output((EagleComplex*)bufferIq.Pointer, count);

            //Demodulate if we can
            if (demodulationEnabled)
            {
                //Filter
                count = filterIf.Process(bufferIq, bufferIq, count);

                //Send out
                portIf.Output((EagleComplex*)bufferIq.Pointer, count);

                //Demodulate
                count = propDemodulator.Value.Process((EagleComplex*)bufferIq.Pointer, count, bufferAudioL, bufferAudioR);

                //Resample to standard and send out
                audioResampler.Process(audioResamplingRate, new float*[] { bufferAudioL, bufferAudioR }, count, new float*[] { bufferAudioResampledL, bufferAudioResampledR }, EagleRadio.BUFFER_SIZE, false, (int outCount) =>
                {
                    //Deinterlace audio buffer
                    EagleStereoPair* buffer = MergeAudioBuffers(outCount);

                    //Send out
                    portAudio.Output(buffer, outCount);

                    //Test
                    for (int i = 0; i < outCount; i++)
                    {
                        test.Write(BitConverter.GetBytes(buffer[i].left), 0, 4);
                        test.Write(BitConverter.GetBytes(buffer[i].right), 0, 4);
                    }
                });
            }
        }

        private unsafe EagleStereoPair* MergeAudioBuffers(int count)
        {
            //Borrow the IQ buffer and use it as a float buffer
            float* output = (float*)bufferIq.Pointer;

            //Get buffers
            float* l = bufferAudioResampledL.Pointer;
            float* r = bufferAudioResampledR.Pointer;

            //Deinterlace
            int outIndex = 0;
            for (int i = 0; i < count; i++)
            {
                output[outIndex++] = l[i];
                output[outIndex++] = r[i];
            }

            return (EagleStereoPair*)output;
        }

        public override void Dispose()
        {
            base.Dispose();

            //Free native buffers
            bufferIq.Dispose();
            bufferAudioL.Dispose();
            bufferAudioR.Dispose();
            bufferAudioResampledL.Dispose();
            bufferAudioResampledR.Dispose();

            //Free native components
            rotator.Dispose();
            filterIf.Dispose();
            audioResampler.Dispose();
        }
    }
}
