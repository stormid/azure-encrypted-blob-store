using System;
using System.Collections.Generic;

namespace AzureEncryptedBlobStore
{
    public interface IEncryptedBlobStore<TData> where TData : class
    {
        /// <summary>
        /// Encrypts and saves
        /// </summary>
        /// <param name="data">data to be encrypted and saved</param>
        /// <returns>path to the saved data</returns>
        string Save(TData data);

        TData Retrieve(string path);

        IEnumerable<BlobStoreReference> List(string path = "");

        void Delete(string path);
    }
}