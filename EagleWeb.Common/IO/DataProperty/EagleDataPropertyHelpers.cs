using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO.DataProperty
{
    public static class EagleDataPropertyHelpers
    {
        /// <summary>
        /// Binds to OnWebSet in a builder-like manner.
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static IEagleDataPropertyWritable<T> BindOnWebSet<T>(this IEagleDataPropertyWritable<T> ctx, EagleDataPropertyWritable_OnWebSetEventArgs<T> binding)
        {
            ctx.OnWebSet += binding;
            return ctx;
        }

        public static EagleDataPropertySelector<T> AsSelector<T>(this IEagleDataPropertyWritable<string> ctx, ICollection<T> collection) where T : IEagleIdProvider
        {
            return new EagleDataPropertySelector<T>(ctx, collection);
        }
    }
}
