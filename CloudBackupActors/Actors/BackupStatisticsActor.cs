using System;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using CloudBackupActors.Messages;
using ZipDiff.Core;

namespace CloudBackupActors.Actors
{
    /// <summary>
    /// Logs statistics showing changes to folders.
    /// </summary>
    public class BackupStatisticsActor : ReceiveActor
    {
        private readonly ILoggingAdapter Logger = Context.GetLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupStatisticsActor"/> class.
        /// </summary>
        public BackupStatisticsActor()
        {
            Receive<FolderChangesMessage>(message =>
            {
                Logger.Info("Received: {0} for {1}", typeof(FolderChangesMessage).Name, message.ZipFilePath);
                LogChanges(message.ZipFilePath, message.Differences);
            });

            Receive<string>(message =>
            {
                Logger.Info("Received: {0}", message);
                Console.WriteLine(message);
                Logger.Info(message);
            });
        }

        private void LogChanges(string zipFilePath, Differences differences)
        {
            Console.WriteLine("In {0}...", zipFilePath);
            Logger.Info("In {0}...", zipFilePath);

            var added = differences.Added;
            var changed = differences.Changed;
            var removed = differences.Removed;

            int numberOfDifferences = added.Count + changed.Count + removed.Count;

            if (numberOfDifferences > 0)
            {
                Console.WriteLine("Number of differences = {0}.", numberOfDifferences);
                Logger.Info("Number of differences = {0}.", numberOfDifferences);
            }

            if (added.Any())
            {
                Console.WriteLine("Added...");
                Logger.Info("Added...");
                added.Keys.ToList().ForEach(k => { Console.WriteLine(k); Logger.Info(k); });
            }

            if (changed.Any())
            {
                Console.WriteLine("Changed...");
                Logger.Info("Changed...");
                changed.Keys.ToList().ForEach(k => { Console.WriteLine(k); Logger.Info(k); });
            }

            if (removed.Any())
            {
                Console.WriteLine("Removed...");
                Logger.Info("Removed...");
                removed.Keys.ToList().ForEach(k => { Console.WriteLine(k); Logger.Info(k); });
            }
        }
    }
}