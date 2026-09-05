/*
    Creates the normalized AuditAddIn database and grants read-only access to
    the ExcelApiPoc.Api IIS application-pool identity.

    Execute once on SRVHPV after the previous AuditAddIn databases have been
    backed up, verified and removed. Then run population and validation scripts
    010 through 090 in sequence.
*/

USE [master];
GO

IF DB_ID(N'AuditAddIn') IS NOT NULL
    THROW 52000, 'Database AuditAddIn already exists.', 1;
GO

CREATE DATABASE [AuditAddIn];
GO

USE [AuditAddIn];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* Major domains retained from the existing database. */
CREATE SCHEMA [Accounts];
GO
CREATE SCHEMA [Template];
GO

/* Accounting-framework identity, official effective versions and provenance. */
CREATE TABLE [Accounts].[AccountFramework]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [Code] nvarchar(20) NOT NULL,
    [Name_sk] nvarchar(500) NOT NULL,
    [CreatedAtUtc] datetime2(0) NOT NULL CONSTRAINT [DF_Accounts_AccountFramework_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Accounts_AccountFramework] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_Accounts_AccountFramework_Code] UNIQUE ([Code]),
    CONSTRAINT [CK_Accounts_AccountFramework_Code] CHECK (LEN(LTRIM(RTRIM([Code]))) > 0 AND [Code] = UPPER(LTRIM(RTRIM([Code]))))
);
GO

CREATE TABLE [Accounts].[AccountFrameworkVersion]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [AccountFrameworkId] int NOT NULL,
    [VersionCode] nvarchar(50) NOT NULL,
    [ValidFrom] date NOT NULL,
    [ValidTo] date NULL, -- NULL identifies the open/current version.
    [LegalReference] nvarchar(500) NULL,
    [SourceUrl] nvarchar(1000) NULL,
    [SourceSha256] char(64) NULL,
    [CreatedAtUtc] datetime2(0) NOT NULL CONSTRAINT [DF_Accounts_AccountFrameworkVersion_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Accounts_AccountFrameworkVersion] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Accounts_AccountFrameworkVersion_Framework] FOREIGN KEY ([AccountFrameworkId]) REFERENCES [Accounts].[AccountFramework] ([Id]),
    CONSTRAINT [UQ_Accounts_AccountFrameworkVersion_Code] UNIQUE ([AccountFrameworkId], [VersionCode]),
    CONSTRAINT [UQ_Accounts_AccountFrameworkVersion_Start] UNIQUE ([AccountFrameworkId], [ValidFrom]),
    CONSTRAINT [CK_Accounts_AccountFrameworkVersion_Dates] CHECK ([ValidTo] IS NULL OR [ValidTo] >= [ValidFrom]),
    CONSTRAINT [CK_Accounts_AccountFrameworkVersion_Hash] CHECK ([SourceSha256] IS NULL OR [SourceSha256] NOT LIKE '%[^0-9A-F]%')
);
GO

CREATE UNIQUE INDEX [UX_Accounts_AccountFrameworkVersion_Current]
    ON [Accounts].[AccountFrameworkVersion] ([AccountFrameworkId]) WHERE [ValidTo] IS NULL;
GO

/* Imported official reference data. Production code does not query this table. */
CREATE TABLE [Accounts].[OfficialAccounts]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [AccountFrameworkVersionId] int NOT NULL,
    [AccountCode] nvarchar(10) NOT NULL,
    [AccountName_sk] nvarchar(500) NOT NULL,
    [AccountLevel] tinyint NOT NULL,
    [ParentAccountCode] nvarchar(10) NULL, -- Import key; a parent need not be present in an incomplete source.
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_Accounts_OfficialAccounts] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Accounts_OfficialAccounts_Version] FOREIGN KEY ([AccountFrameworkVersionId]) REFERENCES [Accounts].[AccountFrameworkVersion] ([Id]),
    CONSTRAINT [UQ_Accounts_OfficialAccounts_Code] UNIQUE ([AccountFrameworkVersionId], [AccountCode]),
    CONSTRAINT [UQ_Accounts_OfficialAccounts_SortOrder] UNIQUE ([AccountFrameworkVersionId], [SortOrder]),
    CONSTRAINT [CK_Accounts_OfficialAccounts_Level] CHECK ([AccountLevel] BETWEEN 1 AND 3 AND [AccountLevel] = LEN([AccountCode])),
    CONSTRAINT [CK_Accounts_OfficialAccounts_Parent] CHECK ([ParentAccountCode] IS NULL OR LEN([ParentAccountCode]) < LEN([AccountCode]))
);
GO

