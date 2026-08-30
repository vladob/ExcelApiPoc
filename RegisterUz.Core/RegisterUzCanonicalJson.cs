using System.Buffers;
using System.Security.Cryptography;
using System.Text.Json;

namespace RegisterUz.Core;

/// <summary>
/// Produces a stable semantic representation of RegisterUZ JSON.
/// JSON object-property order and insignificant whitespace are ignored.
/// Relationship identifier arrays are treated as unordered multisets, while
/// every other array retains its source order.
/// </summary>
public static class RegisterUzCanonicalJson
{
    private static readonly HashSet<string> UnorderedRelationshipIdArrays =
        new(StringComparer.Ordinal)
        {
            "idUctovnychZavierok",
            "idVyrocnychSprav",
            "idUctovnychVykazov"
        };

    public static byte[] ComputeSha256(string json)
    {
        byte[] canonicalUtf8 = CanonicalizeToUtf8(json);
        return SHA256.HashData(canonicalUtf8);
    }

    public static byte[] CanonicalizeToUtf8(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using JsonDocument document = JsonDocument.Parse(json);
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions
               {
                   Indented = false,
                   SkipValidation = false
               }))
        {
            WriteElement(writer, document.RootElement, containingPropertyName: null);
        }

        return buffer.WrittenSpan.ToArray();
    }

    private static void WriteElement(
        Utf8JsonWriter writer,
        JsonElement element,
        string? containingPropertyName)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (JsonProperty property in element
                             .EnumerateObject()
                             .OrderBy(property => property.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteElement(writer, property.Value, property.Name);
                }
                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                IEnumerable<JsonElement> values = element.EnumerateArray().ToArray();
                if (containingPropertyName is not null &&
                    UnorderedRelationshipIdArrays.Contains(containingPropertyName))
                {
                    values = values
                        .Select(value => new
                        {
                            Element = value,
                            Id = ReadRelationshipId(value, containingPropertyName)
                        })
                        .OrderBy(value => value.Id)
                        .Select(value => value.Element)
                        .ToArray();
                }

                foreach (JsonElement value in values)
                    WriteElement(writer, value, containingPropertyName: null);
                writer.WriteEndArray();
                break;

            case JsonValueKind.String:
                writer.WriteStringValue(element.GetString());
                break;

            case JsonValueKind.Number:
                writer.WriteRawValue(element.GetRawText(), skipInputValidation: true);
                break;

            case JsonValueKind.True:
                writer.WriteBooleanValue(true);
                break;

            case JsonValueKind.False:
                writer.WriteBooleanValue(false);
                break;

            case JsonValueKind.Null:
                writer.WriteNullValue();
                break;

            default:
                throw new JsonException($"Unsupported JSON value kind: {element.ValueKind}.");
        }
    }

    private static long ReadRelationshipId(JsonElement value, string propertyName)
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt64(out long id))
        {
            throw new JsonException(
                $"RegisterUZ property '{propertyName}' must contain only integer identifiers.");
        }

        return id;
    }
}
