using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Akka.Actor;
using Akka.Event;
using Akka.Routing;
using CloudBackupActors.Messages;

namespace CloudBackupActors.Actors
{
    /// <summary>
    /// Manages cloud backup from start to finish.
    /// </summary>
    public class CloudBackupActor : ReceiveActor
    {
        private int _numberOfFolders;
        private int _numberOfFoldersProcessed;
        private IActorRef _zipActor;
        private IActorRef _backupActor;
        private IEnumerable<string> _sourceFolderPaths;
        private readonly ILoggingAdapter Logger = Context.GetLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudBackupActor"/> class.
        /// </summary>
        public CloudBackupActor()
        {
            GetSourceFolderPaths();

            CreateZipActorPool();
            CreateChildActors();

            Receive<StartMessage>(message =>
            {
                Logger.Info(LogMessageParts.ReceivedStart);
                Start();
                Sender.Tell(new StartedMessage());
            });

            Receive<IncrementFolderCountMessage>(message =>
            {
                Logger.Info(LogMessageParts.ReceivedIncrementFolderCount + Sender.Path);
                IncrementFolderCount();
                Sender.Tell(new FolderCountIncrementedMessage(message.ZipKind));
            });

            Receive<StopMessage>(message =>
            {
                Logger.Info(LogMessageParts.ReceivedStop);
                Stop();
            });
        }

        protected override void PreRestart(Exception reason, object message)
        {
            Console.WriteLine("CloudBackupActor PreRestart because: " + reason.Message);
            Logger.Info("CloudBackupActor PreRestart because: " + reason.Message);
            base.PreRestart(reason, message);
        }

        protected override void PostRestart(Exception reason)
        {
            Console.WriteLine("CloudBackupActor PostRestart because: " + reason.Message);
            Logger.Info("CloudBackupActor PostRestart because: " + reason.Message);
            base.PostRestart(reason);
        }

        private void GetSourceFolderPaths()
        {
            var sourceFolderPathsFilePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "SourceFolderPaths.txt");
            _sourceFolderPaths = File.ReadAllLines(sourceFolderPathsFilePath);
            _numberOfFolders = _sourceFolderPaths.Count();

            bool finished = _numberOfFolders == 0;

            BackupLogFilesIfFinished(finished);
        }

        /// <summary>
        /// Creates a ZipActor pool.
        /// </summary>
        /// <remarks>
        /// We use a pool to avoid a build up of messages in the <see cref="ZipActor"/>, zip being a long-running operation.
        /// </remarks>
        private void CreateZipActorPool()
        {
            SupervisorStrategy strategy = new OneForOneStrategy(exception =>
            {
                if (exception is IOException)
                {
                    Logger.Warning(LogMessageParts.SkippingFolder + exception.Message);

                    IncrementFolderCount();

                    return Directive.Resume;
                }

                return Directive.Restart;
            });

            _zipActor = Context
                .ActorOf(Props.Create<ZipActor>()
                .WithRouter((new RoundRobinPool(_numberOfFolders))
                .WithSupervisorStrategy(strategy)), "Zip");
        }

        private void CreateChildActors()
        {
            Context.ActorOf(Props.Create<BackupStatisticsActor>(), "BackupStatistics");
            _backupActor = Context.ActorOf(Props.Create<BackupActor>(), "Backup");
        }

        private void Start()
        {
            var sourceFolderPathsForVisualStudio = _sourceFolderPaths.Where(p => p.Contains("Visual Studio"));
            SendZipMessagesForVisualStudioProjects(sourceFolderPathsForVisualStudio);

            var sourceFolderPathsForEncryption = _sourceFolderPaths.Except(sourceFolderPathsForVisualStudio);
            SendZipMessagesForFolders(sourceFolderPathsForEncryption);
        }

        private void IncrementFolderCount()
        {
            _numberOfFoldersProcessed++;

            bool finished = _numberOfFoldersProcessed == _numberOfFolders;

            BackupLogFilesIfFinished(finished);
        }

        private void BackupLogFilesIfFinished(bool finished)
        {
            if (finished)
            {
                Console.WriteLine(LogMessageParts.FinishedProcessing, _numberOfFolders);
                Logger.Info(LogMessageParts.FinishedProcessing, _numberOfFolders);

                Thread.Sleep(500);

                _backupActor.Tell(new BackupLogFilesMessage());
            }
        }

        private void Stop()
        {
            Context.System.Shutdown();
        }

        /// <summary>
        /// Sends zip messages for Visual Studio project folders.
        /// </summary>
        /// <param name="sourceFolderPaths">The source folder paths.</param>
        private void SendZipMessagesForVisualStudioProjects(IEnumerable<string> sourceFolderPaths)
        {
            if (sourceFolderPaths.Any())
            {
                Console.WriteLine(LogMessageParts.VisualStudioProjects);
                Logger.Info(LogMessageParts.VisualStudioProjects);
                Console.WriteLine(Environment.NewLine);

                SendZipMessages(sourceFolderPaths, ZipKind.VisualStudio);

                Console.WriteLine(Environment.NewLine);
            }
        }

        /// <summary>
        /// Sends zip messages for folders (excluding Visual Studio ones).
        /// </summary>
        /// <param name="sourceFolderPaths">The source folder paths.</param>
        private void SendZipMessagesForFolders(IEnumerable<string> sourceFolderPaths)
        {
            if (sourceFolderPaths.Any())
            {
                Console.WriteLine(LogMessageParts.FoldersForEncryption);
                Logger.Info(LogMessageParts.FoldersForEncryption);
                Console.WriteLine(Environment.NewLine);

                SendZipMessages(sourceFolderPaths, ZipKind.Regular);

                Console.WriteLine(Environment.NewLine);
            }
        }

        private void SendZipMessages(IEnumerable<string> sourceFolderPaths, ZipKind zipKind)
        {
            foreach (var path in sourceFolderPaths)
            {
                Console.WriteLine(LogMessageParts.Processing, path);
                Logger.Info(LogMessageParts.Processing, path);
                //_zipActor.Tell(new ZipMessage(path, zipKind));
                _zipActor.Tell(new ZipMessage(path, zipKind), Self);
            }
        }
    }
}