/* Reviewed production snapshot for each whole framework version. */
CREATE TABLE [Accounts].[Accounts]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [AccountFrameworkVersionId] int NOT NULL,
    [AccountCode] nvarchar(10) NOT NULL,
    [AccountName_sk] nvarchar(500) NOT NULL,
    [AccountLevel] tinyint NOT NULL,
    [ParentAccountId] int NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_Accounts_Accounts] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Accounts_Accounts_Version] FOREIGN KEY ([AccountFrameworkVersionId]) REFERENCES [Accounts].[AccountFrameworkVersion] ([Id]),
    CONSTRAINT [UQ_Accounts_Accounts_Code] UNIQUE ([AccountFrameworkVersionId], [AccountCode]),
    CONSTRAINT [UQ_Accounts_Accounts_SortOrder] UNIQUE ([AccountFrameworkVersionId], [SortOrder]),
    CONSTRAINT [UQ_Accounts_Accounts_VersionId] UNIQUE ([AccountFrameworkVersionId], [Id]),
    CONSTRAINT [FK_Accounts_Accounts_Parent] FOREIGN KEY ([AccountFrameworkVersionId], [ParentAccountId]) REFERENCES [Accounts].[Accounts] ([AccountFrameworkVersionId], [Id]),
    CONSTRAINT [CK_Accounts_Accounts_Level] CHECK ([AccountLevel] BETWEEN 1 AND 3 AND [AccountLevel] = LEN([AccountCode]))
);
GO

CREATE INDEX [IX_Accounts_Accounts_Parent] ON [Accounts].[Accounts] ([AccountFrameworkVersionId], [ParentAccountId]);
GO

/* Inclusive account-group ranges used when no exact synthetic account exists. */
CREATE TABLE [Accounts].[AccountRanges]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [AccountFrameworkVersionId] int NOT NULL,
    [AccountLevel] tinyint NOT NULL,
    [FromAccountCode] nvarchar(10) NOT NULL,
    [ToAccountCode] nvarchar(10) NOT NULL,
    [ParentAccountId] int NULL,
    [AccountName_sk] nvarchar(500) NOT NULL,
    [SortOrder] int NOT NULL,
    CONSTRAINT [PK_Accounts_AccountRanges] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Accounts_AccountRanges_Version] FOREIGN KEY ([AccountFrameworkVersionId]) REFERENCES [Accounts].[AccountFrameworkVersion] ([Id]),
    CONSTRAINT [FK_Accounts_AccountRanges_Parent] FOREIGN KEY ([AccountFrameworkVersionId], [ParentAccountId]) REFERENCES [Accounts].[Accounts] ([AccountFrameworkVersionId], [Id]),
    CONSTRAINT [UQ_Accounts_AccountRanges_Range] UNIQUE ([AccountFrameworkVersionId], [AccountLevel], [FromAccountCode], [ToAccountCode]),
    CONSTRAINT [UQ_Accounts_AccountRanges_SortOrder] UNIQUE ([AccountFrameworkVersionId], [SortOrder]),
    CONSTRAINT [CK_Accounts_AccountRanges_Level] CHECK ([AccountLevel] BETWEEN 1 AND 3),
    CONSTRAINT [CK_Accounts_AccountRanges_Bounds] CHECK ([FromAccountCode] <= [ToAccountCode])
);
GO

/* RegisterUZ report-template projection with normalized internal relationships. */
CREATE TABLE [Template].[Templates]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [ErpId] int NOT NULL, -- Stable RegisterUZ template identifier.
    [Name_sk] nvarchar(500) NULL,
    [Name_en] nvarchar(500) NULL,
    [MfSpecification] nvarchar(500) NULL,
    [ValidFrom] date NULL,
    [ValidTo] date NULL,
    CONSTRAINT [PK_Template_Templates] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [UQ_Template_Templates_ErpId] UNIQUE ([ErpId]),
    CONSTRAINT [CK_Template_Templates_Dates] CHECK ([ValidFrom] IS NULL OR [ValidTo] IS NULL OR [ValidTo] >= [ValidFrom])
);
GO

