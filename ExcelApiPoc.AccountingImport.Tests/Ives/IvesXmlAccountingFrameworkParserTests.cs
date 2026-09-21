using ExcelApiPoc.AccountingImport.Services.Ives;

namespace ExcelApiPoc.AccountingImport.Tests.Ives;

public sealed class IvesXmlAccountingFrameworkParserTests
{
    [Fact]
    public void Parse_MapsCrystalDetailsToAccountingFrameworkRows()
    {
        string path = CreateSyntheticXml();

        try
        {
            IvesAccountingFrameworkParseResult result =
                new IvesXmlAccountingFrameworkParser().Parse(path);

            Assert.Equal("00999999", result.Ico);
            Assert.Equal(2025, result.FiscalYear);
            Assert.Equal(2, result.SourceRowCount);
            Assert.Equal(2, result.Rows.Count);

            IvesAccountingFrameworkSourceRow first =
                result.Rows[0];

            Assert.Equal(1, first.SequenceNumber);
            Assert.Equal(1, first.SourceRowNumber);
            Assert.Equal(
                "021.1    .      .    .       .   .š.",
                first.SourceAccountCode);
            Assert.Equal(
                "021.1.....š.",
                first.AccountCode);
            Assert.Equal("Budovy", first.AccountName);
            Assert.Equal("H", first.ActivityCode);
            Assert.Equal("A", first.Type);
            Assert.Equal("A", first.PsFlag);
            Assert.Equal("N", first.BuFlag);
            Assert.Equal("N", first.RuFlag);
            Assert.Equal("N", first.PlFlag);
            Assert.Equal("EUR", first.Currency);
            Assert.Equal(
                new DateTime(2012, 9, 1),
                first.ValidFrom);
            Assert.Null(first.ValidTo);

            IvesAccountingFrameworkSourceRow second =
                result.Rows[1];

            Assert.Equal(
                new DateTime(2008, 1, 1),
                second.ValidFrom);
            Assert.Equal(
                new DateTime(2015, 12, 31),
                second.ValidTo);
        }
        finally
        {
            DeleteSyntheticXml(path);
        }
    }

    [Fact]
    public void Parse_RejectsWrongDocumentKindInFilename()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            "ExcelApiPoc.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        string path = Path.Combine(
            directory,
            "U_DENNIK_00999999_2025.xml");

        File.WriteAllText(path, SyntheticXml());

        try
        {
            InvalidDataException exception =
                Assert.Throws<InvalidDataException>(
                    () =>
                        new IvesXmlAccountingFrameworkParser()
                            .Parse(path));

            Assert.Contains(
                "does not identify an IVES accounting framework",
                exception.Message);
        }
        finally
        {
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

        File.WriteAllText(path, SyntheticXml());
        return path;
    }

    private static string SyntheticXml() =>
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
  <Details Level="1">
    <Section SectionNumber="0">
      <Field Name="Field5" FieldName="{uctRozvrh.Nazov_uctu}">
        <Value>Old account</Value>
      </Field>
      <Field Name="Field11" FieldName="{uctRozvrh.Platnost_od}">
        <Value>2008-01-01T00:00:00</Value>
      </Field>
      <Field Name="Field12" FieldName="{uctRozvrh.platnost_do}">
        <Value>2015-12-31T00:00:00</Value>
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
        <Value>N</Value>
      </Field>
      <Field Name="Field1" FieldName="{uctRozvrh.Druh_cinnosti}">
        <Value>H</Value>
      </Field>
      <Field Name="Field3" FieldName="{@au}">
        <Value>021.1 1  .019   .    .       .   . .</Value>
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
""";

    private static void DeleteSyntheticXml(string path)
    {
        string directory = Path.GetDirectoryName(path)!;

        if (File.Exists(path))
            File.Delete(path);

        if (Directory.Exists(directory))
            Directory.Delete(directory);
    }
}
