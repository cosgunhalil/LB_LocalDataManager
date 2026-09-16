namespace LB.LocalDataManager
{
    /// <summary>
    /// Implement this on a saved type to stamp its schema version into the file, so a later
    /// build can tell which shape it is reading and migrate it. Types that do not implement
    /// it are written exactly as before, with no header.
    /// <code>
    /// [Serializable]
    /// public class PlayerData : IVersionedData
    /// {
    ///     public int SchemaVersion { get { return 2; } }
    ///     public int Id;
    ///     public string DisplayName;   // was "Name" in version 1
    /// }
    /// </code>
    /// </summary>
    /// <remarks>
    /// Raise the number whenever the shape changes in a way old files do not survive, and
    /// register an <see cref="IDataMigration"/> from the previous version to the new one.
    /// </remarks>
    public interface IVersionedData
    {
        /// <summary>
        /// Schema version this type currently writes. Starts at 1; keep it a constant, not
        /// something computed per instance.
        /// </summary>
        int SchemaVersion { get; }
    }
}
