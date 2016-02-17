using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudBackupActors.Actors
{
    /// <summary>
    /// Common parts of log messages (also used by Console out).
    /// </summary>

    public static class LogMessageParts
    {
        public const string Starting = "Starting...";
        public const string FinishedIn = "Finished in {0}s.";
        public const string VisualStudioProjects = "Visual Studio projects...";
        public const string FoldersForEncryption = "Folders for encryption...";
        public const string SkippingFolder = "Skipping folder... ";
        public const string FinishedProcessing = "Finished processing {0} source folders, shutting down actor system...";
        public const string Processing = "Processing {0}...";
        public const string Received = "Received: {0}";
        public const string ReceivedStart = "Received: StartMessage";
        public const string ReceivedIncrementFolderCount = "Received: IncrementFolderCountMessage from ";
    }
}
