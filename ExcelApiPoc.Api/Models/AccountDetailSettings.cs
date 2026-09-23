using System.Text.Json;

namespace ExcelApiPoc.Api.Models;

public sealed record PredefinedTextDto(int TextId, string CategoryCode, string TextSk, int SortOrder);
public sealed record AccountTextDefaultDto(string Account, string CategoryCode, int TextId);
public sealed record TextCategoryDto(string Code, string DisplayNameSk, int SortOrder);
public sealed record AccountDetailTextSettings(
    IReadOnlyList<TextCategoryDto> Categories,
    IReadOnlyList<PredefinedTextDto> Texts,
    IReadOnlyList<AccountTextDefaultDto> Defaults);
public sealed record AccountDetailLayoutDto(int VersionNo, JsonElement Definition);
public sealed record CreatePredefinedTextRequest(string CategoryCode, string TextSk, int? SortOrder);
public sealed record UpdatePredefinedTextRequest(string TextSk, int SortOrder);
public sealed record SetAccountTextDefaultRequest(int TextId);
