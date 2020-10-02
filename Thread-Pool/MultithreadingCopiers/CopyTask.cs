using System;
using System.IO;

namespace Thread_Pool.MultithreadingCopiers
{
    public class CopyTask
    {
        public readonly string Src;
        public readonly string Dest;

        public bool Finished { get; private set; } = false;

        public CopyTask(string src, string dest)
        {
            Src = src;
            Dest = dest;
        }
        
        public void Perform()
        {
            try
            {
                File.Copy(Src, Dest, true);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            finally
            {
                Console.WriteLine("Copied file from " + Src + " to " + Dest);
                Finished = true;
            }
        }
    }
}