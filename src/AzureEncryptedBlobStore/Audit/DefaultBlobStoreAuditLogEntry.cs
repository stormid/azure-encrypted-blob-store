using System;

namespace AzureEncryptedBlobStore.Audit
{
    public class DefaultBlobStoreAuditLogEntry : IBlobStoreAuditLogEntry
    {
        public DefaultBlobStoreAuditLogEntry(string reference, string message, string category)
        {
            Reference = reference;
            Category = category;
            Message = message;
            Timestamp = DateTime.UtcNow;
        }

        public string Reference { get; }
        public DateTimeOffset Timestamp { get; }
        public string Category { get; }
        public string Message { get; }
    }
}