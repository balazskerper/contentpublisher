using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ContentPublisherLib
{
    public static class PublisherIO
    {
        #region operations

        /// <summary>
        /// Creates a directory and returns a PublisherActionResult with the result
        /// </summary>
        /// <param name="directorypath">Path of the directory to be created</param>
        /// <param name="currentusersecurity">CurrentUserSecurity object used to check access rights</param>
        /// <returns>PublisherActionResult containing specific information in both Failed and Succesful cases</returns>
        public static PublisherActionResult CreateDirectory(string directorypath, CurrentUserSecurity currentusersecurity)
        {
            try
            {
                string existingparent = GetLastExistingAncestorDirectory(directorypath);

                PublisherActionResult precheckresult = CreateDirectoryPreCheck(directorypath, existingparent, currentusersecurity);

                if (precheckresult.ResultType != PublisherTypes.ActionResultType.Successful)
                {
                    return precheckresult;
                }
                else
                {
                    Directory.CreateDirectory(directorypath);
                    return new PublisherActionResult(
                        PublisherTypes.ActionResultType.Successful,
                        $"Directory ({directorypath}) is succesfully created!"
                    );
                }
            }
            catch (Exception)
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"There was an unexpected error while creating the directory ({directorypath})!"
                );
            }
        }

        /// <summary>
        /// Copies the file to a given path
        /// </summary>
        /// <param name="sourcepath">Path of the source file</param>
        /// <param name="targetpath">Path of the target file</param>
        /// <param name="currentusersecurity">CurrentUserSecurity object used to check access rights</param>
        /// <param name="overwrite">Whether to overwrite the existing file or not</param>
        /// <returns>PublisherActionResult containing specific information in both Failed and Succesful cases</returns>
        public static PublisherActionResult CopyFileToFile(string sourcepath, string targetpath, CurrentUserSecurity currentusersecurity, bool overwrite)
        {
            PublisherActionResult precheckresult = CopyFileToFilePreCheck(sourcepath, targetpath, currentusersecurity);

            if (precheckresult.ResultType != PublisherTypes.ActionResultType.Successful)
            {
                return precheckresult;
            }
            else if ((File.Exists(targetpath) && overwrite))
            {
                try
                {
                    File.Copy(sourcepath, targetpath, true);
                }
                catch (Exception)
                {
                    return new PublisherActionResult(
                        PublisherTypes.ActionResultType.Failed,
                        $"There was an unexpected error while overwriting the file ({targetpath})!"
                    );
                }
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Successful,
                    $"File succesfully copied ({sourcepath} => {targetpath})"
                );
            }
            else
            {
                try
                {
                    File.Copy(sourcepath, targetpath, false);
                }
                catch (Exception)
                {
                    return new PublisherActionResult(
                        PublisherTypes.ActionResultType.Failed,
                        $"There was an unexpected error while copying the file({sourcepath} => {targetpath})!"
                    );
                }
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Successful,
                    $"File succesfully copied ({sourcepath} => {targetpath})"
                );
            }

        }

        /// <summary>
        /// Copies the file to a given directory
        /// </summary>
        /// <param name="sourcepath">Path of the source file</param>
        /// <param name="targetpath">Path of the target directory</param>
        /// <param name="currentusersecurity">CurrentUserSecurity object used to check access rights</param>
        /// <param name="overwrite">Whether to overwrite the already existing file or not</param>
        /// <returns>PublisherActionResult containing specific information in both Failed and Succesful cases</returns>
        public static PublisherActionResult CopyFileToDirectory(string sourcepath, string targetpath, CurrentUserSecurity currentusersecurity, bool overwrite)
        {
            if (String.IsNullOrEmpty(sourcepath))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"Source path ({sourcepath}) is not valid!"
                );
            }
            else if (String.IsNullOrEmpty(targetpath))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"Target path ({targetpath}) is not valid!"
                );
            }
            else
            {
                return CopyFileToFile(sourcepath, Path.Combine(targetpath, Path.GetFileName(sourcepath)), currentusersecurity, overwrite);
            }
        }

        /// <summary>
        /// Archives a fileto a given path
        /// </summary>
        /// <param name="filepath">Path of the file to be archived</param>
        /// <param name="currentusersecurity">CurrentUserSecurity object used to check access rights</param>
        /// <param name="zippath">Path of the archive</param>
        /// <param name="overwritearchvie">Whether to overwrite the archive or not</param>
        /// <returns>PublisherActionResult containing specific information in both Failed and Succesful cases</returns>
        public static PublisherActionResult ArchiveFile(string filepath, CurrentUserSecurity currentusersecurity, string zippath = null, bool overwritearchvie = false)
        {

            string archivepath = zippath;

            try
            {
                if (archivepath == null || String.IsNullOrEmpty(archivepath))
                {
                    archivepath = GetArchiveFilePath(filepath);
                }

                PublisherActionResult precheckresult = ArchiveFilePreCheck(filepath, archivepath, currentusersecurity, overwritearchvie);

                if (precheckresult.ResultType != PublisherTypes.ActionResultType.Successful)
                {
                    return precheckresult;
                }
                else
                {
                    try
                    {
                        if (File.Exists(archivepath) && overwritearchvie)
                        {
                            File.Delete(archivepath);
                        }
                    }
                    catch (Exception)
                    {
                        return new PublisherActionResult(
                            PublisherTypes.ActionResultType.Failed,
                            $"There was an unexpected error while deleting the already existing archive file ({archivepath})!"
                        );
                    }
                    try
                    {
                        using (var zip = ZipFile.Open(archivepath, ZipArchiveMode.Create))
                        {
                            zip.CreateEntryFromFile(filepath, Path.GetFileName(filepath));
                        }
                    }
                    catch (Exception)
                    {
                        return new PublisherActionResult(
                            PublisherTypes.ActionResultType.Failed,
                            $"There was an unexpected error while archiving the file ({filepath} => {archivepath})!"
                        );
                    }
                    return new PublisherActionResult(
                        PublisherTypes.ActionResultType.Successful,
                        $"File succesfully archived ({filepath} => {archivepath})!"
                    );
                }
            }
            catch (Exception)
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"There was an unexpected error while archiving the file ({filepath})!"
                );
            }
        }

        /// <summary>
        /// Archives a directory and its content
        /// </summary>
        /// <param name="directorypath">Path of the directory to archive</param>
        /// <param name="currentusersecurity">CurrentUserSecurity object used to check access rights</param>
        /// <param name="zippath">Path of the archive file</param>
        /// <param name="overwritearchive">Whether to overwrite the existing archive or not</param>
        /// <returns>PublisherActionResult containing specific information in both Failed and Succesful cases</returns>
        public static PublisherActionResult ArchiveDirectory(string directorypath, CurrentUserSecurity currentusersecurity, string zippath = null, bool overwritearchive = false)
        {
            string archivepath = zippath;
            try
            {
                if (!Directory.Exists(directorypath))
                {
                    return new PublisherActionResult(
                        PublisherTypes.ActionResultType.WithWarnings,
                        $"Directory ({directorypath}) does not exist!"
                    );
                }

                if (archivepath.IsNullOrWhiteSpace())
                {
                    archivepath = GetArchiveDirectoryPath(directorypath);
                }

                PublisherActionResult precheckresult = ArchivedirectoryPreCheck(directorypath, currentusersecurity, archivepath, overwritearchive);

                if (precheckresult.ResultType != PublisherTypes.ActionResultType.Successful)
                {
                    return precheckresult;
                }
                else
                {
                    try
                    {
                        if (File.Exists(archivepath) && overwritearchive)
                        {
                            File.Delete(archivepath);
                        }
                    }
                    catch (Exception)
                    {
                        return new PublisherActionResult(
                            PublisherTypes.ActionResultType.Failed,
                            $"There was an unexpected error while deleting the already existing archive ({archivepath})!"
                        );
                    }

                    try
                    {
                        ZipFile.CreateFromDirectory(directorypath, archivepath, CompressionLevel.Fastest, true);
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                }
            }
            catch (Exception)
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"There was an unexpected error while archiving the directory ({directorypath})!"
                );
            }
            return new PublisherActionResult(
                PublisherTypes.ActionResultType.Successful,
                $"Directory succesfully archived ({directorypath} => ({zippath}))!"
            );
        }

        #endregion operations

        #region prechecks

        /// <summary>
        /// Returns error specific PublisherTypes.ActionResultType.Failed or PublisherTypes.ActionResultType.WithWarnings
        /// type PublisherActionResults in case parameters are incorrect, or ActionResultType.Successful if they are correct
        /// </summary>
        /// <param name="directorypath">Path of the directory to create</param>
        /// <param name="existingancestor">Path to last existing ancestor</param>
        /// <param name="currentusersecurity">CurrentUserSecurity object used to check access rights</param>
        /// <returns>PublisherActionResult with specific ActionResultType and message</returns>
        private static PublisherActionResult CreateDirectoryPreCheck(string directorypath, string existingancestor, CurrentUserSecurity currentusersecurity)
        {
            if (existingancestor == null)
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"Could not determine root directory for ({directorypath})!"
                );
            }
            else if (String.IsNullOrEmpty(directorypath))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"Target directory path ({directorypath}) was invalid!"
                );
            }
            else if (!currentusersecurity.HasAccess(new DirectoryInfo(existingancestor), FileSystemRights.Write))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"No write access to the target directory ({existingancestor})!"
                );
            }
            else if (Directory.Exists(directorypath))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.WithWarnings,
                    $"Directory ({directorypath}) already exists!"
                );
            }
            else
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Successful,
                    $"Pre Check Succesful!"
                );
            }
        }

        /// <summary>
        /// Returns error specific PublisherTypes.ActionResultType.Failed or PublisherTypes.ActionResultType.WithWarnings
        /// type PublisherActionResults in case parameters are incorrect, or ActionResultType.Successful if they are correct
        /// </summary>
        /// <param name="sourcepath">Path of the source file</param>
        /// <param name="targetpath">Path of the target file</param>
        /// <param name="currentusersecurity">CurrentUserSecurity object used to check access rights</param>
        /// <returns>PublisherActionResult with specific ActionResultType and message</returns>
        public static PublisherActionResult CopyFileToFilePreCheck(string sourcepath, string targetpath, CurrentUserSecurity currentusersecurity)
        {
            if (String.IsNullOrEmpty(sourcepath))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"Source path ({sourcepath}) is not valid!"
                );
            }
            else if (!File.Exists(sourcepath))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"Source file ({sourcepath}) does not exist!"
                );
            }
            else if (!currentusersecurity.HasAccess(new FileInfo(sourcepath), FileSystemRights.Read))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"No read access to source file ({sourcepath})!"
                );
            }
            else if (String.IsNullOrEmpty(targetpath))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"Target path ({targetpath}) is not valid!"
                );
            }
            else if (!Directory.Exists(Path.GetDirectoryName(targetpath)))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"Target directory ({Path.GetDirectoryName(targetpath)}) does not exist!"
                );
            }
            else if (!currentusersecurity.HasAccess(new DirectoryInfo(Path.GetDirectoryName(targetpath)), FileSystemRights.Write))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"No write access to target path ({targetpath})!"
                );
            }
            else
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Successful,
                    $"Pre Check Succesful!"
                );
            }
        }

        /// <summary>
        /// Returns error specific PublisherTypes.ActionResultType.Failed or PublisherTypes.ActionResultType.WithWarnings
        /// type PublisherActionResults in case parameters are incorrect, or ActionResultType.Successful if they are correct
        /// </summary>
        /// <param name="filepath">Path of the file to archive</param>
        /// <param name="archivepath">Path of the archive file</param>
        /// <param name="currentusersecurity">CurrentUserSecurity object used to check access rights</param>
        /// <param name="overwritearchvie">Whether to overwrite an already existing archive</param>
        /// <returns>PublisherActionResult with specific ActionResultType and message</returns>
        public static PublisherActionResult ArchiveFilePreCheck(string filepath, string archivepath, CurrentUserSecurity currentusersecurity, bool overwritearchvie)
        {
            if (archivepath == null || String.IsNullOrEmpty(archivepath))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"Zip path ({archivepath}) is invalid!"
                );
            }
            else if (!File.Exists(filepath))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.WithWarnings,
                    $"File ({filepath}) does not exist (nothing has been archived)!"
                );
            }
            else if (!currentusersecurity.HasAccess(new FileInfo(filepath), FileSystemRights.Read))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"No read access to the source file ({filepath})!"
                );
            }
            else if (File.Exists(archivepath) && !overwritearchvie)
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.WithWarnings,
                    $"Zip archive ({archivepath}) already exists!"
                );
            }
            else if (!currentusersecurity.HasAccess(new DirectoryInfo(Path.GetDirectoryName(archivepath)), FileSystemRights.Write))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"No write access to target path ({archivepath})!"
                );
            }
            else
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Successful,
                    $"Pre Check Succesful!"
                );
            }
        }

        /// <summary>
        /// Returns error specific PublisherTypes.ActionResultType.Failed or PublisherTypes.ActionResultType.WithWarnings
        /// type PublisherActionResults in case parameters are incorrect, or ActionResultType.Successful if they are correct
        /// </summary>
        /// <param name="directorypath">Path of the directory to be archived</param>
        /// <param name="currentusersecurity">CurrentUserSecurity object used to check access rights</param>
        /// <param name="archivepath">path of the archive file</param>
        /// <param name="overwritearchive">Whether to overwrite an already existing archive</param>
        /// <returns>PublisherActionResult with specific ActionResultType and message</returns>
        public static PublisherActionResult ArchivedirectoryPreCheck(string directorypath, CurrentUserSecurity currentusersecurity, string archivepath, bool overwritearchive)
        {
            if (String.IsNullOrEmpty(archivepath))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"Zip archive path ({archivepath}) is not valid!"
                );
            }
            else if (File.Exists(archivepath) && !overwritearchive)
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.WithWarnings,
                    $"Zip archive ({archivepath}) already exists!"
                );
            }
            else if (!currentusersecurity.HasAccess(new DirectoryInfo(directorypath), FileSystemRights.Read))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"No read access to the source directory ({directorypath})!"
                );
            }
            else if (!currentusersecurity.HasAccess(new DirectoryInfo(Path.GetDirectoryName(archivepath)), FileSystemRights.Write))
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Failed,
                    $"No write access to the archive target path ({archivepath})!"
                );
            }
            else
            {
                return new PublisherActionResult(
                    PublisherTypes.ActionResultType.Successful,
                    $"Pre Check Succesful!"
                );
            }
        }

        #endregion prechecks

        #region helpers

        /// <summary>
        /// Returns an archive path based on a given file path, and the current datetime stamp
        /// </summary>
        /// <param name="filepath">Path to the file</param>
        /// <returns>Archive path as string</returns>
        private static string GetArchiveFilePath(string filepath)
        {
            string archivepath;
            if (!String.IsNullOrEmpty(filepath))
            {
                archivepath = Path.Combine(Path.GetDirectoryName(filepath), Path.ChangeExtension(filepath, "zip"));
                archivepath = Path.Combine(Path.GetDirectoryName(archivepath), Path.GetFileNameWithoutExtension(archivepath) + GetCurrentTimeStamp() + Path.GetExtension(archivepath));
            }
            else
            {
                archivepath = null;
            }
            return archivepath;
        }

        /// <summary>
        /// Returns an archive path based on a given directory path, and the current datetime stamp
        /// </summary>
        /// <param name="directorypath">Path to the directory</param>
        /// <returns>Archive path as string</returns>
        private static string GetArchiveDirectoryPath(string directorypath)
        {
            string archivepath = String.Empty;
            if (!String.IsNullOrEmpty(directorypath))
            {
                archivepath = Path.Combine(Directory.GetParent(AppendDirectorySeparatorToDirectoryPath(directorypath)).FullName,
                    Path.GetDirectoryName(AppendDirectorySeparatorToDirectoryPath(directorypath)) + GetCurrentTimeStamp() + ".zip");
            }
            return archivepath;
        }

        /// <summary>
        /// Returns the current datetime stamp as string with a _delimiter
        /// </summary>
        /// <returns>Current datetime stamp as string</returns>
        private static string GetCurrentTimeStamp()
        {
            StringBuilder timestamp = new StringBuilder();

            timestamp.Append("_" + DateTime.UtcNow.Year.ToString() + "_" + DateTime.UtcNow.Month.ToString() + "_" + DateTime.UtcNow.Day.ToString());
            timestamp.Append("_" + DateTime.UtcNow.Hour.ToString() + "_" + DateTime.UtcNow.Minute.ToString() + "_" + DateTime.UtcNow.Second.ToString());

            return timestamp.ToString();
        }

        /// <summary>
        /// Appends Path.DirectorySeparatorChar at the end of the directory path if needed
        /// </summary>
        /// <param name="directorypath">Directory path</param>
        /// <returns>Directory path with Path.DirectorySeparatorChar appended</returns>
        public static string AppendDirectorySeparatorToDirectoryPath(string directorypath)
        {
            string returnpath = directorypath;
            if (!returnpath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                returnpath += Path.DirectorySeparatorChar.ToString();
            }
            return returnpath;
        }

        /// <summary>
        /// Get the path of the last existing ancestor directory from a path
        /// </summary>
        /// <param name="directorypath">Path of the directory to check</param>
        /// <returns>Path of the last existing ancestor directory, or null in case of an exception, or no paths found</returns>
        public static string GetLastExistingAncestorDirectory(string directorypath)
        {
            try
            {
                DirectoryInfo dirinfo = new DirectoryInfo(directorypath);
                foreach (DirectoryInfo dir in dirinfo.ListDirectoryAncestors().Reverse<DirectoryInfo>())
                {
                    if (dir.Exists)
                    {
                        return dir.FullName;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
        #endregion helpers
    }
}
