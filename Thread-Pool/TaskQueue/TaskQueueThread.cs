using System.Collections.Generic;
using System.Threading;

namespace Thread_Pool.TaskQueue
{
    public class TaskQueueThread
    {
        private const int LoopDelay = 500;
        
        private bool _isRunning = true;
        public readonly List<TaskQueue.TaskDelegate> ActiveTasks = new List<TaskQueue.TaskDelegate>();

        public void ThreadLoop()
        {
            while (_isRunning)
            {
                if (ActiveTasks.Count > 0)
                {
                    TaskQueue.TaskDelegate task = ActiveTasks[0];
                    task.Invoke();
                    
                    // Removed from active when done
                    ActiveTasks.RemoveAt(0);
                }
                else
                {
                    Thread.Sleep(LoopDelay);
                }
            }
        }

        public void AddTask(TaskQueue.TaskDelegate task)
        {
            ActiveTasks.Add(task);
        }

        public void ForceStop()
        {
            _isRunning = false;
        }
    }
}