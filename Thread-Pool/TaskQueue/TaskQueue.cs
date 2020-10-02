using System;
using System.Collections.Generic;
using System.Threading;

namespace Thread_Pool.TaskQueue
{
    public class TaskQueue
    {
        public delegate void TaskDelegate();

        private const int MainLoopDelay = 500;

        private bool _isRunning = true;
        
        private readonly List<TaskDelegate> _tasks = new List<TaskDelegate>();
        private readonly TaskQueueThread[] TaskQueueThreads;

        public TaskQueue(int threadCount)
        {
            TaskQueueThreads = new TaskQueueThread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                TaskQueueThread newQueueThread = new TaskQueueThread();
                TaskQueueThreads[i] = newQueueThread;
                Thread newSystemThread = new Thread(newQueueThread.ThreadLoop);
                newSystemThread.Start();
            }
            Console.WriteLine("Created Thread Pool of " + TaskQueueThreads.Length + " threads");

            Thread taskQueueThread = new Thread(MainLoop);
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
                    TaskDelegate task = _tasks[0];

                    int minActiveTasksCount = TaskQueueThreads[0].ActiveTasks.Count;
                    int indexOfThreadWithMinActiveTasks = 0;
                    for (int i = 1; i < TaskQueueThreads.Length; i++)
                    {
                        int count = TaskQueueThreads[i].ActiveTasks.Count;
                        if (count < minActiveTasksCount)
                        {
                            minActiveTasksCount = count;
                            indexOfThreadWithMinActiveTasks = i;
                        }
                    }
                    
                    TaskQueueThreads[indexOfThreadWithMinActiveTasks].AddTask(task);
                    Console.WriteLine("Added Task to Thread " + indexOfThreadWithMinActiveTasks);
                    _tasks.RemoveAt(0);
                }
                else
                {
                    Thread.Sleep(MainLoopDelay);
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
            foreach (TaskQueueThread thread in TaskQueueThreads)
            {
                thread.ForceStop();
            }
        }
    }
}