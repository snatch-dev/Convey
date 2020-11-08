using System;

namespace Convey.Persistence.OpenStack.OCS.OpenStack.Responses
{
    internal class ObjectMetadata
    {
        public string hash { get; set; }
        public DateTime last_modified { get; set; }
        public long bytes { get; set; }
        public string name { get; set; }
        public string content_type { get; set; }
    }
}
