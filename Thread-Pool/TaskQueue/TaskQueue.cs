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

        private readonly TaskQueueThread[] _taskQueueThreads;

        private bool _isRunning = true;

        public TaskQueue(int threadCount)
        {
            if (threadCount <= 0) throw new ArgumentException("Thread count must be > 0", nameof(threadCount));

            _taskQueueThreads = new TaskQueueThread[threadCount];
            for (var i = 0; i < threadCount; i++)
            {
                var newQueueThread = new TaskQueueThread();
                _taskQueueThreads[i] = newQueueThread;
                var newSystemThread = new Thread(newQueueThread.ThreadLoop);
                newSystemThread.Start();
            }

            var taskQueueThread = new Thread(MainLoop);
            taskQueueThread.Start();
        }

        private void MainLoop()
        {
            var sw = new SpinWait();

            while (_isRunning)
                if (_queuedTasks.TryDequeue(out var task))
                {
                    // Add task to thread with minimum queued tasks
                    var minQueuedTasksCount = _taskQueueThreads[0].QueuedTasksCount;
                    var indexOfThreadWithMinQueuedTasks = 0;
                    for (var i = 1; i < _taskQueueThreads.Length; i++)
                    {
                        var count = _taskQueueThreads[i].QueuedTasksCount;
                        if (count < minQueuedTasksCount)
                        {
                            minQueuedTasksCount = count;
                            indexOfThreadWithMinQueuedTasks = i;
                        }
                    }
                    _taskQueueThreads[indexOfThreadWithMinQueuedTasks].AddTask(task);
                }
                else
                {
                    sw.SpinOnce();
                }
        }

        public void EnqueueTask(TaskDelegate task)
        {
            _queuedTasks.Enqueue(task);
        }

        public void ForceStop()
        {
            _isRunning = false;
            foreach (var thread in _taskQueueThreads) thread.ForceStop();
        }
    }
}