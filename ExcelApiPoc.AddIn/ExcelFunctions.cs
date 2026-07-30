using ExcelDna.Integration;

namespace ExcelApiPoc.AddIn
{
    public static class ExcelFunctions
    {
        [ExcelFunction(
            Name = "POC.TEST",
            Description = "Verifies that the Excel-DNA proof-of-concept add-in is loaded.")]
        public static string Test()
        {
            return "Excel-DNA add-in is working!";
        }
    }
}