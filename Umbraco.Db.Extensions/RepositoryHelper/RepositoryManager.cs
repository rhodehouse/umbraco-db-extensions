using System.Collections.Generic;
using Umbraco.Core;

namespace Umbraco.Db.Extensions
{
    public static class RepositoryManager
    {
        public static void Sync(ApplicationContext context, IEnumerable<IDatabaseSync> repositories)
        {
            var factory = new DatabaseFactory(context);
            foreach(var repo in repositories)
            {
                repo.Sync();
            }
        }

        public static void Uninstall(ApplicationContext context, IEnumerable<IDatabaseSync> repositories)
        {
            var factory = new DatabaseFactory(context);
            foreach (var repo in repositories)
            {
                repo.Uninstall();
            }
        }
    }
}
