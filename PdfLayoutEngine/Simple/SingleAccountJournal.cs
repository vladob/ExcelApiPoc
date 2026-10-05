using System;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
namespace PdfLayoutEngine.Simple;
// A source account with independent MD/DAL values, and an authoritative YYYYMM period.
public static class SingleAccountJournal
{
    public static bool Period(string? raw,out int year,out int month){year=month=0;return raw!=null&&Regex.IsMatch(raw,@"^\d{6}$")&&int.TryParse(raw.Substring(0,4),out year)&&year>=1900&&int.TryParse(raw.Substring(4,2),out month)&&month>=1&&month<=12;}
    public static void Identify(ImportResult result,string period){
        result.Identifiers["accountingPeriod"]=period;result.Identifiers["fiscalYearBasis"]="accounting period (verified against all rows at L4)";
        if(Period(period,out var year,out var month)){result.Identifiers["fiscalYear"]=year.ToString();result.Identifiers["PeriodFrom"]=new DateTime(year,month,1).ToString("dd.MM.yyyy");result.Identifiers["PeriodTo"]=new DateTime(year,month,DateTime.DaysInMonth(year,month)).ToString("dd.MM.yyyy");}
        else result.Issues.Add(new ImportIssue{Code="invalidAccountingPeriod",Message="Expected YYYYMM accounting period: "+period});
    }
    public static void Validate(ImportResult result){
        void Error(string code,SourceRow row,string message)=>result.Issues.Add(new ImportIssue{Code=code,Page=row.Page>0?(int?)row.Page:null,Message=CompactSession.Value(row.Fields,"SourceLocation")+": "+message});
        string expected=CompactSession.Value(result.Identifiers,"accountingPeriod");int badDate=0,outsideMonth=0,outsideYear=0,zero=0;
        foreach(var row in result.Rows){
            string period=CompactSession.Value(row.Fields,"AccountingPeriod"),account=Regex.Replace(CompactSession.Value(row.Fields,"Account"),@"\s","");
            if(!Period(period,out var year,out var month)||period!=expected)Error("inconsistentAccountingPeriod",row,"Expected "+expected+", found "+period);
            foreach(var side in new[]{"Debit","Credit"}){
                string raw=CompactSession.Value(row.Fields,side+"Amount");
                if(raw.Length==0)row.Amounts[side+"Amount"]=0m;
                else if(ExportControls.Money(raw,out var amount))row.Amounts[side+"Amount"]=amount;
                else{row.Amounts[side+"Amount"]=null;Error("invalidAmount",row,side+": "+raw);}
                row.Fields[side+"Account"]=row.Amounts[side+"Amount"].GetValueOrDefault()!=0m?account:"";
            }
            if(!Regex.IsMatch(account,@"^\d{3,12}$"))Error("invalidAccount",row,account);
            if(row.Amounts.Values.All(a=>a.HasValue&&a.Value==0m)){row.Kind="ZeroPosting";zero++;}
            string dateText=CompactSession.Value(row.Fields,"Date");
            if(!CompactSession.Date(dateText,out var date)||date.Year<1900)badDate++;
            else if(date.Year!=year)outsideYear++;else if(date.Month!=month)outsideMonth++;
        }
        void Warning(string code,string message,int count){if(count>0)result.Issues.Add(new ImportIssue{Code=code,Severity="warning",Message=count.ToString(CultureInfo.InvariantCulture)+" rows: "+message});}
        Warning("invalidDate","invalid posting date; retained for auditor review.",badDate);
        Warning("dateOutsideFiscalYear","posting date is outside the accounting year; initially excluded from calculation.",outsideYear);
        Warning("dateOutsideAccountingMonth","posting date is outside the accounting month but within the year; retained.",outsideMonth);
        Warning("zeroAmountRows","neither debit nor credit; retained as source rows, not canonical postings.",zero);
        if(!result.Rows.Any(r=>r.Kind=="Record"))result.Issues.Add(new ImportIssue{Code="noData",Message="No nonzero journal postings."});
    }
}
