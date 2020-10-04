using System;
using System.Collections.Generic;
using System.Threading;

namespace Thread_Pool.TaskQueue
{
    public class TaskQueue
    {
        public delegate void TaskDelegate();

        private readonly TaskQueueThread[] _taskQueueThreads;
        private readonly List<TaskDelegate> _tasks = new List<TaskDelegate>();

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
            {
                if (_tasks.Count > 0)
                {
                    // Add task to thread with minimum active tasks
                    var task = _tasks[0];

                    var minActiveTasksCount = _taskQueueThreads[0].ActiveTasks.Count;
                    var indexOfThreadWithMinActiveTasks = 0;
                    for (var i = 1; i < _taskQueueThreads.Length; i++)
                    {
                        var count = _taskQueueThreads[i].ActiveTasks.Count;
                        if (count < minActiveTasksCount)
                        {
                            minActiveTasksCount = count;
                            indexOfThreadWithMinActiveTasks = i;
                        }
                    }

                    _taskQueueThreads[indexOfThreadWithMinActiveTasks].AddTask(task);
                    _tasks.RemoveAt(0);
                }
                else
                {
                    sw.SpinOnce();
                }
            }
        }

        public void EnqueueTask(TaskDelegate task)
        {
            _tasks.Add(task);
        }

        public void ForceStop()
        {
            _isRunning = false;
            foreach (var thread in _taskQueueThreads) thread.ForceStop();
        }
    }
}