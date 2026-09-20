using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Ives;
using System.Text;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesXmlJournalParserTests
{
    [Fact]
    public void Parse_MapsCrystalDetailsAndAggregatesGroupFooters()
    {
        string path = CreateSyntheticXml();

        try
        {
            IvesJournalParseResult result =
                new IvesXmlJournalParser().Parse(path);

            Assert.Equal("00999999", result.Ico);
            Assert.Equal(2025, result.FiscalYear);
            Assert.Equal(new DateTime(2025, 1, 1), result.PeriodStart);
            Assert.Equal(new DateTime(2025, 12, 31), result.PeriodEnd);
            Assert.Equal(2, result.TransactionRows.Count);
            Assert.Empty(result.ModuleRows);
            Assert.Single(result.ReportTotalRows);

            IvesJournalSourceRow first = result.TransactionRows[0];
            Assert.Equal(new DateTime(2025, 1, 2), first.PostingDate);
            Assert.Equal("TEST001", first.DocumentNumber);
            Assert.Equal("518.100", first.DebitCompositeAccount);
            Assert.Equal("321.200", first.CreditCompositeAccount);
            Assert.Equal(100m, first.Amount);
            Assert.Equal("€", first.Currency);
            Assert.Equal("Synthetic first", first.Text);
            Assert.Equal("UCT", first.Module);
            Assert.Contains("XML detail 1", first.SourceLocation);

            Assert.Equal(
                150m,
                result.ReportTotalRows[0].ReportedAmounts.Single());
        }
        finally
        {
            DeleteSyntheticXml(path);
        }
    }

    [Fact]
    public void Import_UsesSharedIvesValidationAndCanonicalMapping()
    {
        string path = CreateSyntheticXml();

        try
        {
            var importer = new IvesJournalImporter();

            Assert.True(importer.CanImport(path, "IVES"));

            JournalImport result = importer.Import(path);

            Assert.Equal("XML", result.TechnicalType);
            Assert.Equal("IVES", result.AccountingFormat);
            Assert.Equal("00999999", result.Ico);
            Assert.Equal(2025, result.FiscalYear);
            Assert.Equal(2, result.Rows.Count);
            Assert.True(result.ImportReport.IsValid);
            Assert.Empty(result.ImportReport.Diagnostics);

            JournalRow first = result.Rows[0];
            Assert.Equal("518.100", first.DebitAccount);
            Assert.Equal("321.200", first.CreditAccount);
            Assert.Equal(100m, first.DebitAmount);
            Assert.Equal(100m, first.CreditAmount);
            Assert.Contains("XML detail 1", first.SourceLocation);
        }
        finally
        {
            DeleteSyntheticXml(path);
        }
    }

    private static string CreateSyntheticXml()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "ExcelApiPoc-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        string path = Path.Combine(
            directory,
            "U_DENNIK_00999999_2025.xml");

        File.WriteAllText(
            path,
            SyntheticXml(),
            new UTF8Encoding(false));

        return path;
    }

    private static string SyntheticXml()
    {
        return """
<?xml version="1.0" encoding="UTF-8"?>
<CrystalReport xmlns="urn:crystal-reports:schemas:report-detail">
  <Group Level="1">
    <Group Level="2">
      <Group Level="3">
        <Group Level="4">
          <Details Level="5">
            <Section SectionNumber="0">
              <Field FieldName="{uctDennik.Datum_vytv_pohybu}"><Value>2025-01-02T00:00:00</Value></Field>
              <Field FieldName="{uctDennik.Cislo_dokladu}"><Value>TEST001</Value></Field>
              <Field FieldName="{uctDennik.aU_Text_MD}"><Value>518.100</Value></Field>
              <Field FieldName="{uctDennik.aU_Text_Dal}"><Value>321.200</Value></Field>
              <Field FieldName="{uctDennik.Obnos}"><Value>100.00</Value></Field>
              <Field FieldName="{uctDennik.Symbol_meny}"><Value>€</Value></Field>
              <Field FieldName="{uctDennik.text_operacie}"><Value>Synthetic first</Value></Field>
            </Section>
            <Section SectionNumber="1">
              <Field FieldName="{uctDennik.Priznak_modulu}"><Value>UCT</Value></Field>
            </Section>
          </Details>
          <Details Level="5">
            <Section SectionNumber="0">
              <Field FieldName="{uctDennik.Datum_vytv_pohybu}"><Value>2025-01-03T00:00:00</Value></Field>
              <Field FieldName="{uctDennik.Cislo_dokladu}"><Value>TEST002</Value></Field>
              <Field FieldName="{uctDennik.aU_Text_MD}"><Value>321.200</Value></Field>
              <Field FieldName="{uctDennik.aU_Text_Dal}"><Value>518.100</Value></Field>
              <Field FieldName="{uctDennik.Obnos}"><Value>50.00</Value></Field>
              <Field FieldName="{uctDennik.Symbol_meny}"><Value>€</Value></Field>
              <Field FieldName="{uctDennik.text_operacie}"><Value>Synthetic second</Value></Field>
            </Section>
            <Section SectionNumber="1">
              <Field FieldName="{uctDennik.Priznak_modulu}"><Value>UCT</Value></Field>
            </Section>
          </Details>
        </Group>
      </Group>
      <GroupFooter>
        <Section SectionNumber="0">
          <Field Name="SumofObnos1" FieldName="Sum ({uctDennik.Obnos}, {uctDennik.Kod_meny})"><Value>100.00</Value></Field>
        </Section>
      </GroupFooter>
    </Group>
    <Group Level="2">
      <GroupFooter>
        <Section SectionNumber="0">
          <Field Name="SumofObnos1" FieldName="Sum ({uctDennik.Obnos}, {uctDennik.Kod_meny})"><Value>50.00</Value></Field>
        </Section>
      </GroupFooter>
    </Group>
  </Group>
</CrystalReport>
""";
    }

    private static void DeleteSyntheticXml(string path)
    {
        string directory = Path.GetDirectoryName(path)!;

        if (File.Exists(path))
            File.Delete(path);

        if (Directory.Exists(directory))
            Directory.Delete(directory);
    }
}
