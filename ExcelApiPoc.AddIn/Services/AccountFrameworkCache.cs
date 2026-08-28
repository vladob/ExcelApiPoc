using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AccountFrameworkCache
    {
        public static string GetPath(string frameworkCode, int fiscalYear, int contractVersion)
        {
            ValidateArguments(frameworkCode, fiscalYear, contractVersion);
            string localAppData = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "ExcelApiPoc", "Cache", "AccountFrameworks", frameworkCode.Trim().ToUpperInvariant(), fiscalYear.ToString(CultureInfo.InvariantCulture), $"applicable-v{contractVersion}.json");
        }

        public static string Save(string frameworkCode, int fiscalYear, int contractVersion, string json)
        {
            string targetPath = GetPath(frameworkCode, fiscalYear, contractVersion);
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("JSON content cannot be empty.", nameof(json));
            }

            string directory = Path.GetDirectoryName(targetPath);
            Directory.CreateDirectory(directory);
            string temporaryPath = targetPath + ".tmp";

            try
            {
                File.WriteAllText(temporaryPath, json, new UTF8Encoding(false));

                if (File.Exists(targetPath))
                {
                    File.Replace(temporaryPath, targetPath, null);
                }
                else
                {
                    File.Move(temporaryPath, targetPath);
                }

                return targetPath;
            }
            finally
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
            }
        }

        public static string Load(string frameworkCode, int fiscalYear, int contractVersion)
        {
            string path = GetPath(frameworkCode, fiscalYear, contractVersion);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException("No cached account framework is available.", path);
            }
            string json = File.ReadAllText(path, Encoding.UTF8);

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidDataException("The cached account framework is empty.");
            }
            return json;
        }

        private static void ValidateArguments(string frameworkCode, int fiscalYear, int contractVersion)
        {
            if (string.IsNullOrWhiteSpace(frameworkCode))
            {
                throw new ArgumentException("Framework code is required.", nameof(frameworkCode));
            }

            foreach (char character in frameworkCode.Trim())
            {
                if (!char.IsLetterOrDigit(character) && character != '_' && character != '-')
                {
                    throw new ArgumentException("Framework code contains an invalid character.", nameof(frameworkCode));
                }
            }

            if (fiscalYear < 1900 || fiscalYear > 9999)
            {
                throw new ArgumentOutOfRangeException(nameof(fiscalYear));
            }

            if (contractVersion <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(contractVersion));
            }
        }
    }
}