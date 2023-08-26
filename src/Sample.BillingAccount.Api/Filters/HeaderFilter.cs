using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Sample.BillingAccount.Api.Constants;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Sample.BillingAccount.Api.Filters;

public class HeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters.Add(
            new OpenApiParameter
            {
                Description = "ConversationId Header used for the request",
                In = ParameterLocation.Header,
                Name = Headers.ConversationId,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" },
                Example = new OpenApiString(Guid.NewGuid().ToString())
            });
    }
}
