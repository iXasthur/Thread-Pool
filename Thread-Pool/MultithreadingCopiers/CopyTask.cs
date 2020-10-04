using System;
using System.IO;

namespace Thread_Pool.MultithreadingCopiers
{
    public class CopyTask
    {
        public enum CopyStatus
        {
            Waiting,
            Successful,
            Error
        }

        public readonly string Dest;
        public readonly string Src;

        public CopyTask(string src, string dest)
        {
            Src = src;
            Dest = dest;

            Status = CopyStatus.Waiting;
        }

        public CopyStatus Status { get; private set; }

        public void Perform()
        {
            try
            {
                File.Copy(Src, Dest, true);
                Console.WriteLine("Copied file from " + Src + " to " + Dest);
                Status = CopyStatus.Successful;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Status = CopyStatus.Error;
            }
        }
    }
}