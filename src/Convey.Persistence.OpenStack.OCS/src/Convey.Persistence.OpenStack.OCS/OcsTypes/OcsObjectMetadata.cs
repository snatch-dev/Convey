using System;

namespace Convey.Persistence.OpenStack.OCS.OcsTypes
{
    public class OcsObjectMetadata
    {
        public string Hash { get; }
        public DateTime LastModified { get; }
        public long Bytes { get; }
        public string Name { get; }
        public string ContentType { get; }

        public OcsObjectMetadata(string hash, DateTime lastModified, long bytes, string name, string contentType)
        {
            Hash = hash;
            LastModified = lastModified;
            Bytes = bytes;
            Name = name;
            ContentType = contentType;
        }
    }
}
