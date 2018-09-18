using System;
using System.Diagnostics;
using System.IO;
using Akka.Actor;
using CloudBackupActors.Actors;
using CloudBackupActors.Messages;
using NLog;

namespace CloudBackupActors
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Logger.Info("Starting...");

            using (var actorSystem = ActorSystem.Create("CloudBackupActorSystem"))
            {
                var sourceFolderPathsFilePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "SourceFolderPaths.txt");

                var actor = actorSystem.ActorOf(Props.Create(() => new CloudBackupActor(sourceFolderPathsFilePath)), "CloudBackup");

                actor.Tell(new StartMessage());

                actorSystem.AwaitTermination();
                Logger.Info("Actor system shutdown...");
            }

            stopwatch.Stop();

            long minutes = (stopwatch.ElapsedMilliseconds / (1000 * 60)) % 60;
            long seconds = (stopwatch.ElapsedMilliseconds / 1000) % 60;

            Logger.Info($"Finished in {minutes}m {seconds}s.");
        }
    }
}