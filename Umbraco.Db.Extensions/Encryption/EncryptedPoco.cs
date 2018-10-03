using System;

namespace Umbraco.Db.Extensions
{
    internal class EncryptedPoco
    {
        public int Id { get; set; }
        public string IV { get; set; }
        public string Data { get; set; }
        public DateTime Created { get; set; }
    }
}
