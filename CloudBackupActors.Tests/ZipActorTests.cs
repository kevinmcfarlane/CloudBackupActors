using Akka.Actor;
using Akka.TestKit.NUnit;
using CloudBackupActors.Actors;
using CloudBackupActors.Messages;
using NUnit.Framework;

namespace CloudBackupActors.Tests
{

    // TODO:

    [TestFixture]
    public class ZipActorTests : TestKit
    {
        //[Test]
        //public void ZipActor_WhenReceivedRegularZip_ShouldIncrementRegularFolderCount()
        //{
        //    // Arrange
        //    var zipProps = Props.Create(() => new ZipActor());
        //    var zipActor = ActorOf(zipProps);

        //    string path = @"C:\Users\Kevin\Documents\Cloud Backup Test";

        //    // Act
        //    zipActor.Tell(new ZipMessage(path, ZipKind.Regular));

        //    // Assert
        //    var message = ExpectMsgFrom<string>(zipActor);

        //    Assert.IsTrue(message.Contains("No changes detected in " + path));

        //    //Assert.AreEqual("abc", zipActor.UnderlyingActor.Message);
        //    //var message = ExpectMsg<IncrementFolderCountMessage>();
        //    //Assert.AreEqual(expected: ZipKind.Regular, actual: message.ZipKind);
        //}

        //[Test]
        //public void TestCase()
        //{
        //    // Arrange
        //    //var statsProp = Props.Create(() => new BackupStatisticsActor());
        //    //var statsActor = ActorOf(statsProp, "BackupStatistics");

        //    var zipProps = Props.Create(() => new ZipActor());
        //    var zipActor = ActorOf(zipProps);

        //    var statsActor = CreateTestProbe();

        //    string path = @"C:\Users\Kevin\Documents\Cloud Backup Test";
        //    zipActor.Tell(new ZipMessage(path, ZipKind.Regular));

        //    // Act

        //    statsActor.ExpectMsgFrom<string>(zipActor);
        //}

        //[Test]
        //public void ZipActor_WhenReceivedVisualStudioZip_ShouldIncrementVisualStudioFolderCount()
        //{
        //    // Arrange
        //    // (make ZipActor child of TestActor)
        //    var props = Props.Create(() => new ZipActor());
        //    var actor = ActorOfAsTestActorRef<ZipActor>(props, TestActor);

        //    string path = "some path";

        //    // Act
        //    actor.Tell(new ZipMessage(path, ZipKind.VisualStudio));

        //    // Assert
        //    var message = ExpectMsg<IncrementFolderCountMessage>();
        //    Assert.AreEqual(expected: ZipKind.VisualStudio, actual: message.ZipKind);
        //}
    }
}