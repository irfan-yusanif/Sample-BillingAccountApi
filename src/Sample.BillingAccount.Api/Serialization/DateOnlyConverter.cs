﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sample.BillingAccount.Api.Serialization;

public class DateOnlyConverter : JsonConverter<DateOnly>
{
    private const string SerializationFormat = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return DateOnly.Parse(value!);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString(SerializationFormat));
}