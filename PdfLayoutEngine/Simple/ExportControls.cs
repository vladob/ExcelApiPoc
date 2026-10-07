using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace PdfLayoutEngine.Simple;
// Printed export controls are checked separately from canonical business reconciliation.
public static class ExportControls
{
    public static bool Money(string raw,out decimal value){string s=Regex.Replace(raw??"",@"\s", "").Replace(",",".");if(s=="-"||s=="*"){value=0;return true;}s=s.TrimStart('*');return decimal.TryParse(s,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out value);}
    public static void Validate(ImportResult result,string category,bool documentGroups){
        void Error(string code,SourceRow row,string message)=>result.Issues.Add(new ImportIssue{Code=code,Severity="error",Page=row.Page>0?(int?)row.Page:null,Message=CompactSession.Value(row.Fields,"SourceLocation")+": "+message});
        decimal Amount(SourceRow row,string key)=>row.Amounts.TryGetValue(key,out var v)&&v.HasValue?v.Value:0;
        void Compare(SourceRow row,string key,decimal sum){if(!row.Amounts.TryGetValue(key,out var v)||!v.HasValue||Math.Abs(v.Value-sum)>.01m)Error("printedTotalMismatch",row,key+" calculated "+sum.ToString(CultureInfo.InvariantCulture));}
        var records=result.Rows.Where(v=>v.Kind=="Record").ToArray();if(records.Length==0){result.Issues.Add(new ImportIssue{Code="noData",Message="No importable rows."});return;}
        if(category=="GL"){
            var totals=result.Rows.Where(v=>v.Kind=="ReportTotal").ToArray();if(totals.Length!=1){Error("missingReportTotal",records[0],"Expected one report total.");return;}
            foreach(var k in new[]{"TurnoverDebit","TurnoverCredit"})Compare(totals[0],k,records.Sum(v=>Amount(v,k)));
            foreach(var row in records)if(Math.Abs(Amount(row,"OpeningNetto")+Amount(row,"TurnoverDebit")-Amount(row,"TurnoverCredit")-Amount(row,"ClosingNetto"))>.01m)Error("balanceMismatch",row,"Opening + debit - credit differs from closing.");
            return;
        }
        if(category!="AJ")return;
        var dates=new Dictionary<string,decimal[]>();
        string doc="";decimal debit=0,credit=0,groupDebit=0,groupCredit=0;bool hasRecords=false,endsWithTotal=false;int totalsCount=0;
        foreach(var row in result.Rows){
            var label=CompactSession.Value(row.Fields,"Structure");
            if(row.Kind=="DocumentStart"){
                if(doc.Length>0)Error("missingDocumentTotal",row,doc);
                doc=label.StartsWith("Doklad:")?label.Substring(7).Trim():"";groupDebit=groupCredit=0;
                if(doc.Length==0)Error("missingDocumentNumber",row,label);endsWithTotal=false;
            }else if(row.Kind=="Record"){
                if(documentGroups&&doc.Length==0)Error("missingDocumentHeader",row,"Record without document header.");
                var a=Amount(row,"Amount");string day=CompactSession.Date(CompactSession.Value(row.Fields,"Date"),out var date)?date.ToString("yyyy-MM-dd"):"invalid";
                if(!dates.ContainsKey(day))dates[day]=new decimal[2];if(CompactSession.Value(row.Fields,"DebitAccount").Length>0)dates[day][0]+=a;if(CompactSession.Value(row.Fields,"CreditAccount").Length>0)dates[day][1]+=a;if(CompactSession.Value(row.Fields,"DebitAccount").Length>0){debit+=a;groupDebit+=a;}if(CompactSession.Value(row.Fields,"CreditAccount").Length>0){credit+=a;groupCredit+=a;}hasRecords=true;endsWithTotal=false;
            }else if(row.Kind=="DocumentTotal"||row.Kind=="DateTotal"){
                if(row.Kind=="DocumentTotal"&&(doc.Length==0||label.Substring("Spolu za doklad".Length).Trim()!=doc))Error("documentTotalMismatch",row,label);
                if(row.Kind=="DateTotal"){
                    string raw=label.Substring("Spolu za ".Length).Trim();if(!CompactSession.Date(raw,out var dayDate)||!dates.TryGetValue(dayDate.ToString("yyyy-MM-dd"),out var sums))Error("invalidDateTotal",row,label);else{Compare(row,"DebitAccount",sums[0]);Compare(row,"CreditAccount",sums[1]);}
                }else{Compare(row,"DebitAccount",groupDebit);Compare(row,"CreditAccount",groupCredit);}groupDebit=groupCredit=0;doc="";
            }else if(row.Kind=="ReportTotal"){
                if(doc.Length>0)Error("missingDocumentTotal",row,doc);
                Compare(row,"DebitAccount",debit);Compare(row,"CreditAccount",credit);debit=credit=0;groupDebit=groupCredit=0;endsWithTotal=true;totalsCount++;dates.Clear();
            }
        }
        if(doc.Length>0||hasRecords&&(!endsWithTotal||totalsCount==0))Error("missingReportTotal",records.Last(),"Export does not end with validated totals.");
    }
}
