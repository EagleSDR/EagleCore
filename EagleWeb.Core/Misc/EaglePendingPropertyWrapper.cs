using EagleWeb.Common.NetObjects.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Misc
{
    /// <summary>
    /// Wrapper for a property that allows it to be better utilized in a worker thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class EaglePendingPropertyWrapper<T> 
    {
        public EaglePendingPropertyWrapper(IEaglePortProperty<T> property, bool pendingDefault)
        {
            this.property = property;
            pending = pendingDefault;
            pendingValue = property.Value;
            property.OnChanged += Property_OnChanged;
        }

        private readonly IEaglePortProperty<T> property;

        private bool pending;
        private T pendingValue;

        private void Property_OnChanged(IEaglePortPropertySetArgs<T> args)
        {
            lock (this)
            {
                pending = true;
                pendingValue = args.Value;
            }
        }

        public IEaglePortProperty<T> Property => property;

        public bool IsPending(ref T result)
        {
            lock (this)
            {
                if (pending)
                {
                    pending = false;
                    result = pendingValue;
                    return true;
                } else
                {
                    return false;
                }
            }
        }
    }

    static class EaglePendingPropertyWrapperHelper
    {
        public static EaglePendingPropertyWrapper<T> AsPendingProperty<T>(this IEaglePortProperty<T> ctx, bool pendingDefault = true)
        {
            return new EaglePendingPropertyWrapper<T>(ctx, pendingDefault);
        }
    }
}
