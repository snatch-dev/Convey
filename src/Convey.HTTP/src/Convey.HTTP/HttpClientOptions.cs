using System.Collections.Generic;

namespace Convey.HTTP
{
    public class HttpClientOptions
    {
        public string Type { get; set; }
        public int Retries { get; set; }
        public IDictionary<string, string> Services { get; set; }
    }
}