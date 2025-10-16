using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace Order.Api.Swagger
{
    public class ApiKeyOperationFilter : IOperationFilter
    {
        private readonly string _apiKey;

        public ApiKeyOperationFilter(string apiKey)
        {
            _apiKey = apiKey;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            // Agregar el parámetro ApiKey con el valor por defecto
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Api-Key",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new Microsoft.OpenApi.Any.OpenApiString(_apiKey)
                },
                Description = "API Key para comunicación entre servicios (pre-cargado desde configuración)"
            });
        }
    }
}
