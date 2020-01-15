using System.Collections.Generic;

namespace Convey.HTTP
{
    public class HttpClientOptions
    {
        public string Type { get; set; }
        public int Retries { get; set; }
        public IDictionary<string, string> Services { get; set; }
        public RequestMaskingOptions RequestMasking { get; set; }

        public class RequestMaskingOptions
        {
            public bool Enabled { get; set; }
            public IEnumerable<string> UrlParts { get; set; }
            public string MaskTemplate { get; set; }
        }
    }
}