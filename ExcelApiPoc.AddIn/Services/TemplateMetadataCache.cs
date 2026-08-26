using System;
using System.IO;
using System.Text;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class TemplateMetadataCache
    {
        public static string GetMetadataPath(int templateErpId)
        {
            return Path.Combine(GetTemplateDirectory(templateErpId), "metadata.json");
        }

        public static string GetPackagePath(int templateErpId,int contractVersion)
        {
            if (contractVersion <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(contractVersion));
            }

            return Path.Combine(GetTemplateDirectory(templateErpId), $"package-v{contractVersion}.json");
        }

        public static string SaveMetadata(int templateErpId,string json)
        {
            return SaveJson(GetMetadataPath(templateErpId),json);
        }

        public static string LoadMetadata(int templateErpId)
        {
            return LoadJson(GetMetadataPath(templateErpId),"audit-template metadata");
        }

        public static string SavePackage(int templateErpId, int contractVersion, string json)
        {
            return SaveJson(GetPackagePath(templateErpId, contractVersion),json);
        }

        public static string LoadPackage(int templateErpId, int contractVersion)
        {
            return LoadJson(GetPackagePath(templateErpId, contractVersion), "audit-template package");
        }

        private static string GetTemplateDirectory(int templateErpId)
        {
            if (templateErpId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(templateErpId));
            }
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData,"ExcelApiPoc","Cache","Templates",templateErpId.ToString());
        }

        private static string SaveJson(string targetPath,string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("JSON content cannot be empty.",nameof(json));
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
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        private static string LoadJson(string path,string contentDescription)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"No cached {contentDescription} is available.",path);
            }
            string json =File.ReadAllText(path, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidDataException($"The cached {contentDescription} is empty.");
            }
            return json;
        }
    }
}