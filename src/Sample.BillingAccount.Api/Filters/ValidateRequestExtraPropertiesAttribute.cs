using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sample.BillingAccount.Api.Constants;

namespace Sample.BillingAccount.Api.Filters;

public class ValidateRequestExtraPropertiesAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var conversationId = context.HttpContext.Request.Headers.TryGetValue(Headers.ConversationId, out var stringValues)
            ? stringValues.First()
            : null;

        if (string.IsNullOrWhiteSpace(conversationId))
        {
            context.Result = new BadRequestObjectResult("ConversationId must be specified");
        }
    }
}
