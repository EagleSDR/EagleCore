using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EagleWeb.Core.Web.Util
{
    public class PostingQueue<T>
    {
        public PostingQueue()
        {
            queue = new List<TaskCompletionSource<T>>();
        }

        private readonly List<TaskCompletionSource<T>> queue;

        public void Post(T value)
        {
            lock(queue)
            {
                //Search for an unused task
                for (int i = 0; i < queue.Count; i++)
                {
                    if (queue[i].TrySetResult(value))
                        return;
                }

                //We'll need to create a new task!
                TaskCompletionSource<T> task = new TaskCompletionSource<T>();
                task.SetResult(value);
                queue.Add(task);
            }
        }

        public async Task<T> ReceiveAsync()
        {
            TaskCompletionSource<T> result;

            //Obtain either the first value OR create a new one if needed
            lock(queue)
            {
                if (queue.Count > 0)
                {
                    result = queue[0];
                } else
                {
                    result = new TaskCompletionSource<T>();
                    queue.Add(result);
                }
            }

            //Run
            T value = await result.Task;

            //Remove it
            lock (queue)
                queue.Remove(result);

            return value;
        }
    }
}
