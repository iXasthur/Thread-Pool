using System;
using System.Threading;
using Thread_Pool.MultithreadingCopiers;

namespace Thread_Pool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TaskQueue.TaskQueue taskQueue = new TaskQueue.TaskQueue(20);

            try
            {
                CatalogCopier copier = new CatalogCopier("srcCopy", "destCopy", taskQueue);
                copier.Perform();
                Console.WriteLine("Successfully completed copy operations.");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                Console.WriteLine("Destroying ThreadQueue.");
                taskQueue.ForceStop();
            }
        }
    }
}