using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using PdfLayoutEngine.Validation;

namespace PdfLayoutEngine.Definitions;

public sealed class LayoutDefinitionLoader
{
    private readonly LayoutDefinitionValidator _validator;
    private readonly JsonSerializerOptions _options;

    public LayoutDefinitionLoader()
        : this(new LayoutDefinitionValidator())
    {
    }

    public LayoutDefinitionLoader(LayoutDefinitionValidator validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
    }

    public LayoutLoadResult Load(string json)
    {
        if (json == null) throw new ArgumentNullException(nameof(json));
        try
        {
            var definition = JsonSerializer.Deserialize<LayoutDefinition>(json, _options);
            if (definition == null)
                return LayoutLoadResult.Failure(new ValidationMessage(ValidationSeverity.Error, "$", "The JSON document is empty."));

            return new LayoutLoadResult(definition, _validator.Validate(definition));
        }
        catch (JsonException exception)
        {
            return LayoutLoadResult.Failure(new ValidationMessage(ValidationSeverity.Error, exception.Path ?? "$", exception.Message));
        }
    }

    public LayoutLoadResult Load(Stream jsonStream)
    {
        if (jsonStream == null) throw new ArgumentNullException(nameof(jsonStream));
        using (var reader = new StreamReader(jsonStream, System.Text.Encoding.UTF8, true, 1024, true))
            return Load(reader.ReadToEnd());
    }
}

public sealed class LayoutLoadResult
{
    private readonly IReadOnlyList<ValidationMessage> _messages;

    internal LayoutLoadResult(LayoutDefinition? definition, IEnumerable<ValidationMessage> messages)
    {
        Definition = definition;
        _messages = Array.AsReadOnly(messages.ToArray());
    }

    public LayoutDefinition? Definition { get; }
    public IReadOnlyList<ValidationMessage> Messages => _messages;
    public bool IsValid => Definition != null && _messages.All(message => message.Severity != ValidationSeverity.Error);

    internal static LayoutLoadResult Failure(ValidationMessage message) => new LayoutLoadResult(null, new[] { message });
}
