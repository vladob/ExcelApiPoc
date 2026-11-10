using System;
using System.IO;

namespace AccountBalanceVerification.AddIn
{
    internal static class PdfExportLocation
    {
        private static string remembered;
        private static string StorePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AccountBalanceVerification", "last-pdf-directory.txt");

        internal static string InitialDirectory
        {
            get {
                if (remembered == null) {
                    remembered = "";
                    try { if (File.Exists(StorePath)) remembered = File.ReadAllText(StorePath).Trim(); }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
                // A removed or disconnected destination must not prevent export.
                return Directory.Exists(remembered) ? remembered : "";
            }
        }

        internal static void Remember(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory)) return;
            remembered = directory;
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(StorePath));
                File.WriteAllText(StorePath, directory);
            } catch (IOException) { /* Remember for this session even if persistence fails. */ }
            catch (UnauthorizedAccessException) { }
        }
    }
}
