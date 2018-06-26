using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentPublisherLib
{
    public static class PublisherTypes
    {
        public enum ActionType
        {
            [Description("Archive Folder")]
            ArchiveDirectory,
            [Description("Archive Folder")]
            ArchiveDirectoryOverwrite,
            [Description("Archive File")]
            ArchiveFile,
            [Description("Archive File")]
            ArchiveFileOverwrite,
            [Description("Create Folder")]
            CreateDirectory,
            [Description("Copy File to Folder")]
            CopyFileToDirectory,
            [Description("Copy File to Folder Overwrite")]
            CopyFileToDirectoryOverwrite,
            [Description("Copy File to File")]
            CopyFileToFile,
            [Description("Copy File to File Overwrite")]
            CopyFileToFileOverwrite
        }

        public enum ActionResultType
        {
            [Description("Action Succesful")]
            Successful,
            [Description("Action Failed")]
            Failed,
            [Description("Action Succesful With Warnings")]
            WithWarnings
        }

        public enum PublisherMode { Overwrite, SubOnly, Archive }
        public enum PublisherSourceType { File, Directory }
        public enum PublisherTargetType { File, Directory, Archive }
    }
}
