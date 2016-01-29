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
            Console.WriteLine("In " + zipFilePath);
            Logger.Info("In " + zipFilePath);

            var added = differences.Added;

            if (added.Any())
            {
                Console.WriteLine("Added...");
                Logger.Info("Added...");
                added.Keys.ToList().ForEach(k => { Console.WriteLine(k); Logger.Info(k); });
            }

            var changed = differences.Changed;

            if (changed.Any())
            {
                Console.WriteLine("Changed...");
                Logger.Info("Changed...");
                changed.Keys.ToList().ForEach(k => { Console.WriteLine(k); Logger.Info(k); });
            }

            var removed = differences.Removed;

            if (removed.Any())
            {
                Console.WriteLine("Removed...");
                Logger.Info("Removed...");
                removed.Keys.ToList().ForEach(k => { Console.WriteLine(k); Logger.Info(k); });
            }
        }
    }
}