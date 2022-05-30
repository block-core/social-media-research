﻿using System;
using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Threading;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;


namespace FileBaseDirectoryWatcher_01
{
    public class FileWatcher
    {
        private const string filePath = @"*** file path ***";
        static DateTimeOffset lastChanged = DateTimeOffset.UtcNow;
        static string lastChangedFile = null;
        static FileSystemWatcher watcher;

        private readonly IConfiguration configuration;

        public FileWatcher(IConfiguration configuration)
        {
            this.configuration = configuration;

            watcher = new FileSystemWatcher(@"d:/social-media");

            watcher.NotifyFilter = NotifyFilters.Attributes
                                         | NotifyFilters.CreationTime
                                         | NotifyFilters.DirectoryName
                                         | NotifyFilters.FileName
                                         | NotifyFilters.LastAccess
                                         | NotifyFilters.LastWrite
                                         | NotifyFilters.Security
                                         | NotifyFilters.Size;

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;

            watcher.Filter = "*.txt";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }


        #region FileWatcher Events
        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed ||
                (lastChanged.AddMilliseconds(1000) > DateTimeOffset.UtcNow && lastChangedFile == e.FullPath)
               )
            {
                return;
            }
            lastChanged = DateTimeOffset.UtcNow;
            lastChangedFile = e.FullPath;
            Console.WriteLine($"Changed: {e.FullPath}");

        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            Console.WriteLine(value);
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e) =>
            Console.WriteLine($"Deleted: {e.FullPath}");

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            string path = Path.GetDirectoryName(e.FullPath);
            string oldFileName = Path.GetFileName(e.OldName);
            string newFileName = Path.GetFileName(e.Name);

            Console.WriteLine($"Renamed:");
            Console.WriteLine($"    Path: {path.Replace('\\', '/')}");
            Console.WriteLine($"    Old File Name: {oldFileName}");
            Console.WriteLine($"    New File Name: {newFileName}");
        }

        private static void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private static void PrintException(Exception? ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
            }
        }
        #endregion



    }
}
