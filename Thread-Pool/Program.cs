using System;
using System.Threading;

namespace Thread_Pool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var queue = new TaskQueue.TaskQueue(20);
            queue.EnqueueTask(PrintHello);
            queue.EnqueueTask(PrintHello);
            queue.EnqueueTask(PrintHello);
            queue.EnqueueTask(PrintHello);
        }

        private static void PrintHello()
        {
            Console.WriteLine("Hello form Main!");
        }
    }
}