CREATE TABLE [Template].[Tables]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [TableErpId] int NOT NULL, -- Stable AuditAddIn/RegisterUZ table identifier.
    [TemplateId] int NOT NULL,
    [Name_sk] nvarchar(200) NULL,
    [Name_en] nvarchar(200) NULL,
    [NumberOfColumns] int NULL,
    [NumberOfDataColumns] int NULL,
    [DontHaveRowNumbers] bit NOT NULL CONSTRAINT [DF_Template_Tables_DontHaveRowNumbers] DEFAULT (0),
    [TableOrdinal] int NOT NULL,
    CONSTRAINT [PK_Template_Tables] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Template_Tables_Template] FOREIGN KEY ([TemplateId]) REFERENCES [Template].[Templates] ([Id]),
    CONSTRAINT [UQ_Template_Tables_ErpId] UNIQUE ([TableErpId]),
    CONSTRAINT [UQ_Template_Tables_Ordinal] UNIQUE ([TemplateId], [TableOrdinal]),
    CONSTRAINT [CK_Template_Tables_Columns] CHECK ([NumberOfColumns] IS NULL OR [NumberOfColumns] >= 0),
    CONSTRAINT [CK_Template_Tables_DataColumns] CHECK ([NumberOfDataColumns] IS NULL OR [NumberOfDataColumns] >= 0 AND ([NumberOfColumns] IS NULL OR [NumberOfDataColumns] <= [NumberOfColumns]))
);
GO

CREATE INDEX [IX_Template_Tables_Template] ON [Template].[Tables] ([TemplateId]);
GO

CREATE TABLE [Template].[Headers]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [TableId] int NOT NULL,
    [Text_sk] nvarchar(max) NULL,
    [Text_en] nvarchar(max) NULL,
    [RowPosition] int NOT NULL,
    [ColumnPosition] int NOT NULL,
    [RowSpan] int NOT NULL,
    [ColumnSpan] int NOT NULL,
    [HeaderOrdinal] int NOT NULL,
    CONSTRAINT [PK_Template_Headers] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Template_Headers_Table] FOREIGN KEY ([TableId]) REFERENCES [Template].[Tables] ([Id]),
    CONSTRAINT [UQ_Template_Headers_Ordinal] UNIQUE ([TableId], [HeaderOrdinal]),
    CONSTRAINT [UQ_Template_Headers_Position] UNIQUE ([TableId], [RowPosition], [ColumnPosition]),
    CONSTRAINT [CK_Template_Headers_Position] CHECK ([RowPosition] >= 0 AND [ColumnPosition] >= 0),
    CONSTRAINT [CK_Template_Headers_Span] CHECK ([RowSpan] > 0 AND [ColumnSpan] > 0)
);
GO

CREATE TABLE [Template].[Rows]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [TableId] int NOT NULL,
    [RowNumber] int NULL,
    [Designation] nvarchar(100) NULL,
    [Text_sk] nvarchar(max) NULL,
    [Text_en] nvarchar(max) NULL,
    [IsSumRow] bit NOT NULL CONSTRAINT [DF_Template_Rows_IsSumRow] DEFAULT (0),
    [Category_sk] nvarchar(max) NULL,
    [MappingCaption_sk] nvarchar(500) NULL,
    [RowOrdinal] int NOT NULL,
    CONSTRAINT [PK_Template_Rows] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Template_Rows_Table] FOREIGN KEY ([TableId]) REFERENCES [Template].[Tables] ([Id]),
    CONSTRAINT [UQ_Template_Rows_Ordinal] UNIQUE ([TableId], [RowOrdinal]),
    CONSTRAINT [CK_Template_Rows_RowNumber] CHECK ([RowNumber] IS NULL OR [RowNumber] >= 0)
);
GO

CREATE UNIQUE INDEX [UX_Template_Rows_Number] ON [Template].[Rows] ([TableId], [RowNumber]) WHERE [RowNumber] IS NOT NULL;
GO

/* Calculation configuration is versioned independently from official wording. */
CREATE TABLE [Accounts].[ValueSource]
(
    [Code] nvarchar(50) NOT NULL,
    [Description] nvarchar(200) NOT NULL,
    CONSTRAINT [PK_Accounts_ValueSource] PRIMARY KEY CLUSTERED ([Code])
);
GO

