using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Thread_Pool.TaskQueue
{
    public class TaskQueue
    {
        public delegate void TaskDelegate();

        // Can be modified from multiple threads
        private readonly ConcurrentQueue<TaskDelegate> _queuedTasks = new ConcurrentQueue<TaskDelegate>();

        private readonly Thread[] _threads;

        private bool _isRunning = true;

        public TaskQueue(int threadCount)
        {
            if (threadCount <= 0) throw new ArgumentException("Thread count must be > 0", nameof(threadCount));

            _threads = new Thread[threadCount];
            for (var i = 0; i < threadCount; i++)
            {
                var thread = new Thread(ThreadLoop);
                _threads[i] = thread;
                thread.Start();
            }
        }

        private void ThreadLoop()
        {
            var sw = new SpinWait();

            while (_isRunning)
                if (_queuedTasks.TryDequeue(out var task))
                    task.Invoke();
                else
                    sw.SpinOnce();
        }

        public void EnqueueTask(TaskDelegate task)
        {
            _queuedTasks.Enqueue(task);
        }

        public void ForceStop()
        {
            _isRunning = false;
        }
    }
}