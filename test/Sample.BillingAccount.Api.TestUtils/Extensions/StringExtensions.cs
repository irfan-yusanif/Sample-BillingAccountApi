using System.Text.RegularExpressions;

namespace Sample.BillingAccount.Api.TestUtils.Extensions;

public static class StringExtensions
{
    public static string ToStructuredLogMessage(this string logMessage, params object?[] values)
    {
        var rgx = new Regex("{[^0-9.]*?}");

        for (var i = 0; i <= values.Length; i++)
        {
            logMessage = rgx.Replace(logMessage, "{" + i + "}", 1);
        }

        return string.Format(logMessage, values);
    }
}