CREATE TABLE [Accounts].[CalculationConfigurationVersion]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [AccountFrameworkVersionId] int NOT NULL,
    [AccountingModelCode] nvarchar(20) NOT NULL, -- Preserves the API model value, currently GOV.
    [Code] nvarchar(50) NOT NULL,
    [Description] nvarchar(500) NULL,
    [ValidFrom] date NOT NULL,
    [ValidTo] date NULL,
    [CreatedAtUtc] datetime2(0) NOT NULL CONSTRAINT [DF_Accounts_CalculationConfigurationVersion_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_Accounts_CalculationConfigurationVersion] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Accounts_CalculationConfigurationVersion_FrameworkVersion] FOREIGN KEY ([AccountFrameworkVersionId]) REFERENCES [Accounts].[AccountFrameworkVersion] ([Id]),
    CONSTRAINT [UQ_Accounts_CalculationConfigurationVersion_Code] UNIQUE ([AccountFrameworkVersionId], [Code]),
    CONSTRAINT [CK_Accounts_CalculationConfigurationVersion_Dates] CHECK ([ValidTo] IS NULL OR [ValidTo] >= [ValidFrom])
);
GO

CREATE UNIQUE INDEX [UX_Accounts_CalculationConfigurationVersion_Current]
    ON [Accounts].[CalculationConfigurationVersion] ([AccountFrameworkVersionId]) WHERE [ValidTo] IS NULL;
GO

CREATE TABLE [Accounts].[AccountCalculationRules]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [CalculationConfigurationVersionId] int NOT NULL,
    [AccountId] int NOT NULL,
    [Legend] nvarchar(10) NULL,
    [AssetsValueSourceCode] nvarchar(50) NULL,
    [LiabilitiesValueSourceCode] nvarchar(50) NULL,
    CONSTRAINT [PK_Accounts_AccountCalculationRules] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Accounts_AccountCalculationRules_Configuration] FOREIGN KEY ([CalculationConfigurationVersionId]) REFERENCES [Accounts].[CalculationConfigurationVersion] ([Id]),
    CONSTRAINT [FK_Accounts_AccountCalculationRules_Account] FOREIGN KEY ([AccountId]) REFERENCES [Accounts].[Accounts] ([Id]),
    CONSTRAINT [FK_Accounts_AccountCalculationRules_AssetsSource] FOREIGN KEY ([AssetsValueSourceCode]) REFERENCES [Accounts].[ValueSource] ([Code]),
    CONSTRAINT [FK_Accounts_AccountCalculationRules_LiabilitiesSource] FOREIGN KEY ([LiabilitiesValueSourceCode]) REFERENCES [Accounts].[ValueSource] ([Code]),
    CONSTRAINT [UQ_Accounts_AccountCalculationRules_Account] UNIQUE ([CalculationConfigurationVersionId], [AccountId])
);
GO

CREATE TABLE [Accounts].[TemplateFrameworkVersion]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [TemplateId] int NOT NULL,
    [AccountFrameworkVersionId] int NOT NULL,
    [CalculationConfigurationVersionId] int NOT NULL,
    CONSTRAINT [PK_Accounts_TemplateFrameworkVersion] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Accounts_TemplateFrameworkVersion_Template] FOREIGN KEY ([TemplateId]) REFERENCES [Template].[Templates] ([Id]),
    CONSTRAINT [FK_Accounts_TemplateFrameworkVersion_FrameworkVersion] FOREIGN KEY ([AccountFrameworkVersionId]) REFERENCES [Accounts].[AccountFrameworkVersion] ([Id]),
    CONSTRAINT [FK_Accounts_TemplateFrameworkVersion_Configuration] FOREIGN KEY ([CalculationConfigurationVersionId]) REFERENCES [Accounts].[CalculationConfigurationVersion] ([Id]),
    CONSTRAINT [UQ_Accounts_TemplateFrameworkVersion] UNIQUE ([TemplateId], [CalculationConfigurationVersionId])
);
GO

