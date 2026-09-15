#!/usr/bin/env python3
"""Generate the static Template 699 data-update scripts from reviewed baseline rows."""

from __future__ import annotations

import re
from collections import Counter
from pathlib import Path


ROOT = Path(__file__).resolve().parents[4]
BASELINE = ROOT / "Database/AuditAddIn/Baseline/V1_0"
OUTPUT = Path(__file__).with_name("010_PopulateTemplate699Calculation.sql")

ROW_RE = re.compile(
    r"(?:SELECT\s+6990[123]\s+AS\s+\[TableErpId\]|(?:UNION ALL )?SELECT\s+(6990[123]))"
    r"(?:,|\s+AS.*?,)\s*(\d+)\s+(?:AS\s+\[RowNumber\],|,)\s*"
    r"N?'(?:[^']|'')*'\s+(?:AS\s+\[Designation\],|,)\s*N'((?:[^']|'')*)'.*?,\s*([01])\s+(?:AS\s+\[IsSumRow\],|,)",
)


def template_rows() -> dict[tuple[int, int], tuple[str, bool]]:
    text = (BASELINE / "051_PopulateTemplateRows.sql").read_text(encoding="utf-8-sig")
    rows: dict[tuple[int, int], tuple[str, bool]] = {}
    current_table = None
    for line in text.splitlines():
        if "SELECT 6990" not in line:
            continue
        table_match = re.search(r"SELECT\s+(6990[123])", line)
        if table_match:
            current_table = int(table_match.group(1))
        # All 699 rows use this stable positional shape; SQL strings escape apostrophes twice.
        fields = re.findall(r"N?'((?:[^']|'')*)'|NULL|-?\d+", line)
        numbers = re.findall(r"(?<![\w'])\d+(?![\w'])", line)
        if current_table is None or len(numbers) < 3:
            continue
        row_number = int(numbers[1])
        quoted = re.findall(r"N'((?:[^']|'')*)'", line)
        if len(quoted) < 2:
            continue
        text_sk = quoted[1].replace("''", "'")
        # IsSumRow is the first 0/1 after the English text. Its stable position is
        # easier to recover from the tail than from commas inside captions.
        tail = line.split("N'", 3)[-1]
        is_sum = bool(re.search(r"',\s*1\s*,", tail))
        rows[(current_table, row_number)] = (text_sk, is_sum)
    return rows


def profit_accounts() -> set[str]:
    text = (BASELINE / "020_PopulateOfficialAccounts.sql").read_text(encoding="utf-8-sig")
    return set(re.findall(r"N'PROFIT'.*?N'2022-01-01'.*?N'(\d{3})'", text))


def expand_token(token: str, accounts: set[str]) -> set[str]:
    token = token.upper().rstrip("AÚ")
    if re.fullmatch(r"\d{3}", token):
        return {token} if token in accounts else set()
    # XX placeholders denote entity-specific synthetic accounts outside the
    # reviewed statutory chart. They must not expand to every official account
    # in that class; such expansion would create false destinations.
    if re.fullmatch(r"\d{2}X", token):
        return set()
    return set()


def codes(fragment: str, accounts: set[str]) -> set[str]:
    found: set[str] = set()
    for token in re.findall(r"(?<!\d)(?:\d{3}|\d{2}X)(?:A|AÚ)?(?!\d)", fragment.upper()):
        found.update(expand_token(token, accounts))
    for group in re.findall(r"účtová skupina\s+(\d{2})", fragment, flags=re.I):
        found.update(code for code in accounts if code.startswith(group))
    return found


def mappings(rows: dict[tuple[int, int], tuple[str, bool]], accounts: set[str]):
    result = []
    for (table, row), (caption, is_sum) in sorted(rows.items()):
        if is_sum:
            continue
        if table == 69901:
            split = re.split(r"\s*-\s*/", caption, maxsplit=1)
            primary = codes(split[0], accounts)
            secondary = codes(split[1], accounts) if len(split) == 2 else set()
            if row == 20:
                primary = codes(caption, accounts)
            for code in sorted(primary):
                result.append([table, row, code, 1, 0, "Assets", "ClosingDebit"])
            for code in sorted(secondary):
                result.append([table, row, code, 0, 1, "Liabilities", "ClosingCredit"])
        elif table == 69902:
            for code in sorted(codes(caption, accounts)):
                result.append([table, row, code, 0, 1, "Liabilities", "ClosingNetto"])
        else:
            for code in sorted(codes(caption, accounts)):
                result.append([
                    table, row, code,
                    1, 0, "Assets",
                    "ClosingNetto" if code_side(row) == "expense" else "ClosingCreditNetto",
                ])

    occurrences = Counter((table, code) for table, _, code, *_ in result)
    for item in result:
        item.insert(3, 1 if occurrences[(item[0], item[2])] > 1 else 0)
    return result


