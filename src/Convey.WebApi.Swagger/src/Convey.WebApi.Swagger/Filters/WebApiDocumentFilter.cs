using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Convey.WebApi.Swagger.Filters
{
    internal sealed class WebApiDocumentFilter : IDocumentFilter
    {
        private readonly WebApiEndpointDefinitions _definitions;
        private const string InBody = "body";
        private const string InQuery = "query";

        private readonly Func<OpenApiPathItem, string, OpenApiOperation> _getOperation = (item, path) =>
        {
            switch (path)
            {
                case "GET":
                    item.AddOperation(OperationType.Get, new OpenApiOperation());
                    return item.Operations[OperationType.Get];
                case "POST":
                    item.AddOperation(OperationType.Post, new OpenApiOperation());
                    return item.Operations[OperationType.Post];
                case "PUT":
                    item.AddOperation(OperationType.Put, new OpenApiOperation());
                    return item.Operations[OperationType.Put];
                case "DELETE":
                    item.AddOperation(OperationType.Delete, new OpenApiOperation());
                    return item.Operations[OperationType.Delete]; 
            }
            return null;
        };

        public WebApiDocumentFilter(WebApiEndpointDefinitions definitions)
            => _definitions = definitions;
        
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var definition in _definitions)
            {
                var pathItem = new OpenApiPathItem();
                var operation = _getOperation(pathItem, definition.Method);
                operation.Responses = new OpenApiResponses();
                operation.Parameters = new List<OpenApiParameter>();

                foreach (var parameter in definition.Parameters)
                {
                    if (parameter.In is InBody)
                    {
                        operation.Parameters.Add(new OpenApiParameter
                        {
                            Name = parameter.Name,
                            Schema = new OpenApiSchema
                            {
                                Type = parameter.Type,
                                Example = new OpenApiString(JsonConvert.SerializeObject(parameter.Example))
                            }
                        });
                    }
                    else if (parameter.In is InQuery)
                    {
                        operation.Parameters.Add(new OpenApiParameter
                        {
                            Name = parameter.Name,
                            Schema = new OpenApiSchema
                            {
                                Type = parameter.Type,
                                Example = new OpenApiString(JsonConvert.SerializeObject(parameter.Example))
                            }
                        });
                    }
                }

                foreach (var response in definition.Responses)
                {
                    operation.Responses.Add(response.StatusCode.ToString(), new OpenApiResponse
                    {
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            { "body", new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Type = response.Type,
                                        Example = new OpenApiString(JsonConvert.SerializeObject(response.Example))
                                    }
                                }
                            }
                        }
                    });
                }
                
                swaggerDoc.Paths.Add($"/{definition.Path}", pathItem);
            }
        }
    }
}