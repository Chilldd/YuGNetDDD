using System.Text.Json;
using System.Text.Json.Serialization;

namespace YuG.Api.Helpers;

/// <summary>
/// JSON 转换器工厂，将 long/long? 序列化为字符串（解决 JavaScript 精度丢失问题）
/// </summary>
public class LongToStringConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(long) || typeToConvert == typeof(long?);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(long))
            return new LongConverter();

        if (typeToConvert == typeof(long?))
            return new NullableLongConverter();

        return null;
    }

    private class LongConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return long.Parse(reader.GetString()!);

            return reader.GetInt64();
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    private class NullableLongConverter : JsonConverter<long?>
    {
        public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
                return long.Parse(reader.GetString()!);

            return reader.GetInt64();
        }

        public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value.Value.ToString());
        }
    }
}
