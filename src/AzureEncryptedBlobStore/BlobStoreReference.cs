namespace AzureEncryptedBlobStore
{
    public struct BlobStoreReference
    {
        public BlobStoreReference(string path, bool isDirectory = false)
        {
            Path = path;
            IsDirectory = isDirectory;
        }

        public string Path { get; }
        public bool IsDirectory { get; set; }
    }
}