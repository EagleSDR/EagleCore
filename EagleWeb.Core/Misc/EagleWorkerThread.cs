using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EagleWeb.Core.Misc
{
    abstract class EagleWorkerThread : EagleWorkerEventQueue
    {
        public EagleWorkerThread(string threadName)
        {
            this.threadName = threadName;
        }

        private readonly string threadName;
        private object startStopLock = new object();
        private Thread thread;
        private volatile bool stopping = false;

        /// <summary>
        /// Starts the worker thread.
        /// </summary>
        public void StartWorker()
        {
            lock (startStopLock)
            {
                //Check if a thread is already active
                if (thread != null)
                    return;

                //Reset state
                stopping = false;

                //Spawn worker
                thread = new Thread(Worker);
                thread.Name = threadName;
                thread.IsBackground = true;
                thread.Priority = ThreadPriority.Highest;
                thread.Start();
            }
        }

        /// <summary>
        /// Stops the worker thread, hanging while it exits.
        /// </summary>
        public void StopWorker()
        {
            lock (startStopLock)
            {
                //Check if the thread is active
                if (thread == null)
                    return;

                //Request stop
                stopping = true;

                //Wait for thread to terminate
                if (Thread.CurrentThread.ManagedThreadId != thread.ManagedThreadId)
                    thread.Join();

                //Set state
                thread = null;
            }
        }

        protected virtual void WorkerStarting()
        {
            //Can be optionally overwritten by the user
        }

        protected virtual void WorkerStopping()
        {
            //Can be optionally overwritten by the user
        }

        /// <summary>
        /// Continously called while the worker is active.
        /// </summary>
        protected abstract void Work();

        private void Worker()
        {
            WorkerStarting();
            while (!stopping)
                Work();
            WorkerStopping();
        }
    }
}
