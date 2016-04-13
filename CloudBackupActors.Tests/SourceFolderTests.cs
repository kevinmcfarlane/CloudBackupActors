using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Akka.Actor;
using Akka.TestKit.NUnit;
using CloudBackupActors.Actors;
using NUnit.Framework;

namespace CloudBackupActors.Tests
{
    /// <summary>
    /// Tests for reading of source folders, e.g., returning correct list when there are empty entries.
    /// </summary>
    /// <seealso cref="Akka.TestKit.NUnit.TestKit" />
    [TestFixture]
    public class SourceFolderTests : TestKit
    {
        private const string SourceFolderPathsFileName = "SourceFolderPaths.txt";
        private readonly string SourceFolderPathsFilePath = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, SourceFolderPathsFileName);

        [Test]
        public void ReadAllPaths_ReturnsAllPaths()
        {
            // Arrange
            string[] expected = {
                @"C:\Users\Kevin\Documents\Cloud Backup Test",
                @"C:\Users\Kevin\Documents\Letters",
                @"C:\Users\Kevin\Documents\LINQPad Queries"
            };

            string contents =
                expected[0] + Environment.NewLine +
                expected[1] + Environment.NewLine +
                expected[2];

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { SourceFolderPathsFilePath, new MockFileData(contents) },
            });

            // Act
            var actor =
                ActorOfAsTestActorRef<CloudBackupActor>(Props.Create(() => new CloudBackupActor(SourceFolderPathsFilePath, fileSystem)))
                .UnderlyingActor;

            // Assert
            CollectionAssert.AreEqual(expected, actual: actor.SourceFolderPaths);
        }

        [Test]
        public void ReadAllPaths_WhenSomeAreEmpty_ReturnsOnlyNonEmptyPaths()
        {
            // Arrange
            string[] sourceFolderPaths = {
                @"C:\Users\Kevin\Documents\Cloud Backup Test",
                @" ",
                @"  ",
                @"C:\Users\Kevin\Documents\LINQPad Queries"
            };

            string contents =
                sourceFolderPaths[0] + Environment.NewLine +
                sourceFolderPaths[1] + Environment.NewLine +
                sourceFolderPaths[2] + Environment.NewLine +
                sourceFolderPaths[3];

            string[] expected = {
                @"C:\Users\Kevin\Documents\Cloud Backup Test",
                @"C:\Users\Kevin\Documents\LINQPad Queries"
            };

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { SourceFolderPathsFilePath, new MockFileData(contents) },
            });

            // Act
            var actor =
                ActorOfAsTestActorRef<CloudBackupActor>(Props.Create(() => new CloudBackupActor(SourceFolderPathsFilePath, fileSystem)))
                .UnderlyingActor;

            // Assert
            CollectionAssert.AreEqual(expected, actual: actor.SourceFolderPaths);
        }

        [Test]
        public void ReadAllPaths_WhenAllAreEmpty_ReturnsNoPaths()
        {
            // Arrange
            string[] sourceFolderPaths = {
                @"",
                @"    ",
                @"  "
            };

            string[] expected = { };

            string contents =
                sourceFolderPaths[0] + Environment.NewLine +
                sourceFolderPaths[1] + Environment.NewLine +
                sourceFolderPaths[2];

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { SourceFolderPathsFilePath, new MockFileData(contents) },
            });

            // Act
            var actor =
                ActorOfAsTestActorRef<CloudBackupActor>(Props.Create(() => new CloudBackupActor(SourceFolderPathsFilePath, fileSystem)))
                .UnderlyingActor;

            // Assert
            CollectionAssert.AreEqual(expected, actual: actor.SourceFolderPaths);
        }
    }
}