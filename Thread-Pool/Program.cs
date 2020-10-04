using System;
using Thread_Pool.MultithreadingCopiers;

namespace Thread_Pool
{
    internal class Program
    {

        private const string InvalidArgsStr = "Invalid arguments.\nArgs: sourceCatalog destinationCatalog threadCount";
        
        private static void Main(string[] args)
        {
            if (!(args.Length == 3 && int.TryParse(args[2], out var threadCount)))
            {
                Console.WriteLine(InvalidArgsStr);
                return;
            }

            TaskQueue.TaskQueue taskQueue;
            
            try
            {
                taskQueue = new TaskQueue.TaskQueue(threadCount);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                return;
            }
            
            try
            {
                var copier = new CatalogCopier(args[0], args[1], taskQueue);
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
                // taskQueue.ForceStop();
            }
        }
    }
}