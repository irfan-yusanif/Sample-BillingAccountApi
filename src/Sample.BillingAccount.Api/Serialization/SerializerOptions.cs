using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sample.BillingAccount.Api.Serialization;

public static class SerializerOptions
{
    static SerializerOptions()
    {
        Default = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Default.Converters.Add(new DateOnlyConverter());
        Default.Converters.Add(new JsonStringEnumConverter());
    }

    public static JsonSerializerOptions Default { get; }
}

