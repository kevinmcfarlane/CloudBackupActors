using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using CloudBackupActors.Actors;
using CloudBackupActors.Messages;
using NUnit.Framework;

namespace CloudBackupActors.Tests
{
    [TestFixture]
    public class ZipActorTests : TestKit
    {
        //[Test]
        //public void ZipActor_WhenReceivedRegularZip_ShouldIncrementRegularFolderCount()
        //{
        //    // Arrange
        //    // (make ZipActor child of TestActor)
        //    var zipProps = Props.Create(() => new ZipActor());
        //    var zipActor = ActorOf(zipProps);

        //    string path = @"C:\Users\Kevin\Documents\Cloud Backup Test";

        //    // Act
        //    zipActor.Tell(new ZipMessage(path, ZipKind.Regular));

        //    // Assert
        //    var message = ExpectMsg<string>();

        //    Assert.IsTrue(message.Contains("No changes detected in " + path));


        //    //Assert.AreEqual("abc", zipActor.UnderlyingActor.Message);
        //    //var message = ExpectMsg<IncrementFolderCountMessage>();
        //    //Assert.AreEqual(expected: ZipKind.Regular, actual: message.ZipKind);
        //}

        //[Test]
        //public void TestCase()
        //{
        //    // Arrange
        //    var stats = CreateTestProbe();
        //    var zipProps = Props.Create(() => new ZipActor(stats));
        //    var zipActor = ActorOf(zipProps, "Zip");

        //    string path = @"C:\Users\Kevin\Documents\Cloud Backup Test";

        //    // Act
        //    zipActor.Tell(new ZipMessage(path, ZipKind.Regular));

        //    stats.ExpectMsgFrom<string>(zipActor, message => message.Contains("No changes detected in " + path));
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
