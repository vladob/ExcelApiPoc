using System.Collections.Generic;
namespace PdfLayoutEngine.Simple;
public enum ImportLevel { Recognize = 1, Identify = 2, Extract = 3, Validate = 4, Normalize = 5 }
public sealed class ImportResult
{
    public ImportLevel RequestedLevel { get; set; }
    public int CompletedLevel { get; set; }
    public string Status { get; set; } = "unrecognized";
    public string? LayoutId { get; set; }
    public string? Category { get; set; }
    public string Format { get; set; } = "";
    public int PageCount { get; set; }
    public int DecodedPageCount { get; set; }
    public Dictionary<string,string?> Identifiers { get; set; } = new Dictionary<string,string?>();
    public List<SourceRow> Rows { get; set; } = new List<SourceRow>();
    public List<ImportIssue> Issues { get; set; } = new List<ImportIssue>();
}
public sealed class SourceRow
{
    public int Page { get; set; }
    public double BaselinePt { get; set; }
    public string Kind { get; set; } = "";
    public string Account { get; set; } = "";
    public string Name { get; set; } = "";
    public Dictionary<string,string> Fields { get; set; } = new Dictionary<string,string>();
    public Dictionary<string,decimal?> Amounts { get; set; } = new Dictionary<string,decimal?>();
}
public sealed class ImportIssue
{
    public string Code { get; set; } = "";
    public string Severity { get; set; } = "error";
    public int? Page { get; set; }
    public string Message { get; set; } = "";
}
