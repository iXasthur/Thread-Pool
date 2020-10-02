using System;
using System.IO;

namespace Thread_Pool.MultithreadingCopiers
{
    public class CatalogCopier
    {

        private CopyTask[] _copyTasks;
        private readonly TaskQueue.TaskQueue _taskQueue;
        
        public CatalogCopier(string src, string dest, TaskQueue.TaskQueue taskQueue)
        {
            _taskQueue = taskQueue;
            
            FileAttributes srcAttr = File.GetAttributes(src);
            FileAttributes destAttr = File.GetAttributes(dest);

            if (!srcAttr.HasFlag(FileAttributes.Directory))
            {
                throw new Exception("'" + Path.GetFullPath(src) + "' is not directory");
            }
            
            if (!destAttr.HasFlag(FileAttributes.Directory))
            {
                throw new Exception("'" + Path.GetFullPath(src) + "' is not directory");
            }
            
            CreateCopyTasks(src, dest);
        }

        private void CreateCopyTasks(string src, string dest)
        {
            string[] srcDirFiles = Directory.GetFiles(src);
            _copyTasks = new CopyTask[srcDirFiles.Length];
            
            for (var i = 0; i < srcDirFiles.Length; i++)
            {
                string fileAbsolutePath = Path.GetFullPath(srcDirFiles[i]);
                string destinationPath = Path.GetFullPath(dest) + @"\" + Path.GetFileName(srcDirFiles[i]);
                
                CopyTask task = new CopyTask(fileAbsolutePath, destinationPath);
                _copyTasks[i] = task;
            }
        }

        public void Perform()
        {
            foreach (CopyTask task in _copyTasks)
            {
                _taskQueue.EnqueueTask(task.Perform);
            }
        }

    }
}