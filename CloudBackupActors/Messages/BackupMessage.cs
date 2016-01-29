using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBackupActors.Messages
{
    /// <summary>
    /// Instruction for backing up a zip archive to cloud storage.
    /// </summary>
    public class BackupMessage
    {
        /// <summary>
        /// The source folder path.
        /// </summary>
        public readonly string SourceFolderPath;
        /// <summary>
        /// The zip file path.
        /// </summary>
        public readonly string ZipFilePath;
        /// <summary>
        /// The kind of zip operation.
        /// </summary>
        public readonly ZipKind ZipKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipMessage" /> class.
        /// </summary>
        /// <param name="sourceFolderPath">The source folder path.</param>
        /// <param name="zipFilePath">The zip file path.</param>
        /// <param name="zipKind">The kind of zip operation.</param>
        public BackupMessage(string sourceFolderPath, string zipFilePath, ZipKind zipKind)
        {
            SourceFolderPath = sourceFolderPath;
            ZipFilePath = zipFilePath;
            ZipKind = zipKind;
        }
    }
}
