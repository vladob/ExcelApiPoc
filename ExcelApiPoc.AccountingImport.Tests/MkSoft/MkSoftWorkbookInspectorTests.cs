using ExcelApiPoc.AccountingImport.Services.MkSoft;

namespace ExcelApiPoc.AccountingImport.Tests.MkSoft;

public sealed class MkSoftWorkbookInspectorTests
{
    [Fact]
    public void Classifies_general_ledger_signature()
    {
        string[] headers = CreateHeaders(
            MkSoftWorkbookInspector.GeneralLedgerFieldCount,
            "gl");

        Add(
            headers,
            "ucet",
            "nazov",
            "ucet3",
            "nazov3",
            "ucet1",
            "nazov1",
            "stredisko",
            "zakazka",
            "kodobratu",
            "pocstavm",
            "pocstavd",
            "obratym",
            "obratyd",
            "obratysm",
            "obratysd",
            "zostatokm",
            "zostatokd");

        Assert.Equal(
            MkSoftWorkbookKind.GeneralLedger,
            MkSoftWorkbookInspector.ClassifyHeaders(headers));
    }

    [Fact]
    public void Classifies_accounting_journal_signature()
    {
        string[] headers = CreateHeaders(
            MkSoftWorkbookInspector.AccountingJournalFieldCount,
            "aj");

        Add(
            headers,
            "id",
            "pc",
            "typ",
            "x_doklad",
            "datum",
            "datumpokl",
            "text",
            "firmaid",
            "specialkod",
            "id2",
            "dokladid",
            "pc1",
            "ucetm",
            "ucetd",
            "ciastka1m",
            "ciastka1d",
            "ciastka2m",
            "ciastka2d",
            "ciastkam",
            "ciastkad",
            "ciastkacmm",
            "ciastkacmd",
            "menam",
            "menad",
            "strediskom",
            "strediskod",
            "zakazkam",
            "zakazkad",
            "kodobratum",
            "kodobratud",
            "popis",
            "autogen");

        Assert.Equal(
            MkSoftWorkbookKind.AccountingJournal,
            MkSoftWorkbookInspector.ClassifyHeaders(headers));
    }

    [Fact]
    public void Rejects_incomplete_signature()
    {
        string[] headers = CreateHeaders(
            MkSoftWorkbookInspector.GeneralLedgerFieldCount,
            "gl");

        Add(
            headers,
            "ucet",
            "nazov",
            "pocstavm",
            "pocstavd",
            "obratym",
            "obratyd",
            "zostatokm");

        Assert.Equal(
            MkSoftWorkbookKind.Unknown,
            MkSoftWorkbookInspector.ClassifyHeaders(headers));
    }

    [Fact]
    public void Rejects_wrong_column_count()
    {
        string[] headers = CreateHeaders(
            MkSoftWorkbookInspector.GeneralLedgerFieldCount - 1,
            "gl");

        Add(
            headers,
            "ucet",
            "nazov",
            "ucet3",
            "nazov3",
            "ucet1",
            "nazov1",
            "stredisko",
            "zakazka",
            "kodobratu",
            "pocstavm",
            "pocstavd",
            "obratym",
            "obratyd",
            "obratysm",
            "obratysd",
            "zostatokm",
            "zostatokd");

        Assert.Equal(
            MkSoftWorkbookKind.Unknown,
            MkSoftWorkbookInspector.ClassifyHeaders(headers));
    }

    [Fact]
    public void Inspects_real_mksoft_general_ledger_when_configured()
    {
        string? path = Environment.GetEnvironmentVariable(
            "MKSOFT_GL_TEST_FILE");

        if (string.IsNullOrWhiteSpace(path))
            return;

        MkSoftWorkbookInspection result =
            MkSoftWorkbookInspector.Inspect(path);

        Assert.Equal(MkSoftWorkbookKind.GeneralLedger, result.Kind);
        Assert.Equal(41, result.FieldCount);
        Assert.Equal(191, result.DataRowCount);
        Assert.Contains("ucet", result.Headers);
        Assert.Contains("nazov", result.Headers);
        Assert.Contains("pocstavm", result.Headers);
        Assert.Contains("obratym", result.Headers);
        Assert.Contains("zostatokm", result.Headers);
    }

    [Fact]
    public void Inspects_real_mksoft_accounting_journal_when_configured()
    {
        string? path = Environment.GetEnvironmentVariable(
            "MKSOFT_AJ_TEST_FILE");

        if (string.IsNullOrWhiteSpace(path))
            return;

        MkSoftWorkbookInspection result =
            MkSoftWorkbookInspector.Inspect(path);

        Assert.Equal(MkSoftWorkbookKind.AccountingJournal, result.Kind);
        Assert.Equal(251, result.FieldCount);
        Assert.Equal(5654, result.DataRowCount);
        Assert.Contains("id", result.Headers);
        Assert.Contains("pc1", result.Headers);
        Assert.Contains("ucetm", result.Headers);
        Assert.Contains("ucetd", result.Headers);
        Assert.Contains("ciastkam", result.Headers);
        Assert.Contains("ciastkad", result.Headers);
    }

    private static string[] CreateHeaders(
        int count,
        string prefix)
    {
        return Enumerable.Range(0, count)
            .Select(index => prefix + "_unused_" + index)
            .ToArray();
    }

    private static void Add(
        string[] headers,
        params string[] required)
    {
        for (int index = 0; index < required.Length; index++)
            headers[index] = required[index];
    }
}
