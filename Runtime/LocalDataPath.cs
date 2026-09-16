using System;
using System.Text;
using UnityEngine;

namespace LB.LocalDataManager
{
    /// <summary>
    /// Resolves where local data files live. Both <see cref="LocalDataSaver"/> and
    /// <see cref="LocalDataLoader"/> go through here so the two can never drift apart.
    /// </summary>
    public static class LocalDataPath
    {
        /// <summary>Extension appended to every file name.</summary>
        public const string Extension = ".txt";

        /// <summary>
        /// Characters a file name may not contain. Windows rejects these outright while
        /// Linux and macOS accept most of them; they are rejected everywhere so a name
        /// that works on one platform cannot fail on another.
        /// </summary>
        private static readonly char[] InvalidCharacters = { '<', '>', ':', '"', '|', '?', '*' };

        /// <summary>
        /// Directory the data files are written to: <see cref="Application.persistentDataPath"/>
        /// on every platform, in the editor as well as in a player build.
        /// </summary>
        public static string RootDirectory
        {
            get { return Application.persistentDataPath; }
        }

        /// <summary>
        /// Full path for <paramref name="fileName"/>. Pass a bare name: the directory and the
        /// <c>.txt</c> extension are added here, so "PlayerData" - not "PlayerData.txt".
        /// Forward slashes make subfolders: "slots/autosave" is a file named "autosave" in a
        /// "slots" folder.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// <paramref name="fileName"/> is empty, absolute, contains a "." or ".." segment, or
        /// contains a character that is not portable across platforms.
        /// </exception>
        public static string GetPathFor(string fileName)
        {
            return RootDirectory + "/" + ValidateFileName(fileName) + Extension;
        }

        /// <summary>
        /// Checks <paramref name="fileName"/> against the rules above and returns it with
        /// separators normalized to "/".
        /// </summary>
        private static string ValidateFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName.Trim().Length == 0)
            {
                throw new ArgumentException("File name must not be empty.", "fileName");
            }

            var normalized = fileName.Replace('\\', '/');

            if (normalized[0] == '/' || normalized.Contains(":"))
            {
                throw new ArgumentException(
                    "File name must be relative to the data folder, but was '" + fileName + "'.",
                    "fileName");
            }

            var segments = normalized.Split('/');
            var rebuilt = new StringBuilder(normalized.Length);

            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i];

                if (segment.Length == 0)
                {
                    throw new ArgumentException(
                        "File name must not contain an empty folder name: '" + fileName + "'.",
                        "fileName");
                }

                if (segment == "." || segment == "..")
                {
                    throw new ArgumentException(
                        "File name must not navigate out of the data folder: '" + fileName + "'.",
                        "fileName");
                }

                if (segment.IndexOfAny(InvalidCharacters) >= 0 || HasControlCharacter(segment))
                {
                    throw new ArgumentException(
                        "File name contains a character that is not allowed on every platform: '"
                        + fileName + "'.",
                        "fileName");
                }

                var last = segment[segment.Length - 1];
                if (last == '.' || last == ' ')
                {
                    // Windows silently trims these, which would change the file a name maps to.
                    throw new ArgumentException(
                        "File name must not end with a dot or a space: '" + fileName + "'.",
                        "fileName");
                }

                if (i > 0)
                {
                    rebuilt.Append('/');
                }

                rebuilt.Append(segment);
            }

            return rebuilt.ToString();
        }

        private static bool HasControlCharacter(string segment)
        {
            for (var i = 0; i < segment.Length; i++)
            {
                if (char.IsControl(segment[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
