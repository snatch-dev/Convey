using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Convey.WebApi.Swagger.Filters;

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
        var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

        foreach (var pathDefinition in _definitions.GroupBy(d => d.Path))
        {
            var pathItem = new OpenApiPathItem();

            foreach (var methodDefinition in pathDefinition)
            {
                var operation = _getOperation(pathItem, methodDefinition.Method);
                operation.Responses = new OpenApiResponses();
                operation.Parameters = new List<OpenApiParameter>();

                foreach (var parameter in methodDefinition.Parameters)
                {
                    if (parameter.In is InBody)
                    {
                        operation.RequestBody = new OpenApiRequestBody()
                        {
                            Content = new Dictionary<string, OpenApiMediaType>()
                            {
                                {
                                    "application/json", new OpenApiMediaType()
                                    {
                                        Schema = new OpenApiSchema
                                        {
                                            Type = parameter.Type.Name,
                                            Example = new OpenApiString(JsonSerializer.Serialize(parameter.Example,
                                                jsonSerializerOptions))
                                        }
                                    }
                                }
                            }
                        };
                    }
                    else if (parameter.In is InQuery)
                    {
                        if (parameter.Type.GetInterface("IQuery") is not null)
                        {
                            operation.RequestBody = new OpenApiRequestBody()
                            {
                                Content = new Dictionary<string, OpenApiMediaType>()
                                {
                                    {
                                        "application/json", new OpenApiMediaType()
                                        {
                                            Schema = new OpenApiSchema
                                            {
                                                Type = parameter.Type.Name,
                                                Example = new OpenApiString(
                                                    JsonSerializer.Serialize(parameter.Example,
                                                        jsonSerializerOptions))
                                            }
                                        }
                                    }
                                }
                            };
                        }
                        else
                        {
                            operation.Parameters.Add(new OpenApiParameter
                            {
                                Name = parameter.Name,
                                Schema = new OpenApiSchema
                                {
                                    Type = parameter.Type.Name,
                                    Example = new OpenApiString(JsonSerializer.Serialize(parameter.Example,
                                        jsonSerializerOptions))
                                }
                            });
                        }
                    }
                }

                foreach (var response in methodDefinition.Responses)
                {
                    operation.Responses.Add(response.StatusCode.ToString(), new OpenApiResponse
                    {
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                "application/json", new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Type = response.Type?.Name,
                                        Example = new OpenApiString(JsonSerializer.Serialize(response.Example,
                                            jsonSerializerOptions))
                                    }
                                }
                            }
                        }
                    });
                }
            }

            swaggerDoc.Paths.Add($"/{pathDefinition.Key}", pathItem);
        }
    }
}