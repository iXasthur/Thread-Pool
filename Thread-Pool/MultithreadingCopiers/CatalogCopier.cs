﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Thread_Pool.MultithreadingCopiers
{
    public class CatalogCopier
    {
        private readonly List<CopyTask> _copyTasks = new List<CopyTask>();
        private readonly List<string> _missingDirectoriesPaths = new List<string>();
        private readonly TaskQueue.TaskQueue _taskQueue;

        public CatalogCopier(string src, string dest, TaskQueue.TaskQueue taskQueue)
        {
            _taskQueue = taskQueue;

            var srcAttr = File.GetAttributes(src);
            if (!srcAttr.HasFlag(FileAttributes.Directory))
                throw new Exception("'" + Path.GetFullPath(src) + "' is not directory.");

            if (File.Exists(dest))
            {
                var destAttr = File.GetAttributes(dest);
                if (!destAttr.HasFlag(FileAttributes.Directory))
                    throw new Exception("'" + Path.GetFullPath(src) + "' is not directory.");
            }
            else
            {
                _missingDirectoriesPaths.Add(dest);
            }

            CreateCopyTasks(src, dest);
        }

        public int TasksCount => _copyTasks.Count;
        public int CompletedTasksCount => _copyTasks.Count(task => task.Status != CopyTask.CopyStatus.Waiting);
        public int SuccessfulTasksCount => _copyTasks.Count(task => task.Status == CopyTask.CopyStatus.Successful);
        public int ErrorTasksCount => _copyTasks.Count(task => task.Status == CopyTask.CopyStatus.Error);

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

                if (!Directory.Exists(destinationPath)) _missingDirectoriesPaths.Add(destinationPath);
                ;

                CreateCopyTasks(dirAbsolutePath, destinationPath);
            }
        }

        public void Perform()
        {
            foreach (var dirPath in _missingDirectoriesPaths) Directory.CreateDirectory(dirPath);
            foreach (var task in _copyTasks) _taskQueue.EnqueueTask(task.Perform);

            // Wait for every task to complete
            var sw = new SpinWait();
            while (!AllTasksCompleted()) sw.SpinOnce();
        }

        private bool AllTasksCompleted()
        {
            foreach (var task in _copyTasks)
                if (task.Status == CopyTask.CopyStatus.Waiting)
                    return false;
            return true;
        }
    }
}