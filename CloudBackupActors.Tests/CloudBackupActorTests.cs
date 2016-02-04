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
        [Test]
        public void CloudBackupActor_WhenReceivedStart_ShouldSendStarted()
        {
            // Arrange
            var actor = ActorOf(Props.Create(() => new CloudBackupActor()));

            // Act
            actor.Tell(new StartMessage());

            // Assert
            var message = ExpectMsg<StartedMessage>();
        }

        [Test]
        public void CloudBackupActor_ShouldLogStartMessage()
        {
            // Arrange
            var actor = ActorOf(Props.Create(() => new CloudBackupActor()));

            // Act-Assert
            EventFilter.Info("Received: StartMessage").ExpectOne(() =>
            {
                actor.Tell(new StartMessage());
            });
        }

        [Test]
        public void CloudBackupActor_ShouldLogIncrementFolderCountMessage()
        {
            // Arrange
            var actor = ActorOf(Props.Create(() => new CloudBackupActor()));
            string path = @"C:\Users\Kevin\Documents\Cloud Backup Test";

            // Act-Assert
            EventFilter
                .Info(message: null, start: null, contains: "Received: IncrementFolderCountMessage from ")
                .ExpectOne(() =>
            {
                actor.Tell(new IncrementFolderCountMessage(new ZipMessage(path, ZipKind.Regular).ZipKind));
            });
        }

        [Test]
        public void CloudBackupActor_WhenReceivedIncrementFolderCount_ShouldSendFolderCountIncremented()
        {
            // Arrange
            var actor = ActorOf(Props.Create(() => new CloudBackupActor()));
            string path = @"C:\Users\Kevin\Documents\Cloud Backup Test";

            // Act
            actor.Tell(new IncrementFolderCountMessage(new ZipMessage(path, ZipKind.Regular).ZipKind));

            // Assert
            var message = ExpectMsg<FolderCountIncrementedMessage>();
            Assert.AreEqual(ZipKind.Regular, message.ZipKind);
        }
    }
}
