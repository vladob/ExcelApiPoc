using ExcelApiPoc.AddIn.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class PerformanceMaterialityWorksheetWriter
    {
        public const string WorksheetName = "PerformanceMat";
        private const string MetadataName = "__PerformanceMatMetadata";

        public static void AddWorksheet(Excel.Workbook workbook,
            AccountingEntityPackageEnvelope package, int fiscalYear, long? selectedStatementId = null)
        {
            string path = Path.Combine(Path.GetTempPath(), "PerformanceMat-" + Guid.NewGuid().ToString("N") + ".xlsx");
            Excel.Workbook template = null;
            var autoCorrect = workbook.Application.AutoCorrect;
            bool previousAutoFill = autoCorrect.AutoFillFormulasInLists;
            try
            {
                // Each materiality row has its own source. Excel must not propagate
                // the last formula (often =NA()) over the other table rows.
                autoCorrect.AutoFillFormulasInLists = false;
                using (var source = typeof(PerformanceMaterialityWorksheetWriter).Assembly
                    .GetManifestResourceStream("ExcelApiPoc.AddIn.Templates.PerformanceMateriality.xlsx"))
                using (var target = File.Create(path))
                {
                    if (source == null) throw new InvalidOperationException("Performance materiality template is missing.");
                    source.CopyTo(target);
                }
                template = workbook.Application.Workbooks.Open(path, UpdateLinks: 0, ReadOnly: true,
                    AddToMru: false, IgnoreReadOnlyRecommended: true);
                // Copy both sheets together so structured references remain inside the destination workbook.
                template.Worksheets.Copy(After: workbook.Worksheets[workbook.Worksheets.Count]);
                var sheet = (Excel.Worksheet)workbook.Worksheets["ISA320"];
                var metadata = (Excel.Worksheet)workbook.Worksheets["Metadata"];
                sheet.Name = WorksheetName;
                metadata.Name = MetadataName;
                metadata.Visible = Excel.XlSheetVisibility.xlSheetVeryHidden;
                BindName(workbook, "SelectedSignificance", "='PerformanceMat'!$B$48");
                BindName(workbook, "Hodnota", "='PerformanceMat'!$B$49");
                BindName(workbook, "VyznamnostPercent", "='PerformanceMat'!$B$52");
                BindName(workbook, "ValidateSource", "=ValuesOverview[Zdroj]");
                BindName(workbook, "SelectionForSource", "=TableSources[Zdroj]");
                foreach (Excel.Name name in workbook.Names)
                    if (name.Name == "Zdroj" && Convert.ToString(name.RefersTo).Contains("#REF!")) name.Delete();

                var mappings = ReadMappings(metadata);
                // Replace XLOOKUP/array formulas with compatible scalar formulas and
                // remove every illustrative amount from the supplied template.
                sheet.Range["B3:C9"].ClearContents();
                SetFormula(metadata.Range["H15:H41"],
                    "=INDEX(TableSources[Source],MATCH([@[SignificanceSourceType_sk]],TableSources[Zdroj],0))");
                var currentLinks = new object[7, 1];
                var previousLinks = new object[7, 1];
                for (int row = 0; row < 7; row++)
                {
                    currentLinks[row, 0] = "=B" + (3 + row);
                    previousLinks[row, 0] = "=C" + (3 + row);
                }
                SetFormula(sheet.Range["B14:B20"], currentLinks);
                SetFormula(sheet.Range["B26:B32"], currentLinks);
                SetFormula(sheet.Range["B38:B44"], previousLinks);
                sheet.Range["B49"].ClearContents(); // remove the template's single-cell array formula
                SetFormula(sheet.Range["B49"], "=INDEX(SignificanceCurrentYear[[Kritická 100%]],MATCH(SelectedSignificance,SignificanceCurrentYear[Zdroj],0))");
                SetFormula(sheet.Range["B51"], "=INDEX(SignificanceCurrentYear[[Kritická 5%]],MATCH(SelectedSignificance,SignificanceCurrentYear[Zdroj],0))");
                SetFormula(sheet.Range["B53"], "=VyznamnostPercent*Hodnota");

                metadata.Range["J1:S1"].Value2 = new object[,] { { "Source", "FiscalYear", "ReportId", "TableKey",
                    "PrintedRow", "RowOrdinal", "DataColumnOrdinal", "OfficialValue", "Status", "SelectedStatementId" } };
                var scope = package?.FinancialStatements.FirstOrDefault(x =>
                    x.Statement.Id == selectedStatementId)?.Statement;
                int evidenceRow = 2;
                int missing = 0;
                var overviewFormulas = new object[7, 2];
                for (int row = 3; row <= 9; row++)
                {
                    string source = Convert.ToString(((Excel.Range)sheet.Cells[row, 1]).Value2);
                    for (int column = 2; column <= 3; column++)
                    {
                        int year = fiscalYear - (column - 2);
                        var value = PerformanceMaterialityResolver.Resolve(package, mappings, source, year,
                            column == 2 ? selectedStatementId : null, scope);
                        var cell = (Excel.Range)sheet.Cells[row, column];
                        int firstRow = evidenceRow;
                        if (!value.Value.HasValue)
                        {
                            missing++;
                            overviewFormulas[row - 3, column - 2] = "=NA()";
                            cell.AddComment(year + ": " + value.Problem);
                            metadata.Range[metadata.Cells[evidenceRow, 10], metadata.Cells[evidenceRow, 19]].Value2 =
                                new object[,] { { source, year, null, null, null, null, null, null, value.Problem,
                                    column == 2 ? (object)selectedStatementId : null } };
                            evidenceRow++;
                        }
                        else
                        {
                            foreach (var evidence in value.Evidence)
                            {
                                metadata.Range[metadata.Cells[evidenceRow, 10], metadata.Cells[evidenceRow, 19]].Value2 =
                                    new object[,] { { source, year, (double)evidence.ReportId, evidence.TableKey,
                                        evidence.RowNumber, evidence.RowOrdinal, evidence.DataColumnOrdinal,
                                        (double)evidence.Value, "Official RegisterUZ value", column == 2 ? (object)selectedStatementId : null } };
                                evidenceRow++;
                            }
                            overviewFormulas[row - 3, column - 2] =
                                "=SUM('" + MetadataName + "'!Q" + firstRow + ":Q" + (evidenceRow - 1) + ")";
                            cell.AddComment("Official RegisterUZ data for " + year + ". Source rows: " + MetadataName + "!J" + firstRow + ":S" + (evidenceRow - 1));
                        }
                    }
                }
                SetFormula(sheet.Range["B3:C9"], overviewFormulas);
                var evidenceTable = metadata.ListObjects.Add(Excel.XlListObjectSourceType.xlSrcRange,
                    metadata.Range["J1:S" + (evidenceRow - 1)], Type.Missing, Excel.XlYesNoGuess.xlYes, Type.Missing);
                evidenceTable.Name = "PerformanceMatEvidence";
                sheet.Range["A55"].Value2 = "RegisterUZ: " + fiscalYear + " / " + (fiscalYear - 1) +
                    (missing == 0 ? ". All source values available." : ". " + missing + " source values unavailable (#N/A); see cell comments.");
                sheet.Range["A55:H56"].Merge();
                sheet.Range["A55"].WrapText = true;
                sheet.Range["A55"].Font.Size = 10;
                sheet.Range["A55:H56"].RowHeight = 20;
                BindSignificance(workbook, sheet);
                AuditWorkbookWorksheetLayout.ApplyAuditWorkColor(sheet);
                sheet.Calculate();
            }
            finally
            {
                try
                {
                    if (template != null) template.Close(SaveChanges: false);
                    if (File.Exists(path)) File.Delete(path);
                }
                finally
                {
                    autoCorrect.AutoFillFormulasInLists = previousAutoFill;
                }
            }
        }

        public static void BindSignificance(Excel.Workbook workbook, Excel.Worksheet sheet)
        {
            var table = sheet.ListObjects["ImplementationSignificance"];
            var labels = table.ListColumns[1].DataBodyRange;
            for (int row = 1; row <= labels.Rows.Count; row++)
                if (Convert.ToString(((Excel.Range)labels.Cells[row, 1]).Value2) == "Hodnota z vykonávacej významnosti")
                {
                    var value = (Excel.Range)table.ListColumns["Hodnoty"].DataBodyRange.Cells[row, 1];
                    BindName(workbook, "_WB_significance", "='" + sheet.Name + "'!" + value.Address[true, true, Excel.XlReferenceStyle.xlA1]);
                    return;
                }
            throw new InvalidOperationException("PerformanceMat has no implementation significance value row.");
        }

        private static void SetFormula(Excel.Range target, object formula)
        {
            string location = target.Worksheet.Name + "!" +
                target.Address[true, true, Excel.XlReferenceStyle.xlA1];
            try
            {
                // Formula uses invariant English syntax; special-character table
                // headers still require Excel's nested structured-reference brackets.
                target.Formula = formula;
            }
            catch (COMException ex)
            {
                throw new InvalidOperationException(
                    "PerformanceMat formula assignment failed at " + location +
                    ". Formula: " + (formula is object[,]
                        ? "per-cell formula matrix" : Convert.ToString(formula)), ex);
            }
        }

        private static void BindName(Excel.Workbook workbook, string name, string refersTo)
        {
            foreach (Excel.Name existing in workbook.Names)
                if (string.Equals(existing.Name, name, StringComparison.OrdinalIgnoreCase))
                { existing.RefersTo = refersTo; return; }
            workbook.Names.Add(Name: name, RefersTo: refersTo);
        }

        private static List<SignificanceMapping> ReadMappings(Excel.Worksheet metadata)
        {
            var table = metadata.ListObjects["ListOfTablesFull"];
            var result = new List<SignificanceMapping>();
            for (int row = 1; row <= table.DataBodyRange.Rows.Count; row++)
                result.Add(new SignificanceMapping {
                    TableKey = Convert.ToString(((Excel.Range)table.ListColumns["Name"].DataBodyRange.Cells[row, 1]).Value2),
                    RowNumber = Convert.ToInt32(((Excel.Range)table.ListColumns["Row"].DataBodyRange.Cells[row, 1]).Value2, CultureInfo.InvariantCulture),
                    Source = Convert.ToString(((Excel.Range)table.ListColumns["SignificanceSourceType_sk"].DataBodyRange.Cells[row, 1]).Value2)
                });
            return result;
        }
    }
}
