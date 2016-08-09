using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.Routing;
using CloudBackupActors.Messages;

using Debug = System.Diagnostics.Debug;

namespace CloudBackupActors.Actors
{
    /// <summary>
    /// Manages cloud backup from start to finish.
    /// </summary>
    public class CloudBackupActor : ReceiveActor
    {
        private readonly IFileSystem _fileSystem;
        private readonly ILoggingAdapter Logger = Context.GetLogger();
        private int _numberOfFolders;
        private int _numberOfFoldersProcessed;
        private IActorRef _zipActor;
        private IActorRef _backupActor;
        private List<string> _sourceFolderPaths = new List<string>();

        /// <summary>
        /// Gets the collection of source folder paths to be zipped.
        /// </summary>
        /// <value>
        /// The source folder paths.
        /// </value>
        public ReadOnlyCollection<string> SourceFolderPaths
        {
            get
            {
                return _sourceFolderPaths.AsReadOnly();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudBackupActor"/> class.
        /// </summary>
        /// <param name="sourceFolderPathsFilePath">The source folder paths file path.</param>
        public CloudBackupActor(string sourceFolderPathsFilePath)
            : this(
                sourceFolderPathsFilePath,
                // Default implementation which calls System.IO
                fileSystem: new FileSystem() 
            )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudBackupActor"/> class.
        /// </summary>
        /// <remarks>Use for unit testing of source folders input.</remarks>
        /// <param name="sourceFolderPathsFilePath">The source folder paths file path.</param>
        /// <param name="fileSystem">The file system.</param>
        public CloudBackupActor(string sourceFolderPathsFilePath, IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;

            ReadAllSourceFolderPaths(sourceFolderPathsFilePath);

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

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries: 0,
                withinTimeMilliseconds: 0,
                decider: Decider.From(exception =>
                {
                    Logger.Error("{0}{1} - {2}", LogMessageParts.ApplicationTerminating, Sender.Path.Name, exception.Message);

                    return Directive.Stop;
                }),
                loggingEnabled: false);
        }

        /// <summary>
        /// Reads all source folder paths, removing any empty entries before storing.
        /// </summary>
        /// <param name="sourceFolderPathsFilePath">The source folder paths file path.</param>
        private void ReadAllSourceFolderPaths(string sourceFolderPathsFilePath)
        {
            _sourceFolderPaths = _fileSystem.File.ReadAllLines(sourceFolderPathsFilePath).ToList();
            _sourceFolderPaths.RemoveAll(path => string.IsNullOrWhiteSpace(path));
            _numberOfFolders = _sourceFolderPaths.Count;

            bool nothingToDo = !_sourceFolderPaths.Any();

            if (nothingToDo)
            {
                Self.Tell(new StopMessage());
            }
        }

        /// <summary>
        /// Creates a ZipActor pool.
        /// </summary>
        /// <remarks>
        /// We use a pool to avoid a build up of messages in the <see cref="ZipActor"/>, zip being a long-running operation.
        /// </remarks>
        private void CreateZipActorPool()
        {
            SupervisorStrategy strategy = new OneForOneStrategy(
                maxNrOfRetries: 0,
                withinTimeMilliseconds: 0,
                decider: Decider.From(exception =>
                {
                    if (exception is IOException)
                    {
                        Logger.Warning("{0}{1} - {2}", LogMessageParts.SkippingFolder, Sender.Path.Name, exception.Message);

                        IncrementFolderCount();

                        return Directive.Resume;
                    }
                    else
                    {
                        Logger.Error("{0}{1} - {2}", LogMessageParts.ApplicationTerminating, Sender.Path.Name, exception.Message);

                        return Directive.Stop;
                    }
                }),
                loggingEnabled: false);

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

            if (finished)
            {
                BackupLogFiles();
            }
        }

        private void BackupLogFiles()
        {
            Console.WriteLine(LogMessageParts.FinishedProcessing, _numberOfFolders);
            Logger.Info(LogMessageParts.FinishedProcessing, _numberOfFolders);

            Debug.Assert(_backupActor != null, "BackupActor should have been created.");
            _backupActor.Tell(new BackupLogFilesMessage());
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
                _zipActor.Tell(new ZipMessage(path, zipKind), Self);
                //_zipActor.Tell(new ZipMessage(path, zipKind));
            }
        }
    }
}