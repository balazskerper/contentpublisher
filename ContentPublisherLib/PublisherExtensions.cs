using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ContentPublisherLib
{
    public static class PublisherExtensions
    {
        /// <summary>
        /// Creates a list of DirectoryInfo objects of all possible directories that are possible ancestors of a given directory
        /// </summary>
        /// <param name="directoryinfo">DirectoryInfo the function is called on</param>
        /// <returns>List of DirectoryInfo objects for each possible ancestor</returns>
        public static List<DirectoryInfo> ListDirectoryAncestors(this DirectoryInfo directoryinfo)
        {
            if (directoryinfo == null) return null;

            List<DirectoryInfo> dirinfos = new List<DirectoryInfo>();

            if (directoryinfo.Parent != null)
            {
                dirinfos.AddRange(ListDirectoryAncestors(directoryinfo.Parent));
            }
            dirinfos.Add(directoryinfo);

            return dirinfos;
        }

        /// <summary>
        /// Checks whether a string is null or empty
        /// </summary>
        /// <param name="text">String to check</param>
        /// <returns>True if the string is null or empty, False otherwise</returns>
        public static bool IsNullOrEmpty(this string text)
        {
            return String.IsNullOrEmpty(text);
        }

        /// <summary>
        /// Checks whether a string is null or contains whitespace only
        /// </summary>
        /// <param name="text">String to check</param>
        /// <returns>True if the string is null or contains only whitespace characters, False otherwise</returns>
        public static bool IsNullOrWhiteSpace(this string text)
        {
            return String.IsNullOrWhiteSpace(text);
        }

        /// <summary>
        /// Checks whether an IEnumerable<T> is null or empty 
        /// </summary>
        /// <typeparam name="T">Type contained in the IEnumerable</typeparam>
        /// <param name="enumerable">IEnumerable<T> to check</param>
        /// <returns>True if the IEnumerable<T> is null or empty, False otherwise</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null) return true;

            if (enumerable is ICollection<T> collection) return collection.Count < 1;

            return !enumerable.Any();
        }

        /// <summary>
        /// Returns the Decsription attribute value of an Enum if exists
        /// </summary>
        /// <param name="value">Enum to check</param>
        /// <returns>Description attribute value as string if the attribute is present, null if not</returns>
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    if (Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) is DescriptionAttribute attr)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }
    }
}
