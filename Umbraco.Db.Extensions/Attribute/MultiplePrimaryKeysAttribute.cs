using System.Reflection;
using Umbraco.Core.Persistence;

namespace Umbraco.Db.Extensions
{
    public class MultiplePrimaryKeysAttribute : DbExtra
    {
        public string PrimaryKeyName { get; set; }
        public string[] PrimaryKeys { get; set; }        

        internal override void Process(Database db, TableData data, PropertyInfo property)
        {
            db.Execute($@"
                ALTER TABLE {data.TableName} 
                ADD CONSTRAINT {PrimaryKeyName} PRIMARY KEY ({string.Join(",", PrimaryKeys)})"
            );
        }
    }
}