def code_side(row: int) -> str:
    return "expense" if row in {11, 12, 13, 14, 16, 17, 18, 19, 20, 22, 23, 24, 25, 26, 46, 47, 48, 50, 51, 52, 53, 54, 58, 59, 60} else "income"


FORMULAS = {
    69901: {1:[2,33,74],2:[3,11,21],3:list(range(4,11)),11:list(range(12,21)),21:list(range(22,33)),33:[34,41,53,66,71],34:list(range(35,41)),41:[42,*range(46,53)],42:list(range(43,46)),53:[54,*range(58,66)],54:list(range(55,58)),66:list(range(67,71)),71:[72,73],74:list(range(75,79))},
    69902: {79:[80,101,141],80:[81,85,86,87,90,93,97,100],81:list(range(82,85)),87:[88,89],90:[91,92],93:list(range(94,97)),97:[98,99],101:[102,118,121,122,136,139,140],102:[103,*range(107,118)],103:list(range(104,107)),118:[119,120],122:[123,*range(127,136)],123:list(range(124,127)),136:[137,138],141:list(range(142,146))},
    69903: {1:[3,4,5],2:list(range(3,10)),10:[11,12,13,14,15,20,21,24,25,26],15:list(range(16,20)),21:[22,23],27:[2,-10],28:[3,4,5,6,7,-11,-12,-13,-14],29:[30,31,35,39,42,43,44],31:list(range(32,35)),35:list(range(36,39)),39:[40,41],45:[46,47,48,49,52,53,54],49:[50,51],55:[29,-45],56:[27,55],57:[58,59],61:[56,-57,-60]},
}


