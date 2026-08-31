using System.Text.Json;
using System.Text.Json.Serialization;

namespace RegisterUz.Core;

public sealed class BooleanOrStringJsonConverter : JsonConverter<bool?>
{
    public override bool? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String when bool.TryParse(reader.GetString(), out bool value) => value,
            _ => throw new JsonException(
                "Expected null, a JSON boolean, or the string 'true' or 'false'.")
        };

    public override void Write(
        Utf8JsonWriter writer,
        bool? value,
        JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteBooleanValue(value.Value);
        else
            writer.WriteNullValue();
    }
}
