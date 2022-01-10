using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.NetObjects.IO
{
    public interface IEaglePortPropertySetArgs<T>
    {
        /// <summary>
        /// The port that this is from. Note that the value of it will not have been updated yet.
        /// </summary>
        IEaglePortProperty<T> Port { get; }

        /// <summary>
        /// The client that initiated the change. NOTE: MAY BE NULL!
        /// </summary>
        IEagleClient Client { get; }

        /// <summary>
        /// True if a web client initiated this, false if it was initiated on the serverside.
        /// </summary>
        bool FromWeb { get; }

        /// <summary>
        /// The incoming value that will be set.
        /// </summary>
        T Value { get; set; }
    }

    public delegate void IEaglePortProperty_Handler<T>(IEaglePortPropertySetArgs<T> args);

    public interface IEaglePortProperty<T> : IEagleObjectPort
    {
        /// <summary>
        /// Event called when the value is about to be changed. Throw an exception to cancel.
        /// </summary>
        event IEaglePortProperty_Handler<T> OnChanged;

        /// <summary>
        /// The value assigned.
        /// </summary>
        T Value { get; set; }

        /// <summary>
        /// Is this property able to be changed from the web?
        /// </summary>
        bool IsWebEditable { get; set; }

        /// <summary>
        /// Makes this able to be manipulated by the web.
        /// </summary>
        /// <returns></returns>
        IEaglePortProperty<T> MakeWebEditable();

        /// <summary>
        /// Binds to the OnChanged event in a more builder-friendly method.
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        IEaglePortProperty<T> BindOnChanged(IEaglePortProperty_Handler<T> binding);

        /// <summary>
        /// Requires a permission for a web client to set this.
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        IEaglePortProperty<T> RequirePermission(string permission);
    }
}
