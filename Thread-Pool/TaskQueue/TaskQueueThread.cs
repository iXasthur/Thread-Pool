using System.Collections.Generic;
using System.Threading;

namespace Thread_Pool.TaskQueue
{
    public class TaskQueueThread
    {
        private readonly List<TaskQueue.TaskDelegate> _activeTasks = new List<TaskQueue.TaskDelegate>();

        private bool _isRunning = true;

        public int ActiveTasksCount => _activeTasks.Count;

        public void ThreadLoop()
        {
            var sw = new SpinWait();

            while (_isRunning)
                if (_activeTasks.Count > 0)
                {
                    var task = _activeTasks[0];
                    task.Invoke();

                    // Removed from active when done
                    _activeTasks.RemoveAt(0);
                }
                else
                {
                    sw.SpinOnce();
                }
        }

        public void AddTask(TaskQueue.TaskDelegate task)
        {
            _activeTasks.Add(task);
        }

        public void ForceStop()
        {
            _isRunning = false;
        }
    }
}