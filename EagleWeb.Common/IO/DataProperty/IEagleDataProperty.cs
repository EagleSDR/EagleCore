using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO.DataProperty
{
    /// <summary>
    /// Defines a property that will be replicated to the web.
    /// </summary>
    public interface IEagleDataProperty<T>
    {
        /// <summary>
        /// The full unique ID for this property.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The value of this property. Replicated on the web.
        /// </summary>
        T Value { get; set; }
    }
}
