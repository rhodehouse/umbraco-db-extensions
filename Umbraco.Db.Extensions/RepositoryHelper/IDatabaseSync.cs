namespace Umbraco.Db.Extensions
{
    public interface IDatabaseSync
    {
        void Sync();
        void Uninstall();
    }
}
