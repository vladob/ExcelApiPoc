using System.Text;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AccountCodeNormalizer
    {
        public static string Normalize(string value)
        {
            string source = value ?? string.Empty;
            var result = new StringBuilder(source.Length);
            foreach (char character in source)
                if (!char.IsWhiteSpace(character))
                    result.Append(character);
            return result.ToString();
        }
    }
}
