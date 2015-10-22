using System.IO;
using AzureEncryptedBlobStore.Audit;
using AzureEncryptedBlobStore.Config;
using Newtonsoft.Json;

namespace AzureEncryptedBlobStore
{
    public abstract class EncryptedJsonBlobStore<TData, TConfiguration> :
        EncryptedBlobStore<TData, TConfiguration>
        where TData : class
        where TConfiguration : IEncryptedBlobStoreConfiguration
    {
        protected EncryptedJsonBlobStore(TConfiguration configuration, IEncryptedBlobStoreAuditor auditor) : base(configuration, auditor)
        {
        }

        protected override Stream ToStream(TData data)
        {
            var serializer = new JsonSerializer();
            var s = new MemoryStream();
            var wtr = new StreamWriter(s);
            var writer = new JsonTextWriter(wtr);
            serializer.Serialize(writer, data);
            writer.Flush();
            s.Seek(0, SeekOrigin.Begin);
            return s;
        }

        protected override TData FromStream(Stream stream)
        {
            var serializer = new JsonSerializer();
            using (var rdr = new StreamReader(stream))
            {
                using (var jRdr = new JsonTextReader(rdr))
                {
                    var model = serializer.Deserialize<TData>(jRdr);
                    return model;
                }
            }
        }
    }

    public abstract class EncryptedJsonBlobStore<TData> :
        EncryptedJsonBlobStore<TData, EncryptedBlobStoreConfiguration>
        where TData : class
    {
        protected EncryptedJsonBlobStore(EncryptedBlobStoreConfiguration configuration, IEncryptedBlobStoreAuditor auditor) : base(configuration, auditor)
        {
        }

        protected EncryptedJsonBlobStore(EncryptedBlobStoreConfiguration configuration) : this(configuration, new DefaultEncryptedBlobStoreAuditor())
        {
        }
    }

}