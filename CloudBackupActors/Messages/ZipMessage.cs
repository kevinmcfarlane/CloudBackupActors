namespace CloudBackupActors.Messages
{
    /// <summary>
    /// Instruction to perform a zip operation.
    /// </summary>
    public class ZipMessage
    {
        public readonly string SourceFolderPath;
        public readonly ZipKind ZipKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipMessage" /> class.
        /// </summary>
        /// <param name="sourceFolderPath">The source folder path.</param>
        /// <param name="zipKind">The kind of zip operation.</param>
        public ZipMessage(string sourceFolderPath, ZipKind zipKind)
        {
            SourceFolderPath = sourceFolderPath;
            ZipKind = zipKind;
        }
    }
}