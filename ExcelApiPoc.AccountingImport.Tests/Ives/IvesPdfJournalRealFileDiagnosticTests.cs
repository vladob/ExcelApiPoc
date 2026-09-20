using ExcelApiPoc.AccountingImport.Services.Ives;
using PdfLayoutEngine.Definitions;
using PdfLayoutEngine.IText.Recognition;
using PdfLayoutEngine.Recognition;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesPdfJournalRealFileDiagnosticTests
{
    private const string EnvironmentVariable =
        "IVES_JOURNAL_PDF_TEST_FILE";

    private const string LayoutResourceName =
        "ExcelApiPoc.AccountingImport.PdfLayouts." +
        "Ives.journal.v1.json";

    private static readonly Regex IcoPattern = new(
        @"I\s*[ČC]\s*O\s*:\s*(?<ico>\d{8})",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex PeriodPattern = new(
        @"Dátum\s+vytvorenia\s+od:\s*(?<from>\d{2}\.\d{2}\.\d{4}),?\s*" +
        @"do:\s*(?<to>\d{2}\.\d{2}\.\d{4})",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private readonly ITestOutputHelper output;

    public IvesPdfJournalRealFileDiagnosticTests(
        ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Recognize_real_pdf_structure_from_environment_variable()
    {
        string? path =
            Environment.GetEnvironmentVariable(EnvironmentVariable);

        if (string.IsNullOrWhiteSpace(path))
        {
            output.WriteLine(
                EnvironmentVariable + " is not set.");
            return;
        }

        Assert.True(
            File.Exists(path),
            "PDF file does not exist: " + path);

        LayoutDefinition layout = LoadLayout();

        LayoutRecognitionResult result =
            new ITextLayoutRecognizer().Recognize(path, layout);

        string[] icos = result.Document.Tokens
            .Select(token => IcoPattern.Match(token.Text))
            .Where(match => match.Success)
            .Select(match => match.Groups["ico"].Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        string[] periods = result.Document.Pages
            .Select(page => string.Join(
                " ",
                page.Tokens
                    .OrderByDescending(token => token.Baseline)
                    .ThenBy(token => token.Left)
                    .Select(token => token.Text.Trim())
                    .Where(text => text.Length > 0)))
            .Select(text => PeriodPattern.Match(text))
            .Where(match => match.Success)
            .Select(match =>
                match.Groups["from"].Value +
                " .. " +
                match.Groups["to"].Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        int sectionCount = result.Sections.Sum(
            item => item.Sections.Count);

        var records = result.RecordDiscovery.Records.ToArray();

        output.WriteLine("IVES real PDF structure diagnostic");
        output.WriteLine("----------------------------------");
        output.WriteLine(
            "File             : {0}",
            Path.GetFileName(path));
        output.WriteLine(
            "Layout           : {0}",
            layout.Id);
        output.WriteLine(
            "Pages            : {0}",
            result.Document.Pages.Count);
        output.WriteLine(
            "Body sections    : {0}",
            sectionCount);
        output.WriteLine(
            "Baseline records : {0}",
            records.Length);
        output.WriteLine(
            "Diagnostics      : {0}",
            result.Diagnostics.Count);
        output.WriteLine(
            "IČO values       : {0}",
            string.Join(", ", icos));
        output.WriteLine(
            "Periods          : {0}",
            string.Join(", ", periods));

        output.WriteLine("");
        output.WriteLine("Record shapes:");

        foreach (var shape in records
                     .GroupBy(record => new
                     {
                         Groups = record.Groups.Count,
                         Tokens = record.SourceTokens.Count,
                         record.CrossesPageBoundary
                     })
                     .OrderByDescending(group => group.Count())
                     .Take(20))
        {
            output.WriteLine(
                "  groups {0}, tokens {1}, crosses page {2}: {3}",
                shape.Key.Groups,
                shape.Key.Tokens,
                shape.Key.CrossesPageBoundary,
                shape.Count());
        }

        output.WriteLine("");
        output.WriteLine("First body records:");

        foreach (var record in records.Take(12))
        {
            output.WriteLine(
                "  page {0}, baseline {1:F3}, left {2:F3}, right {3:F3}: {4}",
                record.StartPageNumber,
                record.Groups[0].Baseline,
                record.Left,
                record.Right,
                record.Text);
        }

        Assert.Equal(219, result.Document.Pages.Count);
        Assert.Equal(219, sectionCount);
        Assert.Single(icos);
        Assert.Equal("00325465", icos[0]);
        Assert.Single(periods);
        Assert.Equal(
            "01.01.2025 .. 31.12.2025",
            periods[0]);
    }

    private static LayoutDefinition LoadLayout()
    {
        Assembly assembly =
            typeof(IvesJournalImporter).Assembly;

        using Stream? stream =
            assembly.GetManifestResourceStream(LayoutResourceName);

        Assert.NotNull(stream);

        LayoutLoadResult result =
            new LayoutDefinitionLoader().Load(stream!);

        Assert.True(
            result.IsValid,
            string.Join(
                Environment.NewLine,
                result.Messages.Select(message => message.Message)));

        return Assert.IsType<LayoutDefinition>(
            result.Definition);
    }
}
