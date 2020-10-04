using System.Collections.Generic;
using System.Threading;

namespace Thread_Pool.TaskQueue
{
    public class TaskQueueThread
    {
        public readonly List<TaskQueue.TaskDelegate> ActiveTasks = new List<TaskQueue.TaskDelegate>();
        
        private bool _isRunning = true;

        public void ThreadLoop()
        {
            var sw = new SpinWait();
            
            while (_isRunning)
            {
                if (ActiveTasks.Count > 0)
                {
                    var task = ActiveTasks[0];
                    task.Invoke();

                    // Removed from active when done
                    ActiveTasks.RemoveAt(0);
                }
                else
                {
                    sw.SpinOnce();
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