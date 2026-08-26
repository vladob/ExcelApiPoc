using System;
using System.IO;
using System.Text;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class TemplateMetadataCache
    {
        public static string GetMetadataPath(int templateErpId)
        {
            if (templateErpId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(templateErpId));
            }

            string localAppData =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData);

            return Path.Combine(
                localAppData,
                "ExcelApiPoc",
                "Cache",
                "Templates",
                templateErpId.ToString(),
                "metadata.json");
        }

        public static string SaveMetadata(
            int templateErpId,
            string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException(
                    "Template JSON cannot be empty.",
                    nameof(json));
            }

            string metadataPath =
                GetMetadataPath(templateErpId);

            string directory =
                Path.GetDirectoryName(metadataPath);

            Directory.CreateDirectory(directory);

            string temporaryPath =
                metadataPath + ".tmp";

            try
            {
                File.WriteAllText(
                    temporaryPath,
                    json,
                    new UTF8Encoding(false));

                if (File.Exists(metadataPath))
                {
                    File.Replace(
                        temporaryPath,
                        metadataPath,
                        null);
                }
                else
                {
                    File.Move(
                        temporaryPath,
                        metadataPath);
                }

                return metadataPath;
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        public static string LoadMetadata(int templateErpId)
        {
            string metadataPath =
                GetMetadataPath(templateErpId);

            if (!File.Exists(metadataPath))
            {
                throw new FileNotFoundException(
                    "No cached audit-template metadata is available.",
                    metadataPath);
            }

            string json =
                File.ReadAllText(metadataPath, Encoding.UTF8);

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidDataException(
                    "The cached audit-template metadata is empty.");
            }

            return json;
        }
    }
}