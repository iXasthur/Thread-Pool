using System.Collections.Generic;
using System.Threading;

namespace Thread_Pool.TaskQueue
{
    public class TaskQueueThread
    {
        private readonly ManualResetEvent _mrse = new ManualResetEvent(false);
        public readonly List<TaskQueue.TaskDelegate> ActiveTasks = new List<TaskQueue.TaskDelegate>();
        
        private bool _isRunning = true;

        public void ThreadLoop()
        {
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
                    _mrse.Reset(); // Pause thread
                }

                _mrse.WaitOne();
            }
        }

        public void AddTask(TaskQueue.TaskDelegate task)
        {
            ActiveTasks.Add(task);
            _mrse.Set(); // Resume thread
        }

        public void ForceStop()
        {
            _isRunning = false;
            _mrse.Set(); // Resume thread
        }
    }
}