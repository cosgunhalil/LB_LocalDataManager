using System;
using System.Globalization;

namespace LB.LocalDataManager
{
    /// <summary>
    /// The one-line version marker written above the serialized payload:
    /// <c>#ldmv2</c> followed by a newline, then the data.
    /// </summary>
    /// <remarks>
    /// A header instead of a JSON envelope on purpose. Unity's serializer cannot round-trip
    /// a top-level generic type, so an <c>Envelope&lt;T&gt;</c> would force the payload to be
    /// double-encoded as an escaped string. A line prefix stays serializer-agnostic, costs a
    /// handful of bytes, and a file without it is unambiguously a pre-versioning save.
    /// <para>
    /// It is written before the <see cref="IDataProcessor"/> runs, so an encrypted save keeps
    /// its version inside the ciphertext and under the integrity check.
    /// </para>
    /// </remarks>
    internal static class SchemaHeader
    {
        private const string Marker = "#ldmv";
        private const char Separator = '\n';

        /// <summary>Version reported for a file written before versioning existed.</summary>
        internal const int NoVersion = 0;

        /// <summary>Prefixes <paramref name="data"/> with the marker for <paramref name="version"/>.</summary>
        internal static string Write(string data, int version)
        {
            return Marker + version.ToString(CultureInfo.InvariantCulture) + Separator + data;
        }

        /// <summary>
        /// Splits a stored file into its version and its payload. Text without a marker is
        /// returned as it is at <see cref="NoVersion"/>.
        /// </summary>
        internal static string Read(string stored, out int version)
        {
            version = NoVersion;

            if (string.IsNullOrEmpty(stored) || !stored.StartsWith(Marker, StringComparison.Ordinal))
            {
                return stored;
            }

            var lineEnd = stored.IndexOf(Separator);
            if (lineEnd < 0)
            {
                return stored;
            }

            var digits = stored.Substring(Marker.Length, lineEnd - Marker.Length);

            int parsed;
            if (!int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out parsed))
            {
                // Starts like a header but is not one; treat the whole thing as payload
                // rather than silently dropping its first line.
                return stored;
            }

            version = parsed;
            return stored.Substring(lineEnd + 1);
        }
    }
}
