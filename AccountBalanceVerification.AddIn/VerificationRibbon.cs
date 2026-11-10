using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ExcelDna.Integration.CustomUI;
using ExcelDna.Integration;
using Excel = Microsoft.Office.Interop.Excel;
namespace AccountBalanceVerification.AddIn
{
    public sealed class VerificationAddIn : IExcelAddIn
    {
        public void AutoOpen() { }
        public void AutoClose() => VerificationRibbon.Close();
    }
    [ComVisible(true)]
    public sealed class VerificationRibbon : ExcelRibbon
    {
        public override string GetCustomUI(string ribbonId) =>
            "<customUI xmlns='http://schemas.microsoft.com/office/2009/07/customui' onLoad='OnRibbonLoad'><ribbon><tabs><tab id='abvTab' label='Inventarizácia'><group id='abvCreate' label='Zošit'><button id='abvNew' label='Vytvoriť inventarizačný zošit' size='large' imageMso='FileNew' getEnabled='GetEnabled' onAction='CreateWorkbook'/></group><group id='abvOutput' label='Tlač a export'><box id='abvOutputBox' boxStyle='vertical'><dropDown id='abvOutputKind' showLabel='false' sizeString='Menovacie dekréty     ' getEnabled='GetEnabled' getSelectedItemIndex='GetOutputIndex' onAction='SelectOutput'><item id='abvInventoryItem' label='Inventarizácia'/><item id='abvRecapItem' label='Rekapitulácia'/><item id='abvDecreeItem' label='Menovacie dekréty'/></dropDown><button id='abvPrint' label='Tlačiť' imageMso='PrintPreviewAndPrint' getEnabled='GetEnabled' onAction='PrintWorkbook'/><button id='abvPdf' label='Exportovať do PDF' imageMso='FileSaveAsPdfOrXps' getEnabled='GetEnabled' getSupertip='GetPdfTooltip' onAction='ExportPdf'/></box></group></tab></tabs></ribbon></customUI>";
        // Session-only selection: 0 inventory, 1 recap, 2 individual decrees.
        private static int outputIndex;
        public int GetOutputIndex(IRibbonControl control) => outputIndex;
        public void SelectOutput(IRibbonControl control, string selectedId, int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex > 2 || busy) return;
            outputIndex = selectedIndex; Invalidate();
        }
        public string GetPdfTooltip(IRibbonControl control) => outputIndex == 2
            ? "Samostatný PDF súbor pre každého príjemcu. Vyberiete cieľový priečinok."
            : outputIndex == 1 ? "Exportovať iba rekapituláciu do jedného PDF súboru."
            : "Exportovať titulnú stranu a účtové hárky do jedného PDF súboru.";
        private static VerificationRibbon current;
        private IRibbonUI ribbon;
        private Excel.Application application;
        private Timer timer;
        private bool busy, editing;
        public void OnRibbonLoad(IRibbonUI ui)
        {
            Close(); current = this; ribbon = ui;
            application = (Excel.Application)ExcelDnaUtil.Application;
            application.WorkbookActivate += WorkbookChanged;
            application.WorkbookDeactivate += WorkbookChanged;
            application.SheetActivate += SheetChanged;
            timer = new Timer { Interval = 500 };
            timer.Tick += PollEditMode;
            timer.Start();
        }
        internal static void Close()
        {
            if (current == null) return;
            current.timer?.Stop(); current.timer?.Dispose();
            try {
                if (current.application != null) {
                    current.application.WorkbookActivate -= current.WorkbookChanged;
                    current.application.WorkbookDeactivate -= current.WorkbookChanged;
                    current.application.SheetActivate -= current.SheetChanged;
                }
            } catch (COMException) { /* Excel is shutting down. */ }
            current.ribbon = null; current.application = null; current = null;
        }
        private void WorkbookChanged(Excel.Workbook book) => Invalidate();
        private void SheetChanged(object sheet) => Invalidate();
        private void Invalidate()
        {
            try { ribbon?.Invalidate(); } catch (COMException) { }
        }
        private static bool IsEditing()
        {
            // Excel-DNA exposes the native edit-state check in recent versions.
            // Reflection keeps the add-in compatible with older installed builds.
            var method = typeof(ExcelDnaUtil).GetMethod("IsInFormulaEditMode", Type.EmptyTypes);
            if (method != null) return (bool)method.Invoke(null, null);
            var app = (Excel.Application)ExcelDnaUtil.Application;
            object bars = ((dynamic)app).CommandBars;
            try { return !((dynamic)bars).GetEnabledMso("FileNewDefault"); }
            finally { WorkbookOutput.Release(bars); }
        }
        private void PollEditMode(object sender, EventArgs e)
        {
            if (busy) return;
            bool value;
            try { value = IsEditing(); } catch { value = true; }
            if (value != editing) { editing = value; Invalidate(); }
        }
        public bool GetEnabled(IRibbonControl control)
        {
            Excel.Workbook book = null;
            try {
                if (busy || IsEditing()) return false;
                var app = (Excel.Application)ExcelDnaUtil.Application;
                if (!app.Ready) return false;
                if (control.Id == "abvNew" || control.Id == "abvOutputKind") return true;
                book = app.ActiveWorkbook;
                bool decrees, recap;
                if (!WorkbookOutput.OutputAvailable(book, out decrees, out recap)) return false;
                return outputIndex == 2 ? decrees : outputIndex == 1 ? recap : true;
            } catch { return false; }
            finally { WorkbookOutput.Release(book); }
        }
        private void Run(IRibbonControl control, Action action)
        {
            if (busy) return;
            try {
                if (IsEditing()) {
                    MessageBox.Show("Najprv dokončite úpravu bunky klávesom Enter alebo ju zrušte klávesom Esc. Potom zopakujte príkaz.",
                        "Inventarizácia", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (!GetEnabled(control)) {
                    MessageBox.Show("Príkaz teraz nie je dostupný. Skontrolujte aktívny inventarizačný zošit a dokončite otvorené dialógové okná.", "Inventarizácia");
                    return;
                }
                busy = true; Invalidate(); action();
            } catch (Exception ex) {
                MessageBox.Show("Príkaz sa nepodarilo vykonať. Ak upravujete bunku, stlačte Enter alebo Esc a skúste znova.\n\n" + ex.Message,
                    "Inventarizácia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            } finally { busy = false; Invalidate(); }
        }
        private void OutputSelected(bool pdf)
        {
            if (outputIndex == 2) WorkbookOutput.Decrees(pdf ? 2 : 1);
            else WorkbookOutput.Output(pdf, recapOnly: outputIndex == 1);
        }
        public void PrintWorkbook(IRibbonControl control) => Run(control, () => OutputSelected(false));
        public void ExportPdf(IRibbonControl control) => Run(control, () => OutputSelected(true));
        public void CreateWorkbook(IRibbonControl control) => Run(control, CreateWorkbookCore);
        private void CreateWorkbookCore()
        {
            Excel.Workbooks books = null;
            Excel.Workbook book = null;
            bool completed = false;
            try
            {
                var app = (Excel.Application)ExcelDnaUtil.Application;
                books = app.Workbooks;
                book = books.Add(Excel.XlWBATemplate.xlWBATWorksheet);
                // Match the existing audit add-in: activate before constructing/showing
                // the modal dialog, so Excel restores focus to this new workbook.
                book.Activate();
                using (var form = new CreateWorkbookForm(book))
                    completed = form.ShowDialog() == DialogResult.OK;
            }
            finally
            {
                try
                {
                    if (book != null && !completed) book.Close(SaveChanges: false);
                }
                finally
                {
                    if (book != null) Marshal.ReleaseComObject(book);
                    if (books != null) Marshal.ReleaseComObject(books);
                }
            }
        }
    }
}
