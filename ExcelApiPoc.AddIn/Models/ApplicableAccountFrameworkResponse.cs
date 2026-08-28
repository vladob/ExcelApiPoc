using System;

namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class ApplicableAccountFrameworkResponse
    {
        public string FrameworkCode { get; set; }
        public string FrameworkName { get; set; }
        public int FrameworkVersionId { get; set; }
        public string VersionCode { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string LegalReference { get; set; }
        public string SourceUrl { get; set; }
        public string SourceSha256 { get; set; }
        public AccountDefinitionMetadataResponse[] Definitions { get; set; }
        public AccountRangeMetadataResponse[] Ranges { get; set; }
    }

    internal sealed class AccountDefinitionMetadataResponse
    {
        public int AccountLevel { get; set; }
        public string AccountCode { get; set; }
        public string OfficialName { get; set; }
    }

    internal sealed class AccountRangeMetadataResponse
    {
        public int AccountLevel { get; set; }
        public string FromAccountCode { get; set; }
        public string ToAccountCode { get; set; }
        public string OfficialName { get; set; }
    }
}