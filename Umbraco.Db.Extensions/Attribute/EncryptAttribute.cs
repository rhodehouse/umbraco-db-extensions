using System;

namespace Umbraco.Db.Extensions
{
    public class EncryptAttribute : Attribute
    {
        /// <summary>
        /// Name of property to save the key to
        /// </summary>
        public string KeyPropertyName { get; set; }
    }
}
