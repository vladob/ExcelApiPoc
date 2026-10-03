using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using ExcelApiPoc.AccountingImport.Models;
using PdfLayoutEngine.Simple;
using PdfLayoutEngine.IText.Simple;
using PdfLayoutEngine.Xps;
namespace ExcelApiPoc.AccountingImport.Services.Layouts
{
    /// <summary>Opt-in staged importer. The caller owns the session and schedules it off the UI thread.</summary>
    public sealed class SimpleLayoutImporter : IDisposable
    {
        private readonly string path;
        private readonly LayoutSession session;
        public GeneralLedgerImport Canonical { get; private set; }
        public SimpleLayoutImporter(string filePath, string layoutPath)
        {
            path = Path.GetFullPath(filePath);
            var layout = LayoutDefinition.Load(layoutPath);
            IPageSource source;
            switch(Path.GetExtension(path).ToLowerInvariant())
            {
                case ".pdf": source = new PdfPageSource(path); break;
                case ".xps": case ".oxps": source = new XpsPageSource(path); break;
                default: throw new NotSupportedException("This importer accepts PDF, XPS and OXPS.");
            }
            session = new LayoutSession(source,layout);
        }
        public ImportResult Examine(ImportLevel level, CancellationToken token = default)
        {
            if (level < ImportLevel.Recognize || level > ImportLevel.Normalize) throw new ArgumentOutOfRangeException(nameof(level));
            var result=session.Examine(level==ImportLevel.Normalize?ImportLevel.Validate:level,token);
            result.RequestedLevel=level;
            if(level==ImportLevel.Normalize && result.Status=="completed" && Canonical==null)
            {
                token.ThrowIfCancellationRequested();
                if(result.Category!="GL")throw new NotSupportedException("Canonical mapping currently supports GL only.");
                Canonical=Map(result); result.CompletedLevel=5;
            }
            return result;
        }
        private GeneralLedgerImport Map(ImportResult r)
        {
            string hash;
            using(var sha=SHA256.Create())using(var input=File.OpenRead(path))hash=BitConverter.ToString(sha.ComputeHash(input)).Replace("-","").ToLowerInvariant();
            var gl=new GeneralLedgerImport { SourceFileName=Path.GetFileName(path), SourceFilePath=path, SourceFileHash=hash,
                TechnicalType=r.Format, AccountingFormat="IfoSoft", CompanyName=r.Identifiers["entityName"], Ico=r.Identifiers["cin"],
                FiscalYear=int.TryParse(r.Identifiers["fiscalYear"],out var year)?year:0,
                ThroughMonth=int.Parse(r.Identifiers["periodTo"].Substring(0,2)),
                PeriodHeader=r.Identifiers["periodFrom"]+" - "+r.Identifiers["periodTo"], ImportedAtUtc=DateTime.UtcNow };
            // Both RECORD kinds are printed detail. Synthetic/class/report totals remain in the source result only.
            foreach(var row in r.Rows.Where(x=>x.Kind=="ItemRow" || x.Kind=="AnalyticalRow"))
            {
                var a=row.Amounts; decimal opening=a["openingNet"].Value, closing=a["closingNet"].Value;
                var mapped=new GeneralLedgerRow { SequenceNumber=gl.Rows.Count+1, SourceRecordNumber=r.Rows.IndexOf(row)+1,
                    SourcePageNumber=row.Page, SourceRowKind=row.Kind, AccountCode=row.Account, AccountName=row.Name,
                    SourceOpeningNet=opening, SourceClosingNet=closing,
                    OpeningDebit=Math.Max(opening,0), OpeningCredit=Math.Max(-opening,0),
                    ClosingDebit=Math.Max(closing,0), ClosingCredit=Math.Max(-closing,0),
                    AnnualDebitTurnover=a["turnoverDebit"].Value, AnnualCreditTurnover=a["turnoverCredit"].Value,
                    PeriodDebitTurnover=a["periodDebit"].Value, PeriodCreditTurnover=a["periodCredit"].Value };
                // Keep exact identifiers. Do not invent synthetic/analytical splits for nonstandard accounts.
                foreach(var key in new[]{"ZAKC","KP","KZ","T","STR","ZAK"})if(row.Fields.TryGetValue(key,out var v))mapped.SourceDimensions[key]=v;
                gl.Rows.Add(mapped);
            }
            return gl;
        }
        public void Dispose()=>session.Dispose();
    }
}
