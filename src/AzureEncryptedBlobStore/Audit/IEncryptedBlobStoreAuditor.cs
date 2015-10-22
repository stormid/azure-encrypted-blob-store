using System;

namespace AzureEncryptedBlobStore.Audit
{
    public interface IBlobStoreAuditLogEntry
    {
        string Reference { get; }
        DateTimeOffset Timestamp { get; }
        string Category { get; }
        string Message { get; }
    }

    public interface IEncryptedBlobStoreAuditor
    {
        void Log(IBlobStoreAuditLogEntry log);
    }
}