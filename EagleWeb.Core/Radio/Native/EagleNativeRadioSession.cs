using EagleWeb.Common.Plugin.Interfaces.RadioSession;
using EagleWeb.Common.Radio;
using EagleWeb.Core.Misc;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Core.Radio.Native
{
    delegate void EagleNativeRadioSession_OnTickEventArgs(EagleNativeRadioSession session);
    unsafe class EagleNativeRadioSession : EagleWorkerEventQueue, IDisposable
    {
        public EagleNativeRadioSession(int bufferSize)
        {
            //Create a GC handle on ourselves so we can locate ourselves in callbacks
            gc = GCHandle.Alloc(this);

            //Create native handle
            handle = EagleNativeMethods.eaglesession_create(
                bufferSize,
                (IntPtr)gc,
                funcNativeProcess,
                funcNativePipeConfigure,
                funcNativePipePush,
                funcNativeDemodulatorConfigure,
                funcNativeDemodulatorProcess
            );
        }

        private readonly IntPtr handle;
        private readonly GCHandle gc;
        private readonly EagleNativeWrapper<IEagleRadioDemodulator> demodulator = new EagleNativeWrapper<IEagleRadioDemodulator>();
        private readonly IBasePortAdapter[] ports = new IBasePortAdapter[]
        {
            new ComplexPortAdapter("Input"),
            new ComplexPortAdapter("VFO"),
            new ComplexPortAdapter("IF"),
            new StereoPortAdapter("Audio")
        };

        private readonly EagleNativeMethods.eaglesession_process_cb funcNativeProcess = NativeProcess;
        private readonly EagleNativeMethods.eaglesession_pipe_configure_cb funcNativePipeConfigure = NativePipeConfigure;
        private readonly EagleNativeMethods.eaglesession_pipe_push_cb funcNativePipePush = NativePipePush;
        private readonly EagleNativeMethods.eaglesession_demodulator_configure_cb funcNativeDemodulatorConfigure = NativeDemodulatorConfigure;
        private readonly EagleNativeMethods.eaglesession_demodulator_process_cb funcNativeDemodulatorProcess = NativeDemodulatorProcess;

        private bool disposed = false;

        /* API */

        public event EagleNativeRadioSession_OnTickEventArgs OnTick;

        public IEagleRadioPort<EagleComplex> PortInput => (IEagleRadioPort<EagleComplex>)ports[0];
        public IEagleRadioPort<EagleComplex> PortVFO => (IEagleRadioPort<EagleComplex>)ports[1];
        public IEagleRadioPort<EagleComplex> PortIF => (IEagleRadioPort<EagleComplex>)ports[2];
        public IEagleRadioPort<EagleStereoPair> PortAudio => (IEagleRadioPort<EagleStereoPair>)ports[3];

        public void SetDemodulator(IEagleRadioDemodulator newDemodulator)
        {
            RunOnWorkerThread(() =>
            {
                demodulator.Object = newDemodulator;
                EagleNativeMethods.eaglesession_set_demodulator(GetHandle(), demodulator.Handle);
            });
        }

        public void SetBandwidth(float bandwidth)
        {
            RunOnWorkerThread(() =>
            {
                EagleNativeMethods.eaglesession_set_bandwidth(GetHandle(), bandwidth);
            });
        }

        public void SetFrequencyOffset(float frequencyOffset)
        {
            RunOnWorkerThread(() =>
            {
                EagleNativeMethods.eaglesession_set_frequency_offset(GetHandle(), frequencyOffset);
            });
        }

        public IntPtr GetHandle()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
            return handle;
        }

        public virtual void Dispose()
        {
            //IMPORTANT: THIS CALL IS UNSAFE! THIS SHOULD BE OVERWRITTEN TO BE CALLED ONLY WHEN IT IS SAFE TO DO SO!

            //Free demodulator
            demodulator.Clear();

            //Destroy session
            EagleNativeMethods.eaglesession_destroy(GetHandle());

            //Free GC
            gc.Free();

            //Set flag
            disposed = true;
        }

        /* STATIC NATIVE METHODS */

        private static EagleNativeRadioSession FromIntPtr(IntPtr userCtx)
        {
            return (EagleNativeRadioSession)GCHandle.FromIntPtr(userCtx).Target;
        }

        private static void NativeProcess(IntPtr user_ctx)
        {
            EagleNativeRadioSession session = FromIntPtr(user_ctx);
            session.ProcessWorkerEvents();
            session.OnTick?.Invoke(session);
        }

        private static void NativePipeConfigure(IntPtr user_ctx, int pipe_id, float sampleRate)
        {
            FromIntPtr(user_ctx).ports[pipe_id].Configure(sampleRate);
        }

        private static void NativePipePush(IntPtr user_ctx, int pipe_id, EagleComplex* buffer, int count)
        {
            FromIntPtr(user_ctx).ports[pipe_id].Push(buffer, count);
        }

        private static float NativeDemodulatorConfigure(IntPtr user_ctx, IntPtr demodulator_ctx, float input_sample_rate, int buffer_size)
        {
            try
            {
                return EagleNativeWrapper<IEagleRadioDemodulator>.FromHandle(demodulator_ctx).Configure(input_sample_rate, buffer_size);
            } catch
            {
                return -1;
            }
        }

        private static int NativeDemodulatorProcess(IntPtr user_ctx, IntPtr demodulator_ctx, EagleComplex* buffer, float* audioL, float* audioR, int count)
        {
            return EagleNativeWrapper<IEagleRadioDemodulator>.FromHandle(demodulator_ctx).Process(buffer, count, audioL, audioR);
        }

        /* PORT IMPLEMENTATIONS */

        interface IBasePortAdapter
        {
            void Configure(float sampleRate);
            void Push(EagleComplex* ptr, int count);
        }

        abstract class BasePortAdapter<T> : IEagleRadioPort<T>, IBasePortAdapter where T : unmanaged
        {
            public BasePortAdapter(string name)
            {
                this.name = name;
            }

            private string name;
            private float sampleRate;

            public string Name => name;
            public float SampleRate => sampleRate;

            public event IEagleRadioPort_SampleRateChanged<T> OnSampleRateChanged;
            public abstract event IEagleRadioPort_Output<T> OnOutput;

            public void Configure(float sampleRate)
            {
                this.sampleRate = sampleRate;
                OnSampleRateChanged?.Invoke(this, sampleRate);
            }

            public abstract void Push(EagleComplex* buffer, int count);
        }

        class ComplexPortAdapter : BasePortAdapter<EagleComplex>
        {
            public ComplexPortAdapter(string name) : base(name)
            {
            }

            public override event IEagleRadioPort_Output<EagleComplex> OnOutput;

            public override void Push(EagleComplex* buffer, int count)
            {
                OnOutput?.Invoke(this, buffer, count);
            }
        }

        class StereoPortAdapter : BasePortAdapter<EagleStereoPair>
        {
            public StereoPortAdapter(string name) : base(name)
            {
            }

            public override event IEagleRadioPort_Output<EagleStereoPair> OnOutput;

            public override void Push(EagleComplex* buffer, int count)
            {
                OnOutput?.Invoke(this, (EagleStereoPair*)buffer, count);
            }
        }
    }
}
