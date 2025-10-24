using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace Api.Gateway.WebClient.Swagger
{
    /// <summary>
    /// Operation filter para agregar el header Accept-Language a todas las operaciones en Swagger UI
    /// </summary>
    public class AcceptLanguageHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Description = "Idioma preferido para el contenido (es = Español, en = English). Default: es",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new OpenApiString("es"),
                    Enum = new List<IOpenApiAny>
                    {
                        new OpenApiString("es"),
                        new OpenApiString("en")
                    }
                }
            });
        }
    }
}
