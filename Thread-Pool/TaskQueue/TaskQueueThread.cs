using System.Collections.Concurrent;
using System.Threading;

namespace Thread_Pool.TaskQueue
{
    public class TaskQueueThread
    {
        // Can be modified from multiple threads
        private readonly ConcurrentQueue<TaskQueue.TaskDelegate> _queuedTasks =
            new ConcurrentQueue<TaskQueue.TaskDelegate>();

        private bool _isRunning = true;

        public int QueuedTasksCount => _queuedTasks.Count;

        public void ThreadLoop()
        {
            var sw = new SpinWait();

            while (_isRunning)
                if (_queuedTasks.TryDequeue(out var task))
                    task.Invoke();
                else
                    sw.SpinOnce();
        }

        public void AddTask(TaskQueue.TaskDelegate task)
        {
            _queuedTasks.Enqueue(task);
        }

        public void ForceStop()
        {
            _isRunning = false;
        }
    }
}