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
    public sealed class CompactLayoutImporter : IDisposable
    {
        readonly string path;
        readonly CompactSession session;
        public object Canonical { get; private set; }
        public CompactLayoutImporter(string file,string definitions)
        {
            path=Path.GetFullPath(file);
            var layouts=(Directory.Exists(definitions)?Directory.GetFiles(definitions,"*.json"):new[]{definitions}).Select(CompactLayout.Load).ToArray();
            IPageSource source;
            switch(Path.GetExtension(path).ToLowerInvariant()){
                case ".pdf":source=new PdfPageSource(path);break;
                case ".xps":case ".oxps":source=new XpsPageSource(path);break;
                default:throw new NotSupportedException("Expected PDF, XPS or OXPS.");
            }
            session=new CompactSession(source,layouts);
        }
        public ImportResult Examine(ImportLevel level,CancellationToken token=default)
        {
            if(level<ImportLevel.Recognize||level>ImportLevel.Normalize)throw new ArgumentOutOfRangeException(nameof(level));
            var r=session.Examine(level==ImportLevel.Normalize?ImportLevel.Validate:level,token);r.RequestedLevel=level;
            if(level==ImportLevel.Normalize&&r.Status=="completed"&&Canonical==null){token.ThrowIfCancellationRequested();Canonical=Map(r);r.CompletedLevel=5;}
            return r;
        }
        object Map(ImportResult r)
        {
            string hash;using(var sha=SHA256.Create())using(var input=File.OpenRead(path))hash=BitConverter.ToString(sha.ComputeHash(input)).Replace("-","").ToLowerInvariant();
            string company=CompactSession.Value(r.Identifiers,"AccountingEntityName");int year=int.TryParse(CompactSession.Value(r.Identifiers,"fiscalYear"),out int y)?y:0;
            var sourceNumbers=r.Rows.Select((row,index)=>(row,index)).ToDictionary(x=>x.row,x=>x.index+1);
            var rows=r.Rows.Where(row=>session.Layout.ImportKinds.Contains(row.Kind)).ToArray();
            if(r.Category=="GL"){
                var gl=new GeneralLedgerImport{SourceFileName=Path.GetFileName(path),SourceFilePath=path,SourceFileHash=hash,TechnicalType=r.Format,AccountingFormat="IfoSoft",CompanyName=company,Ico=null,FiscalYear=year,ImportedAtUtc=DateTime.UtcNow,PeriodHeader=CompactSession.Value(r.Identifiers,"PeriodFrom")+" - "+CompactSession.Value(r.Identifiers,"PeriodTo")};
                if(int.TryParse(CompactSession.Value(r.Identifiers,"PeriodTo").Split('/')[0],out int month))gl.ThroughMonth=month;
                foreach(var row in rows){
                    decimal? opening=Get(row,"OpeningNetto"),closing=Get(row,"ClosingNetto");
                    var v=new GeneralLedgerRow{SequenceNumber=gl.Rows.Count+1,SourceRecordNumber=sourceNumbers[row],SourcePageNumber=row.Page,SourceRowKind=row.Kind,AccountCode=row.Account,AccountName=row.Name,SourceOpeningNet=opening,SourceClosingNet=closing,SourceAvailableAmountFields=row.Amounts.Keys.ToArray(),
                        OpeningDebit=opening.HasValue?Math.Max(opening.Value,0):Get(row,"OpeningDebit")??0,OpeningCredit=opening.HasValue?Math.Max(-opening.Value,0):Get(row,"OpeningCredit")??0,
                        ClosingDebit=closing.HasValue?Math.Max(closing.Value,0):Get(row,"ClosingDebit")??0,ClosingCredit=closing.HasValue?Math.Max(-closing.Value,0):Get(row,"ClosingCredit")??0,
                        AnnualDebitTurnover=Get(row,"TurnoverDebit")??0,AnnualCreditTurnover=Get(row,"TurnoverCredit")??0,PeriodDebitTurnover=Get(row,"PeriodTurnoverDebit")??0,PeriodCreditTurnover=Get(row,"PeriodTurnoverCredit")??0};
                    foreach(var field in row.Fields.Where(k=>!row.Amounts.ContainsKey(k.Key)))v.SourceDimensions[field.Key]=field.Value;
                    gl.Rows.Add(v);
                }return gl;
            }
            if(r.Category=="AJ"){
                var aj=new JournalImport{SourceFileName=Path.GetFileName(path),SourceFilePath=path,SourceFileHash=hash,TechnicalType=r.Format,AccountingFormat="IfoSoft",CompanyName=company,Ico=null,FiscalYear=year,ImportedAtUtc=DateTime.UtcNow};
                foreach(var row in rows){
                    CompactSession.Date(CompactSession.Value(row.Fields,"Date"),out var date);
                    string debit=CompactSession.Value(row.Fields,"DebitAccount","Debit"),credit=CompactSession.Value(row.Fields,"CreditAccount","Credit");decimal amount=Get(row,"Amount")??Get(row,"Sum")??0;
                    string doc=CompactSession.Value(row.Fields,"DocumentNumber","DocumentNo"),sub=CompactSession.Value(row.Fields,"DocumentSubnumber","Ordinary");
                    aj.Rows.Add(new JournalRow{SourceFields=new System.Collections.Generic.Dictionary<string,string>(row.Fields),SequenceNumber=aj.Rows.Count+1,SourceRecordNumber=sourceNumbers[row],SourceLocation="page "+row.Page,DocumentNumber=doc+(sub.Length>0?"/"+sub:""),PostingDate=date,Description=CompactSession.Value(row.Fields,"OperationDescription","Description1"),DebitAccount=debit,CreditAccount=credit,DebitAmount=debit.Length>0?(decimal?)amount:null,CreditAmount=credit.Length>0?(decimal?)amount:null});
                }
                if(aj.FiscalYear==0&&aj.Rows.Select(v=>v.PostingDate.Year).Distinct().Count()==1)aj.FiscalYear=aj.Rows[0].PostingDate.Year;
                return aj;
            }
            var af=new AccountingFrameworkImport{SourceFileName=Path.GetFileName(path),SourceFilePath=path,SourceFileHash=hash,TechnicalType=r.Format,AccountingFormat="IfoSoft",CompanyName=company,Ico=null,FiscalYear=year,ImportedAtUtc=DateTime.UtcNow};
            foreach(var row in rows){string syn=CompactSession.Value(row.Fields,"SyntheticAccount"),ana=CompactSession.Value(row.Fields,"AnalythicAccount");af.Rows.Add(new AccountingFrameworkRow{SourceFields=new System.Collections.Generic.Dictionary<string,string>(row.Fields),SequenceNumber=af.Rows.Count+1,SourceRecordNumber=sourceNumbers[row],SourceSyntheticCode=syn,SourceAnalyticalCode=ana,SyntheticCode=syn,AnalyticalCode=ana,AccountCode=syn+ana,AccountName=CompactSession.Value(row.Fields,"Name"),Type=CompactSession.Value(row.Fields,"Type"),TaxFlag=CompactSession.Value(row.Fields,"Tax"),BalanceFlag=CompactSession.Value(row.Fields,"Saldo"),VatFlag=CompactSession.Value(row.Fields,"Vat"),RowKind=AccountingFrameworkRowKind.AnalyticalAccount});}
            return af;
        }
        static decimal? Get(SourceRow row,string key)=>row.Amounts.TryGetValue(key,out var v)?v:null;
        public void Dispose()=>session.Dispose();
    }
}
