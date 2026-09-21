using ExcelApiPoc.AccountingImport.Models;
using ExcelApiPoc.AccountingImport.Services.Ives;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesAccountingFrameworkImporterTests
{
    [Theory]
    [InlineData("UCT_ROZVRH_00325465_2025.xlsx")]
    [InlineData("UCT_ROZVRH_00325465_2025.csv")]
    [InlineData("UCT_ROZVRH_00325465_2025.xml")]
    [InlineData("UCT_ROZVRH_00325465_2025.pdf")]
    public void CanImport_RecognizesAllSupportedIvesFrameworkFormats(
        string fileName)
    {
        var importer = new IvesAccountingFrameworkImporter();

        Assert.True(importer.CanImport(fileName, "IVES"));
        Assert.True(importer.CanImport(fileName, "ives"));
        Assert.False(importer.CanImport(fileName, "IfoSoft"));
    }

    [Fact]
    public void CanImport_RejectsNonFrameworkIvesFile()
    {
        var importer = new IvesAccountingFrameworkImporter();

        Assert.False(importer.CanImport(
            "HL_KNIHA_00325465_2025.xml",
            "IVES"));
    }

    [Fact]
    public void Import_XmlFixture_MapsIvesSemanticsWithoutReusingIfoSoftFlags()
    {
        string path = CreateSyntheticXml();

        try
        {
            AccountingFrameworkImport result =
                new IvesAccountingFrameworkImporter().Import(path);

            Assert.Equal("XML", result.TechnicalType);
            Assert.Equal("IVES", result.AccountingFormat);
            Assert.Equal("00999999", result.Ico);
            Assert.Equal(2025, result.FiscalYear);
            Assert.Single(result.Rows);

            AccountingFrameworkRow row = result.Rows[0];

            Assert.Equal("021", row.SyntheticCode);
            Assert.Equal(".1.....š.", row.AnalyticalCode);
            Assert.Equal("021.1.....š.", row.AccountCode);
            Assert.Equal("Budovy", row.AccountName);
            Assert.Equal("H", row.ActivityCode);
            Assert.Equal("A", row.Type);
            Assert.Equal("A", row.PsFlag);
            Assert.Equal("N", row.BuFlag);
            Assert.Equal("N", row.RuFlag);
            Assert.Equal("N", row.PlFlag);
            Assert.Equal("EUR", row.Currency);
            Assert.Equal(new DateTime(2012, 9, 1), row.ValidFrom);
            Assert.Null(row.ValidTo);
            Assert.Equal(
                AccountingFrameworkRowKind.AnalyticalAccount,
                row.RowKind);

            Assert.Null(row.SubsidiaryFlag);
            Assert.Null(row.TaxFlag);
            Assert.Null(row.BalanceFlag);
            Assert.Null(row.VatFlag);
        }
        finally
        {
            string directory = Path.GetDirectoryName(path)!;
            if (File.Exists(path))
                File.Delete(path);
            if (Directory.Exists(directory))
                Directory.Delete(directory);
        }
    }

    private static string CreateSyntheticXml()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "ExcelApiPoc.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        string path = Path.Combine(
            directory,
            "UCT_ROZVRH_00999999_2025.xml");

        File.WriteAllText(
            path,
            """
<CrystalReport xmlns="urn:crystal-reports:schemas:report-detail">
  <Details Level="1">
    <Section SectionNumber="0">
      <Field Name="Field5" FieldName="{uctRozvrh.Nazov_uctu}">
        <Value>Budovy</Value>
      </Field>
      <Field Name="Field11" FieldName="{uctRozvrh.Platnost_od}">
        <Value>2012-09-01T00:00:00</Value>
      </Field>
      <Field Name="Field12" FieldName="{uctRozvrh.platnost_do}">
        <Value></Value>
      </Field>
      <Field Name="Field17" FieldName="{@ru}">
        <Value>N</Value>
      </Field>
      <Field Name="Field18" FieldName="{@PL}">
        <Value>N</Value>
      </Field>
      <Field Name="Field19" FieldName="{@BU}">
        <Value>N</Value>
      </Field>
      <Field Name="Field20" FieldName="{@PS}">
        <Value>A</Value>
      </Field>
      <Field Name="Field1" FieldName="{uctRozvrh.Druh_cinnosti}">
        <Value>H</Value>
      </Field>
      <Field Name="Field3" FieldName="{@au}">
        <Value>021.1    .      .    .       .   .š.</Value>
      </Field>
      <Field Name="Field13" FieldName="{uctRozvrh.Typ}">
        <Value>A</Value>
      </Field>
      <Field Name="Field14" FieldName="{uctRozvrh.Kod_meny}">
        <Value>EUR</Value>
      </Field>
    </Section>
  </Details>
</CrystalReport>
""");

        return path;
    }
}
