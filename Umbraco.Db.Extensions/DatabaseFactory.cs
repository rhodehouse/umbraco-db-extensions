using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Umbraco.Db.Extensions
{
    public interface IDatabaseFactory
    {
        Database Database { get; }
        DatabaseSchemaHelper DatabaseHelper { get; }
    }

    public sealed class DatabaseFactory : IDatabaseFactory
    {
        public DatabaseFactory(ApplicationContext context)
        {
            Database = context.DatabaseContext.Database;
            DatabaseHelper = new DatabaseSchemaHelper
                (
                    context.DatabaseContext.Database,
                    context.ProfilingLogger.Logger,
                    context.DatabaseContext.SqlSyntax
                );
        }

        public Database Database { get; }

        public DatabaseSchemaHelper DatabaseHelper { get; }
    }
}
