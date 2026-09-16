namespace LB.LocalDataManager
{
    /// <summary>
    /// Brings a saved file from one schema version to the next. Migrations work on the
    /// serialized text, not on objects, because the old shape is exactly what no longer
    /// deserializes into the current type.
    /// </summary>
    /// <remarks>
    /// Register migrations with <see cref="MigrationRegistry.Register{T}"/>. On load they
    /// are chained: a file at version 1 with migrations 1-&gt;2 and 2-&gt;3 registered goes
    /// through both before the loader deserializes it.
    /// </remarks>
    public interface IDataMigration
    {
        /// <summary>Version of the data this migration accepts.</summary>
        int FromVersion { get; }

        /// <summary>Version the data is at once this migration has run. Must be higher than
        /// <see cref="FromVersion"/>.</summary>
        int ToVersion { get; }

        /// <summary>
        /// Rewrites <paramref name="data"/> from <see cref="FromVersion"/> to
        /// <see cref="ToVersion"/>. May throw; the loader treats that as a failed load.
        /// </summary>
        string Migrate(string data);
    }
}
