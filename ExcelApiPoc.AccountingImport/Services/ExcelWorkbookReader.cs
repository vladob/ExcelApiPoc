using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ExcelApiPoc.AccountingImport.Services
{
    internal static class ExcelWorkbookReader
    {
        private static readonly Encoding Windows1250;
        private static readonly Encoding Windows1252;

        static ExcelWorkbookReader()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Windows1250 = Encoding.GetEncoding(
                1250,
                EncoderFallback.ExceptionFallback,
                DecoderFallback.ExceptionFallback);

            Windows1252 = Encoding.GetEncoding(
                1252,
                EncoderFallback.ExceptionFallback,
                DecoderFallback.ExceptionFallback);
        }

        public static string GetUrbisText(
    IExcelDataReader reader,
    int columnIndex,
    string sourceExtension)
        {
            string value = GetText(reader, columnIndex);

            if (value == null ||
                !string.Equals(
                    sourceExtension,
                    ".xls",
                    StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            try
            {
                byte[] originalBytes =
                    Windows1252.GetBytes(value);

                return Windows1250.GetString(originalBytes);
            }
            catch (EncoderFallbackException)
            {
                // BIFF8 may already contain correctly decoded Unicode.
                // In that case, preserve it without conversion.
                return value;
            }
            catch (DecoderFallbackException)
            {
                return value;
            }
        }

        public static IExcelDataReader Open(string filePath)
        {
            FileStream stream = File.Open(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);

            try
            {
                return ExcelReaderFactory.CreateReader(
                    stream,
                    new ExcelReaderConfiguration
                    {
                        FallbackEncoding = Encoding.GetEncoding(1250),
                        LeaveOpen = false
                    });
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        public static string ValidateSingleWorksheet(
            IExcelDataReader reader,
            string sourceFileName)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.ResultsCount == 1)
            {
                return reader.Name;
            }

            var worksheetNames = new List<string>();

            do
            {
                worksheetNames.Add(reader.Name ?? "(unnamed)");
            }
            while (reader.NextResult());

            throw new InvalidDataException(
                "Urbis workbook '" + sourceFileName + "' contains " +
                reader.ResultsCount + " worksheets: " +
                string.Join(", ", worksheetNames) +
                ". Exactly one worksheet is required.");
        }

        public static string GetText(
            IExcelDataReader reader,
            int columnIndex)
        {
            object value = reader.GetValue(columnIndex);

            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            if (value is string text)
            {
                return text;
            }

            if (value is DateTime date)
            {
                return date.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture);
            }

            if (value is IFormattable formattable)
            {
                return formattable.ToString(
                    null,
                    CultureInfo.InvariantCulture);
            }

            return value.ToString();
        }

        public static bool IsBlank(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }
    }
}