using System.Security.Cryptography;

namespace AzureEncryptedBlobStore.Config
{
    public interface IEncryptedBlobStoreConfiguration
    {
        /// <summary>
        /// Azure storage connection string
        /// </summary>
        string ConnectionString { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>can be generated from openssl using command: openssl aes-256-cbc -P -nosalt</remarks>
        string Key { get; }

        string Container { get; }
    }
}