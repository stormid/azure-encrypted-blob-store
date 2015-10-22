namespace AzureEncryptedBlobStore.Config
{
    public class EncryptedBlobStoreConfiguration : IEncryptedBlobStoreConfiguration
    {
        public EncryptedBlobStoreConfiguration(string connectionString, string key, string container)
        {
            ConnectionString = connectionString;
            Key = key;
            Container = container;
        }

        public string ConnectionString { get; }
        public string Key { get; }

        public string Container { get; set; }
    }
}