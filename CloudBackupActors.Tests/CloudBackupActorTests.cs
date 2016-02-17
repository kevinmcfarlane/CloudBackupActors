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
        private const string SourceFolderPath = @"C:\Users\Kevin\Documents\Cloud Backup Test";

        [Test]
        public void WhenReceivedStart_ShouldSendStarted()
        {
            // Arrange
            var actor = ActorOf(Props.Create(() => new CloudBackupActor()));

            // Act
            actor.Tell(new StartMessage());

            // Assert
            var message = ExpectMsg<StartedMessage>();
        }

        [Test]
        public void ShouldLogStartMessage()
        {
            // Arrange
            var actor = ActorOf(Props.Create(() => new CloudBackupActor()));

            // Act-Assert
            EventFilter.Info(LogMessageParts.ReceivedStart).ExpectOne(() =>
            {
                actor.Tell(new StartMessage());
            });
        }

        [Test]
        public void ShouldLogIncrementFolderCountMessage()
        {
            // Arrange
            var actor = ActorOf(Props.Create(() => new CloudBackupActor()));
            string path = SourceFolderPath;

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
            var actor = ActorOf(Props.Create(() => new CloudBackupActor()));
            string path = SourceFolderPath;

            // Act
            actor.Tell(new IncrementFolderCountMessage(new ZipMessage(path, ZipKind.Regular).ZipKind));

            // Assert
            var message = ExpectMsg<FolderCountIncrementedMessage>();
            Assert.AreEqual(ZipKind.Regular, message.ZipKind);
        }
    }
}
