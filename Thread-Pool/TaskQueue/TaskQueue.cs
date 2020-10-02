using System;
using System.Collections.Generic;
using System.Threading;

namespace Thread_Pool.TaskQueue
{
    public class TaskQueue
    {
        private readonly TaskQueueThread[] _taskQueueThreads;
        private readonly List<TaskDelegate> _tasks = new List<TaskDelegate>();

        private ManualResetEvent _mrse = new ManualResetEvent(false);
        private bool _isRunning = true;

        public delegate void TaskDelegate();

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

            Console.WriteLine("Created Thread Pool of " + _taskQueueThreads.Length + " threads");

            var taskQueueThread = new Thread(MainLoop);
            Console.WriteLine("Starting MainLoop");
            taskQueueThread.Start();
        }

        private void MainLoop()
        {
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
                    // Console.WriteLine("Added Task to Thread " + indexOfThreadWithMinActiveTasks);
                    _tasks.RemoveAt(0);
                }
                else
                {
                    _mrse.Reset(); // Pause thread
                }

                _mrse.WaitOne();
            }
        }

        public void EnqueueTask(TaskDelegate task)
        {
            _tasks.Add(task);
            _mrse.Set(); // Resume thread
        }

        public void ForceStop()
        {
            _isRunning = false;
            foreach (var thread in _taskQueueThreads) thread.ForceStop();
            _mrse.Set(); // Resume thread
        }
    }
}