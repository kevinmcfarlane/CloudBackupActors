using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using CloudBackupActors.AkkaHelpers;
using CloudBackupActors.Messages;
using ZipDiff.Core;

namespace CloudBackupActors.Actors
{
    /// <summary>
    /// Manages zip operations on folders.
    /// </summary>
    public class ZipActor : ReceiveActor
    {
        private readonly ILoggingAdapter Logger = Context.GetLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipActor"/> class.
        /// </summary>
        public ZipActor()
        {
            Receive<ZipMessage>(
                handleIf => handleIf.ZipKind == ZipKind.VisualStudio,
                message => HandleVisualStudioZipMessage(message));

            Receive<ZipMessage>(
                handleIf => handleIf.ZipKind == ZipKind.Regular,
                message => HandleZipMessage(message));
        }

        protected override void PreStart()
        {
            //Console.WriteLine("ZipActor PreStart");
        }

        protected override void PostStop()
        {
            Context.Parent.Tell(new StopMessage());
        }

        protected override void PreRestart(Exception reason, object message)
        {
            Console.WriteLine("ZipActor PreRestart because: " + reason.Message);
            Logger.Info("ZipActor PreRestart because: " + reason.Message);
            base.PreRestart(reason, message);
        }

        protected override void PostRestart(Exception reason)
        {
            Console.WriteLine("ZipActor PostRestart because: " + reason.Message);
            Logger.Info("ZipActor PostRestart because: " + reason.Message);
            base.PostRestart(reason);
        }

        private void HandleVisualStudioZipMessage(ZipMessage message)
        {
            var path = message.SourceFolderPath;
            Logger.Info("Received: {0} for {1}", typeof(ZipMessage).Name, path);

            if (TryCreateVisualStudioZip(path))
            {
                string zipFilePath = GetVisualStudioZipFilePath(path);
                Context.ActorSelection(ActorPaths.BackupActor.Path).Tell(new BackupMessage(path, zipFilePath, ZipKind.VisualStudio));
            }
            else
            {
                string noChangesDetected = string.Format("No changes detected in {0}...", path);
                Context.ActorSelection(ActorPaths.BackupStatisticsActor.Path).Tell(noChangesDetected);
                Context.ActorSelection(ActorPaths.CloudBackupActor.Path).Tell(new IncrementFolderCountMessage(message.ZipKind));
            }
        }

        private void HandleZipMessage(ZipMessage message)
        {
            var path = message.SourceFolderPath;
            Logger.Info("Received: {0} for {1}", typeof(ZipMessage).Name, path);

            if (TryCreateZip(path))
            {
                Encrypt(path);
                string encryptedFilePath = GetEncryptedFilePath(path);
                Context.ActorSelection(ActorPaths.BackupActor.Path).Tell(new BackupMessage(path, encryptedFilePath, ZipKind.Regular));
            }
            else
            {
                string noChangesDetected = string.Format("No changes detected in {0}...", path);
                Context.ActorSelection(ActorPaths.BackupStatisticsActor.Path).Tell(noChangesDetected);
                Context.ActorSelection(ActorPaths.CloudBackupActor.Path).Tell(new IncrementFolderCountMessage(message.ZipKind), Self);
            }
        }

        /// <summary>
        /// Creates zip from Visual Studio project folder if there are changes.
        /// </summary>
        /// <param name="sourceFolderPath">The source folder path.</param>
        /// <returns>
        /// True if Visual Studio zip created; otherwise false.
        /// </returns>
        private bool TryCreateVisualStudioZip(string sourceFolderPath)
        {
            string zipFilePath = GetVisualStudioZipFilePath(sourceFolderPath);
            string previewZipFilePath = CreatePreviewZip(sourceFolderPath, zipFilePath);

            RemoveBinFolders(previewZipFilePath);

            bool result = TryCreateZip(zipFilePath, previewZipFilePath);

            return result;
        }

        /// <summary>
        /// Creates zip from folder if there are changes.
        /// </summary>
        /// <param name="sourceFolderPath">The source folder path.</param>
        /// <returns>
        /// True if zip created; otherwise false.
        /// </returns>
        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        private bool TryCreateZip(string sourceFolderPath)
        {
            if (!Directory.Exists(sourceFolderPath))
            {
                throw new DirectoryNotFoundException(sourceFolderPath + " does not exist.");
            }

            string zipFilePath = GetZipFilePath(sourceFolderPath);
            string previewZipFilePath = CreatePreviewZip(sourceFolderPath, zipFilePath);

            bool result = TryCreateZip(zipFilePath, previewZipFilePath);

            return result;
        }

