using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ContentPublisherLib
{
    public class CurrentUserSecurity
    {
        WindowsIdentity _currentUser;
        WindowsPrincipal _currentPrincipal;

        public CurrentUserSecurity()
        {
            _currentUser = WindowsIdentity.GetCurrent();
            _currentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
        }

        /// <summary>
        /// Checks whether the current user has the given access rights to a directory
        /// </summary>
        /// <param name="directory">DirectoryInfo of the given directory</param>
        /// <param name="right">FileSystemRights value, for the desired access type</param>
        /// <returns>Returns True if the current user has the given access to the directory, and False if not</returns>
        public bool HasAccess(DirectoryInfo directory, FileSystemRights right)
        {
            AuthorizationRuleCollection acl = directory.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
            return HasFileOrDirectoryAccess(right, acl);
        }

        /// <summary>
        /// Checks whether the current user has the given access rights to a file
        /// </summary>
        /// <param name="file">FileInfo of the given file</param>
        /// <param name="right">FileSystemRights value, for the desired access type</param>
        /// <returns>Returns True if the current user has the given access to the file, and False if not</returns>
        public bool HasAccess(FileInfo file, FileSystemRights right)
        {
            AuthorizationRuleCollection acl = file.GetAccessControl().GetAccessRules(true, true, typeof(SecurityIdentifier));
            return HasFileOrDirectoryAccess(right, acl);
        }

        /// <summary>
        /// Checks whether the current user has access rights based on an AuthorizationRuleCollection
        /// </summary>
        /// <param name="right">FileSystemRights value, for the desired access type</param>
        /// <param name="aclcollection">AuthorizationRuleCollection to match against</param>
        /// <returns>Returns True if the current user has the given access, and False if not</returns>
        private bool HasFileOrDirectoryAccess(FileSystemRights right, AuthorizationRuleCollection aclcollection)
        {
            bool allow = false;
            bool inheritedAllow = false;
            bool inheritedDeny = false;

            foreach (var acl in aclcollection)
            {
                FileSystemAccessRule currentRule = (FileSystemAccessRule)acl;
                if (_currentUser.User.Equals(currentRule.IdentityReference) ||
                    _currentPrincipal.IsInRole((SecurityIdentifier)currentRule.IdentityReference))
                {

                    if (currentRule.AccessControlType.Equals(AccessControlType.Deny))
                    {
                        if ((currentRule.FileSystemRights & right) == right)
                        {
                            if (currentRule.IsInherited)
                            {
                                inheritedDeny = true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    else if (currentRule.AccessControlType.Equals(AccessControlType.Allow))
                    {
                        if ((currentRule.FileSystemRights & right) == right)
                        {
                            if (currentRule.IsInherited)
                            {
                                inheritedAllow = true;
                            }
                            else
                            {
                                allow = true;
                            }
                        }
                    }
                }
            }
            if (allow)
            {
                return true;
            }
            return inheritedAllow && !inheritedDeny;
        }
    }
}
