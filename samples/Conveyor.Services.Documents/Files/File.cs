using System;
using System.IO;

namespace Conveyor.Services.Documents.Files
{
    public abstract class File
    {
        public Guid DocumentId { get; }
        public string FileName { get; }
        protected File(Guid documentId, string fileName)
        {
            DocumentId = documentId;
            FileName = fileName;
        }
        public abstract byte[] AsRaw();
        public abstract Stream AsStream();
    }
}