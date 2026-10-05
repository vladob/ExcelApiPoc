using System;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
namespace PdfLayoutEngine.Simple;
// Separate debit/credit exports and printed synthetic-account controls.
public static class IndependentControls
{
    public static void Validate(ImportResult result,string mode,string[] fields,bool printedTotals=true,string accountPattern=@"^\d{6}$"){
        void Error(string code,SourceRow row,string text)=>result.Issues.Add(new ImportIssue{Code=code,Message=text,Page=row.Page>0?(int?)row.Page:null});
        decimal Amount(SourceRow row,string field)=>row.Amounts.TryGetValue(field,out var v)&&v.HasValue?v.Value:0m;
        var records=result.Rows.Where(v=>v.Kind=="Record").ToArray();
        if(records.Length==0){result.Issues.Add(new ImportIssue{Code="noData",Message="No importable records."});return;}
        void Compare(SourceRow total,SourceRow[] members){foreach(var field in fields){if(!total.Amounts.TryGetValue(field,out var printed)||!printed.HasValue||members.Any(r=>!r.Amounts.TryGetValue(field,out var v)||!v.HasValue)||Math.Abs(members.Sum(r=>Amount(r,field))-printed.Value)>.005m)Error("printedTotalMismatch",total,field+": extracted "+members.Sum(r=>Amount(r,field)).ToString(CultureInfo.InvariantCulture)+", printed "+printed);}}
        if(mode=="separateSides"){
            foreach(var row in records)foreach(var side in new[]{"Debit","Credit"}){
                var account=CompactSession.Value(row.Fields,side+"Account");
                if(account.Length==0&&Amount(row,side+"Amount")!=0m)Error("amountWithoutAccount",row,side+" amount has no account.");
                if(account.Length>0&&!Regex.IsMatch(account,@"^\d{6}$"))Error("invalidAccount",row,side+" account: "+account);
            }
            if(printedTotals){var totals=result.Rows.Where(r=>r.Kind=="ReportTotal").ToArray();if(totals.Length!=1)Error("missingReportTotal",records[0],"Expected one printed journal total.");else Compare(totals[0],records);}
        }else{
            foreach(var row in records){
                if(!Regex.IsMatch(row.Account,accountPattern))Error("invalidAccount",row,row.Account);
                if(Math.Abs(Amount(row,"OpeningNetto")+Amount(row,"TurnoverDebit")-Amount(row,"TurnoverCredit")-Amount(row,"ClosingNetto"))>.005m)Error("balanceMismatch",row,row.Account+": opening + annual debit - annual credit differs from closing.");
            }
            foreach(var group in records.GroupBy(r=>r.Account.Length>=3?r.Account.Substring(0,3):r.Account)){
                var totals=result.Rows.Where(r=>r.Kind=="SyntheticTotal"&&CompactSession.Value(r.Fields,"SubtotalAccount")==group.Key).ToArray();
                if(totals.Length!=1)Error("missingAccountTotal",group.First(),"Expected one printed synthetic total for "+group.Key);else Compare(totals[0],group.ToArray());
            }
            foreach(var total in result.Rows.Where(r=>r.Kind=="SyntheticTotal"))if(!records.Any(r=>r.Account.StartsWith(CompactSession.Value(total.Fields,"SubtotalAccount"),StringComparison.Ordinal)))Error("orphanAccountTotal",total,"Synthetic total has no extracted accounts.");
        }
    }
}
