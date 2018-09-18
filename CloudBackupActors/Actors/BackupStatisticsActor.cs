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
            });
        }

        private void LogChanges(string zipFilePath, Differences differences)
        {
            Logger.Info("In {0}...", zipFilePath);

            var added = differences.Added;
            var changed = differences.Changed;
            var removed = differences.Removed;

            int numberOfDifferences = added.Count + changed.Count + removed.Count;

            if (numberOfDifferences > 0)
            {
                Logger.Info("Number of differences = {0}.", numberOfDifferences);
            }

            if (added.Any())
            {
                Logger.Info("Added...");
                added.Keys.ToList().ForEach(k => { Logger.Info(k); });
            }

            if (changed.Any())
            {
                Logger.Info("Changed...");
                changed.Keys.ToList().ForEach(k => { Logger.Info(k); });
            }

            if (removed.Any())
            {
                Logger.Info("Removed...");
                removed.Keys.ToList().ForEach(k => { Logger.Info(k); });
            }
        }
    }
}