/* One account contribution to one report row; titles are always joined from Accounts. */
CREATE TABLE [Accounts].[ReportAccountMappings]
(
    [Id] int IDENTITY(1,1) NOT NULL,
    [TemplateFrameworkVersionId] int NOT NULL,
    [TemplateRowId] int NOT NULL,
    [AccountCalculationRuleId] int NOT NULL,
    [RequiresAnalyticalMapping] bit NOT NULL,
    [IncludeInBrutto] bit NOT NULL,
    [IncludeInCorrection] bit NOT NULL,
    [Side] varchar(20) NOT NULL,
    [ValueSourceCode] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_Accounts_ReportAccountMappings] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_Accounts_ReportAccountMappings_TemplateFramework] FOREIGN KEY ([TemplateFrameworkVersionId]) REFERENCES [Accounts].[TemplateFrameworkVersion] ([Id]),
    CONSTRAINT [FK_Accounts_ReportAccountMappings_Row] FOREIGN KEY ([TemplateRowId]) REFERENCES [Template].[Rows] ([Id]),
    CONSTRAINT [FK_Accounts_ReportAccountMappings_Rule] FOREIGN KEY ([AccountCalculationRuleId]) REFERENCES [Accounts].[AccountCalculationRules] ([Id]),
    CONSTRAINT [FK_Accounts_ReportAccountMappings_ValueSource] FOREIGN KEY ([ValueSourceCode]) REFERENCES [Accounts].[ValueSource] ([Code]),
    CONSTRAINT [UQ_Accounts_ReportAccountMappings] UNIQUE ([TemplateFrameworkVersionId], [TemplateRowId], [AccountCalculationRuleId]),
    CONSTRAINT [CK_Accounts_ReportAccountMappings_Side] CHECK ([Side] IN ('Assets', 'Liabilities')),
    CONSTRAINT [CK_Accounts_ReportAccountMappings_Columns] CHECK ([IncludeInBrutto] = 1 OR [IncludeInCorrection] = 1)
);
GO

CREATE INDEX [IX_Accounts_ReportAccountMappings_Row] ON [Accounts].[ReportAccountMappings] ([TemplateRowId]);
GO

/* Direct normalized formula graph. Transitive execution plans are generated. */
CREATE TABLE [Template].[RowCalculationTerms]
(
    [TargetRowId] int NOT NULL,
    [SourceRowId] int NOT NULL,
    [Coefficient] int NOT NULL,
    CONSTRAINT [PK_Template_RowCalculationTerms] PRIMARY KEY CLUSTERED ([TargetRowId], [SourceRowId]),
    CONSTRAINT [FK_Template_RowCalculationTerms_Target] FOREIGN KEY ([TargetRowId]) REFERENCES [Template].[Rows] ([Id]),
    CONSTRAINT [FK_Template_RowCalculationTerms_Source] FOREIGN KEY ([SourceRowId]) REFERENCES [Template].[Rows] ([Id]),
    CONSTRAINT [CK_Template_RowCalculationTerms_Coefficient] CHECK ([Coefficient] <> 0),
    CONSTRAINT [CK_Template_RowCalculationTerms_DifferentRows] CHECK ([TargetRowId] <> [SourceRowId])
);
GO

CREATE INDEX [IX_Template_RowCalculationTerms_Source] ON [Template].[RowCalculationTerms] ([SourceRowId]);
GO

/* NULL date means the open version; otherwise the version valid on that date. */
CREATE FUNCTION [Accounts].[GetAccounts]
(
    @FrameworkCode nvarchar(20), @ApplicableDate date
)
RETURNS TABLE
AS
RETURN
(
    SELECT af.[Code] AS [FrameworkCode], af.[Name_sk] AS [FrameworkName_sk],
           afv.[Id] AS [AccountFrameworkVersionId], afv.[VersionCode], afv.[ValidFrom], afv.[ValidTo],
           a.[Id] AS [AccountId], a.[AccountCode], a.[AccountName_sk], a.[AccountLevel],
           a.[ParentAccountId], a.[SortOrder]
    FROM [Accounts].[AccountFramework] af
    INNER JOIN [Accounts].[AccountFrameworkVersion] afv ON afv.[AccountFrameworkId] = af.[Id]
    INNER JOIN [Accounts].[Accounts] a ON a.[AccountFrameworkVersionId] = afv.[Id]
    WHERE af.[Code] = @FrameworkCode
      AND ((@ApplicableDate IS NULL AND afv.[ValidTo] IS NULL)
        OR (@ApplicableDate IS NOT NULL AND afv.[ValidFrom] <= @ApplicableDate
            AND (afv.[ValidTo] IS NULL OR afv.[ValidTo] >= @ApplicableDate)))
);
GO

