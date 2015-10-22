using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AzureEncryptedBlobStore.Audit;
using AzureEncryptedBlobStore.Config;
using AzureEncryptionExtensions;
using AzureEncryptionExtensions.Providers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace AzureEncryptedBlobStore
{
    public abstract class EncryptedBlobStore<TData> :
        EncryptedBlobStore<TData, EncryptedBlobStoreConfiguration> 
        where TData : class
    {
        protected EncryptedBlobStore(EncryptedBlobStoreConfiguration configuration) : base(configuration, new DefaultEncryptedBlobStoreAuditor())
        {
        }
    }

    public abstract class EncryptedBlobStore<TData, TConfiguration> : IEncryptedBlobStore<TData>
        where TData : class 
        where TConfiguration : IEncryptedBlobStoreConfiguration
    {
        private readonly TConfiguration _configuration;
        private readonly IEncryptedBlobStoreAuditor _auditor;

        protected EncryptedBlobStore(TConfiguration configuration, IEncryptedBlobStoreAuditor auditor)
        {
            _configuration = configuration;
            _auditor = auditor;
        }

        private void Log(string id, string message, string category)
        {
            _auditor.Log(new DefaultBlobStoreAuditLogEntry(id, message, category));
        }

        public string Save(TData data)
        {
            var container = GetContainer(_configuration);

            if (!container.Exists())
            {
                container.Create(BlobContainerPublicAccessType.Off);
                Log(null, $"Created container {container.Name}", nameof(Save));
            }

            var path = Save(container, data);
            OnBlobSaveSuccess(data, path);
            return path;
        }

        public TData Retrieve(string path)
        {
            var container = GetContainer(_configuration);
            return Retrieve(container, path);
        }

        public IEnumerable<BlobStoreReference> List(string path)
        {
            var container = GetContainer(_configuration);
            var dir = container.GetDirectoryReference(path ?? "");
            if (dir != null)
            {
                return dir.ListBlobs().Select(b =>
                {
                    var localPath = b.StorageUri.PrimaryUri.LocalPath.Replace(b.Container.Name, string.Empty).Trim('/');
                    if (b is CloudBlobDirectory)
                    {
                        return new BlobStoreReference(localPath, true);
                    }
                    return new BlobStoreReference(localPath);
                });
            }
            return Enumerable.Empty<BlobStoreReference>();
        }

        public void Delete(string path)
        {
            var container = GetContainer(_configuration);
            Delete(container, path);
        }

        private void Delete(CloudBlobContainer container, string path)
        {
            var blob = container.GetBlockBlobReference(path);
            if (blob.Exists())
            {
                blob.Delete(DeleteSnapshotsOption.IncludeSnapshots);
                Log(blob.Name, $"Deleted blob {blob.StorageUri.PrimaryUri.LocalPath}", nameof(Delete));
            }
        }

        protected virtual CloudBlobContainer GetContainer(TConfiguration configuration)
        {
            var account = GetStorageAccount(_configuration);
            var client = account.CreateCloudBlobClient();
            var containerName = GetContainerReference(configuration);
            return client.GetContainerReference(containerName);
        }

        protected virtual CloudStorageAccount GetStorageAccount(TConfiguration configuration)
        {
            return CloudStorageAccount.Parse(_configuration.ConnectionString);
        }

        protected virtual string GetContainerReference(TConfiguration configuration)
        {
            return configuration.Container;
        }

        protected abstract Stream ToStream(TData data);

        protected abstract TData FromStream(Stream stream);

        protected virtual IBlobCryptoProvider GetCryptoProvider(TConfiguration configuration)
        {
            var keyBytes = GetEncryptionKey(configuration);
            return new SymmetricBlobCryptoProvider(keyBytes.ToArray());
        }

        protected virtual IEnumerable<byte> GetEncryptionKey(TConfiguration configuration)
        {
            return Regex.Matches(configuration.Key, "..").OfType<Match>().Select(m => Convert.ToByte(m.Value, 16));
        }

        protected virtual string GetBlobReference(TData data)
        {
            var path = DateTime.UtcNow.ToString("yyyy/MM/dd");
            var reference = GetBlobName(data);
            return $"{path}/{reference}";
        }

        protected virtual string GetBlobName(TData data)
        {
            return Guid.NewGuid().ToString();
        }

        private TData Retrieve(CloudBlobContainer container, string path)
        {
            var blob = container.GetBlockBlobReference(path);
            if (blob.Exists())
            {
                var provider = GetCryptoProvider(_configuration);
                var stream = new MemoryStream();
                blob.DownloadToStreamEncrypted(provider, stream);
                if (stream.CanRead)
                {
                    Log(blob.Name, $"Decrypted blob {blob.Name}", nameof(Retrieve));
                    stream.Seek(0, SeekOrigin.Begin);
                    return FromStream(stream);
                }
            }
            Log(blob.Name, $"blob not found : {blob.Name}", nameof(Retrieve));
            return null;
        }

        private string Save(CloudBlobContainer container, TData data)
        {
            var blob = container.GetBlockBlobReference(GetBlobReference(data));
            if(blob.Exists()) throw new InvalidOperationException($"a data stream with the given reference already exists (ref: {blob.Name}");

            var s = ToStream(data);
            var provider = GetCryptoProvider(_configuration);
            blob.UploadFromStreamEncrypted(provider, s);
            Log(blob.Name, $"Saved new blob {blob.Name}", nameof(Save));
            return blob.Name;
        }

        protected virtual void OnBlobSaveSuccess(TData data, string path)
        {
            
        }
    }
}