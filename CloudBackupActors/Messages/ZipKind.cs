namespace CloudBackupActors.Messages
{
    /// <summary>
    /// The kind of zip operation.
    /// </summary>
    public enum ZipKind
    {
        /// <summary>
        /// All contents will be added to the zip archive.
        /// </summary>
        Regular,

        /// <summary>
        /// A subset of contents will be added to the zip archive, e.g., will exclude *.dll, *.exe...
        /// </summary>
        VisualStudio
    }
}