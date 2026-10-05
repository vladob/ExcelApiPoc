using System.Diagnostics;
using System.Text.Json;
using ExcelApiPoc.AccountingImport.Services.Layouts;
using PdfLayoutEngine.Simple;
int timeoutSeconds=120;
var timeoutOptions=args.Where(a=>a.StartsWith("--timeout-seconds=",StringComparison.Ordinal)).ToArray();
if(timeoutOptions.Length>1||timeoutOptions.Length==1&&(!int.TryParse(timeoutOptions[0].Substring("--timeout-seconds=".Length),out timeoutSeconds)||timeoutSeconds<1||timeoutSeconds>3600))throw new ArgumentException("Timeout must be 1..3600 seconds.");
args=args.Where(a=>!a.StartsWith("--timeout-seconds=",StringComparison.Ordinal)).ToArray();
if(args.Length<3){Console.Error.WriteLine("Usage: SimpleRunner <layout.json-or-directory> <level:1..5> <file-directory-or-manifest.csv> [output-directory] [selected-fiscal-year] [--timeout-seconds=120]");return 1;}
if(!int.TryParse(args[1],out int level)||level<1||level>5)throw new ArgumentException("Level must be 1..5.");
string output=args.Length>3?args[3]:"TestResults/CompactLayouts";Directory.CreateDirectory(output);
var files=Path.GetExtension(args[2]).Equals(".csv",StringComparison.OrdinalIgnoreCase)&&IsManifest(args[2])?ReadManifest(args[2]):Directory.Exists(args[2])?Directory.GetFiles(args[2]).Where(f=>new[]{".pdf",".xps",".oxps",".dbf",".csv",".xml",".xls",".xlsx"}.Contains(Path.GetExtension(f).ToLowerInvariant())).OrderBy(f=>f).Select(f=>new Input(f)):new[]{new Input(args[2])};
var options=new JsonSerializerOptions{WriteIndented=true,PropertyNamingPolicy=JsonNamingPolicy.CamelCase};
using var summary=new StreamWriter(Path.Combine(output,"summary.csv"));summary.WriteLine("File,RequestedLevel,CompletedLevel,Status,Layout,Category,FiscalYear,DecodedPages,Rows,Milliseconds,Errors,Warnings,ExpectedYearCheck,ExpectedCategoryCheck");
int? selectedYear=args.Length>4?int.Parse(args[4]):null;
if(selectedYear.HasValue && (selectedYear<1900||selectedYear>9999))throw new ArgumentException("Selected fiscal year must be 1900..9999.");
int failures=0,ordinal=0;
foreach(var input in files){
    var watch=Stopwatch.StartNew();string stem=(++ordinal).ToString("D4")+"-"+Path.GetFileName(input.FilePath);
    try{
        using var importer=new CompactLayoutImporter(input.FilePath,args[0],selectedYear);using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        var result=importer.Examine((ImportLevel)level,timeout.Token);
        string year=CompactSession.Value(result.Identifiers,"fiscalYear");
        string yearCheck=string.IsNullOrEmpty(input.ExpectedYear)?"notSpecified":level<2?"notExamined":year.Length==0?"unavailable":year==input.ExpectedYear?"pass":"fail";
        string category=ExpectedCategory(input.PerceivedCategory);
        string categoryCheck=category.Length==0?"notSpecified":result.Category==null?"unavailable":category==result.Category?"pass":"fail";
        string status=yearCheck=="fail"||categoryCheck=="fail"?"expectationMismatch":result.Status;
        File.WriteAllText(Path.Combine(output,stem+".json"),JsonSerializer.Serialize(new{input,result,canonical=importer.Canonical,expectedYearCheck=yearCheck,expectedCategoryCheck=categoryCheck},options));
        int errors=result.Issues.Count(i=>i.Severity=="error"),warnings=result.Issues.Count(i=>i.Severity=="warning");
        summary.WriteLine(string.Join(",",new[]{input.FilePath,level.ToString(),result.CompletedLevel.ToString(),status,result.LayoutId??"",result.Category??"",year,result.DecodedPageCount.ToString(),result.Rows.Count.ToString(),watch.ElapsedMilliseconds.ToString(),errors.ToString(),warnings.ToString(),yearCheck,categoryCheck}.Select(Csv)));summary.Flush();
        Console.WriteLine($"{Path.GetFileName(input.FilePath)}: {status}; decoded {result.DecodedPageCount} pages; {result.Rows.Count} source rows; {watch.ElapsedMilliseconds} ms; {errors} errors");
        if(status!="completed")failures++;
    }catch(Exception ex){failures++;summary.WriteLine(string.Join(",",new[]{input.FilePath,level.ToString(),"0","error","","","","0","0",watch.ElapsedMilliseconds.ToString(),"1","0","unavailable","unavailable"}.Select(Csv)));summary.Flush();File.WriteAllText(Path.Combine(output,stem+".error.json"),JsonSerializer.Serialize(new{input,error=ex.ToString()},options));Console.Error.WriteLine(input.FilePath+": "+ex.Message);}
}
return failures>0?2:0;
static string Csv(string s)=>"\""+s.Replace("\"","\"\"")+"\"";
static string ExpectedCategory(string raw)=>raw.Trim().ToUpperInvariant() switch{"GL" or "HL_KNIHA"=>"GL","AJ" or "U_DENNIK" or "DENNIK"=>"AJ","AF" or "UCT_ROZVRH" or "UROZVRH"=>"AF",_=>""};
static IEnumerable<Input> ReadManifest(string path){
    using var parser=new Microsoft.VisualBasic.FileIO.TextFieldParser(path);parser.SetDelimiters(";");parser.HasFieldsEnclosedInQuotes=true;
    var header=parser.ReadFields()??throw new InvalidDataException("Empty manifest.");int fileIndex=Array.IndexOf(header,"FilePath");if(fileIndex<0)throw new InvalidDataException("Manifest requires FilePath.");
    string Field(string[] values,string key){int i=Array.IndexOf(header,key);return i<0?"":values[i];}
    while(!parser.EndOfData){var fields=parser.ReadFields()!;if(fields.Length!=header.Length)throw new InvalidDataException("Malformed manifest at line "+parser.LineNumber);string file=fields[fileIndex];if(!Path.IsPathRooted(file))file=Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!,file));yield return new Input(file,Field(fields,"ExpectedYear"),Field(fields,"PerceivedCategory"),Field(fields,"AccountingEntity"));}
}

static bool IsManifest(string path){using var r=new StreamReader(path);var line=(r.ReadLine()??"").TrimStart('\uFEFF');return line.Split(';').Any(v=>v.Trim().Trim('"')=="FilePath");}

record Input(string FilePath,string ExpectedYear="",string PerceivedCategory="",string AccountingEntity="");
