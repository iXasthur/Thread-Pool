using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Thread_Pool.MultithreadingCopiers
{
    public class CatalogCopier
    {

        private List<CopyTask> _copyTasks = new List<CopyTask>();
        private readonly TaskQueue.TaskQueue _taskQueue;
        
        public CatalogCopier(string src, string dest, TaskQueue.TaskQueue taskQueue)
        {
            _taskQueue = taskQueue;
            
            FileAttributes srcAttr = File.GetAttributes(src);
            FileAttributes destAttr = File.GetAttributes(dest);

            if (!srcAttr.HasFlag(FileAttributes.Directory))
            {
                throw new Exception("'" + Path.GetFullPath(src) + "' is not directory.");
            }
            
            if (!destAttr.HasFlag(FileAttributes.Directory))
            {
                throw new Exception("'" + Path.GetFullPath(src) + "' is not directory.");
            }
            
            CreateCopyTasks(src, dest);
        }

        private void CreateCopyTasks(string src, string dest)
        {
            string[] srcDirFiles = Directory.GetFiles(src);
            foreach (string filePath in srcDirFiles)
            {
                string fileAbsolutePath = Path.GetFullPath(filePath);
                string destinationPath = Path.GetFullPath(dest) + @"\" + Path.GetFileName(filePath);
                
                CopyTask task = new CopyTask(fileAbsolutePath, destinationPath);
                _copyTasks.Add(task);
            }

            string[] srcDirDirs = Directory.GetDirectories(src);
            foreach (string dirPath in srcDirDirs)
            {
                string dirAbsolutePath = Path.GetFullPath(dirPath);
                string destinationPath = Path.GetFullPath(dest) + @"\" + Path.GetFileName(dirPath);

                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }
                
                CreateCopyTasks(dirAbsolutePath, destinationPath);
            }
        }

        public void Perform()
        {
            foreach (CopyTask task in _copyTasks)
            {
                _taskQueue.EnqueueTask(task.Perform);
            }

            // Wait for every task to complete
            SpinWait sw = new SpinWait();
            while (!AllTasksCompleted())
            {
                sw.SpinOnce();
            }
        }

        public bool AllTasksCompleted()
        {
            foreach (CopyTask task in _copyTasks)
            {
                if (task.Finished == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
}