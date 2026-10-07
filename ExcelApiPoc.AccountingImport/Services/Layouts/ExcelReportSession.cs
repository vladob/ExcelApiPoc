using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using PdfLayoutEngine.Simple;

namespace ExcelApiPoc.AccountingImport.Services.Layouts
{
    // Column numbers are zero based; identifier row numbers are one based.
    public sealed class ExcelReportDefinition
    {
        public ExcelReportIdentifier[] Identifiers { get; set; } = Array.Empty<ExcelReportIdentifier>();
        public ExcelReportRule[] Rules { get; set; } = Array.Empty<ExcelReportRule>();
        public string[] CompactFields { get; set; } = Array.Empty<string>();
        public string[] TotalFields { get; set; } = Array.Empty<string>();
        public string[] RequiredFields { get; set; } = Array.Empty<string>();
    }
    public sealed class ExcelReportIdentifier
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Pattern { get; set; } = "";
    }
    public sealed class ExcelReportRule
    {
        public string Kind { get; set; } = "";
        public Dictionary<int, string> Match { get; set; } = new Dictionary<int, string>();
        public int[] AllowedColumns { get; set; } = Array.Empty<int>();
        public Dictionary<string, int> Columns { get; set; } = new Dictionary<string, int>();
        public bool AwaitAmounts { get; set; }
    }

    internal sealed class ExcelReportSession
    {
        readonly ExcelLayoutDefinition layout;
        readonly ExcelReportDefinition definition;
        readonly ImportResult result;
        static readonly string[] Kinds = { "Record", "Transaction", "Subtotal", "ReportTotal", "Continuation", "Amounts", "Group" };
        static string Cell(string[] row, int column) => column < row.Length ? (row[column] ?? "").Trim() : "";
        static bool Matches(string value, string pattern) => Regex.IsMatch(value, pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
        static string Field(SourceRow row, string name) => CompactSession.Value(row.Fields, name);
        void Issue(string code, string message, string severity = "error") => result.Issues.Add(new ImportIssue { Code = code, Message = message, Severity = severity });

        internal ExcelReportSession(ExcelLayoutDefinition layout, ImportResult result)
        {
            this.layout = layout; this.result = result; definition = layout.Report;
            if (definition.Rules.Length == 0 || definition.Rules.Any(r => !Kinds.Contains(r.Kind) || r.Match.Count == 0 || r.Match.Keys.Any(c => c < 0) || r.Columns.Values.Any(c => c < 0) || r.AllowedColumns.Any(c => c < 0)) ||
                definition.Identifiers.Any(i => i.Row < 1 || i.Row > layout.Headers.Length || i.Column < 0) || definition.TotalFields.Any(k => !layout.Amounts.Contains(k)))
                throw new InvalidDataException("Invalid Excel report layout " + layout.Id);
        }

        internal void Examine(ImportLevel level, Func<CancellationToken, IEnumerable<string[]>> read, CancellationToken token)
        {
            if (level >= ImportLevel.Identify && result.CompletedLevel < 2)
            {
                var head = read(token).Take(layout.Headers.Length).ToArray();
                foreach (var identifier in definition.Identifiers)
                {
                    var match = Regex.Match(Cell(head[identifier.Row - 1], identifier.Column), identifier.Pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
                    if (!match.Success) { Issue("invalidIdentifier", "Cannot read report metadata at worksheet row " + identifier.Row); continue; }
                    foreach (string key in new[] { "cin", "AccountingEntityName", "PeriodFrom", "PeriodTo" })
                        if (match.Groups[key].Success) result.Identifiers[key] = match.Groups[key].Value.Trim();
                }
                if (result.Identifiers.ContainsKey("PeriodFrom") || result.Identifiers.ContainsKey("PeriodTo"))
                {
                    if (!CompactSession.Date(CompactSession.Value(result.Identifiers, "PeriodFrom"), out var start) ||
                        !CompactSession.Date(CompactSession.Value(result.Identifiers, "PeriodTo"), out var end) || start > end || start.Year != end.Year)
                        Issue("invalidPeriod", "Expected one report fiscal year.");
                    else { result.Identifiers["fiscalYear"] = end.Year.ToString(CultureInfo.InvariantCulture); result.Identifiers["fiscalYearBasis"] = "report header"; }
                }
                result.CompletedLevel = 2;
            }
            if (level >= ImportLevel.Extract && result.CompletedLevel < 3)
            {
                SourceRow pending = null, previous = null;
                int number = layout.Headers.Length;
                string group = "";
                foreach (var cells in read(token).Skip(layout.Headers.Length))
                {
                    number++; token.ThrowIfCancellationRequested();
                    if (cells.All(string.IsNullOrWhiteSpace)) continue;
                    if (result.Rows.Count >= 500000) throw new InvalidDataException("Maximum source rows exceeded.");
                    var matches = definition.Rules.Where(r => r.Match.All(m => Matches(Cell(cells, m.Key), m.Value))).ToArray();
                    var row = new SourceRow { Kind = matches.Length == 1 ? matches[0].Kind : "Unknown" };
                    row.Fields["SourceLocation"] = "worksheet row " + number;
                    row.Fields["Group"] = group;
                    for (int c = 0; c < cells.Length; c++) row.Fields["source:" + (c + 1)] = cells[c];
                    result.Rows.Add(row);
                    if (matches.Length != 1) { Issue("unresolvedRow", row.Fields["SourceLocation"] + ": expected one row classification, found " + matches.Length); continue; }
                    var rule = matches[0];
                    if (rule.AllowedColumns.Length > 0 && cells.Where((value, c) => !string.IsNullOrWhiteSpace(value) && !rule.AllowedColumns.Contains(c)).Any())
                        Issue("unexpectedCells", row.Fields["SourceLocation"] + ": populated cells outside the declared layout.");
                    foreach (var column in layout.Columns) row.Fields[column.Key] = Cell(cells, column.Value);
                    foreach (var column in rule.Columns) row.Fields[column.Key] = Cell(cells, column.Value);
                    foreach (string key in definition.CompactFields) if (row.Fields.ContainsKey(key)) row.Fields[key] = Regex.Replace(row.Fields[key], @"\s", "");
                    if (row.Fields.TryGetValue("Date", out var rawDate) && DateTime.TryParseExact(rawDate, new[] { "yyyy-MM-dd", "d.M.yyyy", "dd.MM.yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                        row.Fields["Date"] = date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                    if (row.Kind == "Amounts")
                    {
                        if (pending == null) Issue("orphanAmounts", row.Fields["SourceLocation"]);
                        else
                        {
                            foreach (string key in layout.Amounts) pending.Fields[key] = Field(row, key);
                            pending.Fields["AmountSourceLocation"] = row.Fields["SourceLocation"];
                            pending = null;
                        }
                        continue;
                    }
                    if (pending != null) { Issue("missingAmounts", pending.Fields["SourceLocation"]); pending = null; }
                    if (row.Kind == "Continuation")
                    {
                        if (previous == null || previous.Kind != "Record") Issue("orphanContinuation", row.Fields["SourceLocation"]);
                        else foreach (var field in row.Fields) previous.Fields["continuation:" + number + ":" + field.Key] = field.Value;
                        previous = null;
                        continue;
                    }
                    previous = row.Kind == "Record" ? row : null;
                    if (row.Kind == "Group") { group = Field(row, "GroupName"); row.Fields["Group"] = group; }
                    row.Account = Field(row, "Account"); row.Name = Field(row, "Name");
                    if (row.Account.Length >= 3) { row.Fields["SyntheticAccount"] = row.Account.Substring(0, 3); row.Fields["AnalythicAccount"] = row.Account.Substring(3); }
                    if (rule.AwaitAmounts) pending = row;
                }
                if (pending != null) Issue("missingAmounts", pending.Fields["SourceLocation"]);
                result.CompletedLevel = 3;
            }
            if (level >= ImportLevel.Validate && result.CompletedLevel < 4)
            {
                var records = result.Rows.Where(r => r.Kind == "Record").ToArray();
                if (records.Length == 0) Issue("noData", "No importable report rows.");
                foreach (var row in result.Rows.Where(r => r.Kind == "Record" || r.Kind == "Subtotal" || r.Kind == "ReportTotal"))
                {
                    foreach (string key in layout.Amounts)
                    {
                        if (ExportControls.Money(Field(row, key), out var amount)) row.Amounts[key] = amount;
                        else { row.Amounts[key] = null; Issue("invalidAmount", row.Fields["SourceLocation"] + ": " + key); }
                    }
                    foreach (var sum in layout.AmountSums)
                        row.Amounts[sum.Key] = sum.Value.All(k => row.Amounts.TryGetValue(k, out var a) && a.HasValue) ? sum.Value.Sum(k => row.Amounts[k].Value) : (decimal?)null;
                    if (row.Kind != "Record") continue;
                    foreach (string key in definition.RequiredFields) if (Field(row, key).Length == 0) Issue("missingField", row.Fields["SourceLocation"] + ": " + key);
                    if (layout.Category == "AJ")
                    {
                        if (!CompactSession.Date(Field(row, "Date"), out var date) || date.Year < 1900) Issue("invalidDate", row.Fields["SourceLocation"], "warning");
                        if (Field(row, "DebitAccount").Length == 0 && Field(row, "CreditAccount").Length == 0) Issue("missingAccount", row.Fields["SourceLocation"]);
                    }
                    if (layout.Category == "GL" && new[] { "OpeningNetto", "TurnoverDebit", "TurnoverCredit", "ClosingNetto" }.All(k => row.Amounts.TryGetValue(k, out var a) && a.HasValue) &&
                        Math.Abs(row.Amounts["OpeningNetto"].Value + row.Amounts["TurnoverDebit"].Value - row.Amounts["TurnoverCredit"].Value - row.Amounts["ClosingNetto"].Value) > .01m)
                        Issue("balanceMismatch", row.Fields["SourceLocation"]);
                }
                if (definition.TotalFields.Length > 0)
                {
                    foreach (var group in records.GroupBy(r => Field(r, "Group")))
                    {
                        var totals = result.Rows.Where(r => r.Kind == "ReportTotal" && Field(r, "Group") == group.Key).ToArray();
                        if (totals.Length != 1) { Issue("missingReportTotal", "Expected one report total for " + group.Key); continue; }
                        foreach (string key in definition.TotalFields)
                            if (!totals[0].Amounts[key].HasValue || group.Any(r => !r.Amounts[key].HasValue) || Math.Abs(group.Sum(r => r.Amounts[key].Value) - totals[0].Amounts[key].Value) > .01m)
                                Issue("printedTotalMismatch", key + " in " + group.Key);
                    }
                }
                result.CompletedLevel = 4;
            }
            result.Status = result.Issues.Any(i => i.Severity == "error") ? "invalid" : "completed";
        }
    }
}