CREATE FUNCTION [Accounts].[GetAccountRanges]
(
    @FrameworkCode nvarchar(20), @ApplicableDate date
)
RETURNS TABLE
AS
RETURN
(
    SELECT afv.[Id] AS [AccountFrameworkVersionId], r.[Id] AS [AccountRangeId], r.[AccountLevel],
           r.[FromAccountCode], r.[ToAccountCode], r.[ParentAccountId], r.[AccountName_sk], r.[SortOrder]
    FROM [Accounts].[AccountFramework] af
    INNER JOIN [Accounts].[AccountFrameworkVersion] afv ON afv.[AccountFrameworkId] = af.[Id]
    INNER JOIN [Accounts].[AccountRanges] r ON r.[AccountFrameworkVersionId] = afv.[Id]
    WHERE af.[Code] = @FrameworkCode
      AND ((@ApplicableDate IS NULL AND afv.[ValidTo] IS NULL)
        OR (@ApplicableDate IS NOT NULL AND afv.[ValidFrom] <= @ApplicableDate
            AND (afv.[ValidTo] IS NULL OR afv.[ValidTo] >= @ApplicableDate)))
);
GO

/* Expands the direct formula graph into the leaf-level API execution plan. */
CREATE FUNCTION [Template].[GetCalculationPlan]
(
    @TemplateErpId int
)
RETURNS TABLE
AS
RETURN
(
    WITH [DirectTerms] AS
    (
        SELECT tr.[Id] AS [TargetRowId], sr.[Id] AS [SourceRowId], ct.[Coefficient]
        FROM [Template].[RowCalculationTerms] ct
        INNER JOIN [Template].[Rows] tr ON tr.[Id] = ct.[TargetRowId]
        INNER JOIN [Template].[Tables] tt ON tt.[Id] = tr.[TableId]
        INNER JOIN [Template].[Templates] t ON t.[Id] = tt.[TemplateId]
        INNER JOIN [Template].[Rows] sr ON sr.[Id] = ct.[SourceRowId]
        WHERE t.[ErpId] = @TemplateErpId
    ),
    [Expansion] AS
    (
        SELECT d.[TargetRowId] AS [RootTargetRowId], d.[SourceRowId], CONVERT(bigint, d.[Coefficient]) AS [Coefficient], 1 AS [Depth]
        FROM [DirectTerms] d
        UNION ALL
        SELECT e.[RootTargetRowId], d.[SourceRowId], e.[Coefficient] * d.[Coefficient], e.[Depth] + 1
        FROM [Expansion] e
        INNER JOIN [DirectTerms] d ON d.[TargetRowId] = e.[SourceRowId]
    ),
    [TargetLevels] AS
    (
        SELECT [RootTargetRowId], MAX([Depth]) AS [CalculationLevel]
        FROM [Expansion]
        GROUP BY [RootTargetRowId]
    ),
    [LeafTerms] AS
    (
        SELECT e.[RootTargetRowId], e.[SourceRowId], SUM(e.[Coefficient]) AS [Coefficient]
        FROM [Expansion] e
        WHERE NOT EXISTS (SELECT 1 FROM [DirectTerms] d WHERE d.[TargetRowId] = e.[SourceRowId])
        GROUP BY e.[RootTargetRowId], e.[SourceRowId]
        HAVING SUM(e.[Coefficient]) <> 0
    )
    SELECT targetTable.[TableErpId] AS [SumTableErpId], targetRow.[RowNumber] AS [SumRow],
           sourceTable.[TableErpId] AS [SourceTableErpId], sourceRow.[RowNumber] AS [SourceRow],
           CONVERT(int, leaf.[Coefficient]) AS [Coefficient], levels.[CalculationLevel]
    FROM [LeafTerms] leaf
    INNER JOIN [TargetLevels] levels ON levels.[RootTargetRowId] = leaf.[RootTargetRowId]
    INNER JOIN [Template].[Rows] targetRow ON targetRow.[Id] = leaf.[RootTargetRowId]
    INNER JOIN [Template].[Tables] targetTable ON targetTable.[Id] = targetRow.[TableId]
    INNER JOIN [Template].[Rows] sourceRow ON sourceRow.[Id] = leaf.[SourceRowId]
    INNER JOIN [Template].[Tables] sourceTable ON sourceTable.[Id] = sourceRow.[TableId]
);
GO

