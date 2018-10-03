using System;
using System.Reflection;
using Umbraco.Core.Persistence;

namespace Umbraco.Db.Extensions
{
    public abstract class DbExtra : Attribute
    {
        internal abstract void Process(Database db, TableData data, PropertyInfo property);
    }
}
