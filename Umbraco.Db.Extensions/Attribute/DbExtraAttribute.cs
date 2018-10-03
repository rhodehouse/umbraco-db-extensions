using System;
using System.Reflection;
using Umbraco.Core.Persistence;

namespace Umbraco.Db.Extensions
{
    public class DbExtraAttribute : DbExtra
    {
        public DbExtraType Type { get; set; }

        private void SetNvarCharMax(Database db, TableData data, PropertyInfo property)
        {
            db.Execute($"ALTER TABLE {data.TableName} ALTER COLUMN [{property.Name}] NVARCHAR(MAX)");
        }

        internal override void Process(Database db, TableData data, PropertyInfo property)
        {
            switch (Type)
            {
                case DbExtraType.NVarCharMax:
                    SetNvarCharMax(db, data, property);
                    break;
                default:
                    var name = Enum.GetName(typeof(DbExtraType), Type);
                    throw new NotImplementedException($"{name} has not yet been implemented");
            }
        }
    }

    public enum DbExtraType
    {
        NVarCharMax
    }
}
