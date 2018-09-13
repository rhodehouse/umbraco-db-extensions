using System;

namespace Umbraco.Db.Extensions
{
    public class DbExtraAttribute : Attribute
    {
        public DbExtraType Type { get; set; }
    }

    public class MultiplePrimaryKeysAttribute : DbExtraAttribute
    {
        public string PrimaryKeyName { get; set; }
        public string[] PrimaryKeys { get; set; }
        public new DbExtraType Type => DbExtraType.MultiplePrimaryKeys;
    }

    public enum DbExtraType
    {
        MultiplePrimaryKeys,
        NVarCharMax
    }
}
