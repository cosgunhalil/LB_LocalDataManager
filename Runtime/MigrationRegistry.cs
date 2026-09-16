using System;
using System.Collections.Generic;

namespace LB.LocalDataManager
{
    /// <summary>
    /// Holds the <see cref="IDataMigration"/>s registered per saved type and runs them in
    /// order when an old file is loaded.
    /// <code>
    /// LocalData.Migrations.Register&lt;PlayerData&gt;(new RenameNameToDisplayName());
    /// </code>
    /// </summary>
    public sealed class MigrationRegistry
    {
        /// <summary>
        /// Guards against a registration set that loops back on itself. Chains are short in
        /// practice: one step per schema version.
        /// </summary>
        private const int MaxSteps = 100;

        private readonly Dictionary<Type, Dictionary<int, IDataMigration>> _migrations =
            new Dictionary<Type, Dictionary<int, IDataMigration>>();

        /// <summary>
        /// Registers <paramref name="migration"/> for <typeparamref name="T"/>.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// The migration does not move the version forward, or another migration is already
        /// registered for the same type and source version.
        /// </exception>
        public void Register<T>(IDataMigration migration)
        {
            if (migration == null)
            {
                throw new ArgumentNullException("migration");
            }

            if (migration.ToVersion <= migration.FromVersion)
            {
                throw new ArgumentException(
                    "A migration must move the version forward, but goes from "
                    + migration.FromVersion + " to " + migration.ToVersion + ".",
                    "migration");
            }

            Dictionary<int, IDataMigration> forType;
            if (!_migrations.TryGetValue(typeof(T), out forType))
            {
                forType = new Dictionary<int, IDataMigration>();
                _migrations[typeof(T)] = forType;
            }

            if (forType.ContainsKey(migration.FromVersion))
            {
                throw new ArgumentException(
                    "A migration from version " + migration.FromVersion + " is already "
                    + "registered for " + typeof(T).Name + ".",
                    "migration");
            }

            forType[migration.FromVersion] = migration;
        }

        /// <summary>Forgets every registration. Mostly useful between tests.</summary>
        public void Clear()
        {
            _migrations.Clear();
        }

        /// <summary>
        /// Runs every migration that applies to <paramref name="data"/>, starting at
        /// <paramref name="version"/> and following the chain as far as it goes. Data at a
        /// version nothing is registered for comes back untouched.
        /// </summary>
        internal string Apply(Type type, int version, string data)
        {
            Dictionary<int, IDataMigration> forType;
            if (!_migrations.TryGetValue(type, out forType))
            {
                return data;
            }

            var steps = 0;

            IDataMigration migration;
            while (forType.TryGetValue(version, out migration))
            {
                data = migration.Migrate(data);
                version = migration.ToVersion;

                if (++steps > MaxSteps)
                {
                    throw new InvalidOperationException(
                        "Migrations for " + type.Name + " did not settle after " + MaxSteps
                        + " steps; the registered chain probably loops.");
                }
            }

            return data;
        }
    }
}
