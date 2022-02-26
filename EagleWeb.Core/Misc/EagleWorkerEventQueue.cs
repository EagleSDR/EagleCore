using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Misc
{
    abstract class EagleWorkerEventQueue
    {
        private ConcurrentQueue<Action> queued = new ConcurrentQueue<Action>();

        protected void RunOnWorkerThread(Action callback)
        {
            queued.Enqueue(callback);
        }

        protected void ProcessWorkerEvents()
        {
            //Run queued commands
            while (queued.TryDequeue(out Action callback))
                callback();
        }
    }
}
