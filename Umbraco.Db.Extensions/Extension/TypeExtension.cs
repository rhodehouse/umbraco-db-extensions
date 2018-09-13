using System;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Persistence;

namespace Umbraco.Db.Extensions
{
    public static partial class TypeExtensions
    {
        public static object Instantiate(this Type type)
        {
            var constructor = type.GetConstructors()
                    .FirstOrDefault(x => x.IsPublic && x.GetParameters().Length == 0);
            if (constructor != null)
            {
                return constructor.Invoke(new object[0]);
            }

            return null;
        }

        public static string GetTableName(this Type type)
        {
            var tableNameAttribute = type.GetCustomAttribute<TableNameAttribute>();
            var tableName = (tableNameAttribute != null) ? tableNameAttribute.Value : type.Name;
            return tableName;
        }
    }
}
