using System;
using System.Linq;
using System.Reflection;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

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

        public static string GetPrimaryKeyName(this Type type)
        {
            var pk = type.GetCustomAttribute<PrimaryKeyAttribute>();
            if(pk != null)
            {
                return pk.Value;
            }
            var properties = type.GetProperties();
            foreach(var property in properties)
            {
                var attr = property.GetCustomAttribute<PrimaryKeyColumnAttribute>();
                if(attr != null)
                {
                    return property.Name;
                }
            }
            return null;
        }

        public static string GetSortOrderColumnName(this Type type)
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var attr = property.GetCustomAttribute<SortOrderAttribute>();
                if (attr != null)
                {
                    return property.Name;
                }
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
