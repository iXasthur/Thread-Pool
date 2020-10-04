using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Thread_Pool.MultithreadingCopiers
{
    public class CatalogCopier
    {
        private readonly List<CopyTask> _copyTasks = new List<CopyTask>();
        private readonly TaskQueue.TaskQueue _taskQueue;

        public CatalogCopier(string src, string dest, TaskQueue.TaskQueue taskQueue)
        {
            _taskQueue = taskQueue;

            var srcAttr = File.GetAttributes(src);
            var destAttr = File.GetAttributes(dest);

            if (!srcAttr.HasFlag(FileAttributes.Directory))
                throw new Exception("'" + Path.GetFullPath(src) + "' is not directory.");

            if (!destAttr.HasFlag(FileAttributes.Directory))
                throw new Exception("'" + Path.GetFullPath(src) + "' is not directory.");

            CreateCopyTasks(src, dest);
        }

        private void CreateCopyTasks(string src, string dest)
        {
            var srcDirFiles = Directory.GetFiles(src);
            foreach (var filePath in srcDirFiles)
            {
                var fileAbsolutePath = Path.GetFullPath(filePath);
                var destinationPath = Path.GetFullPath(dest) + @"\" + Path.GetFileName(filePath);

                var task = new CopyTask(fileAbsolutePath, destinationPath);
                _copyTasks.Add(task);
            }

            var srcDirDirs = Directory.GetDirectories(src);
            foreach (var dirPath in srcDirDirs)
            {
                var dirAbsolutePath = Path.GetFullPath(dirPath);
                var destinationPath = Path.GetFullPath(dest) + @"\" + Path.GetFileName(dirPath);

                if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);

                CreateCopyTasks(dirAbsolutePath, destinationPath);
            }
        }

        public void Perform()
        {
            foreach (var task in _copyTasks) _taskQueue.EnqueueTask(task.Perform);

            // Wait for every task to complete
            var sw = new SpinWait();
            while (!AllTasksCompleted()) sw.SpinOnce();
        }

        private bool AllTasksCompleted()
        {
            foreach (var task in _copyTasks)
                if (task.Finished == false)
                    return false;
            return true;
        }
    }
}