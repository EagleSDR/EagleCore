using EagleWeb.Common.Plugin.Interfaces.Radio;
using System;

namespace EagleWeb.Common.Core.Radio
{
    public delegate void EagleNativeRadio_OnWorkerRunnableEventArgs();
    public delegate void EagleNativeRadio_OnErrorEventArgs(IEagleNativeRadio radio, string message);

    public interface IEagleNativeRadio
    {
        event EagleNativeRadio_OnWorkerRunnableEventArgs OnWorkerRunnable;
        event EagleNativeRadio_OnErrorEventArgs OnError;

        void SetSource(IEagleRadioSource newSource);
        void Unsuspend();
        void Suspend();
        IEagleNativeRadioSession CreateSession();
    }
}
