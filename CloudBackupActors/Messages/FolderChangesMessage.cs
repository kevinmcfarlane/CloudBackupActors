using System;
using System.Collections.Generic;
using System.Linq;
using ZipDiff.Core;

namespace CloudBackupActors.Messages
{
    /// <summary>
    /// Represents changes to files in a folder.
    /// </summary>
    public class FolderChangesMessage
    {
        /// <summary>
        /// The zip file path.
        /// </summary>
        public readonly string ZipFilePath;

        /// <summary>
        /// The changes to files in a folder.
        /// </summary>
        public readonly Differences Differences;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderChangesMessage" /> class.
        /// </summary>
        /// <param name="zipFilePath">The zip file path.</param>
        /// <param name="differences">The changes to files in a folder.</param>
        public FolderChangesMessage(string zipFilePath, Differences differences)
        {
            ZipFilePath = zipFilePath;
            Differences = differences;
        }
    }
}
