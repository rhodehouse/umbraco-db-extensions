namespace Umbraco.Db.Extensions
{
    internal class EncryptionResult
    {
        public EncryptionResult(string encrypted, string iv, byte[] key)
        {
            Encrypted = encrypted;
            IV = iv;
            Key = key;
        }

        public string Encrypted { get; }
        public string IV { get; }
        public byte[] Key { get; }
    }
}
