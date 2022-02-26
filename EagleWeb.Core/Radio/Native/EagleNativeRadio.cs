using EagleWeb.Common.Plugin.Interfaces.Radio;
using EagleWeb.Common.Radio;
using EagleWeb.Core.Misc;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace EagleWeb.Core.Radio.Native
{
    delegate void EagleNativeRadio_OnErrorEventArgs(EagleNativeRadio radio, string message);
    unsafe class EagleNativeRadio : EagleWorkerThread
    {
        public EagleNativeRadio(int bufferSize) : base("Eagle Radio Worker Thread")
        {
            //Set
            this.bufferSize = bufferSize;

            //Create a GC handle on ourselves so we can locate ourselves in callbacks
            gc = GCHandle.Alloc(this);

            //Create native handle
            handle = EagleNativeMethods.eagleradio_create(bufferSize, (IntPtr)gc, funcSourceReadCallback, funcErrorCallback);
        }

        private readonly IntPtr handle;
        private readonly GCHandle gc;
        private readonly int bufferSize;
        private readonly EagleNativeWrapper<IEagleRadioSource> source = new EagleNativeWrapper<IEagleRadioSource>();

        private readonly EagleNativeMethods.eagleradio_source_read_cb funcSourceReadCallback = SourceReadCallback;
        private readonly EagleNativeMethods.eagleradio_error_cb funcErrorCallback = ErrorCallback;

        private bool suspended = true;

        /* API */

        public event EagleNativeRadio_OnErrorEventArgs OnError;

        public void SetSource(IEagleRadioSource newSource)
        {
            RunOnWorkerThread(() =>
            {
                //Signal old source to close
                if (source.Object != null)
                {
                    //Signal
                    source.Object.Close();

                    //Reset native object
                    EagleNativeMethods.eagleradio_change_source(GetHandle(), IntPtr.Zero, 0);

                    //Clear
                    source.Object = null;
                }

                //Run only if there's a new source
                if (newSource != null)
                {
                    //Open
                    float sampleRate;
                    try
                    {
                        sampleRate = newSource.Open(bufferSize);
                    }
                    catch (Exception error)
                    {
                        AbortRadio("Failed to open radio source: " + error.Message);
                        return;
                    }

                    //Set
                    source.Object = newSource;

                    //Set native object
                    EagleNativeMethods.eagleradio_change_source(GetHandle(), source.Handle, sampleRate);
                }
            });
        }

        public void Unsuspend()
        {
            suspended = false;
        }

        public void Suspend()
        {
            suspended = true;
        }

        public EagleNativeRadioSession CreateSession()
        {
            //Create the class
            EagleNativeRadioLinkedSession session = new EagleNativeRadioLinkedSession(this);

            //On the worker thread, add this
            RunOnWorkerThread(() =>
            {
                EagleNativeMethods.eagleradio_add_session(GetHandle(), session.GetHandle());
            });

            return session;
        }

        /* INTERNAL */

        protected override void Work()
        {
            ProcessWorkerEvents();
            if (suspended)
            {
                //Sleep for a bit
                Thread.Sleep(100);
            } else
            {
                //Run the native work method
                EagleNativeMethods.eagleradio_work(GetHandle());
            }
        }

        private void AbortRadio(string message)
        {
            //Suspend
            suspended = true;

            //Fire event
            OnError?.Invoke(this, message);
        }

        private IntPtr GetHandle()
        {
            return handle;
        }

        /* STATIC NATIVE WRAPPER METHODS */

        private static int SourceReadCallback(IntPtr user_ctx, IntPtr source_ctx, EagleComplex* buffer, int count)
        {
            //Get the source
            IEagleRadioSource source = EagleNativeWrapper<IEagleRadioSource>.FromHandle(source_ctx);

            //Process
            try
            {
                return source.Read(buffer, count);
            }
            catch (Exception error)
            {
                ErrorCallback(user_ctx, "Failed to read from source: " + error.Message);
                return 0;
            }
        }

        private static void ErrorCallback(IntPtr user_ctx, string message)
        {
            //Get the radio
            EagleNativeRadio radio = EagleNativeWrapper<EagleNativeRadio>.FromHandle(user_ctx);

            //Abort
            radio.AbortRadio(message);
        }

        /* INTERNAL SESSION CLASS */

        class EagleNativeRadioLinkedSession : EagleNativeRadioSession
        {
            public EagleNativeRadioLinkedSession(EagleNativeRadio radio) : base(radio.bufferSize)
            {
                this.radio = radio;
            }

            private EagleNativeRadio radio;

            public override void Dispose()
            {
                //Run on the radio's worker thread
                radio.RunOnWorkerThread(() =>
                {
                    //Remove from the list within the radio
                    EagleNativeMethods.eagleradio_remove_session(radio.GetHandle(), GetHandle());

                    //Dispose as normal, safely
                    base.Dispose();
                });
            }
        }
    }
}
