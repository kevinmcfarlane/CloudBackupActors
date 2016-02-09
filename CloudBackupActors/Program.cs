using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Akka.Actor;
using CloudBackupActors.Actors;
using CloudBackupActors.AkkaHelpers;
using CloudBackupActors.Messages;
using NLog;

namespace CloudBackupActors
{
    class Program
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        private static ActorSystem CloudBackupActorSystem;

        private static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string starting = LogMessageParts.Starting;
            Console.WriteLine(starting);
            Logger.Info(starting);
            Console.WriteLine(Environment.NewLine);

            CloudBackupActorSystem = ActorSystem.Create("CloudBackupActorSystem");
            var cloudBackupActor = CloudBackupActorSystem.ActorOf(Props.Create<CloudBackupActor>(), "CloudBackup");

            cloudBackupActor.Tell(new StartMessage());

            CloudBackupActorSystem.AwaitTermination();
            Console.WriteLine("Actor system shutdown...");

            stopwatch.Stop();
            string finished = string.Format(LogMessageParts.FinishedIn, (float)stopwatch.ElapsedMilliseconds / 1000);
            Console.WriteLine(finished);
            Logger.Info(finished);
        }

        #region Without Router - no longer used but kept for comparison
        private static void PreRouter()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Console.WriteLine("Starting...");
            Console.WriteLine(Environment.NewLine);

            var sourceFolderPathsFilePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "SourceFolderPaths.txt");
            var sourceFolderPaths = File.ReadAllLines(sourceFolderPathsFilePath);

            CloudBackupActorSystem = ActorSystem.Create("CloudBackupActorSystem");
            var cloudBackupActor = CloudBackupActorSystem.ActorOf(Props.Create(() => new CloudBackupActor(sourceFolderPaths.Count())), "CloudBackup");

            var sourceFolderPathsForPlainText = sourceFolderPaths.Where(p => p.Contains("Visual Studio"));
            BackupVisualStudioProjectsToOneDrive(sourceFolderPathsForPlainText);

            var sourceFolderPathsForEncryption = sourceFolderPaths.Except(sourceFolderPathsForPlainText);
            BackupEncryptedToOneDrive(sourceFolderPathsForEncryption);

            CloudBackupActorSystem.AwaitTermination();
            Console.WriteLine("Actor system shutdown...");

            stopwatch.Stop();
            Console.WriteLine(string.Format("Finished in {0}s.", (float)stopwatch.ElapsedMilliseconds / 1000));
        }

        private static void BackupVisualStudioProjectsToOneDrive(IEnumerable<string> sourceFolderPathsForPlainText)
        {
            Console.WriteLine("Visual Studio projects...");
            Console.WriteLine(Environment.NewLine);

            var zipActor = CloudBackupActorSystem.ActorSelection(ActorPaths.ZipActor.Path);

            foreach (var path in sourceFolderPathsForPlainText)
            {
                Console.WriteLine(string.Format("Processing {0}...", path));
                zipActor.Tell(new ZipMessage(path, ZipKind.VisualStudio));
            }

            Console.WriteLine(Environment.NewLine);
        }

        private static void BackupEncryptedToOneDrive(IEnumerable<string> sourceFolderPathsForEncryption)
        {
            Console.WriteLine("Folders for encryption...");
            Console.WriteLine(Environment.NewLine);

            var zipActor = CloudBackupActorSystem.ActorSelection(ActorPaths.ZipActor.Path);

            foreach (var path in sourceFolderPathsForEncryption)
            {
                Console.WriteLine(string.Format("Processing {0}...", path));
                zipActor.Tell(new ZipMessage(path, ZipKind.Regular));
            }

            Console.WriteLine(Environment.NewLine);
        }

        #endregion
    }
}