        /// <summary>
        /// Creates a preview zip from the source folder path.
        /// </summary>
        /// <remarks>
        /// The preview zip will later be compared with the existing zip for differences.
        /// </remarks>
        /// <param name="sourceFolderPath">The source folder path.</param>
        /// <param name="zipFilePath">The original zip file path.</param>
        /// <returns>
        /// The preview zip path.
        /// </returns>
        private string CreatePreviewZip(string sourceFolderPath, string zipFilePath)
        {
            string previewZipFilePath = zipFilePath.Replace(".zip", ".preview.zip");

            if (File.Exists(previewZipFilePath))
            {
                File.Delete(previewZipFilePath);
            }

            ZipFile.CreateFromDirectory(sourceFolderPath, previewZipFilePath);
            Console.WriteLine("Created Preview Zip: " + previewZipFilePath);
            Logger.Info("Created Preview Zip: " + previewZipFilePath);

            return previewZipFilePath;
        }

        private void RemoveBinFolders(string zipFilePath)
        {
            using (ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Update))
            {
                var folders = archive.Entries.Where(e => e.FullName.Contains(@"\bin\") || e.FullName.Contains(@"\obj\")).ToList();
                folders.ForEach(e => e.Delete());
            }
        }

        private void Encrypt(string sourceFolderPath)
        {
            string zipFilePath = GetZipFilePath(sourceFolderPath);
            string encryptedFilePath = GetEncryptedFilePath(sourceFolderPath);

            Encrypt(zipFilePath, encryptedFilePath);
            Console.WriteLine("Encrypted zip: " + encryptedFilePath);
            Logger.Info("Encrypted zip: " + encryptedFilePath);
        }

        private void Encrypt(string zipFilePath, string encryptedFilePath)
        {
            // Encryption technique omitted
        }

        private Differences TryGetZipChanges(string zipFilePath, string previewZipFilePath)
        {
            var calculator = new DifferenceCalculator(zipFilePath, previewZipFilePath);
            var differences = calculator.GetDifferences();

            return differences;
        }

        private bool TryCreateZip(string zipFilePath, string previewZipFilePath)
        {
            bool result = false;

            if (File.Exists(zipFilePath))
            {
                var differences = TryGetZipChanges(zipFilePath, previewZipFilePath);

                if (ZipChanged(differences))
                {
                    Context.ActorSelection(ActorPaths.BackupStatisticsActor.Path).Tell(new FolderChangesMessage(zipFilePath, differences));

                    File.Delete(zipFilePath);
                    File.Move(previewZipFilePath, zipFilePath);
                    Console.WriteLine("Created Updated Zip: " + zipFilePath);
                    Logger.Info("Created Updated Zip: " + zipFilePath);

                    result = true;
                }
                else
                {
                    File.Delete(previewZipFilePath);
                    Console.WriteLine("Deleted Preview Zip: " + previewZipFilePath);
                    Logger.Info("Deleted Preview Zip: " + previewZipFilePath);
                }
            }
            else
            {
                File.Move(previewZipFilePath, zipFilePath);
                Console.WriteLine("Created New Zip: " + zipFilePath);
                Logger.Info("Created New Zip: " + zipFilePath);

                result = true;
            }

            return result;
        }

        private bool ZipChanged(Differences differences)
        {
            bool result =
                differences.Added.Any() ||
                differences.Changed.Any() ||
                differences.Removed.Any();

            return result;
        }

        private string GetVisualStudioZipFilePath(string sourceFolderPath)
        {
            string visualStudioPath = sourceFolderPath.Substring(0, sourceFolderPath.LastIndexOf("Projects") - 1);

            string zipFilePath = GetZipFilePath(visualStudioPath);

            return zipFilePath;
        }

        private string GetZipFilePath(string folderPath)
        {
            return folderPath + ".zip";
        }

        private string GetEncryptedFilePath(string folderPath)
        {
            return GetZipFilePath(folderPath) + ".enc";
        }
    }
}