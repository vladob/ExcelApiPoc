using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class AccountDetailTextSettings
    {
        public List<AccountDetailCategory> Categories { get; set; }
        public List<AccountDetailText> Texts { get; set; }
        public List<AccountDetailDefault> Defaults { get; set; }
    }

    internal sealed class AccountDetailCategory
    {
        public string Code { get; set; }
        public string DisplayNameSk { get; set; }
        public int SortOrder { get; set; }
    }

    internal sealed class AccountDetailText
    {
        public int TextId { get; set; }
        public string CategoryCode { get; set; }
        public string TextSk { get; set; }
        public int SortOrder { get; set; }
    }

    internal sealed class AccountDetailDefault
    {
        public string Account { get; set; }
        public string CategoryCode { get; set; }
        public int TextId { get; set; }
    }

    internal sealed class AccountDetailLayout
    {
        public int VersionNo { get; set; }
        public JObject Definition { get; set; }
    }
}