def sql() -> str:
    rows = template_rows()
    accounts = profit_accounts()
    mapping_rows = mappings(rows, accounts)
    used = sorted({m[2] for m in mapping_rows})
    lines = ["/* Template 699 calculation package for the PROFIT framework. */", "USE [AuditAddIn];", "GO", "SET NOCOUNT ON;", "SET XACT_ABORT ON;", "GO", "BEGIN TRANSACTION;", ""]
    lines += ["DECLARE @FrameworkVersionId int = (SELECT afv.[Id] FROM [Accounts].[AccountFrameworkVersion] afv INNER JOIN [Accounts].[AccountFramework] af ON af.[Id]=afv.[AccountFrameworkId] WHERE af.[Code]=N'PROFIT' AND afv.[VersionCode]=N'2022-01-01');", "IF @FrameworkVersionId IS NULL THROW 52300, 'PROFIT 2022-01-01 framework version is missing.', 1;", ""]
    lines += ["IF NOT EXISTS (SELECT 1 FROM [Accounts].[CalculationConfigurationVersion] WHERE [AccountFrameworkVersionId]=@FrameworkVersionId AND [Code]=N'PROFIT-2022-01')", "    INSERT INTO [Accounts].[CalculationConfigurationVersion] ([AccountFrameworkVersionId],[AccountingModelCode],[Code],[Description],[ValidFrom],[ValidTo]) VALUES (@FrameworkVersionId,N'PROFIT',N'PROFIT-2022-01',N'Template 699 balance sheet and income statement',CONVERT(date,'2022-01-01'),NULL);", "DECLARE @ConfigurationId int = (SELECT [Id] FROM [Accounts].[CalculationConfigurationVersion] WHERE [AccountFrameworkVersionId]=@FrameworkVersionId AND [Code]=N'PROFIT-2022-01');", ""]
    values = ",\n    ".join(f"(N'{c}',N'{('ClosingNetto' if c[0]=='5' else 'ClosingCreditNetto' if c[0]=='6' else 'ClosingDebit')}',N'{('ClosingDebit' if c[0]=='5' else 'ClosingNetto')}')" for c in used)
    lines += ["IF NOT EXISTS (SELECT 1 FROM [Accounts].[ValueSource] WHERE [Code]=N'ClosingCreditNetto') INSERT INTO [Accounts].[ValueSource] ([Code],[Description]) VALUES (N'ClosingCreditNetto',N'Net closing credit balance (credit minus debit)');", "", "DECLARE @Rules TABLE ([AccountCode] nvarchar(10) NOT NULL PRIMARY KEY,[AssetsSource] nvarchar(50),[LiabilitiesSource] nvarchar(50));", "INSERT INTO @Rules VALUES", "    " + values + ";", "INSERT INTO [Accounts].[AccountCalculationRules] ([CalculationConfigurationVersionId],[AccountId],[AssetsValueSourceCode],[LiabilitiesValueSourceCode])", "SELECT @ConfigurationId,a.[Id],r.[AssetsSource],r.[LiabilitiesSource] FROM @Rules r INNER JOIN [Accounts].[Accounts] a ON a.[AccountFrameworkVersionId]=@FrameworkVersionId AND a.[AccountCode]=r.[AccountCode] LEFT JOIN [Accounts].[AccountCalculationRules] e ON e.[CalculationConfigurationVersionId]=@ConfigurationId AND e.[AccountId]=a.[Id] WHERE e.[Id] IS NULL;", ""]
    lines += ["DECLARE @TemplateId int=(SELECT [Id] FROM [Template].[Templates] WHERE [ErpId]=699);", "IF @TemplateId IS NULL THROW 52301, 'Template 699 is missing.', 1;", "IF NOT EXISTS (SELECT 1 FROM [Accounts].[TemplateFrameworkVersion] WHERE [TemplateId]=@TemplateId AND [CalculationConfigurationVersionId]=@ConfigurationId)", "    INSERT INTO [Accounts].[TemplateFrameworkVersion] ([TemplateId],[AccountFrameworkVersionId],[CalculationConfigurationVersionId]) VALUES (@TemplateId,@FrameworkVersionId,@ConfigurationId);", "DECLARE @TemplateFrameworkVersionId int=(SELECT [Id] FROM [Accounts].[TemplateFrameworkVersion] WHERE [TemplateId]=@TemplateId AND [CalculationConfigurationVersionId]=@ConfigurationId);", ""]
    vals=[]
    for table,row,code,analytic,brutto,corr,side,source in mapping_rows:
        vals.append(f"({table},{row},N'{code}',{analytic},{brutto},{corr},'{side}',N'{source}')")
    lines += ["DECLARE @Mappings TABLE ([TableErpId] int NOT NULL,[RowNumber] int NOT NULL,[AccountCode] nvarchar(10) NOT NULL,[Analytical] bit,[Brutto] bit,[Correction] bit,[Side] varchar(20),[Source] nvarchar(50),PRIMARY KEY ([TableErpId],[RowNumber],[AccountCode]));", "INSERT INTO @Mappings VALUES", "    " + ",\n    ".join(vals) + ";", "INSERT INTO [Accounts].[ReportAccountMappings] ([TemplateFrameworkVersionId],[TemplateRowId],[AccountCalculationRuleId],[RequiresAnalyticalMapping],[IncludeInBrutto],[IncludeInCorrection],[Side],[ValueSourceCode])", "SELECT @TemplateFrameworkVersionId,tr.[Id],acr.[Id],m.[Analytical],m.[Brutto],m.[Correction],m.[Side],m.[Source] FROM @Mappings m INNER JOIN [Template].[Tables] tt ON tt.[TemplateId]=@TemplateId AND tt.[TableErpId]=m.[TableErpId] INNER JOIN [Template].[Rows] tr ON tr.[TableId]=tt.[Id] AND tr.[RowNumber]=m.[RowNumber] INNER JOIN [Accounts].[Accounts] a ON a.[AccountFrameworkVersionId]=@FrameworkVersionId AND a.[AccountCode]=m.[AccountCode] INNER JOIN [Accounts].[AccountCalculationRules] acr ON acr.[CalculationConfigurationVersionId]=@ConfigurationId AND acr.[AccountId]=a.[Id] LEFT JOIN [Accounts].[ReportAccountMappings] e ON e.[TemplateFrameworkVersionId]=@TemplateFrameworkVersionId AND e.[TemplateRowId]=tr.[Id] AND e.[AccountCalculationRuleId]=acr.[Id] WHERE e.[Id] IS NULL;", ""]
    terms=[]
    for table, targets in FORMULAS.items():
        for target, sources in targets.items():
            for signed in sources:
                source=abs(signed); coeff=-1 if signed<0 else 1
                terms.append(f"({table},{target},{table},{source},{coeff})")
    # Statutory after-tax result is also the balance-sheet current-year result.
    terms.append("(69902,100,69903,61,1)")
    lines += ["DECLARE @Terms TABLE ([TargetTable] int NOT NULL,[TargetRow] int NOT NULL,[SourceTable] int NOT NULL,[SourceRow] int NOT NULL,[Coefficient] int,PRIMARY KEY ([TargetTable],[TargetRow],[SourceTable],[SourceRow]));", "INSERT INTO @Terms VALUES", "    " + ",\n    ".join(terms) + ";", "INSERT INTO [Template].[RowCalculationTerms] ([TargetRowId],[SourceRowId],[Coefficient]) SELECT targetRow.[Id],sourceRow.[Id],n.[Coefficient] FROM @Terms n INNER JOIN [Template].[Tables] targetTable ON targetTable.[TemplateId]=@TemplateId AND targetTable.[TableErpId]=n.[TargetTable] INNER JOIN [Template].[Rows] targetRow ON targetRow.[TableId]=targetTable.[Id] AND targetRow.[RowNumber]=n.[TargetRow] INNER JOIN [Template].[Tables] sourceTable ON sourceTable.[TemplateId]=@TemplateId AND sourceTable.[TableErpId]=n.[SourceTable] INNER JOIN [Template].[Rows] sourceRow ON sourceRow.[TableId]=sourceTable.[Id] AND sourceRow.[RowNumber]=n.[SourceRow] LEFT JOIN [Template].[RowCalculationTerms] e ON e.[TargetRowId]=targetRow.[Id] AND e.[SourceRowId]=sourceRow.[Id] WHERE e.[TargetRowId] IS NULL;", "", f"IF (SELECT COUNT_BIG(*) FROM [Accounts].[AccountCalculationRules] WHERE [CalculationConfigurationVersionId]=@ConfigurationId)<>{len(used)} THROW 52303, 'Template 699 account-rule count is invalid.', 1;", f"IF (SELECT COUNT_BIG(*) FROM [Accounts].[ReportAccountMappings] WHERE [TemplateFrameworkVersionId]=@TemplateFrameworkVersionId)<>{len(mapping_rows)} THROW 52304, 'Template 699 report-mapping count is invalid.', 1;", f"IF (SELECT COUNT_BIG(*) FROM @Terms)<>{len(terms)} THROW 52305, 'Template 699 direct-formula count is invalid.', 1;", "IF EXISTS (SELECT 1 FROM @Mappings m LEFT JOIN [Accounts].[ReportAccountMappingDetails] d ON d.[TemplateErpId]=699 AND d.[TableErpId]=m.[TableErpId] AND d.[ReportRowNumber]=m.[RowNumber] AND d.[AccountCode]=m.[AccountCode] WHERE d.[AccountCode] IS NULL) THROW 52306, 'A Template 699 mapping could not be resolved.', 1;", "IF NOT EXISTS (SELECT 1 FROM [Template].[GetCalculationPlan](699)) THROW 52307, 'Template 699 calculation plan is empty.', 1;", "", "UPDATE ct SET [CalculationImplemented]=1 FROM [Accounts].[CalculationTemplate] ct WHERE ct.[TemplateId]=@TemplateId AND ct.[AccountFrameworkId]=(SELECT [AccountFrameworkId] FROM [Accounts].[AccountFrameworkVersion] WHERE [Id]=@FrameworkVersionId);", "IF @@ROWCOUNT<>1 THROW 52302, 'Template 699 calculation readiness row is missing.', 1;", "COMMIT TRANSACTION;", "PRINT 'Template 699 calculation population completed.';", "GO", ""]
    return "\n".join(lines)


if __name__ == "__main__":
    OUTPUT.write_text(sql(), encoding="utf-8-sig", newline="\n")
    print(OUTPUT)
