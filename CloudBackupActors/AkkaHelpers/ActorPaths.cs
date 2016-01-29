namespace CloudBackupActors.AkkaHelpers
{
    /// <summary>
    /// Helper class used to define paths to fixed-name actors
    /// (helps eliminate errors when using <see cref="ActorSelection"/>).
    /// </summary>
    public static class ActorPaths
    {
        public static readonly ActorMetaData CloudBackupActor = new ActorMetaData("CloudBackup");
        public static readonly ActorMetaData ZipActor = new ActorMetaData("Zip", CloudBackupActor);
        public static readonly ActorMetaData BackupActor = new ActorMetaData("Backup", CloudBackupActor);
        public static readonly ActorMetaData BackupStatisticsActor = new ActorMetaData("BackupStatistics", CloudBackupActor);
    }
}