using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using AzureEncryptedBlobStore;
using AzureEncryptedBlobStore.Audit;
using AzureEncryptedBlobStore.Config;
using Newtonsoft.Json;

namespace Sample
{
    public class DataModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public DateTime Date { get; set; }
    }

    public class StreamBlobStore : EncryptedBlobStore<Stream, EncryptedBlobStoreConfiguration>
    {
        public StreamBlobStore(EncryptedBlobStoreConfiguration configuration, IEncryptedBlobStoreAuditor auditor) : base(configuration, auditor)
        {
        }

        protected override Stream ToStream(Stream data)
        {
            return data;
        }

        protected override Stream FromStream(Stream stream)
        {
            return stream;
        }
    }

    public class FoiRequestBlobStore : EncryptedJsonBlobStore<DataModel>
    {
        public FoiRequestBlobStore(string connectionString, string key, string container) : base(new EncryptedBlobStoreConfiguration(connectionString, key, container), new AzureTableBlobStoreAuditor(connectionString, container))
        {
        }

        protected override string GetBlobName(DataModel data)
        {
            return data.Id.ToString();
        }
    }
    
    class Program
    {
        private static void Main(string[] args)
        {
            var a = new AesCryptoServiceProvider();

            var connectionString = ConfigurationManager.ConnectionStrings["storageAccount"].ConnectionString;
            var key = ConfigurationManager.AppSettings["aesKey"] ??
                      BitConverter.ToString(a.Key).Replace("-", string.Empty);

            var blobStore = new FoiRequestBlobStore(connectionString, key, "foirequest");
            var model = new DataModel()
            {
                Id = Guid.NewGuid(),
                Title = "Hello",
                Summary = "Hello world!",
                Date = DateTime.UtcNow
            };

            var path = Save(blobStore, model);
            var retrieved = Retrieve(blobStore, path);

            Display(retrieved);
            List(blobStore);

            blobStore.Delete(path);

            List(blobStore);

            Console.ReadKey(true);
        }

        private static void Display(DataModel retrieved)
        {
            Console.WriteLine(JsonConvert.SerializeObject(retrieved, Formatting.Indented));
        }

        private static void List<TData>(IEncryptedBlobStore<TData> blobStore) where TData : class
        {
            foreach (var file in blobStore.List(DateTime.UtcNow.Date.ToString("yyyy/MM/dd")))
            {
                Console.WriteLine(file.Path);
            }
        }
        
        private static TData Retrieve<TData>(IEncryptedBlobStore<TData> blobStore, string path) where TData : class
        {
            var data = blobStore.Retrieve(path);
            Console.WriteLine("--- Retrieved Data ---");
            return data;
        }

        private static string Save<TData>(IEncryptedBlobStore<TData> blobStore, TData model) where TData : class
        {
            var path = blobStore.Save(model);

            Console.WriteLine("--- Saved Data ---");
            return path;
        }
    }
}
