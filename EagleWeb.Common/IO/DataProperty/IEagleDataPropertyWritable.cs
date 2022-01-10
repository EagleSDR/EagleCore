using EagleWeb.Common.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO.DataProperty
{
    /// <summary>
    /// Defines a property that can be written to from the web.
    /// </summary>
    public interface IEagleDataPropertyWritable<T> : IEagleDataProperty<T>
    {
        /// <summary>
        /// Makes the permission required to set this value.
        /// </summary>
        IEagleDataPropertyWritable<T> RequirePermission(string permission);

        /// <summary>
        /// Event dispatched when a web client sets the value.
        /// </summary>
        public event EagleDataPropertyWritable_OnWebSetEventArgs<T> OnWebSet;
    }

    public delegate void EagleDataPropertyWritable_OnWebSetEventArgs<T>(IEagleDataPropertyWritable<T> property, T value);
}
