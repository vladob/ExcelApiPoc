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
        readonly int? selectedFiscalYear;
        readonly CompactSession session;
        readonly StructuredLayoutImporter structured;
        readonly CrystalLayoutImporter crystal;
        readonly ExcelLayoutImporter excel;
        readonly ExcelApiPoc.AccountingImport.Services.Dbf.IfoSoftDbfJournalImporter dbf;
        readonly ExcelApiPoc.AccountingImport.Services.Dbf.IfoSoftDbfAccountImporter dbfAccounts;
        public object Canonical { get; private set; }
        public CompactLayoutImporter(string file,string definitions,int? fiscalYear=null)
        {
            if(fiscalYear.HasValue&&(fiscalYear<1900||fiscalYear>9999))throw new ArgumentOutOfRangeException(nameof(fiscalYear));
            selectedFiscalYear=fiscalYear;
            path=Path.GetFullPath(file);
            if(new[]{".xls",".xlsx"}.Contains(Path.GetExtension(path).ToLowerInvariant())){excel=new ExcelLayoutImporter(path,definitions,fiscalYear);return;}
            if(Path.GetExtension(path).Equals(".xml",StringComparison.OrdinalIgnoreCase)&&((Directory.Exists(definitions)&&Directory.GetFiles(definitions,"crystal-*.json").Length>0)||Path.GetFileName(definitions).StartsWith("crystal-",StringComparison.OrdinalIgnoreCase))){crystal=new CrystalLayoutImporter(path,definitions,fiscalYear);return;}
            if(new[]{".csv",".xml"}.Contains(Path.GetExtension(path).ToLowerInvariant())){structured=new StructuredLayoutImporter(path,definitions,fiscalYear);return;}
            if(Path.GetExtension(path).Equals(".dbf",StringComparison.OrdinalIgnoreCase)){dbf=new ExcelApiPoc.AccountingImport.Services.Dbf.IfoSoftDbfJournalImporter(path,definitions,fiscalYear);
                if(dbf.Examine(ImportLevel.Recognize).Category==null){dbf.Dispose();dbf=null;dbfAccounts=new ExcelApiPoc.AccountingImport.Services.Dbf.IfoSoftDbfAccountImporter(path,definitions,fiscalYear);}
                return;}
            var layouts=(Directory.Exists(definitions)?Directory.GetFiles(definitions,"*.json").Where(p=>!Path.GetFileName(p).StartsWith("dbf-",StringComparison.OrdinalIgnoreCase)&&!Path.GetFileName(p).StartsWith("structured-",StringComparison.OrdinalIgnoreCase)&&!Path.GetFileName(p).StartsWith("crystal-",StringComparison.OrdinalIgnoreCase)&&!Path.GetFileName(p).StartsWith("excel-",StringComparison.OrdinalIgnoreCase)).ToArray():new[]{definitions}).Select(CompactLayout.Load).ToArray();
            IPageSource source;
            switch(Path.GetExtension(path).ToLowerInvariant()){
                case ".pdf":source=new PdfPageSource(path,retainDecodedPages:false);break;
                case ".xps":case ".oxps":source=new XpsPageSource(path);break;
                default:throw new NotSupportedException("Expected PDF, XPS or OXPS.");
            }
            session=new CompactSession(source,layouts);
        }
        public ImportResult Examine(ImportLevel level,CancellationToken token=default)
        {
            if(level<ImportLevel.Recognize||level>ImportLevel.Normalize)throw new ArgumentOutOfRangeException(nameof(level));
            if(excel!=null){var er=excel.Examine(level,token);Canonical=excel.Canonical;return er;}
            if(crystal!=null){var cr=crystal.Examine(level,token);Canonical=crystal.Canonical;return cr;}
            if(structured!=null){var sr=structured.Examine(level,token);Canonical=structured.Canonical;return sr;}
            if(dbfAccounts!=null){var ar=dbfAccounts.Examine(level,token);Canonical=dbfAccounts.Canonical;return ar;}
            if(dbf!=null){var dr=dbf.Examine(level,token);Canonical=dbf.Canonical;return dr;}
            var r=session.Examine(level==ImportLevel.Normalize?ImportLevel.Validate:level,token);r.RequestedLevel=level;
            if(level==ImportLevel.Normalize&&r.Status=="completed"&&Canonical==null){token.ThrowIfCancellationRequested();Canonical=Map(r,path,selectedFiscalYear,session.Layout.Producer,session.Layout.ImportKinds);r.CompletedLevel=5;}
            return r;
        }
        internal static object Map(ImportResult r,string path,int? selectedFiscalYear,string producer,string[] importKinds)
        {
            string hash;using(var sha=SHA256.Create())using(var input=File.OpenRead(path))hash=BitConverter.ToString(sha.ComputeHash(input)).Replace("-","").ToLowerInvariant();
            string cin=CompactSession.Value(r.Identifiers,"cin");
            string company=CompactSession.Value(r.Identifiers,"AccountingEntityName");int year=int.TryParse(CompactSession.Value(r.Identifiers,"fiscalYear"),out int y)?y:0;
            var sourceNumbers=r.Rows.Select((row,index)=>(row,index)).ToDictionary(x=>x.row,x=>x.index+1);
            var rows=r.Rows.Where(row=>importKinds.Contains(row.Kind)).ToArray();
            if(r.Category=="GL"){
                var gl=new GeneralLedgerImport{SourceFileName=Path.GetFileName(path),SourceFilePath=path,SourceFileHash=hash,TechnicalType=r.Format,AccountingFormat=producer,CompanyName=company,Ico=cin.Length>0?cin:null,FiscalYear=year,ImportedAtUtc=DateTime.UtcNow,PeriodHeader=CompactSession.Value(r.Identifiers,"PeriodFrom")+" - "+CompactSession.Value(r.Identifiers,"PeriodTo")};
                if(CompactSession.Date(CompactSession.Value(r.Identifiers,"PeriodTo"),out var periodEnd))gl.ThroughMonth=periodEnd.Month;
                else if(int.TryParse(CompactSession.Value(r.Identifiers,"PeriodTo").Split('/')[0],out int month))gl.ThroughMonth=month;
                foreach(var row in rows){
                    decimal? opening=Get(row,"OpeningNetto"),closing=Get(row,"ClosingNetto");
                    var v=new GeneralLedgerRow{SequenceNumber=gl.Rows.Count+1,SourceRecordNumber=sourceNumbers[row],SourcePageNumber=row.Page>0?(int?)row.Page:null,SourceRowKind=row.Kind,AccountCode=row.Account,AccountName=row.Name,Type=CompactSession.Value(row.Fields,"Type"),SyntheticCode=CompactSession.Value(row.Fields,"SyntheticAccount"),AnalyticalCode=CompactSession.Value(row.Fields,"AnalythicAccount"),SourceOpeningNet=opening,SourceClosingNet=closing,SourceAvailableAmountFields=row.Amounts.Keys.ToArray(),
                        OpeningDebit=opening.HasValue?Math.Max(opening.Value,0):Get(row,"OpeningDebit")??0,OpeningCredit=opening.HasValue?Math.Max(-opening.Value,0):Get(row,"OpeningCredit")??0,
                        ClosingDebit=closing.HasValue?Math.Max(closing.Value,0):Get(row,"ClosingDebit")??0,ClosingCredit=closing.HasValue?Math.Max(-closing.Value,0):Get(row,"ClosingCredit")??0,
                        AnnualDebitTurnover=Get(row,"TurnoverDebit")??0,AnnualCreditTurnover=Get(row,"TurnoverCredit")??0,PeriodDebitTurnover=Get(row,"PeriodTurnoverDebit")??0,PeriodCreditTurnover=Get(row,"PeriodTurnoverCredit")??0};
                    foreach(var field in row.Fields.Where(k=>!row.Amounts.ContainsKey(k.Key)))v.SourceDimensions[field.Key]=field.Value;
                    gl.Rows.Add(v);
                }return gl;
            }
            if(r.Category=="AJ"){
                var aj=new JournalImport{SourceFileName=Path.GetFileName(path),SourceFilePath=path,SourceFileHash=hash,TechnicalType=r.Format,AccountingFormat=producer,CompanyName=company,Ico=cin.Length>0?cin:null,FiscalYear=year,ImportedAtUtc=DateTime.UtcNow};
                foreach(var row in rows){
                    bool dateParsed=CompactSession.Date(CompactSession.Value(row.Fields,"Date"),out var date);
                    string debit=CompactSession.Value(row.Fields,"DebitAccount","Debit"),credit=CompactSession.Value(row.Fields,"CreditAccount","Credit");decimal amount=Get(row,"Amount")??Get(row,"Sum")??0;
                    string doc=CompactSession.Value(row.Fields,"DocumentNumber","DocumentNo"),sub=CompactSession.Value(row.Fields,"DocumentSubnumber","Ordinary");
                    aj.Rows.Add(new JournalRow{SourceFields=new System.Collections.Generic.Dictionary<string,string>(row.Fields),SequenceNumber=aj.Rows.Count+1,SourceRecordNumber=sourceNumbers[row],SourceLocation=row.Fields.ContainsKey("SourceLocation")?row.Fields["SourceLocation"]:"page "+row.Page,DocumentType=row.Fields.TryGetValue("DocumentType",out var documentType)?documentType:null,DebitCostCenter=debit.Length>0&&row.Fields.TryGetValue("CostCentre",out var debitCentre)?debitCentre:null,CreditCostCenter=credit.Length>0&&row.Fields.TryGetValue("CostCentre",out var creditCentre)?creditCentre:null,DocumentNumber=doc+(sub.Length>0?"/"+sub:""),PostingDate=date,Description=CompactSession.Value(row.Fields,"OperationDescription","Description1"),DebitAccount=debit,CreditAccount=credit,DebitAmount=debit.Length>0?(Get(row,"DebitAmount")??amount):(decimal?)null,CreditAmount=credit.Length>0?(Get(row,"CreditAmount")??amount):(decimal?)null});
                }
                if(selectedFiscalYear.HasValue)aj.FiscalYear=selectedFiscalYear.Value;
                if(aj.FiscalYear==0&&aj.Rows.All(v=>v.PostingDate.Year>=1900)&&aj.Rows.Select(v=>v.PostingDate.Year).Distinct().Count()==1)aj.FiscalYear=aj.Rows[0].PostingDate.Year;
                foreach(var row in aj.Rows)JournalDateReview.Apply(row,aj.FiscalYear,row.PostingDate!=DateTime.MinValue);
                return aj;
            }
            var af=new AccountingFrameworkImport{SourceFileName=Path.GetFileName(path),SourceFilePath=path,SourceFileHash=hash,TechnicalType=r.Format,AccountingFormat=producer,CompanyName=company,Ico=cin.Length>0?cin:null,FiscalYear=year,ImportedAtUtc=DateTime.UtcNow};
            foreach(var row in rows){string syn=CompactSession.Value(row.Fields,"SyntheticAccount"),ana=CompactSession.Value(row.Fields,"AnalythicAccount");af.Rows.Add(new AccountingFrameworkRow{SourceFields=new System.Collections.Generic.Dictionary<string,string>(row.Fields),SequenceNumber=af.Rows.Count+1,SourceRecordNumber=sourceNumbers[row],SourceSyntheticCode=syn,SourceAnalyticalCode=ana,SyntheticCode=syn,AnalyticalCode=ana,AccountCode=row.Account.Length>0?row.Account:syn+ana,AccountName=row.Name,Type=CompactSession.Value(row.Fields,"Type"),TaxFlag=CompactSession.Value(row.Fields,"Tax"),BalanceFlag=CompactSession.Value(row.Fields,"Saldo"),VatFlag=CompactSession.Value(row.Fields,"Vat"),RowKind=ana.Length==0&&syn.Length==3?AccountingFrameworkRowKind.SyntheticAccount:AccountingFrameworkRowKind.AnalyticalAccount});}
            return af;
        }
        static decimal? Get(SourceRow row,string key)=>row.Amounts.TryGetValue(key,out var v)?v:null;
        public void Dispose(){if(excel!=null){excel.Dispose();return;}if(crystal!=null){crystal.Dispose();return;}if(structured!=null){structured.Dispose();return;}if(dbfAccounts!=null)dbfAccounts.Dispose();else if(dbf!=null)dbf.Dispose();else session.Dispose();}
    }
}
