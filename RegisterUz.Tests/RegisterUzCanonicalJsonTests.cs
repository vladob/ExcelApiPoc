using RegisterUz.Core;
using Xunit;

namespace RegisterUz.Tests;

public sealed class RegisterUzCanonicalJsonTests
{
    [Fact]
    public void Property_order_and_whitespace_do_not_change_canonical_hash()
    {
        const string first = """
            { "id": 30514, "ico": "00325554", "konsolidovana": false }
            """;
        const string second = """{"konsolidovana":false,"ico":"00325554","id":30514}""";

        Assert.Equal(Hash(first), Hash(second));
    }

    [Theory]
    [InlineData("idUctovnychZavierok")]
    [InlineData("idVyrocnychSprav")]
    [InlineData("idUctovnychVykazov")]
    public void Relationship_identifier_order_does_not_change_canonical_hash(string propertyName)
    {
        string first = $"{{\"{propertyName}\":[30,10,20],\"id\":1}}";
        string second = $"{{\"id\":1,\"{propertyName}\":[20,30,10]}}";

        Assert.Equal(Hash(first), Hash(second));
    }

    [Fact]
    public void Financial_report_table_order_changes_canonical_hash()
    {
        const string first = """
            {"obsah":{"tabulky":[{"nazov":{"sk":"Aktíva"}},{"nazov":{"sk":"Pasíva"}}]}}
            """;
        const string second = """
            {"obsah":{"tabulky":[{"nazov":{"sk":"Pasíva"}},{"nazov":{"sk":"Aktíva"}}]}}
            """;

        Assert.NotEqual(Hash(first), Hash(second));
    }

    [Fact]
    public void Flattened_value_order_changes_canonical_hash()
    {
        const string first = """{"tabulky":[{"data":["1","2","3","4"]}]}""";
        const string second = """{"tabulky":[{"data":["1","3","2","4"]}]}""";

        Assert.NotEqual(Hash(first), Hash(second));
    }

    [Fact]
    public void Changed_scalar_value_changes_canonical_hash()
    {
        const string first = """{"id":30514,"nazovUJ":"Obec Oreské"}""";
        const string second = """{"id":30514,"nazovUJ":"Obec Oreské nové"}""";

        Assert.NotEqual(Hash(first), Hash(second));
    }

    [Fact]
    public void Duplicate_relationship_identifiers_are_not_silently_removed()
    {
        const string first = """{"idUctovnychVykazov":[10,20]}""";
        const string second = """{"idUctovnychVykazov":[10,20,20]}""";

        Assert.NotEqual(Hash(first), Hash(second));
    }

    private static string Hash(string json) =>
        Convert.ToHexString(RegisterUzCanonicalJson.ComputeSha256(json));
}
