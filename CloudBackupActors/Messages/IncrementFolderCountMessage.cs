namespace CloudBackupActors.Messages
{
    /// <summary>
    /// Instruction to increment the folder count after processing a folder 
    /// with a specific kind of zip operation.
    /// </summary>
    public class IncrementFolderCountMessage
    {
        public readonly ZipKind ZipKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementFolderCountMessage" /> class.
        /// </summary>
        /// <param name="zipKind">The kind of zip operation.</param>
        public IncrementFolderCountMessage(ZipKind zipKind)
        {
            ZipKind = zipKind;
        }
    }
}
