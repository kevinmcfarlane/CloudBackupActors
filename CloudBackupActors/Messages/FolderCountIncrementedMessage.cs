namespace CloudBackupActors.Messages
{
    /// <summary>
    /// Indicates folder count has incremented, i.e., a folder has been processed.
    /// </summary>
    public class FolderCountIncrementedMessage
    {
        public readonly ZipKind ZipKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderCountIncrementedMessage"/> class.
        /// </summary>
        /// <param name="zipKind">The kind of zip operation.</param>
        public FolderCountIncrementedMessage(ZipKind zipKind)
        {
            ZipKind = zipKind;
        }
    }
}
