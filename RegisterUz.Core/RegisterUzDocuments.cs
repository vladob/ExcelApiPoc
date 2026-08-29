using System.Text.Json.Serialization;

namespace RegisterUz.Core;

public sealed record RegisterUzDocument<T>(
    T Value,
    string RawJson,
    DateTime RetrievedAtUtc,
    int HttpStatusCode,
    string? ApiVersion);

public sealed record RegisterUzEntityPackage(
    RegisterUzDocument<AccountingEntityDto> Entity,
    IReadOnlyList<RegisterUzDocument<FinancialStatementDto>> FinancialStatements,
    IReadOnlyList<RegisterUzDocument<AnnualReportDto>> AnnualReports,
    IReadOnlyList<RegisterUzDocument<FinancialReportDto>> FinancialReports,
    IReadOnlyList<RegisterUzDocument<FinancialReportTemplateDto>> Templates);

public sealed record RegisterUzLoadResult(
    string Ico,
    long RegisterUzEntityId,
    int FinancialStatementCount,
    int AnnualReportCount,
    int FinancialReportCount,
    int TemplateCount,
    long SyncRunId,
    DateTime CompletedAtUtc);

public sealed class IdListDto
{
    [JsonPropertyName("id")]
    public long[] Ids { get; init; } = [];

    [JsonPropertyName("existujeDalsieId")]
    public bool HasMoreIds { get; init; }
}

public sealed class LocalizedTextDto
{
    [JsonPropertyName("sk")]
    public string? Sk { get; init; }

    [JsonPropertyName("en")]
    public string? En { get; init; }
}

public sealed class AccountingEntityDto
{
    [JsonPropertyName("id")] public long Id { get; init; }
    [JsonPropertyName("idUctovnychZavierok")] public long[] FinancialStatementIds { get; init; } = [];
    [JsonPropertyName("idVyrocnychSprav")] public long[] AnnualReportIds { get; init; } = [];
    [JsonPropertyName("ico")] public string? Ico { get; init; }
    [JsonPropertyName("dic")] public string? Dic { get; init; }
    [JsonPropertyName("sid")] public string? Sid { get; init; }
    [JsonPropertyName("nazovUJ")] public string? Name { get; init; }
    [JsonPropertyName("mesto")] public string? City { get; init; }
    [JsonPropertyName("ulica")] public string? Street { get; init; }
    [JsonPropertyName("psc")] public string? PostalCode { get; init; }
    [JsonPropertyName("datumZalozenia")] public DateOnly? EstablishedDate { get; init; }
    [JsonPropertyName("datumZrusenia")] public DateOnly? CancellationDate { get; init; }
    [JsonPropertyName("pravnaForma")] public string? LegalFormCode { get; init; }
    [JsonPropertyName("skNace")] public string? SkNaceCode { get; init; }
    [JsonPropertyName("velkostOrganizacie")] public string? OrganizationSizeCode { get; init; }
    [JsonPropertyName("druhVlastnictva")] public string? OwnershipTypeCode { get; init; }
    [JsonPropertyName("kraj")] public string? RegionCode { get; init; }
    [JsonPropertyName("okres")] public string? DistrictCode { get; init; }
    [JsonPropertyName("sidlo")] public string? RegisteredOfficeCode { get; init; }
    [JsonPropertyName("konsolidovana")] public bool? IsConsolidated { get; init; }
    [JsonPropertyName("zdrojDat")] public string? DataSourceCode { get; init; }
    [JsonPropertyName("datumPoslednejUpravy")] public DateOnly? LastModifiedDate { get; init; }
    [JsonPropertyName("stav")] public string? Status { get; init; }
}

public sealed class FinancialStatementDto
{
    [JsonPropertyName("id")] public long Id { get; init; }
    [JsonPropertyName("idUJ")] public long EntityId { get; init; }
    [JsonPropertyName("idUctovnychVykazov")] public long[] FinancialReportIds { get; init; } = [];
    [JsonPropertyName("obdobieOd")] public string? PeriodFrom { get; init; }
    [JsonPropertyName("obdobieDo")] public string? PeriodTo { get; init; }
    [JsonPropertyName("datumPodania")] public DateOnly? SubmissionDate { get; init; }
    [JsonPropertyName("datumZostavenia")] public DateOnly? PreparationDate { get; init; }
    [JsonPropertyName("datumSchvalenia")] public DateOnly? ApprovalDate { get; init; }
    [JsonPropertyName("datumZostaveniaK")] public DateOnly? AssemblyDate { get; init; }
    [JsonPropertyName("datumPrilozeniaSpravyAuditora")] public DateOnly? AuditorReportAttachmentDate { get; init; }
    [JsonPropertyName("nazovFondu")] public string? FundName { get; init; }
    [JsonPropertyName("leiKod")] public string? LeiCode { get; init; }
    [JsonPropertyName("konsolidovana")] public bool? IsConsolidated { get; init; }
    [JsonPropertyName("konsolidovanaZavierkaUstrednejStatnejSpravy")] public bool? IsConsolidatedCentralGovernment { get; init; }
    [JsonPropertyName("suhrnnaUctovnaZavierkaVerejnejSpravy")] public bool? IsSummaryPublicAdministration { get; init; }
    [JsonPropertyName("typ")] public string? Type { get; init; }
    [JsonPropertyName("zdrojDat")] public string? DataSourceCode { get; init; }
    [JsonPropertyName("datumPoslednejUpravy")] public DateOnly? LastModifiedDate { get; init; }
    [JsonPropertyName("stav")] public string? Status { get; init; }
}

