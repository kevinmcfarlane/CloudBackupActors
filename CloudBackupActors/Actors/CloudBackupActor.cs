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
                Logger.Info("Received: StartMessage");
                Start();
            });

            Receive<IncrementFolderCountMessage>(message =>
            {
                Logger.Info("Received: IncrementFolderCountMessage");
                IncrementFolderCount();
            });
        }

        #region Used in non-router version
        /// <summary>
        /// Initializes a new instance of the <see cref="CloudBackupActor"/> class.
        /// </summary>
        /// <param name="numberOfFolders"></param>
        public CloudBackupActor(int numberOfFolders)
        {
            _numberOfFolders = numberOfFolders;

            Context.ActorOf(Props.Create<BackupStatisticsActor>(), "BackupStatistics");
            Context.ActorOf(Props.Create<ZipActor>(), "Zip");
            Context.ActorOf(Props.Create<BackupActor>(), "Backup");

            Receive<IncrementFolderCountMessage>(message =>
            {
                Console.WriteLine("Received: IncrementFolderCountMessage");

                _numberOfFoldersProcessed++;

                if (_numberOfFoldersProcessed == _numberOfFolders)
                {
                    Console.WriteLine(string.Format("Finished processing {0} source folders, shutting down actor system...", _numberOfFolders));
                    Context.System.Shutdown();
                }
            });
        }

        #endregion

        protected override void PreStart()
        {
            //Console.WriteLine("CloudBackupActor PreStart");
        }

        protected override void PostStop()
        {
            //Console.WriteLine("CloudBackupActor PostStop");
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

            StopIfFinished(finished);
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
                    Logger.Warning("Skipping folder... " + exception.Message);

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
            Context.ActorOf(Props.Create<BackupActor>(), "Backup");
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

            StopIfFinished(finished);
        }

        private void Stop()
        {
            Console.WriteLine("Finished processing {0} source folders, shutting down actor system...", _numberOfFolders);
            Logger.Info("Finished processing {0} source folders, shutting down actor system...", _numberOfFolders);

            Thread.Sleep(500);

            Context.System.Shutdown();
        }

        private void StopIfFinished(bool finished)
        {
            if (finished)
            {
                Stop();
            }
        }

        /// <summary>
        /// Sends zip messages for Visual Studio project folders.
        /// </summary>
        /// <param name="sourceFolderPaths">The source folder paths.</param>
        private void SendZipMessagesForVisualStudioProjects(IEnumerable<string> sourceFolderPaths)
        {
            if (sourceFolderPaths.Any())
            {
                Console.WriteLine("Visual Studio projects...");
                Logger.Info("Visual Studio projects...");
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
                Console.WriteLine("Folders for encryption...");
                Logger.Info("Folders for encryption...");
                Console.WriteLine(Environment.NewLine);

                SendZipMessages(sourceFolderPaths, ZipKind.Regular);

                Console.WriteLine(Environment.NewLine);
            }
        }

        private void SendZipMessages(IEnumerable<string> sourceFolderPaths, ZipKind zipKind)
        {
            foreach (var path in sourceFolderPaths)
            {
                Console.WriteLine("Processing {0}...", path);
                Logger.Info("Processing {0}...", path);
                _zipActor.Tell(new ZipMessage(path, zipKind));
            }
        }
    }
}
