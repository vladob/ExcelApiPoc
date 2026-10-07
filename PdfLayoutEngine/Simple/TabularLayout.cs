using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
namespace PdfLayoutEngine.Simple;
// Column coordinates are reference PDF points; row predicates are evaluated in order.
public sealed class TabularLayout
{
    public string[] ContinueAmountKinds {get;set;}=Array.Empty<string>();
    public bool PageNumberInFooter {get;set;}
    public string AccountPattern {get;set;}=@"^\d{6}$";
    public string RequiredPageText {get;set;}="";
    public Dictionary<string,string> IdentifierPatterns {get;set;}=new Dictionary<string,string>();
    public bool BlankAmountsAreZero {get;set;}
    public string[] ContinuationFields {get;set;}=Array.Empty<string>();
    public string ControlMode {get;set;}="";
    public bool DocumentGroups {get;set;}
    public string PageNumberPattern {get;set;}="";
    public string IgnorePattern {get;set;}="";
    public double EntityWidth {get;set;}=.55;
    public string Orientation {get;set;}="";
    public string TitlePattern {get;set;}="";
    public string RequiredHeader {get;set;}="";
    public string ForbiddenHeader {get;set;}="";
    public string LeftLabel {get;set;}="";
    public string RightLabel {get;set;}="";
    public double Left {get;set;}
    public double Right {get;set;}
    public string LastHeaderLabel {get;set;}="";
    public double HeaderPadding {get;set;}=3;
    public Dictionary<string,double[]> Columns {get;set;}=new Dictionary<string,double[]>();
    public bool AmountsInSourceOrder {get;set;}
    public Dictionary<string,string[]> AmountSums {get;set;}=new Dictionary<string,string[]>();
    public string[] Amounts {get;set;}=Array.Empty<string>();
    public List<TabularRule> Rules {get;set;}=new List<TabularRule>();
    public bool ContinueAcrossPages {get;set;}
    public string ContinuationKind {get;set;}="";
    public string ContinuationField {get;set;}="";
    public string GroupPattern {get;set;}="^(?<group>.+?)\\s+EUR$";
    public string[] CompactFields {get;set;}=Array.Empty<string>();
    public void Validate(){
        if(!new[]{"","export","separateSides","accountTotals"}.Contains(ControlMode)||EntityWidth<=0||EntityWidth>1||double.IsNaN(EntityWidth))throw new InvalidDataException("Invalid table options.");
        foreach(var pattern in IdentifierPatterns.Values)new Regex(pattern);
        if(ContinuationFields.Any(k=>!Columns.ContainsKey(k)))throw new InvalidDataException("Unknown continuation field.");
        if(PageNumberPattern.Length>0)new Regex(PageNumberPattern);if(IgnorePattern.Length>0)new Regex(IgnorePattern);
        if(double.IsNaN(Left)||double.IsNaN(Right)||double.IsInfinity(Left)||double.IsInfinity(Right)||Right<=Left||string.IsNullOrEmpty(TitlePattern)||Rules.Count==0||Columns.Count==0)throw new InvalidDataException("Invalid tabular layout.");
        foreach(var c in Columns.Values)if(c.Length!=2||c.Any(v=>double.IsNaN(v)||double.IsInfinity(v))||c[1]<=c[0])throw new InvalidDataException("Invalid column.");
        if(Amounts.Concat(CompactFields).Any(k=>!Columns.ContainsKey(k)))throw new InvalidDataException("Unknown column.");
        if(AmountSums.Values.Any(v=>v.Length==0||v.Any(k=>!Amounts.Contains(k))))throw new InvalidDataException("Unknown sum field.");
        foreach(var rule in Rules){if(!Columns.ContainsKey(rule.Field))throw new InvalidDataException("Unknown predicate field.");new Regex(rule.Pattern);}
        new Regex(TitlePattern);new Regex(GroupPattern);new Regex(AccountPattern);
        if(ContinueAmountKinds.Any(k=>!Rules.Any(r=>r.Kind==k)))throw new InvalidDataException("Unknown amount continuation kind.");
    }
}
public sealed class TabularRule
{
    public string Kind {get;set;}="";
    public string Field {get;set;}="";
    public string Pattern {get;set;}="";
}
