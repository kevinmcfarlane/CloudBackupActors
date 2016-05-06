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

            string starting = LogMessageParts.Starting;
            Console.WriteLine(starting);
            Logger.Info(starting);
            Console.WriteLine(Environment.NewLine);

            using (var actorSystem = ActorSystem.Create("CloudBackupActorSystem"))
            {
                var sourceFolderPathsFilePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "SourceFolderPaths.txt");

                var actor = actorSystem.ActorOf(Props.Create(() => new CloudBackupActor(sourceFolderPathsFilePath)), "CloudBackup");

                actor.Tell(new StartMessage());

                actorSystem.AwaitTermination();
                Console.WriteLine("Actor system shutdown...");
            }

            stopwatch.Stop();

            long minutes = (stopwatch.ElapsedMilliseconds / (1000 * 60)) % 60;
            long seconds = (stopwatch.ElapsedMilliseconds / 1000) % 60;
            string finished = string.Format(LogMessageParts.FinishedIn, minutes, seconds);

            Console.WriteLine(finished);
            Logger.Info(finished);
        }
    }
}