/* API-oriented read models derive all account captions from the production account table. */
CREATE VIEW [Accounts].[AccountCalculationRuleDetails]
AS
SELECT ccv.[Id] AS [CalculationConfigurationVersionId], ccv.[Code] AS [ConfigurationCode],
       ccv.[AccountingModelCode], af.[Code] AS [FrameworkCode], afv.[Id] AS [AccountFrameworkVersionId],
       r.[Id] AS [AccountCalculationRuleId], a.[AccountCode], a.[AccountName_sk], r.[Legend],
       r.[AssetsValueSourceCode], r.[LiabilitiesValueSourceCode]
FROM [Accounts].[AccountCalculationRules] r
INNER JOIN [Accounts].[CalculationConfigurationVersion] ccv ON ccv.[Id] = r.[CalculationConfigurationVersionId]
INNER JOIN [Accounts].[Accounts] a ON a.[Id] = r.[AccountId]
INNER JOIN [Accounts].[AccountFrameworkVersion] afv ON afv.[Id] = a.[AccountFrameworkVersionId]
INNER JOIN [Accounts].[AccountFramework] af ON af.[Id] = afv.[AccountFrameworkId];
GO

CREATE VIEW [Accounts].[ReportAccountMappingDetails]
AS
SELECT t.[ErpId] AS [TemplateErpId], tt.[TableErpId], tr.[RowNumber] AS [ReportRowNumber],
       a.[AccountCode], a.[AccountName_sk], m.[RequiresAnalyticalMapping],
       m.[IncludeInBrutto], m.[IncludeInCorrection], m.[Side], m.[ValueSourceCode]
FROM [Accounts].[ReportAccountMappings] m
INNER JOIN [Accounts].[TemplateFrameworkVersion] tfv ON tfv.[Id] = m.[TemplateFrameworkVersionId]
INNER JOIN [Template].[Templates] t ON t.[Id] = tfv.[TemplateId]
INNER JOIN [Template].[Rows] tr ON tr.[Id] = m.[TemplateRowId]
INNER JOIN [Template].[Tables] tt ON tt.[Id] = tr.[TableId]
INNER JOIN [Accounts].[AccountCalculationRules] r ON r.[Id] = m.[AccountCalculationRuleId]
INNER JOIN [Accounts].[Accounts] a ON a.[Id] = r.[AccountId];
GO

/*
    The API and SQL Server currently run on the same Windows server. The
    application pool therefore connects by Windows integrated security and no
    SQL password is stored in application configuration.
*/
USE [master];
GO

IF SUSER_ID(N'IIS APPPOOL\ExcelApiPoc.Api') IS NULL
BEGIN
    CREATE LOGIN [IIS APPPOOL\ExcelApiPoc.Api] FROM WINDOWS;
END;
GO

IF EXISTS
(
    SELECT 1
    FROM [sys].[server_principals]
    WHERE [name] = N'IIS APPPOOL\ExcelApiPoc.Api'
      AND [type] NOT IN ('U', 'G')
)
    THROW 52001,
        'Server principal [IIS APPPOOL\ExcelApiPoc.Api] exists but is not a Windows principal.',
        1;
GO

USE [AuditAddIn];
GO

CREATE USER [IIS APPPOOL\ExcelApiPoc.Api]
    FOR LOGIN [IIS APPPOOL\ExcelApiPoc.Api]
    WITH DEFAULT_SCHEMA = [dbo];
GO

GRANT CONNECT TO [IIS APPPOOL\ExcelApiPoc.Api];
GRANT SELECT ON SCHEMA::[Accounts] TO [IIS APPPOOL\ExcelApiPoc.Api];
GRANT SELECT ON SCHEMA::[Template] TO [IIS APPPOOL\ExcelApiPoc.Api];
GO

PRINT 'AuditAddIn schema and IIS application-pool access created successfully.';
GO
