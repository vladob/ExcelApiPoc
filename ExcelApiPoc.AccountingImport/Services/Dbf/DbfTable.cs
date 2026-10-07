using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
namespace ExcelApiPoc.AccountingImport.Services.Dbf
{
// Deliberately scoped to the observed dBASE III, non-memo exports.
public sealed class DbfTable : IDisposable
{
    readonly FileStream stream;
    public sealed class Field { public string Name; public char Type; public int Width,Scale,Offset; }
    public sealed class Record { public int Number; public bool Deleted; public Dictionary<string,string> Fields; }
    public List<Field> Fields { get; } = new List<Field>();
    public int RecordCount { get; }
    public int HeaderLength { get; }
    public int RecordLength { get; }
    public int LanguageDriver { get; }
    public DbfTable(string path)
    {
        stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);
        try {
            var h=Read(32);
            if(h[0]!=3)throw new NotSupportedException("Only non-memo dBASE III DBF files are supported.");
            uint count=(uint)(h[4]|h[5]<<8|h[6]<<16|h[7]<<24);
            if(count>int.MaxValue)throw new InvalidDataException("DBF record count is too large.");
            RecordCount=(int)count;HeaderLength=h[8]|h[9]<<8;RecordLength=h[10]|h[11]<<8;LanguageDriver=h[29];
            if(HeaderLength<33 || (HeaderLength-33)%32!=0 || RecordLength<2 || HeaderLength+(long)RecordCount*RecordLength>stream.Length)throw new InvalidDataException("Invalid or truncated DBF record boundaries.");
            int offset=1;
            for(int pos=32;pos<HeaderLength-1;pos+=32){
                var b=Read(32);string name=Encoding.ASCII.GetString(b,0,11).TrimEnd('\0',' ');char type=(char)b[11];
                if(name.Length==0 || Fields.Any(f=>f.Name==name) || b[16]==0 || !"CNDL".Contains(type.ToString()))throw new InvalidDataException("Unsupported or invalid DBF field: "+name);
                if((type=='D'&&b[16]!=8)||(type=='L'&&b[16]!=1)||b[17]>b[16])throw new InvalidDataException("Invalid DBF field dimensions: "+name);
                Fields.Add(new Field{Name=name,Type=type,Width=b[16],Scale=b[17],Offset=offset});offset+=b[16];
            }
            if(Read(1)[0]!=13 || offset!=RecordLength)throw new InvalidDataException("Invalid DBF field terminator or record length.");
            long end=HeaderLength+(long)RecordCount*RecordLength;
            if(stream.Length!=end && stream.Length!=end+1)throw new InvalidDataException("Unexpected bytes after DBF records.");
            if(stream.Length==end+1){stream.Position=end;if(Read(1)[0]!=26)throw new InvalidDataException("Invalid DBF end marker.");}
        }catch {stream.Dispose();throw;}
    }
    byte[] Read(int size){var b=new byte[size];int at=0;while(at<size){int n=stream.Read(b,at,size-at);if(n==0)throw new EndOfStreamException("Truncated DBF.");at+=n;}return b;}
    public IEnumerable<Record> Records(Encoding encoding,CancellationToken token,ISet<string> selected=null)
    {
        stream.Position=HeaderLength;
        for(int i=0;i<RecordCount;i++){
            token.ThrowIfCancellationRequested();var b=Read(RecordLength);
            if(b[0]!=32&&b[0]!=42)throw new InvalidDataException("Invalid deletion flag at DBF record "+(i+1));
            var values=new Dictionary<string,string>(StringComparer.Ordinal);
            if(b[0]!=42)foreach(var f in Fields.Where(f=>selected==null||selected.Contains(f.Name)))values[f.Name]=encoding.GetString(b,f.Offset,f.Width).TrimEnd(' ','\0');
            yield return new Record{Number=i+1,Deleted=b[0]==42,Fields=values};
        }
    }
    public void Dispose()=>stream.Dispose();
}

}
