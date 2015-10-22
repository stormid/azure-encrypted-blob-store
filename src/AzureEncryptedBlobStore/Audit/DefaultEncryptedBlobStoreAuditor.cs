using System;
using System.Diagnostics;

namespace AzureEncryptedBlobStore.Audit
{
    public class DefaultEncryptedBlobStoreAuditor : IEncryptedBlobStoreAuditor
    {
        protected virtual string GetIdentity()
        {
            return Environment.UserName;
        }

        protected virtual void LogCore(IBlobStoreAuditLogEntry log, string identity)
        {
            Trace.WriteLine($"{log.Reference} : {log.Message} by {identity}", log.Category);
        }

        public void Log(IBlobStoreAuditLogEntry log)
        {
            LogCore(log, GetIdentity());
        }
    }
}