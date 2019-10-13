using System.Collections.Generic;

namespace Convey.WebApi
{
    public class WebApiEndpointDefinitions : List<WebApiEndpointDefinition>
    {
    }

    public class WebApiEndpointDefinition
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public IEnumerable<WebApiEndpointParameter> Parameters { get; set; } = new List<WebApiEndpointParameter>();
        public IEnumerable<WebApiEndpointResponse> Responses { get; set; } = new List<WebApiEndpointResponse>();
    }
    
    public class WebApiEndpointParameter
    {
        public string In { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public object Example { get; set; }
    }
    
    public class WebApiEndpointResponse
    {
        public string Type { get; set; }
        public int StatusCode { get; set; }
        public object Example { get; set; }
    }
}