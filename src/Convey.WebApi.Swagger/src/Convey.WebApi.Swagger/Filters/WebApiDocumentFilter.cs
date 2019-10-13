using System;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Convey.WebApi.Swagger.Filters
{
    internal sealed class WebApiDocumentFilter : IDocumentFilter
    {
        private readonly WebApiEndpointDefinitions _definitions;
        private const string InBody = "body";
        private const string InQuery = "query";

        private readonly Func<PathItem, string, Operation> _getOperation = (item, path) =>
        {
            switch (path)
            {
                case "GET":
                    item.Get = new Operation();
                    return item.Get;
                case "POST":
                    item.Post = new Operation();
                    return item.Post;
                case "PUT":
                    item.Put = new Operation();
                    return item.Put;
                case "DELETE":
                    item.Delete = new Operation();
                    return item.Delete;                    
            }
            return null;
        };

        public WebApiDocumentFilter(WebApiEndpointDefinitions definitions)
            => _definitions = definitions;
        
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var definition in _definitions)
            {
                var pathItem = new PathItem();
                var operation = _getOperation(pathItem, definition.Method);
                operation.Responses = new Dictionary<string, Response>();
                operation.Parameters = new List<IParameter>();

                foreach (var parameter in definition.Parameters)
                {
                    if (parameter.In is InBody)
                    {
                        operation.Parameters.Add(new BodyParameter
                        {
                            Name = parameter.Name,
                            Schema = new Schema
                            {
                                Type = parameter.Type,
                                Example = parameter.Example
                            }
                        });
                    }
                    else if (parameter.In == InQuery)
                    {
                        operation.Parameters.Add(new NonBodyParameter
                        {
                            Name = parameter.Name,
                            In = parameter.In,
                            Type = parameter.Type
                        });
                    }
                }

                foreach (var response in definition.Responses)
                {
                    operation.Responses.Add(new KeyValuePair<string, Response>(
                        response.StatusCode.ToString(),
                        new Response
                        {
                            Schema = new Schema
                            {
                                Type = response.Type,
                                Example = response.Example
                            }
                        }));
                }
                
                swaggerDoc.Paths.Add(definition.Path, pathItem);
            }
        }
    }
}