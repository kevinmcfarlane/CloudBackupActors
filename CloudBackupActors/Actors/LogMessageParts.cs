namespace CloudBackupActors.Actors
{
    /// <summary>
    /// Common parts of log messages.
    /// </summary>
    public static class LogMessageParts
    {
        public const string FoldersForEncryption = "Folders for encryption...";
        public const string ApplicationTerminating = "Application terminating because error in actor: ";
        public const string ReceivedStart = "Received: StartMessage";
        public const string ReceivedIncrementFolderCount = "Received: IncrementFolderCountMessage from ";
        public const string ReceivedStop = "Received: StopMessage";
    }
}