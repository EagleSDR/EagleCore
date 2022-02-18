using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Radio.Loop
{
    public interface IEagleLoopProperty<T>
    {
        public T Value { get; }

        /// <summary>
        /// Locks changes while callback is running.
        /// </summary>
        /// <param name="callback"></param>
        public void Lock(Action<T> callback);
    }
}
