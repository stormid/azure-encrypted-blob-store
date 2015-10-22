using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureEncryptedBlobStore.Audit
{
    public class AzureTableBlobStoreAuditor : DefaultEncryptedBlobStoreAuditor
    {
        private class LogEntry : TableEntity
        {
            private static string GetPartitionKey()
            {
                var now = DateTime.UtcNow;
                return new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc).Ticks.ToString();
            }
            public LogEntry() : base(GetPartitionKey(), Guid.NewGuid().ToString())
            {
                
            }

            public string Reference { get; set; }

            public string Identity { get; set; }

            public string Category { get; set; }
            public string Message { get; set; }
        }

        private readonly string _connectionString;
        private CloudTable _table;

        public AzureTableBlobStoreAuditor(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            EnsureTable(tableName.ToLowerInvariant());
        }

        private void EnsureTable(string tableName)
        {
            var account = CloudStorageAccount.Parse(_connectionString);
            var client = account.CreateCloudTableClient();
            _table = client.GetTableReference(tableName);
            _table.CreateIfNotExists();
            
        }

        protected override async void LogCore(IBlobStoreAuditLogEntry log, string identity)
        {
            var entry = new LogEntry
            {
                Reference = log.Reference,
                Identity = identity,
                Timestamp = log.Timestamp,
                Category = log.Category,
                Message = log.Message
            };

            var op = TableOperation.InsertOrReplace(entry);
            await _table.ExecuteAsync(op);
        }
    }
}