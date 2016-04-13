using System;
using System.IO;
using Akka.Actor;
using Akka.TestKit.NUnit;
using CloudBackupActors.Actors;
using CloudBackupActors.Messages;
using NUnit.Framework;

namespace CloudBackupActors.Tests
{
    [TestFixture]
    public class CloudBackupActorTests : TestKit
    {
        private const string SourceFolderPathsFileName = "SourceFolderPaths.txt";
        private const string ZipSourceFolderPath = @"C:\Users\Kevin\Documents\Cloud Backup Test";
        private readonly string SourceFolderPathsFilePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, SourceFolderPathsFileName);

        [Test]
        public void WhenReceivedStart_ShouldSendStarted()
        {
            // Arrange
            var actor = ActorOf(Props.Create(() => new CloudBackupActor(SourceFolderPathsFilePath)));

            // Act
            actor.Tell(new StartMessage());

            // Assert
            var message = ExpectMsg<StartedMessage>();
        }

        [Test]
        public void ShouldLogStartMessage()
        {
            // Arrange
            var actor = ActorOf(Props.Create(() => new CloudBackupActor(SourceFolderPathsFilePath)));

            // Act-Assert
            EventFilter.Info(LogMessageParts.ReceivedStart).ExpectOne(() =>
            {
                actor.Tell(new StartMessage());
            });
        }

        [Test]
        public void ShouldLogStopMessage()
        {
            // Arrange
            var actor = ActorOf(Props.Create(() => new CloudBackupActor(SourceFolderPathsFilePath)));

            // Act-Assert
            EventFilter.Info(LogMessageParts.ReceivedStop).ExpectOne(() =>
            {
                actor.Tell(new StopMessage());
            });
        }

        [Test]
        public void ShouldLogIncrementFolderCountMessage()
        {
            // Arrange
            var actor = ActorOf(Props.Create(() => new CloudBackupActor(SourceFolderPathsFilePath)));
            string path = ZipSourceFolderPath;

            // Act-Assert
            EventFilter
                .Info(message: null, start: null, contains: LogMessageParts.ReceivedIncrementFolderCount)
                .ExpectOne(() =>
            {
                actor.Tell(new IncrementFolderCountMessage(new ZipMessage(path, ZipKind.Regular).ZipKind));
            });
        }

        [Test]
        public void WhenReceivedIncrementFolderCount_ShouldSendFolderCountIncremented()
        {
            // Arrange
            var actor = ActorOf(Props.Create(() => new CloudBackupActor(SourceFolderPathsFilePath)));
            string path = ZipSourceFolderPath;

            // Act
            actor.Tell(new IncrementFolderCountMessage(new ZipMessage(path, ZipKind.Regular).ZipKind));

            // Assert
            var message = ExpectMsg<FolderCountIncrementedMessage>();
            Assert.AreEqual(ZipKind.Regular, message.ZipKind);
        }
    }
}
