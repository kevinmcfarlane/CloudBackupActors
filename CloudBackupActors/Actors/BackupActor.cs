using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Akka.Actor;
using Akka.Event;
using CloudBackupActors.Messages;

namespace CloudBackupActors.Actors
{
    /// <summary>
    /// Backs up zip archives and log files to cloud storage.
    /// </summary>
    public class BackupActor : ReceiveActor
    {
        private readonly ILoggingAdapter Logger = Context.GetLogger();
        private const string BackupFolderPath = @"C:\Users\Kevin\SkyDrive\My Documents";

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupActor"/> class.
        /// </summary>
        public BackupActor()
        {
            Receive<BackupMessage>(
                handleIf => handleIf.ZipKind == ZipKind.VisualStudio,
                message => HandleVisualStudioZipFile(message));

            Receive<BackupMessage>(
                handleIf => handleIf.ZipKind == ZipKind.Regular,
                message => HandleZipFile(message));

            Receive<BackupLogFilesMessage>(
                message => HandleLogFiles());
        }

        protected override void PostStop()
        {
            Context.Parent.Tell(new StopMessage());
        }

        private void HandleVisualStudioZipFile(BackupMessage message)
        {
            string sourceFolderPath = message.SourceFolderPath;
            Logger.Info("Received: {0} for {1}", typeof(BackupMessage).Name, sourceFolderPath);

            string backupFilePath = Path.Combine(BackupFolderPath, Path.GetFileName(message.ZipFilePath));
            BackupZipFile(message, backupFilePath);

            Context.Parent.Tell(new IncrementFolderCountMessage(message.ZipKind));
        }

        private void HandleZipFile(BackupMessage message)
        {
            string sourceFolderPath = message.SourceFolderPath;
            Logger.Info("Received: {0} for {1}", typeof(BackupMessage).Name, sourceFolderPath);

            string backupFilePath = Path.Combine(BackupFolderPath, GetFolderName(sourceFolderPath) + ".zip.encrypted");
            BackupZipFile(message, backupFilePath);

            Context.Parent.Tell(new IncrementFolderCountMessage(message.ZipKind));
        }

        private void HandleLogFiles()
        {
            Logger.Info("Received: {0}", typeof(BackupLogFilesMessage).Name);
            var logFileNames = new List<string> { "logfile.txt", "logfile1.txt" };
            Logger.Info("Backing up log files...");
            logFileNames.ForEach(logFileName => Logger.Info(logFileName));

            Thread.Sleep(500);

            BackupLogFiles(logFileNames);

            Context.Parent.Tell(new StopMessage());
        }

        /// <summary>
        /// Backs up zip file to cloud storage.
        /// </summary>
        /// <param name="message">The backup message.</param>
        /// <param name="backupFilePath">The backup (cloud storage) file path.</param>
        private void BackupZipFile(BackupMessage message, string backupFilePath)
        {
            File.Copy(message.ZipFilePath, backupFilePath, overwrite: true);
            Console.WriteLine("Backed up to: " + backupFilePath);
            Logger.Info("Backed up to: " + backupFilePath);
        }

        /// <summary>
        /// Backs up log files to cloud storage.
        /// </summary>
        /// <param name="logFileNames">The log file names.</param>
        private void BackupLogFiles(List<string> logFileNames)
        {
            foreach (var logFileName in logFileNames)
            {
                string logFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), logFileName);
                string backupFilePath = Path.Combine(BackupFolderPath, logFileName);

                File.Copy(logFilePath, backupFilePath, overwrite: true);
                Console.WriteLine(string.Format("Backed up {0} to: {1}.", logFileName, backupFilePath));
            }
        }

        private string GetFolderName(string folderPath)
        {
            return Path.GetFileName(folderPath);
        }
    }
}