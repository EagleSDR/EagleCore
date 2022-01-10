using EagleWeb.Common.NetObjects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EagleWeb.Core.Radio.Loop
{
    public abstract class EagleLoopThread : EagleLoop
    {
        protected EagleLoopThread(IEagleObjectManagerLink link, JObject info = null) : base(link, info)
        {
            worker = new Thread(WorkerThread);
            worker.IsBackground = true;
            worker.Priority = ThreadPriority.Highest;
            activated = false;
        }

        private Thread worker;
        private bool activated;

        protected void StartWorkerThread(string name)
        {
            //Make sure we haven't already started
            if (activated)
                throw new Exception("Worker thread was already started!");

            //Configure and start
            worker.Name = name;
            worker.Start();
            activated = true;
        }

        private void WorkerThread()
        {
            while (true)
                ProcessWait();
        }
    }
}
