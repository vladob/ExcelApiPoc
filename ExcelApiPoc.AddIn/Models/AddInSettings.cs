namespace ExcelApiPoc.AddIn.Models
{
    internal sealed class AddInSettings
    {
        public string ApiBaseUrl { get; set; }
        public string ApiKey { get; set; }
        public string UiLanguage { get; set; }
        public bool RoundCalculatedAmountsToWholeEuros { get; set; }
    }
}
