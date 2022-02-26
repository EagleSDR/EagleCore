using EagleWeb.Common;
using EagleWeb.Common.Auth;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.NetObjects.Interfaces;
using EagleWeb.Common.NetObjects.IO;
using EagleWeb.Common.Plugin.Interfaces.RadioSession;
using EagleWeb.Common.Radio;
using EagleWeb.Core.Misc;
using EagleWeb.Core.Misc.Module;
using EagleWeb.Core.Radio.Native;
using Newtonsoft.Json.Linq;
using RaptorDspNet;
using RaptorDspNet.raptordsp.analog;
using RaptorDspNet.raptordsp.filter.builder;
using RaptorDspNet.raptordsp.filter.fir;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EagleWeb.Core.Radio
{
    /// <summary>
    /// Represents a VFO.
    /// </summary>
    internal unsafe class EagleRadioSession : EagleObject, IEagleRadioSession, IEagleObjectPingExpiredHandler
    {
        public EagleRadioSession(IEagleObjectContext context, EagleRadio radio, EagleNativeRadioSession session) : base(context)
        {
            //Set
            this.radio = radio;
            this.session = session;
            session.PortAudio.OnOutput += PortAudio_OnOutput;

            //Create modules
            modules = radio.Context.RadioSessionModules.CreateInstance(this);

            //Configure
            context.RequireKeepAlivePings(this);
            context.AllowWebDeletion();
            context.AddExtra("modules", modules.CreateEagleObjectMap());

            //Create events
            portOnError = context.CreateEventDispatcher("OnError");

            //Create properties
            portFreqOffset = context.CreatePropertyLong("FrequencyOffset")
                .BindOnChanged((IEaglePortPropertySetArgs<long> args) => session.SetFrequencyOffset(args.Value))
                .MakeWebEditable();
            portBandwidth = context.CreatePropertyFloat("Bandwidth")
                .BindOnChanged((IEaglePortPropertySetArgs<float> args) => session.SetBandwidth(args.Value))
                .MakeWebEditable();
            portDemodulator = context.CreatePropertyObject<IEagleRadioDemodulator>("Demodulator")
                .BindOnChanged((IEaglePortPropertySetArgs<IEagleRadioDemodulator> args) => session.SetDemodulator(args.Value))
                .MakeWebEditable();
        }

        private void PortAudio_OnOutput(IEagleRadioPort<EagleStereoPair> port, EagleStereoPair* buffer, int count)
        {
            //DEBUG
            byte[] diskBuffer = new byte[sizeof(EagleStereoPair)];
            fixed (byte* diskBufferPtr = diskBuffer)
            {
                for (int i = 0; i < count; i++)
                {
                    Buffer.MemoryCopy(buffer, diskBufferPtr, sizeof(EagleStereoPair), sizeof(EagleStereoPair));
                    test.Write(diskBuffer, 0, diskBuffer.Length);
                }
            }
        }

        private EagleRadio radio;
        private EagleNativeRadioSession session;

        private IEagleModuleInstance<EagleRadioSession, IEagleRadioSessionModule> modules;

        private IEaglePortEventDispatcher portOnError;
        private IEaglePortProperty<long> portFreqOffset;
        private IEaglePortProperty<float> portBandwidth;
        private IEaglePortProperty<IEagleRadioDemodulator> portDemodulator;

        public IEagleRadioPort<EagleComplex> PortVFO => session.PortVFO;
        public IEagleRadioPort<EagleComplex> PortIF => session.PortIF;
        public IEagleRadioPort<EagleStereoPair> PortAudio => session.PortAudio;

        public override void Destroy()
        {
            //Run base
            base.Destroy();

            //Destroy modules
            modules.Destroy();

            //Destroy the native object
            session.Dispose();
        }

        public bool WebPingTimeout()
        {
            //We are about to be timed out. Check to see if any modules would like to override this operation
            //This is useful for things that need to keep running, like recording to disk
            foreach (var app in modules.Modules)
            {
                if (app.Module.KeepAlive)
                    return true;
            }

            return false;
        }

        private static System.IO.FileStream test = new System.IO.FileStream("C:\\Users\\Roman\\Desktop\\test.bin", System.IO.FileMode.Create);
    }
}
