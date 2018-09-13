using System;
using System.Collections.Generic;

namespace Umbraco.Db.Extensions
{
    public abstract class DatabaseSync<T> : IDatabaseSync
    {
        public DatabaseSync(IEnumerable<Type> classes, DatabaseFactory factory)
        {
            _factory = factory;
        }

        private readonly DatabaseFactory _factory;

        public void Sync()
        {
            DatabaseManager.SyncTable<T>(_factory);
        }

        public void Uninstall()
        {
            DatabaseManager.Uninstall(_factory, typeof(T));
        }
    }
}
