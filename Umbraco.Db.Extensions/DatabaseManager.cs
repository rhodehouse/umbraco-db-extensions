using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace Umbraco.Db.Extensions
{
    public static class DatabaseManager
    {
        private static void AddForeignKeys(Database db, IEnumerable<ForeignKey> keys)
        {
            foreach (var key in keys)
            {
                var sql = $@"alter table {key.FKTABLE_NAME}
                            add constraint {key.FK_NAME}
                            foreign key ({key.FKCOLUMN_NAME}) 
                            references {key.PKTABLE_NAME} ({key.PKCOLUMN_NAME});";
                db.Execute(sql);
            }
        }

        private static void DeleteForeignKeys(Database db, IEnumerable<ForeignKey> keys)
        {
            foreach (var key in keys)
            {
                var sql = $@"alter table {key.FKTABLE_NAME}
                            drop constraint {key.FK_NAME}";
                db.Execute(sql);
            }
        }

        private static IEnumerable<string> GetConstraintsFromDbTable(Database db, string tableName)
        {
            var sql = $@"select constraint_name as [Name] 
                        from INFORMATION_SCHEMA.TABLE_CONSTRAINTS
                        where TABLE_NAME = '{tableName}'";
            var constraints = db.Query<QueryResult>(sql);
            return constraints.Select(x => x.Name);
        }

        private static Action GetDbExtraProcess
            (Database db, DbExtraAttribute attribute, TableData data, PropertyInfo property)
        {
            switch (attribute.Type)
            {
                case DbExtraType.MultiplePrimaryKeys:
                    var attr = (MultiplePrimaryKeysAttribute)attribute;
                    if (attr != null)
                    {
                        return () => db.Execute($@"
                            ALTER TABLE {data.TableName} 
                            ADD CONSTRAINT {attr.PrimaryKeyName} PRIMARY KEY ({string.Join(",", attr.PrimaryKeys)})");
                    }
                    break;
                case DbExtraType.NVarCharMax:
                    return () => db.Execute($"ALTER TABLE {data.TableName} ALTER COLUMN [{property.Name}] NVARCHAR(MAX)");
            }

            return () => { };
        }

        private static IEnumerable<ForeignKey> GetForeignKeys(Database db, string tableName)
        {
            return db.Query<ForeignKey>($";exec sp_fkeys '{tableName}'");
        }

        private static IEnumerable<string> GetPropertiesFromDbTable(Database db, string tableName)
        {
            var sql = $@"select 
	                        c.[name]
                        from
	                        sys.columns c
		                        inner join sys.objects o
			                        on o.object_id = c.object_id
				                        and o.[name] = '{tableName}'
                        order by
	                        o.[name], c.column_id";
            var columns = db.Query<QueryResult>(sql);
            return columns.Select(x => x.Name);
        }

        private static IEnumerable<string> GetDbPropertiesFromClass(Type type)
        {
            var properties = type.GetProperties()
                .Where(x => x.GetCustomAttribute<IgnoreAttribute>() == null)
                .Select(x => x.Name);
            return properties;
        }

        private static IEnumerable<T> GetTableCopy<T>(Database db)
        {
            var type = typeof(T);
            var tableName = type.GetTableName();
            var records = db.Query<T>($"select * from {tableName} with (nolock)");
            return records;
        }

        private static TableData GetTableData(Database db, Type type)
        {
            // get the table name                        
            var tableName = type.GetTableName();

            // get a list of the properties on the database associated with the provided class
            var dbProperties = GetPropertiesFromDbTable(db, tableName);

            // get the list of properties from the provided class
            var properties = GetDbPropertiesFromClass(type);

            var extraDbColumns = dbProperties.Except(properties);
            var missingDbColumns = (dbProperties.Any()) ? properties.Except(dbProperties) : new string[0];

            return new TableData
            {
                Exists = dbProperties.Any(),
                ExtraDbColumns = extraDbColumns,
                MissingDbColumns = missingDbColumns,
                TableName = tableName
            };
        }

        public static void SyncTable<T>(DatabaseFactory factory)
        {
            SyncTable(factory, new List<T>());
        }

        public static void SyncTable<T>(DatabaseFactory factory, List<T> startupRecords)
        {
            var db = factory.Database;
            var helper = factory.DatabaseHelper;
            var type = typeof(T);
            var data = GetTableData(db, typeof(T));

            if (!data.Exists || !data.IsInSync)
            {
                var copy = new List<T>();
                var foreignKeys = new List<ForeignKey>();

                if (!data.IsInSync)
                {
                    copy.AddRange(GetTableCopy<T>(db));
                    foreignKeys.AddRange(GetForeignKeys(db, data.TableName));
                    if (foreignKeys.Any())
                    {
                        DeleteForeignKeys(db, foreignKeys);
                    }
                    helper.DropTable(data.TableName);
                }
                else if (startupRecords.Count > 0)
                {
                    copy.AddRange(startupRecords);
                }

                helper.CreateTable(true, typeof(T));

                // take care of items that Umbraco cannot handle
                var attributeHelpers = type.GetProperties()
                    .Select(x => new AttributeHelper<DbExtraAttribute>(x))
                    .Where(x => x.Attribute != null);
                foreach (var a in attributeHelpers)
                {
                    GetDbExtraProcess(db, a.Attribute, data, a.Property)();
                }

                copy.ForEach((x) => db.Insert(x));

                if (foreignKeys.Any())
                {
                    AddForeignKeys(db, foreignKeys);
                }
            }
        }

        public static void SyncTable<T>(Database db, Func<string> fnGetSqlDelete, Func<string> fnGetSqlCreate)
        {
            SyncTable(db, fnGetSqlDelete, fnGetSqlCreate, new List<T>());
        }

        public static void SyncTable<T>
            (Database db, Func<string> fnGetSqlDelete, Func<string> fnGetSqlCreate, List<T> startupRecords)
        {
            var data = GetTableData(db, typeof(T));
            if (!data.Exists || !data.IsInSync)
            {
                var copy = new List<T>();

                if (!data.IsInSync)
                {
                    copy.AddRange(GetTableCopy<T>(db));
                    db.Execute(fnGetSqlDelete());
                }
                else
                {
                    copy.AddRange(startupRecords);
                }

                db.Execute(fnGetSqlCreate());
                copy.ForEach((x) => db.Insert(x));
            }
        }

        public static void Uninstall(DatabaseFactory factory, Type type)
        {
            var tableName = type.GetTableName();
            var foreignKeys = GetForeignKeys(factory.Database, tableName);
            if (foreignKeys.Any())
            {
                DeleteForeignKeys(factory.Database, foreignKeys);
            }
            factory.DatabaseHelper.DropTable(tableName);
        }
    }

    internal sealed class TableData
    {
        // whether or not the table exists on the database
        public bool Exists { get; set; }

        // columns on the database table that need to be removed
        public IEnumerable<string> ExtraDbColumns { get; set; }

        public bool IsInSync
        {
            get
            {
                return (ExtraDbColumns == null || !ExtraDbColumns.Any())
                    && (MissingDbColumns == null || !MissingDbColumns.Any());
            }
        }

        // columns that need to be added to the database table
        public IEnumerable<string> MissingDbColumns { get; set; }

        public string TableName { get; set; }
    }

    internal sealed class QueryResult
    {
        public string Name { get; set; }
    }

    internal sealed class AttributeHelper<T> where T : Attribute
    {
        public AttributeHelper() { }

        public AttributeHelper(PropertyInfo property)
        {
            Attribute = property.GetCustomAttribute<T>();
            Property = property;
        }

        public T Attribute { get; set; }

        public PropertyInfo Property { get; set; }
    }

    internal sealed class ForeignKey
    {
        public string PKTABLE_QUALIFIER { get; set; }
        public string PKTABLE_OWNER { get; set; }
        public string PKTABLE_NAME { get; set; }
        public string PKCOLUMN_NAME { get; set; }
        public string FKTABLE_QUALIFIER { get; set; }
        public string FKTABLE_OWNER { get; set; }
        public string FKTABLE_NAME { get; set; }
        public string FKCOLUMN_NAME { get; set; }
        public bool KEY_SEQ { get; set; }
        public bool UPDATE_RULE { get; set; }
        public bool DELETE_RULE { get; set; }
        public string FK_NAME { get; set; }
        public string PK_NAME { get; set; }
        public int DEFERRABILITY { get; set; }
    }
}
