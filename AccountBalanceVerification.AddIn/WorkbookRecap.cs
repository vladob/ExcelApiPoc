using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AccountBalanceVerification.Core;
using Excel = Microsoft.Office.Interop.Excel;

namespace AccountBalanceVerification.AddIn
{
    internal static partial class WorkbookOutput
    {
        private const int CommissionCapacity = 20;
        private static void MergeText(Excel.Worksheet sheet, int row, string text, bool heading = false)
        {
            var range = sheet.Range["A" + row + ":E" + row];
            try { range.Merge(); range.WrapText = true; range.RowHeight = heading ? 28 : 38; range.Font.Bold = heading; }
            finally { Release(range); }
            Text(sheet, "A" + row, text);
        }
        private static void MakeTable(Excel.Worksheet sheet, string address, string name)
        {
            var r = sheet.Range[address]; var tables = sheet.ListObjects;
            try { var t = tables.Add(Excel.XlListObjectSourceType.xlSrcRange, r, Type.Missing, Excel.XlYesNoGuess.xlYes);
                try { t.Name = name; t.TableStyle = "TableStyleLight1"; t.ShowTableStyleRowStripes = false; } finally { Release(t); }
            } finally { Release(tables); Release(r); }
        }
        private static string MapFormula(string key, string field) =>
            "=IF(OR(COUNTIF(ABV_RecapMap[Key],\"" + key + "\")=0,COUNTIFS(ABV_RecapMap[Key],\"" + key + "\",ABV_RecapMap[" + field + "],\"\")>0),\"\",SUMIF(ABV_RecapMap[Key],\"" + key + "\",ABV_RecapMap[" + field + "]))";
        private static void Commission(Excel.Workbook book, Excel.Worksheet sheet, int row, string name)
        {
            Text(sheet, "A" + row, "Funkcia"); Text(sheet, "B" + row, "Meno a priezvisko"); Text(sheet, "D" + row, "Podpis");
            string mask = "(LEN(TRIM(ABV_People[Meno]))>0)*(ABV_People[Rekapitulácia]=\"Komisia\")";
            Formula(sheet, "A" + (row + 1), "=FILTER(ABV_People[Funkcia]," + mask + ",\"\")");
            Formula(sheet, "B" + (row + 1), "=FILTER(ABV_People[Meno]," + mask + ",\"\")");
            Formula(sheet, "D" + (row + 1), "=IF(SUMPRODUCT(" + mask + ")=0,\"\",IF(SEQUENCE(MAX(1,SUMPRODUCT(" + mask + "))),\"............................\"))");
            var r = sheet.Range["A" + row + ":E" + (row + CommissionCapacity)];
            try { r.WrapText = false; r.RowHeight = 40; } finally { Release(r); }
            Anchor(book, sheet, name, "$A$" + (row + 1) + ":$E$" + (row + CommissionCapacity));
        }
        private static int RecapLastRow(Excel.Workbook book)
        {
            var end = NamedRange(book, "_ABV_RecapEnd");
            try { return end.Row; } finally { Release(end); }
        }
        private static void ValidateRecap(Excel.Workbook book, Excel.Worksheet report, List<Signatory> people)
        {
            var named = people.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToList();
            if (named.Any(x => x.RecapRole != "" && x.RecapRole != DecreePlan.Commission && x.RecapRole != DecreePlan.Signer))
                throw new InvalidOperationException("Neplatná úloha osoby pre rekapituláciu.");
            if (named.Count(x => x.RecapRole == DecreePlan.Commission) > CommissionCapacity)
                throw new InvalidOperationException("Prototyp rekapitulácie podporuje najviac 20 členov komisie.");
            if (named.Count(x => x.RecapRole == DecreePlan.Signer) > 1)
                throw new InvalidOperationException("Pre rekapituláciu vyberte najviac jednu podpisujúcu osobu.");
            if (Convert.ToDouble(report.Evaluate("SUMPRODUCT(--ISERROR(A1:E" + RecapLastRow(book) + "))")) > 0)
                throw new InvalidOperationException("Rekapitulácia obsahuje chybu vzorca. Skontrolujte mapovanie a priestor pre členov komisie.");
            if (Convert.ToDouble(report.Evaluate("SUMPRODUCT(--(ABS(ABV_RecapMap[Sign])<>1))")) > 0 ||
                Convert.ToDouble(report.Evaluate("SUMPRODUCT(--(ABV_RecapMap[BalanceSide]<>\"Net\"),--(ABV_RecapMap[BalanceSide]<>\"Debit\"),--(ABV_RecapMap[BalanceSide]<>\"Credit\"))")) > 0)
                throw new InvalidOperationException("Skontrolujte Sign (1 alebo -1) a BalanceSide (Net, Debit, Credit) v mapovaní.");
        }
        private static void PrepareRecapCopy(Excel.Workbook book, Excel.Worksheet source, Excel.Worksheet copy, List<Signatory> people)
        {
            int last = RecapLastRow(book);
            var original = source.Range["A1:E" + last]; var destination = copy.Range["A1:E" + last];
            try {
                // Excel rejects a rectangular PasteSpecial over mixed merged/unmerged
                // cells. Snapshot calculated values and merge geometry, then rebuild
                // only the temporary output range without using the clipboard.
                object values = original.Value2;
                var merges = new HashSet<string>(StringComparer.Ordinal);
                var cells = destination.Cells;
                try {
                    for (int row = 1; row <= last; row++) for (int column = 1; column <= 5; column++) {
                        var cell = (Excel.Range)cells[row, column];
                        try {
                            if (!Convert.ToBoolean(cell.MergeCells)) continue;
                            var area = cell.MergeArea;
                            try { merges.Add(area.Address); } finally { Release(area); }
                        } finally { Release(cell); }
                    }
                } finally { Release(cells); }
                destination.UnMerge();
                destination.ClearContents(); // Remove entire dynamic-array spills.
                destination.Value2 = values;
                foreach (string address in merges) {
                    var area = copy.Range[address];
                    try { area.Merge(); } finally { Release(area); }
                }
            } finally { Release(destination); Release(original); }
            int count = people.Count(x => !string.IsNullOrWhiteSpace(x.Name) && x.RecapRole == DecreePlan.Commission);
            foreach (string name in new[] { "_ABV_RecapCommissionStart", "_ABV_RecapCommissionEnd" }) {
                var anchor = NamedRange(book, name); int first;
                try { first = anchor.Row; } finally { Release(anchor); }
                if (count < CommissionCapacity) {
                    var unused = copy.Range["A" + (first + count) + ":E" + (first + CommissionCapacity - 1)];
                    var rows = unused.EntireRow;
                    try { rows.Hidden = true; } finally { Release(rows); Release(unused); }
                }
            }
            var setup = copy.PageSetup;
            try { setup.PrintArea = "$A$1:$E$" + last; } finally { Release(setup); }
        }
        internal static void CreateRecap(Excel.Workbook book, VerificationPackage package)
        {
            var sections = RecapPlan.Build(package);
            var ledger = AddSheet(book, "Zdroj rekapitulácie", "RecapData");
            var mapping = AddSheet(book, "Mapovanie rekapitulácie", "RecapMap");
            var report = AddSheet(book, "Rekapitulácia", "Recap");
            try {
                MergeText(ledger, 1, "Úplná hlavná kniha pre rekapituláciu (vrátane tried 5–7). Zdroj: " + package.Source);
                string[] headers = { "AccountNo", "AccountName", "Opening", "Debit", "Credit", "Closing", "DebitNet", "CreditNet" };
                for (int i = 0; i < headers.Length; i++) Text(ledger, ((char)('A' + i)) + "3", headers[i]);
                var sourceRows = package.Rows.GroupBy(x => RecapPlan.NormalizeAccount(x.AccountNo)).Select(g => new {
                    Code = g.Key, Name = g.First().AccountName, Opening = g.Sum(x => x.OpeningBalance),
                    Debit = g.Sum(x => x.MovementsDebit), Credit = g.Sum(x => x.MovementsCredit), Closing = g.Sum(x => x.ClosingBalance)
                }).OrderBy(x => x.Code, StringComparer.Ordinal).ToList();
                if (sourceRows.Count == 0) throw new InvalidOperationException("Chýbajú údaje hlavnej knihy pre rekapituláciu.");
                var values = new object[sourceRows.Count, 8];
                for (int i = 0; i < sourceRows.Count; i++) {
                    var x = sourceRows[i]; values[i, 0] = x.Code; values[i, 1] = x.Name;
                    values[i, 2] = (double)x.Opening; values[i, 3] = (double)x.Debit; values[i, 4] = (double)x.Credit; values[i, 5] = (double)x.Closing;
                    values[i, 6] = (double)Math.Max(0, x.Closing); values[i, 7] = (double)Math.Max(0, -x.Closing);
                }
                var data = ledger.Range["A4:H" + (sourceRows.Count + 3)];
                try { data.NumberFormat = "@"; data.Value2 = values; } finally { Release(data); }
                data = ledger.Range["C4:H" + (sourceRows.Count + 3)];
                try { data.NumberFormat = "#,##0.00;-#,##0.00"; } finally { Release(data); }
                MakeTable(ledger, "A3:H" + (sourceRows.Count + 3), "ABV_RecapLedger");
                data = ledger.Range["A:A"]; try { data.ColumnWidth = 16; } finally { Release(data); }
                data = ledger.Range["B:B"]; try { data.ColumnWidth = 48; } finally { Release(data); }
                data = ledger.Range["C:H"]; try { data.ColumnWidth = 18; } finally { Release(data); }

                Text(mapping, "A1", "Mapovanie prototypu pre obce. Overte vzory účtov a ručné hodnoty pred podpisom.");
                Text(mapping, "A2", "Vzory: 02* = skupina; 11201 = presný účet. Prázdny vzor znamená nevyriešené mapovanie. Ručné nuly sú platné hodnoty.");
                string[] mh = { "Key", "Pattern", "Sign", "BalanceSide", "CalcOpening", "CalcAdditions", "CalcReductions", "CalcClosing", "ManualOpening", "ManualAdditions", "ManualReductions", "ManualClosing", "Opening", "Additions", "Reductions", "Closing", "Note" };
                for (int i = 0; i < mh.Length; i++) Text(mapping, ((char)('A' + i)) + "4", mh[i]);
                int mr = 5;
                foreach (var item in sections.SelectMany(x => x.Items)) foreach (var pattern in item.Patterns) {
                    Text(mapping, "A" + mr, item.Key); Text(mapping, "B" + mr, pattern);
                    var sign = mapping.Range["C" + mr]; try { sign.Value2 = item.Sign; } finally { Release(sign); }
                    Text(mapping, "D" + mr, item.BalanceSide); Text(mapping, "Q" + mr, item.Note);
                    string sum(string column) => "SUMIF(ABV_RecapLedger[AccountNo],B" + mr + ",ABV_RecapLedger[" + column + "])";
                    Formula(mapping, "E" + mr, "=IF(B" + mr + "=\"\",\"\",C" + mr + "*" + sum("Opening") + ")");
                    Formula(mapping, "F" + mr, "=IF(B" + mr + "=\"\",\"\",IF(C" + mr + "=1," + sum("Debit") + "," + sum("Credit") + "))");
                    Formula(mapping, "G" + mr, "=IF(B" + mr + "=\"\",\"\",IF(C" + mr + "=1," + sum("Credit") + "," + sum("Debit") + "))");
                    Formula(mapping, "H" + mr, "=IF(B" + mr + "=\"\",\"\",IF(D" + mr + "=\"Debit\"," + sum("DebitNet") + ",IF(D" + mr + "=\"Credit\"," + sum("CreditNet") + ",C" + mr + "*" + sum("Closing") + ")))");
                    for (int i = 0; i < 4; i++) {
                        string manual = ((char)('I' + i)) + mr.ToString(), calculated = ((char)('E' + i)) + mr.ToString();
                        Formula(mapping, ((char)('M' + i)) + mr.ToString(), "=IF(ISNUMBER(" + manual + ")," + manual + ",IF(" + manual + "<>\"\",NA()," + calculated + "))");
                    }
                    mr++;
                }
                MakeTable(mapping, "A4:Q" + (mr - 1), "ABV_RecapMap");
                Input(mapping, "B5:B" + (mr - 1)); Input(mapping, "C5:C" + (mr - 1), "0"); Input(mapping, "D5:D" + (mr - 1));
                Input(mapping, "I5:L" + (mr - 1), "#,##0.00;-#,##0.00"); Input(mapping, "Q5:Q" + (mr - 1));
                Validate(mapping, "C5:C" + (mr - 1), "1,-1"); Validate(mapping, "D5:D" + (mr - 1), "Net,Debit,Credit");
                data = mapping.Range["E5:P" + (mr - 1)]; try { data.NumberFormat = "#,##0.00;-#,##0.00"; } finally { Release(data); }
                data = mapping.Range["A:Q"]; try { data.ColumnWidth = 18; } finally { Release(data); }
                data = mapping.Range["Q:Q"]; try { data.ColumnWidth = 60; data.WrapText = true; } finally { Release(data); }

                MergeText(report, 1, "Inventarizačný zápis inventarizačnej komisie", true);
                MergeText(report, 2, ""); Formula(report, "A2", "=P_EntityName&\", \"&P_AeAddress&\" | IČO: \"&P_ICO");
                MergeText(report, 3, ""); Formula(report, "A3", "=\"Inventarizácia ku dňu \"&" + DateFormula("P_InventoryDate"));
                MergeText(report, 4, "Prototyp pre obce – mapovanie, úplnosť údajov a závery potvrďte pred podpisom.");
                MergeText(report, 6, "I. Úvod", true);
                MergeText(report, 7, ""); Formula(report, "A7", "=\"Na základe príkazu č. \"&P_OrderNumber&\" zo dňa \"&" + DateFormula("P_OrderIssueDate") + "&\" vykonala inventarizačná komisia inventarizáciu majetku, záväzkov a rozdielu majetku a záväzkov.\"");
                Commission(book, report, 9, "_ABV_RecapCommissionStart");
                int rr = 31;
                MergeText(report, rr, ""); Formula(report, "A" + rr++, "=\"Príkaz prevzatý dňa \"&" + DateFormula("P_OrderReceiptDate"));
                MergeText(report, rr++, "Predmet: majetok, pohľadávky, finančné účty, vlastné imanie, záväzky, časové rozlíšenie a podsúvahová evidencia.");
                MergeText(report, rr++, "II. Zhodnotenie inventarizácie – sumy v EUR", true);
                foreach (var section in sections) {
                    rr++; MergeText(report, rr++, section.Title, true);
                    Text(report, "A" + rr, "Predmet / účet");
                    if (section.RollForward) { Text(report, "B" + rr, "Začiatočný stav"); Text(report, "C" + rr, "Prírastky"); Text(report, "D" + rr, "Úbytky"); }
                    Text(report, "E" + rr, "Konečný zostatok");
                    var header = report.Range["A" + rr + ":E" + rr];
                    try { header.WrapText = true; header.RowHeight = 30; header.Font.Bold = true; } finally { Release(header); }
                    rr++;
                    int first = rr;
                    foreach (var item in section.Items) {
                        Text(report, "A" + rr, item.Label);
                        var detail = report.Range["A" + rr + ":E" + rr];
                        try { detail.WrapText = true; detail.RowHeight = 32; } finally { Release(detail); }
                        if (section.RollForward) {
                            Formula(report, "B" + rr, MapFormula(item.Key, "Opening")); Formula(report, "C" + rr, MapFormula(item.Key, "Additions")); Formula(report, "D" + rr, MapFormula(item.Key, "Reductions"));
                        }
                        Formula(report, "E" + rr, MapFormula(item.Key, "Closing"));
                        rr++;
                    }
                    Text(report, "A" + rr, "Spolu");
                    foreach (string c in section.RollForward ? new[] { "B", "C", "D", "E" } : new[] { "E" })
                        Formula(report, c + rr, "=IF(COUNT(" + c + first + ":" + c + (rr - 1) + ")<>" + (rr - first) + ",\"\",SUM(" + c + first + ":" + c + (rr - 1) + "))");
                    rr++;
                    Text(report, "A" + rr, "Záver komisie: doplniť zistenia, rozdiely a odsúhlasenie s evidenciou.");
                    Input(report, "A" + rr + ":E" + rr);
                    var conclusion = report.Range["A" + rr + ":E" + rr];
                    try { conclusion.WrapText = false; conclusion.RowHeight = 20; } finally { Release(conclusion); }
                    rr++;
                }
                rr++; MergeText(report, rr++, "7. Platová inventúra zamestnancov", true);
                MergeText(report, rr, "Doplniť výsledok dokladovej inventúry miezd a prípadné rozdiely."); Input(report, "A" + rr + ":E" + rr); rr += 2;
                MergeText(report, rr++, "III. Záver", true);
                MergeText(report, rr, "Doplniť celkové zhodnotenie inventarizácie a spôsob vysporiadania rozdielov."); Input(report, "A" + rr + ":E" + rr); rr += 2;
                MergeText(report, rr++, "Za správnosť údajov zodpovedajú členovia inventarizačnej komisie.");
                Commission(book, report, rr, "_ABV_RecapCommissionEnd"); rr += CommissionCapacity + 2;
                MergeText(report, rr, ""); Formula(report, "A" + rr++, "=\"V \"&P_Location&\", dňa \"&" + DateFormula("P_DocumentDate"));
                MergeText(report, rr, "Vyjadrenie schvaľujúcej osoby: doplniť."); Input(report, "A" + rr + ":E" + rr); rr++;
                MergeText(report, rr, "............................................................");
                var signatureLine = report.Range["A" + rr + ":E" + rr];
                try { signatureLine.RowHeight = 45; } finally { Release(signatureLine); }
                rr++;
                MergeText(report, rr, ""); Formula(report, "A" + rr, "=TEXTJOIN(\"; \",TRUE,FILTER(ABV_People[Meno]&\" – \"&ABV_People[Funkcia],(ABV_People[Rekapitulácia]=\"Podpisujúci\")*(LEN(TRIM(ABV_People[Meno]))>0),\"[vyberte podpisujúceho]\"))");
                Anchor(book, report, "_ABV_RecapEnd", "$A$" + rr);
                var formatting = report.Range["A1:E" + rr]; try { formatting.Font.Name = "Calibri"; formatting.Font.Size = 11; } finally { Release(formatting); }
                formatting = report.Range["A:A"]; try { formatting.ColumnWidth = 46; } finally { Release(formatting); }
                formatting = report.Range["B:E"]; try { formatting.ColumnWidth = 19; formatting.NumberFormat = "#,##0.00;-#,##0.00"; } finally { Release(formatting); }
                // Keep headings and narratives readable; numeric detail rows may wrap to two lines.

                var setup = report.PageSetup;
                try { setup.PaperSize = Excel.XlPaperSize.xlPaperA4; setup.Orientation = Excel.XlPageOrientation.xlPortrait;
                    setup.Zoom = false; setup.FitToPagesWide = 1; setup.FitToPagesTall = false; setup.PrintArea = "$A$1:$E$" + rr;
                    setup.LeftMargin = 24; setup.RightMargin = 24; setup.CenterFooter = "Strana &P / &N";
                } finally { Release(setup); }
                if (RecapPlan.IsKamienka(package)) WriteSampleComparison(mapping);
            } finally { Release(report); Release(mapping); Release(ledger); }
        }
    }
}
