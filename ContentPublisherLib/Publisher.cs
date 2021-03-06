﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ContentPublisherLib
{
    public class Publisher
    {
        #region propsandconstructors
        public List<PublisherInfo> Publications { get; set; }
        public List<PublisherAction> Actions { get; set; }

        private CurrentUserSecurity _CurrentUserSecurity { get; set; }

        public bool AreAllActionsSuccesful
        {
            get
            {
                return (!Actions.IsNullOrEmpty() &&
                        !Actions.Any(a => a.Result.ResultType.Equals(PublisherTypes.ActionResultType.Failed)))
                    ? true : false;
            }

        }

        public delegate void PublisherActionProgress(int current, int all, PublisherAction action);
        public event PublisherActionProgress ActionDone;

        public Publisher()
        {
            _CurrentUserSecurity = new CurrentUserSecurity();
            Publications = new List<PublisherInfo>();
            Actions = new List<PublisherAction>();

        }

        public Publisher(XDocument xml) : this()
        {
            if (xml?.Root?.Elements("source") != null)
            {
                foreach (XElement source in xml.Root.Elements("source"))
                {
                    Publications.Add(new PublisherInfo(source));
                }
            }
        }

        public Publisher(string xml) : this(XDocument.Load(xml)) { }

        public Publisher(PublisherInfo pinfo) : this()
        {
            if (pinfo != null)
            {
                Publications.Add(pinfo);
            }
        }

        public Publisher(IEnumerable<PublisherInfo> pinfos) : this()
        {
            if (pinfos != null)
            {
                Publications.AddRange(pinfos);
            }
        }
        #endregion propsandconstructors

        #region processingmethods
        /// <summary>
        /// Generates all actions, then does the pre-processing and then runs all actions
        /// </summary>
        public void Run()
        {
            if (!Publications.IsNullOrEmpty())
            {
                GenerateAllActions();
                if (!Actions.IsNullOrEmpty())
                {
                    PreProcessActions();
                    RunAllActions();
                }
            }

        }
        /// <summary>
        /// Pre-processes all actions (orders them by type)
        /// </summary>
        public void PreProcessActions()
        {
            List<PublisherAction> tempactions = new List<PublisherAction>();
            foreach (PublisherTypes.ActionType type in Actions.Select(s => s.Type).Distinct().OrderBy(x => x))
            {
                foreach (PublisherAction action in Actions.Where(w => w.Type.Equals(type)).Distinct())
                {
                    tempactions.Add(action);
                }
            }

            Actions = tempactions;
        }
        /// <summary>
        /// Runs all actions
        /// </summary>
        public void RunAllActions()
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                switch (Actions[i].Type)
                {
                    case PublisherTypes.ActionType.CreateDirectory:
                        Actions[i].Result = PublisherIO.CreateDirectory(Actions[i].TargetPath, _CurrentUserSecurity);
                        ActionDone?.Invoke(i + 1, Actions.Count, Actions[i]);
                        break;
                    case PublisherTypes.ActionType.ArchiveDirectory:
                        Actions[i].Result = PublisherIO.ArchiveDirectory(Actions[i].SourcePath, _CurrentUserSecurity, Actions[i].TargetPath);
                        ActionDone?.Invoke(i + 1, Actions.Count, Actions[i]);
                        break;
                    case PublisherTypes.ActionType.ArchiveDirectoryOverwrite:
                        Actions[i].Result = PublisherIO.ArchiveDirectory(Actions[i].SourcePath, _CurrentUserSecurity, Actions[i].TargetPath, overwritearchive: true);
                        ActionDone?.Invoke(i + 1, Actions.Count, Actions[i]);
                        break;
                    case PublisherTypes.ActionType.ArchiveFile:
                        Actions[i].Result = PublisherIO.ArchiveFile(Actions[i].SourcePath, _CurrentUserSecurity, Actions[i].TargetPath);
                        ActionDone?.Invoke(i + 1, Actions.Count, Actions[i]);
                        break;
                    case PublisherTypes.ActionType.ArchiveFileOverwrite:
                        Actions[i].Result = PublisherIO.ArchiveFile(Actions[i].SourcePath, _CurrentUserSecurity, Actions[i].TargetPath, overwritearchvie: true);
                        ActionDone?.Invoke(i + 1, Actions.Count, Actions[i]);
                        break;
                    case PublisherTypes.ActionType.CopyFileToDirectory:
                        Actions[i].Result = PublisherIO.CopyFileToDirectory(Actions[i].SourcePath, Actions[i].TargetPath, _CurrentUserSecurity, false);
                        ActionDone?.Invoke(i + 1, Actions.Count, Actions[i]);
                        break;
                    case PublisherTypes.ActionType.CopyFileToDirectoryOverwrite:
                        Actions[i].Result = PublisherIO.CopyFileToDirectory(Actions[i].SourcePath, Actions[i].TargetPath, _CurrentUserSecurity, true);
                        ActionDone?.Invoke(i + 1, Actions.Count, Actions[i]);
                        break;
                    case PublisherTypes.ActionType.CopyFileToFile:
                        Actions[i].Result = PublisherIO.CopyFileToFile(Actions[i].SourcePath, Actions[i].TargetPath, _CurrentUserSecurity, false);
                        ActionDone?.Invoke(i + 1, Actions.Count, Actions[i]);
                        break;
                    case PublisherTypes.ActionType.CopyFileToFileOverwrite:
                        Actions[i].Result = PublisherIO.CopyFileToFile(Actions[i].SourcePath, Actions[i].TargetPath, _CurrentUserSecurity, true);
                        ActionDone?.Invoke(i + 1, Actions.Count, Actions[i]);
                        break;
                    default:
                        break;
                }

            }
        }

        #endregion processingmethods

        #region actiongeneratormethods

        /// <summary>
        /// Generates all actions from PublisherActions
        /// </summary>
        public void GenerateAllActions()
        {
            Actions = new List<PublisherAction>();
            foreach (PublisherInfo pinfo in this.Publications)
            {
                if (pinfo.Type.Equals(PublisherTypes.PublisherSourceType.File))
                {
                    GenerateActionsForFiles(pinfo);
                }
                else if (pinfo.Type.Equals(PublisherTypes.PublisherSourceType.Directory))
                {
                    GenerateActionsForDirectories(pinfo);
                }
            }
        }

        /// <summary>
        /// Generates all actions for file sources
        /// </summary>
        /// <param name="pinfo">PublisherInfo representing the source</param>
        public void GenerateActionsForFiles(PublisherInfo pinfo)
        {
            if (pinfo.Destinations != null)
            {
                foreach (PublisherDestination dest in pinfo.Destinations)
                {
                    if (dest.TargetType.Equals(PublisherTypes.PublisherTargetType.File))
                    {
                        GenerateActionsForFilesToFile(pinfo, dest);
                    }
                    else if (dest.TargetType.Equals(PublisherTypes.PublisherTargetType.Directory))
                    {
                        GenerateActionsForFilesToDirectory(pinfo, dest);
                    }
                    else if (dest.TargetType.Equals(PublisherTypes.PublisherTargetType.Archive))
                    {
                        GenerateActionsForFilesToArchive(pinfo, dest);
                    }
                }
            }
        }

        /// <summary>
        /// Generates all actions where the source type is a file, and the target type is also a file
        /// </summary>
        /// <param name="pinfo">PublisherInfo representing the source</param>
        /// <param name="dest">PublisherDestination represeting the target</param>
        private void GenerateActionsForFilesToFile(PublisherInfo pinfo, PublisherDestination dest)
        {
            if (dest.Modes.Contains(PublisherTypes.PublisherMode.Archive) || pinfo.Modes.Contains(PublisherTypes.PublisherMode.Archive))
            {
                Actions.Add(new PublisherAction
                {
                    Type = PublisherTypes.ActionType.ArchiveFile,
                    SourcePath = dest.TargetPath,
                    TargetPath = null
                });
            }
            if (dest.Modes.Contains(PublisherTypes.PublisherMode.Overwrite) || pinfo.Modes.Contains(PublisherTypes.PublisherMode.Overwrite))
            {
                Actions.Add(new PublisherAction
                {
                    Type = PublisherTypes.ActionType.CreateDirectory,
                    TargetPath = Path.GetDirectoryName(dest.TargetPath)
                });
                Actions.Add(new PublisherAction
                {
                    Type = PublisherTypes.ActionType.CopyFileToFileOverwrite,
                    SourcePath = pinfo.SourcePath,
                    TargetPath = dest.TargetPath
                });
            }
            else
            {
                Actions.Add(new PublisherAction
                {
                    Type = PublisherTypes.ActionType.CreateDirectory,
                    TargetPath = Path.GetDirectoryName(dest.TargetPath)
                });
                Actions.Add(new PublisherAction
                {
                    Type = PublisherTypes.ActionType.CopyFileToFile,
                    SourcePath = pinfo.SourcePath,
                    TargetPath = dest.TargetPath
                });
            }
        }

        /// <summary>
        /// Generates all actions where the source type is file, and the target type is diretory
        /// </summary>
        /// <param name="pinfo">PublisherInfo representing the source</param>
        /// <param name="dest">PublisherDestination represeting the target</param>
        private void GenerateActionsForFilesToDirectory(PublisherInfo pinfo, PublisherDestination dest)
        {
            if (dest.Modes.Contains(PublisherTypes.PublisherMode.Archive) || pinfo.Modes.Contains(PublisherTypes.PublisherMode.Archive))
            {
                Actions.Add(new PublisherAction
                {
                    Type = PublisherTypes.ActionType.ArchiveFile,
                    SourcePath = Path.Combine(dest.TargetPath, Path.GetFileName(pinfo.SourcePath)),
                    TargetPath = null
                });
            }
            if (dest.Modes.Contains(PublisherTypes.PublisherMode.Overwrite) || pinfo.Modes.Contains(PublisherTypes.PublisherMode.Overwrite))
            {
                Actions.Add(new PublisherAction
                {
                    Type = PublisherTypes.ActionType.CopyFileToDirectoryOverwrite,
                    SourcePath = pinfo.SourcePath,
                    TargetPath = dest.TargetPath
                });
            }
            else
            {
                Actions.Add(new PublisherAction
                {
                    Type = PublisherTypes.ActionType.CopyFileToDirectory,
                    SourcePath = pinfo.SourcePath,
                    TargetPath = dest.TargetPath
                });
            }
        }

        /// <summary>
        /// Generates all actions where the source type is file and target type is archive
        /// </summary>
        /// <param name="pinfo">PublisherInfo representing the source</param>
        /// <param name="dest">PublisherDestination represeting the target</param>
        private void GenerateActionsForFilesToArchive(PublisherInfo pinfo, PublisherDestination dest)
        {
            if (dest.Modes.Contains(PublisherTypes.PublisherMode.Overwrite) || pinfo.Modes.Contains(PublisherTypes.PublisherMode.Overwrite))
            {
                Actions.Add(new PublisherAction
                {
                    Type = PublisherTypes.ActionType.ArchiveFileOverwrite,
                    SourcePath = pinfo.SourcePath,
                    TargetPath = dest.TargetPath
                });
            }
            else
            {
                Actions.Add(new PublisherAction
                {
                    Type = PublisherTypes.ActionType.ArchiveFile,
                    SourcePath = pinfo.SourcePath,
                    TargetPath = dest.TargetPath
                });
            }
        }

        /// <summary>
        /// Generates all actions where the source type is directory
        /// </summary>
        /// <param name="pinfo">PublisherInfo representing the source</param>
        public void GenerateActionsForDirectories(PublisherInfo pinfo)
        {
            if (pinfo.Destinations != null)
            {
                foreach (PublisherDestination dest in pinfo.Destinations)
                {
                    if (dest.TargetType.Equals(PublisherTypes.PublisherTargetType.Directory))
                    {
                        if (dest.Modes.Contains(PublisherTypes.PublisherMode.Archive) || pinfo.Modes.Contains(PublisherTypes.PublisherMode.Archive))
                        {
                            Actions.Add(new PublisherAction
                            {
                                Type = PublisherTypes.ActionType.ArchiveDirectory,
                                SourcePath = dest.TargetPath,
                                TargetPath = null
                            });
                        }

                        if (dest.Modes.Contains(PublisherTypes.PublisherMode.SubOnly) || pinfo.Modes.Contains(PublisherTypes.PublisherMode.SubOnly))
                        {
                            GenerateActionsForDirectorySubOnly(pinfo, dest);
                        }
                        else
                        {
                            GenerateActionsForDirectoryFull(pinfo, dest);
                        }
                    }
                    else if (dest.TargetType.Equals(PublisherTypes.PublisherTargetType.Archive))
                    {
                        GenerateActionsForDirectoryToArchive(pinfo, dest);
                    }
                }

            }
        }

        /// <summary>
        /// Generates all actions where the source type is directory, and only sub content is copied
        /// </summary>
        /// <param name="pinfo">PublisherInfo representing the source</param>
        /// <param name="dest">PublisherDestination represeting the target</param>
        public void GenerateActionsForDirectorySubOnly(PublisherInfo pinfo, PublisherDestination dest)
        {
            if (Directory.Exists(pinfo.SourcePath) &&
                               _CurrentUserSecurity.HasAccess(new DirectoryInfo(pinfo.SourcePath), FileSystemRights.Read))
            {

                foreach (string folder in Directory.EnumerateDirectories(pinfo.SourcePath, "*",
                    SearchOption.AllDirectories).Select(s => s.Replace(pinfo.SourcePath, String.Empty)))
                {
                    Actions.Add(new PublisherAction
                    {
                        Type = PublisherTypes.ActionType.CreateDirectory,
                        TargetPath = PublisherIO.AppendDirectorySeparatorToDirectoryPath(Path.Combine(dest.TargetPath, folder))
                    });
                }
                foreach (string file in Directory.EnumerateFiles(pinfo.SourcePath, "*", SearchOption.AllDirectories))
                {
                    Actions.Add(new PublisherAction
                    {
                        Type = (dest.Modes.Contains(PublisherTypes.PublisherMode.Overwrite) || pinfo.Modes.Contains(PublisherTypes.PublisherMode.Overwrite)) ?
                            PublisherTypes.ActionType.CopyFileToFileOverwrite : PublisherTypes.ActionType.CopyFileToFile,
                        SourcePath = file,
                        TargetPath = file.Replace(pinfo.SourcePath, dest.TargetPath)
                    });
                }

            }
        }

        /// <summary>
        /// Generates all actions where the source type is directory, and the whole directory is copied
        /// </summary>
        /// <param name="pinfo">PublisherInfo representing the source</param>
        /// <param name="dest">PublisherDestination represeting the target</param>
        public void GenerateActionsForDirectoryFull(PublisherInfo pinfo, PublisherDestination dest)
        {
            if (Directory.Exists(pinfo.SourcePath) && Directory.Exists(Directory.GetParent(pinfo.SourcePath).FullName) &&
                                _CurrentUserSecurity.HasAccess(new DirectoryInfo(pinfo.SourcePath), FileSystemRights.Read))
            {

                foreach (string folder in Directory.EnumerateDirectories(pinfo.SourcePath, "*",
                    SearchOption.AllDirectories).Select(s => s.Replace(pinfo.SourcePath, String.Empty)))
                {
                    Actions.Add(new PublisherAction
                    {
                        Type = PublisherTypes.ActionType.CreateDirectory,
                        TargetPath = PublisherIO.AppendDirectorySeparatorToDirectoryPath(Path.Combine(dest.TargetPath, Path.GetFileName(Path.GetDirectoryName(pinfo.SourcePath)), folder))
                    });
                }
                foreach (string file in Directory.EnumerateFiles(pinfo.SourcePath, "*", SearchOption.AllDirectories))
                {
                    string targetfile = Path.Combine(
                        dest.TargetPath,
                        Path.GetFileName(
                            Path.GetDirectoryName(pinfo.SourcePath)),
                            file.Replace(pinfo.SourcePath, String.Empty).Replace(Path.GetFileName(file), String.Empty),
                        Path.GetFileName(file));

                    Actions.Add(new PublisherAction
                    {
                        Type = (dest.Modes.Contains(PublisherTypes.PublisherMode.Overwrite) || pinfo.Modes.Contains(PublisherTypes.PublisherMode.Overwrite)) ?
                            PublisherTypes.ActionType.CopyFileToFileOverwrite : PublisherTypes.ActionType.CopyFileToFile,
                        SourcePath = file,
                        TargetPath = targetfile
                    });
                }

            }
        }

        /// <summary>
        /// Generates all actions where the source type is directory, and the target type is archive
        /// </summary>
        /// <param name="pinfo">PublisherInfo representing the source</param>
        /// <param name="dest">PublisherDestination represeting the target</param>
        public void GenerateActionsForDirectoryToArchive(PublisherInfo pinfo, PublisherDestination dest)
        {
            if (dest.Modes.Contains(PublisherTypes.PublisherMode.Overwrite) || pinfo.Modes.Contains(PublisherTypes.PublisherMode.Overwrite))
            {
                Actions.Add(new PublisherAction
                {
                    Type = PublisherTypes.ActionType.ArchiveDirectoryOverwrite,
                    SourcePath = pinfo.SourcePath,
                    TargetPath = dest.TargetPath
                });
            }
            else
            {
                Actions.Add(new PublisherAction
                {
                    Type = PublisherTypes.ActionType.ArchiveDirectory,
                    SourcePath = pinfo.SourcePath,
                    TargetPath = dest.TargetPath
                });
            }
        }

        #endregion actiongeneratormethods

        /// <summary>
        /// Gets all actions for a given ActionResultType 
        /// </summary>
        /// <param name="actionType">ActionResultType for which matching actions are returned</param>
        /// <returns>List of actions where ActionResultType matches</returns>
        public List<string> GetActionResultMessages(PublisherTypes.ActionResultType actionType)
        {
            List<string> messages = new List<string>();
            if (Actions != null && (Actions.Where(w => w.Result.ResultType == actionType).Count() > 0))
            {
                messages.AddRange(Actions.Where(w => w.Result.ResultType == actionType).Select(s => s.Result.ResultMessage));
            }
            return messages;
        }

    }
}
