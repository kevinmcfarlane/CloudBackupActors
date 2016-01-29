using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using CloudBackupActors.Messages;

namespace CloudBackupActors.Actors
{
    /// <summary>
    /// Backs up zip archive to cloud storage.
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
                message => HandleVisualStudioZipMessage(message));

            Receive<BackupMessage>(
                handleIf => handleIf.ZipKind == ZipKind.Regular,
                message => HandleZipMessage(message));
        }

        private void HandleVisualStudioZipMessage(BackupMessage message)
        {
            string sourceFolderPath = message.SourceFolderPath;
            Logger.Info("Received: {0} for {1}", typeof(BackupMessage).Name, sourceFolderPath);

            string backupFilePath = Path.Combine(BackupFolderPath, Path.GetFileName(message.ZipFilePath));

            Backup(message, backupFilePath);
            Context.Parent.Tell(new IncrementFolderCountMessage(message.ZipKind));
        }

        private void HandleZipMessage(BackupMessage message)
        {
            string sourceFolderPath = message.SourceFolderPath;
            Logger.Info("Received: {0} for {1}", typeof(BackupMessage).Name, sourceFolderPath);
            string backupFilePath = Path.Combine(BackupFolderPath, GetFolderName(sourceFolderPath) + ".zip.encrypted");

            File.Copy(message.ZipFilePath, backupFilePath, overwrite: true);
            Console.WriteLine("Backed up to: " + backupFilePath);
            Logger.Info("Backed up to: " + backupFilePath);

            Context.Parent.Tell(new IncrementFolderCountMessage(message.ZipKind));
        }

        /// <summary>
        /// Backs up zip file to cloud storage.
        /// </summary>
        /// <param name="message">The backup message.</param>
        /// <param name="backupFilePath">The backup (cloud storage) file path.</param>
        private void Backup(BackupMessage message, string backupFilePath)
        {
            File.Copy(message.ZipFilePath, backupFilePath, overwrite: true);
            Console.WriteLine("Backed up to: " + backupFilePath);
            Logger.Info("Backed up to: " + backupFilePath);
        }

        private string GetFolderName(string folderPath)
        {
            return Path.GetFileName(folderPath);
        }
    }
}
