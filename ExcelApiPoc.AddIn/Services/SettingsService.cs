using ExcelApiPoc.AddIn.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class SettingsService
    {
        private const string DefaultApiBaseUrl = "http://localhost:5080";

        public static string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"ExcelApiPoc","settings.json");

        public static AddInSettings Load()
        {
            if (!File.Exists(SettingsPath))
            {
                return CreateDefault();
            }

            string json = File.ReadAllText(SettingsPath, Encoding.UTF8);
            AddInSettings settings = JsonConvert.DeserializeObject<AddInSettings>(json);

            if (settings == null || string.IsNullOrWhiteSpace(settings.ApiBaseUrl))
            {
                return CreateDefault();
            }

            settings.ApiBaseUrl = NormalizeApiBaseUrl(settings.ApiBaseUrl);
            return settings;
        }

        public static void Save(AddInSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            settings.ApiBaseUrl = NormalizeApiBaseUrl(settings.ApiBaseUrl);
            string directory = Path.GetDirectoryName(SettingsPath);
            Directory.CreateDirectory(directory);
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsPath, json, new UTF8Encoding(false));
        }

        public static Uri BuildApiUri(string relativePath)
        {
            AddInSettings settings = Load();
            string path = relativePath?.TrimStart('/') ?? string.Empty;
            return new Uri( $"{settings.ApiBaseUrl}/{path}", UriKind.Absolute);
        }

        private static AddInSettings CreateDefault()
        {
            return new AddInSettings
            {
                ApiBaseUrl = DefaultApiBaseUrl
            };
        }

        private static string NormalizeApiBaseUrl(string value)
        {
            string normalized = (value ?? string.Empty).Trim().TrimEnd('/');
            if (!Uri.TryCreate(normalized,UriKind.Absolute,out Uri uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException( "The API URL must be a valid HTTP or HTTPS address.");
            }
            return normalized;
        }
    }
}