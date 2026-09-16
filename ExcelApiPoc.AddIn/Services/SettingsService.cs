using ExcelApiPoc.AddIn.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class SettingsService
    {
        private const string DefaultApiBaseUrl = "http://localhost:5080";
        internal const string DefaultUiLanguage = "en-US";
        internal const string SlovakUiLanguage = "sk-SK";

        public static string SettingsPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ExcelApiPoc",
                "settings.json");

        public static AddInSettings Load()
        {
            if (!File.Exists(SettingsPath))
                return CreateDefault();

            string json = File.ReadAllText(SettingsPath, Encoding.UTF8);
            AddInSettings settings =
                JsonConvert.DeserializeObject<AddInSettings>(json);

            if (settings == null ||
                string.IsNullOrWhiteSpace(settings.ApiBaseUrl))
            {
                return CreateDefault();
            }

            settings.ApiBaseUrl =
                NormalizeApiBaseUrl(settings.ApiBaseUrl);
            settings.UiLanguage =
                NormalizeUiLanguage(settings.UiLanguage);
            return settings;
        }

        public static void Save(AddInSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            settings.ApiBaseUrl =
                NormalizeApiBaseUrl(settings.ApiBaseUrl);
            settings.UiLanguage =
                NormalizeUiLanguage(settings.UiLanguage);

            string directory = Path.GetDirectoryName(SettingsPath);
            Directory.CreateDirectory(directory);

            string json =
                JsonConvert.SerializeObject(
                    settings,
                    Formatting.Indented);

            File.WriteAllText(
                SettingsPath,
                json,
                new UTF8Encoding(false));
        }

        public static Uri BuildApiUri(string relativePath)
        {
            AddInSettings settings = Load();
            string path =
                relativePath?.TrimStart('/') ?? string.Empty;

            return new Uri(
                $"{settings.ApiBaseUrl}/{path}",
                UriKind.Absolute);
        }

        public static string NormalizeUiLanguage(string value)
        {
            return string.Equals(
                value,
                SlovakUiLanguage,
                StringComparison.OrdinalIgnoreCase)
                ? SlovakUiLanguage
                : DefaultUiLanguage;
        }

        private static AddInSettings CreateDefault()
        {
            return new AddInSettings
            {
                ApiBaseUrl = DefaultApiBaseUrl,
                UiLanguage = DefaultUiLanguage
            };
        }

        private static string NormalizeApiBaseUrl(string value)
        {
            string normalized =
                (value ?? string.Empty).Trim().TrimEnd('/');

            if (!Uri.TryCreate(
                    normalized,
                    UriKind.Absolute,
                    out Uri uri) ||
                (uri.Scheme != Uri.UriSchemeHttp &&
                 uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new InvalidOperationException(
                    UiText.Get("Settings.InvalidUrl", DefaultUiLanguage));
            }

            return normalized;
        }
    }

    internal static class UiText
    {
        private static readonly IReadOnlyDictionary<string, string>
            English =
                new Dictionary<string, string>(
                    StringComparer.Ordinal)
                {
                    ["Common.Save"] = "Save",
                    ["Common.Cancel"] = "Cancel",
                    ["Common.Close"] = "Close",
                    ["Common.Copy"] = "Copy to Clipboard",
                    ["Common.Yes"] = "Yes",
                    ["Common.No"] = "No",
                    ["Settings.Title"] = "Excel API PoC Settings",
                    ["Settings.ApiBaseUrl"] = "API base URL:",
                    ["Settings.UiLanguage"] = "UI language:",
                    ["Settings.SettingsFile"] = "Settings file: {0}",
                    ["Settings.TestConnection"] = "Test connection",
                    ["Settings.ConnectionTitle"] = "API Connection",
                    ["Settings.ConnectionSuccessful"] = "Connection successful.",
                    ["Settings.ConnectionFailed"] = "The API connection test failed.",
                    ["Settings.InvalidTitle"] = "Invalid Settings",
                    ["Settings.InvalidUrl"] = "Enter a valid HTTP or HTTPS API address.",
                    ["Create.Title"] = "Create Audit Workbook",
                    ["Create.AccountingJournal"] = "Accounting journal: *",
                    ["Create.AccountingFramework"] = "Accounting framework:",
                    ["Create.GeneralLedger"] = "General ledger:",
                    ["Create.Optional"] = "Optional",
                    ["Create.TechnicalType"] = "Technical type:",
                    ["Create.AccountingFormat"] = "Accounting format:",
                    ["Create.Ico"] = "IČO:",
                    ["Create.FiscalYear"] = "Fiscal year:",
                    ["Create.Settings"] = "Settings...",
                    ["Create.Continue"] = "Continue",
                    ["Create.SelectJournal"] = "Select Accounting Journal",
                    ["Create.SelectFramework"] = "Select Accounting Framework",
                    ["Create.SelectGeneralLedger"] = "Select General Ledger",
                    ["Create.MultipleJournalFiles"] = "Multiple accounting-journal files can currently be selected only for Softip-MOP monthly journals.",
                    ["Create.InvalidJournal"] = "Select valid accounting journal files.",
                    ["Create.Import.JournalFormatNotRecognized"] = "The selected accounting journal is not recognized as {0} format. " +
                        "Select the correct Accounting format, or provide an accounting-journal " +
                        "export in a supported {0} format.",
                    ["Create.Validation.JournalRequired"] = "Select an accounting journal.",
                    ["Create.Validation.TechnicalTypeRequired"] = "Select the technical file type.",
                    ["Create.Validation.AccountingFormatRequired"] = "Select the accounting format.",
                    ["Create.Validation.IcoRequired"] = "Enter IČO.",
                    ["Create.Validation.FiscalYearRequired"] = "Enter fiscal year.",
                    ["Create.Validation.FiscalYearInvalid"] = "Enter a valid fiscal year.",
                    ["Create.Validation.GeneralLedgerRequired"] = "The accounting journal does not contain opening balances. Select the corresponding General Ledger so that opening balances can be supplied and closing balances validated.",
                    ["Create.MissingAccounts"] = "The selected accounts-list file does not exist.",
                    ["Create.MissingLedger"] = "The selected general-ledger file does not exist.",
                    ["Create.InvalidFiscalYear"] = "Enter a valid fiscal year.",
                    ["Create.Failed"] = "Accounting journal processing failed.",
                    ["Create.CalculationUnavailable"] = "The accounting data was imported successfully, but a calculation report could not be created.",
                    ["Create.CalculationUnavailableTitle"] = "Calculation Report Unavailable",
                    ["Create.PreflightTitle"] = "Accounting Journal Preflight",
                    ["Error.ShowDetails"] = "Show Details",
                    ["Error.HideDetails"] = "Hide Details",
                    ["Error.TechnicalDetails"] = "Technical details",
                    ["Error.Timestamp"] = "Timestamp",
                    ["Error.Operation"] = "Operation",
                    ["Error.Code"] = "Context / error code",
                    ["Error.Exception"] = "Exception",
                    ["Error.Message"] = "Message",
                    ["Error.InputFiles"] = "Input files",
                    ["Error.Name"] = "Name",
                    ["Error.Size"] = "Size",
                    ["Error.Sha256"] = "SHA-256"


                };

        private static readonly IReadOnlyDictionary<string, string>
            Slovak =
                new Dictionary<string, string>(
                    StringComparer.Ordinal)
                {
                    ["Common.Save"] = "Uložiť",
                    ["Common.Cancel"] = "Zrušiť",
                    ["Common.Close"] = "Zavrieť",
                    ["Common.Copy"] = "Kopírovať do schránky",
                    ["Common.Yes"] = "Áno",
                    ["Common.No"] = "Nie",
                    ["Settings.Title"] = "Nastavenia Excel API PoC",
                    ["Settings.ApiBaseUrl"] = "Základná URL API:",
                    ["Settings.UiLanguage"] = "Jazyk rozhrania:",
                    ["Settings.SettingsFile"] = "Súbor nastavení: {0}",
                    ["Settings.TestConnection"] = "Otestovať pripojenie",
                    ["Settings.ConnectionTitle"] = "Pripojenie k API",
                    ["Settings.ConnectionSuccessful"] = "Pripojenie bolo úspešné.",
                    ["Settings.ConnectionFailed"] = "Test pripojenia k API zlyhal.",
                    ["Settings.InvalidTitle"] = "Neplatné nastavenia",
                    ["Settings.InvalidUrl"] = "Zadajte platnú HTTP alebo HTTPS adresu API.",
                    ["Create.Title"] = "Vytvoriť audítorský zošit",
                    ["Create.AccountingJournal"] = "Účtovný denník: *",
                    ["Create.AccountingFramework"] = "Účtový rozvrh:",
                    ["Create.GeneralLedger"] = "Hlavná kniha:",
                    ["Create.Optional"] = "Voliteľné",
                    ["Create.TechnicalType"] = "Technický typ:",
                    ["Create.AccountingFormat"] = "Účtovný systém:",
                    ["Create.Ico"] = "IČO:",
                    ["Create.FiscalYear"] = "Účtovný rok:",
                    ["Create.Settings"] = "Nastavenia...",
                    ["Create.Continue"] = "Pokračovať",
                    ["Create.SelectJournal"] = "Vybrať účtovný denník",
                    ["Create.SelectFramework"] = "Vybrať účtový rozvrh",
                    ["Create.SelectGeneralLedger"] = "Vybrať hlavnú knihu",
                    ["Create.MultipleJournalFiles"] = "Viac súborov účtovného denníka možno zatiaľ vybrať iba pre mesačné denníky Softip-MOP.",
                    ["Create.Import.JournalFormatNotRecognized"] = "Vybraný účtovný denník nebol rozpoznaný ako formát {0}. " +
                        "Vyberte správny účtovný systém alebo použite export účtovného denníka " +
                        "v podporovanom formáte {0}.",
                    ["Create.InvalidJournal"] = "Vyberte platné súbory účtovného denníka.",
                    ["Create.Validation.JournalRequired"] = "Vyberte účtovný denník.",
                    ["Create.Validation.TechnicalTypeRequired"] = "Vyberte technický typ súboru.",
                    ["Create.Validation.AccountingFormatRequired"] = "Vyberte účtovný systém.",
                    ["Create.Validation.IcoRequired"] = "Zadajte IČO.",
                    ["Create.Validation.FiscalYearRequired"] = "Zadajte účtovný rok.",
                    ["Create.Validation.FiscalYearInvalid"] = "Zadajte platný účtovný rok.",
                    ["Create.Validation.GeneralLedgerRequired"] = "Účtovný denník neobsahuje počiatočné stavy. Vyberte príslušnú hlavnú knihu, aby bolo možné doplniť počiatočné stavy a overiť konečné stavy.",
                    ["Create.MissingAccounts"] = "Vybraný súbor účtového rozvrhu neexistuje.",
                    ["Create.MissingLedger"] = "Vybraný súbor hlavnej knihy neexistuje.",
                    ["Create.InvalidFiscalYear"] = "Zadajte platný účtovný rok.",
                    ["Create.Failed"] = "Spracovanie účtovného denníka zlyhalo.",
                    ["Create.CalculationUnavailable"] = "Účtovné údaje boli úspešne importované, ale výpočet výkazu nebolo možné vytvoriť.",
                    ["Create.CalculationUnavailableTitle"] = "Výpočet výkazu nie je dostupný",
                    ["Create.PreflightTitle"] = "Kontrola účtovného denníka",
                    ["Error.ShowDetails"] = "Zobraziť podrobnosti",
                    ["Error.HideDetails"] = "Skryť podrobnosti",
                    ["Error.TechnicalDetails"] = "Technické podrobnosti",
                    ["Error.Timestamp"] = "Čas",
                    ["Error.Operation"] = "Operácia",
                    ["Error.Code"] = "Kontext / kód chyby",
                    ["Error.Exception"] = "Výnimka",
                    ["Error.Message"] = "Správa",
                    ["Error.InputFiles"] = "Vstupné súbory",
                    ["Error.Name"] = "Názov",
                    ["Error.Size"] = "Veľkosť",
                    ["Error.Sha256"] = "SHA-256"

                };

        public static string Get(string key)
        {
            return Get(key, SettingsService.Load().UiLanguage);
        }

        public static string Get(string key, string language)
        {
            string normalized =
                SettingsService.NormalizeUiLanguage(language);

            if (string.Equals(
                    normalized,
                    SettingsService.SlovakUiLanguage,
                    StringComparison.Ordinal) &&
                Slovak.TryGetValue(key, out string slovak))
            {
                return slovak;
            }

            if (English.TryGetValue(key, out string english))
                return english;

            return key;
        }

        public static string Format(
            string key,
            string language,
            params object[] args)
        {
            return string.Format(
                Get(key, language),
                args ?? Array.Empty<object>());
        }
    }
}