public sealed class AnnualReportDto
{
    [JsonPropertyName("id")] public long Id { get; init; }
    [JsonPropertyName("idUJ")] public long EntityId { get; init; }
    [JsonPropertyName("idUctovnychVykazov")] public long[] FinancialReportIds { get; init; } = [];
    [JsonPropertyName("nazovUJ")] public string? EntityNameAtSubmission { get; init; }
    [JsonPropertyName("typ")] public string? Type { get; init; }
    [JsonPropertyName("nazovFondu")] public string? FundName { get; init; }
    [JsonPropertyName("leiKod")] public string? LeiCode { get; init; }
    [JsonPropertyName("obdobieOd")] public string? PeriodFrom { get; init; }
    [JsonPropertyName("obdobieDo")] public string? PeriodTo { get; init; }
    [JsonPropertyName("datumPodania")] public DateOnly? SubmissionDate { get; init; }
    [JsonPropertyName("datumZostaveniaK")] public DateOnly? AssemblyDate { get; init; }
    [JsonPropertyName("pristupnostDat")] public string? DataAvailability { get; init; }
    [JsonPropertyName("prilohy")] public AttachmentDto[] Attachments { get; init; } = [];
    [JsonPropertyName("zdrojDat")] public string? DataSourceCode { get; init; }
    [JsonPropertyName("datumPoslednejUpravy")] public DateOnly? LastModifiedDate { get; init; }
    [JsonPropertyName("stav")] public string? Status { get; init; }
}

public sealed class FinancialReportDto
{
    [JsonPropertyName("id")] public long Id { get; init; }
    [JsonPropertyName("idUctovnejZavierky")] public long? FinancialStatementId { get; init; }
    [JsonPropertyName("idVyrocnejSpravy")] public long? AnnualReportId { get; init; }
    [JsonPropertyName("idSablony")] public long? TemplateId { get; init; }
    [JsonPropertyName("mena")] public string? CurrencyCode { get; init; }
    [JsonPropertyName("kodDanovehoUradu")] public string? TaxOfficeCode { get; init; }
    [JsonPropertyName("pristupnostDat")] public string? DataAvailability { get; init; }
    [JsonPropertyName("prilohy")] public AttachmentDto[] Attachments { get; init; } = [];
    [JsonPropertyName("obsah")] public FinancialReportContentDto? Content { get; init; }
    [JsonPropertyName("zdrojDat")] public string? DataSourceCode { get; init; }
    [JsonPropertyName("datumPoslednejUpravy")] public DateOnly? LastModifiedDate { get; init; }
    [JsonPropertyName("stav")] public string? Status { get; init; }
}

public sealed class AttachmentDto
{
    [JsonPropertyName("id")] public long Id { get; init; }
    [JsonPropertyName("meno")] public string? FileName { get; init; }
    [JsonPropertyName("mimeType")] public string? MimeType { get; init; }
    [JsonPropertyName("velkostPrilohy")] public long? FileSizeBytes { get; init; }
    [JsonPropertyName("pocetStran")] public int? PageCount { get; init; }
    [JsonPropertyName("digest")] public string? DigestSha256 { get; init; }
    [JsonPropertyName("jazyk")] public string? LanguageCode { get; init; }
}

public sealed class FinancialReportContentDto
{
    [JsonPropertyName("titulnaStrana")] public TitlePageDto? TitlePage { get; init; }
    [JsonPropertyName("tabulky")] public FinancialReportTableDto[] Tables { get; init; } = [];
}

public sealed class AddressDto
{
    [JsonPropertyName("ulica")] public string? Street { get; init; }
    [JsonPropertyName("cislo")] public string? Number { get; init; }
    [JsonPropertyName("psc")] public string? PostalCode { get; init; }
    [JsonPropertyName("mesto")] public string? City { get; init; }

