using Akka.Actor;
using Akka.TestKit.NUnit;
using CloudBackupActors.Actors;
using NUnit.Framework;

namespace CloudBackupActors.Tests
{
    [TestFixture]
    public class BackupStatisticsActorTests : TestKit
    {
        [Test]
        public void WhenReceivedStringMessage_ShouldLogNoChangesDetected()
        {
            // Arrange
            var actor = ActorOf(Props.Create(() => new BackupStatisticsActor()));

            // Act-Assert
            string path = "some zip path";
            string noChangesDetected = string.Format("No changes detected in {0}...", path);
            string received = string.Format("Received: {0}", noChangesDetected);

            EventFilter
                .Info(received)
                .And
                .Info(noChangesDetected)
                .Expect(expectedCount: 2, action: () => actor.Tell(noChangesDetected));
        }
    }
}