    public override string ToString() =>
        string.Join(", ", new[] { $"{Street} {Number}".Trim(), $"{PostalCode} {City}".Trim() }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
}

public sealed class TitlePageDto
{
    [JsonPropertyName("nazovUctovnejJednotky")] public string? EntityName { get; init; }
    [JsonPropertyName("ico")] public string? Ico { get; init; }
    [JsonPropertyName("dic")] public string? Dic { get; init; }
    [JsonPropertyName("sid")] public string? Sid { get; init; }
    [JsonPropertyName("adresa")] public AddressDto? Address { get; init; }
    [JsonPropertyName("pravnaForma")] public string? LegalFormCode { get; init; }
    [JsonPropertyName("skNace")] public string? SkNaceCode { get; init; }
    [JsonPropertyName("typZavierky")] public string? ReportType { get; init; }
    [JsonPropertyName("konsolidovana")] public bool? IsConsolidated { get; init; }
    [JsonPropertyName("konsolidovanaZavierkaUstrednejStatnejSpravy")] public bool? IsConsolidatedCentralGovernment { get; init; }
    [JsonPropertyName("suhrnnaUctovnaZavierkaVerejnejSpravy")] public bool? IsSummaryPublicAdministration { get; init; }
    [JsonPropertyName("typUctovnejJednotky")] public string? EntityType { get; init; }
    [JsonPropertyName("oznacenieObchodnehoRegistra")] public string? CommercialRegister { get; init; }
    [JsonPropertyName("nazovSpravcovskehoFondu")] public string? FundName { get; init; }
    [JsonPropertyName("leiKod")] public string? LeiCode { get; init; }
    [JsonPropertyName("obdobieOd")] public string? PeriodFrom { get; init; }
    [JsonPropertyName("obdobieDo")] public string? PeriodTo { get; init; }
    [JsonPropertyName("predchadzajuceObdobieOd")] public string? PreviousPeriodFrom { get; init; }
    [JsonPropertyName("predchadzajuceObdobieDo")] public string? PreviousPeriodTo { get; init; }
    [JsonPropertyName("datumVyplnenia")] public DateOnly? CompletionDate { get; init; }
    [JsonPropertyName("datumSchvalenia")] public DateOnly? ApprovalDate { get; init; }
    [JsonPropertyName("datumZostavenia")] public DateOnly? PreparationDate { get; init; }
    [JsonPropertyName("datumZostaveniaK")] public DateOnly? AssemblyDate { get; init; }
    [JsonPropertyName("datumPrilozeniaSpravyAuditora")] public DateOnly? AuditorReportAttachmentDate { get; init; }
}

public sealed class FinancialReportTableDto
{
    [JsonPropertyName("nazov")] public LocalizedTextDto? Name { get; init; }
    [JsonPropertyName("data")] public string?[] Data { get; init; } = [];
}

public sealed class FinancialReportTemplateDto
{
    [JsonPropertyName("id")] public long Id { get; init; }
    [JsonPropertyName("nazov")] public string? Name { get; init; }
    [JsonPropertyName("nariadenieMF")] public string? MinistrySpecification { get; init; }
    [JsonPropertyName("platneOd")] public DateOnly? ValidFrom { get; init; }
    [JsonPropertyName("platneDo")] public DateOnly? ValidTo { get; init; }
    [JsonPropertyName("tabulky")] public TemplateTableDto[] Tables { get; init; } = [];
}

public sealed class TemplateTableDto
{
    [JsonPropertyName("nazov")] public LocalizedTextDto? Name { get; init; }
    [JsonPropertyName("hlavicka")] public TemplateHeaderDto[] Headers { get; init; } = [];
    [JsonPropertyName("riadky")] public TemplateRowDto[] Rows { get; init; } = [];
}

public sealed class TemplateHeaderDto
{
    [JsonPropertyName("text")] public LocalizedTextDto? Text { get; init; }
    [JsonPropertyName("riadok")] public int? RowPosition { get; init; }
    [JsonPropertyName("stlpec")] public int? ColumnPosition { get; init; }
    [JsonPropertyName("sirkaStlpca")] public int? ColumnSpan { get; init; }
    [JsonPropertyName("vyskaRiadku")] public int? RowSpan { get; init; }
}

public sealed class TemplateRowDto
{
    [JsonPropertyName("oznacenie")] public string? Designation { get; init; }
    [JsonPropertyName("cisloRiadku")] public int? RowNumber { get; init; }
    [JsonPropertyName("text")] public LocalizedTextDto? Text { get; init; }
}
