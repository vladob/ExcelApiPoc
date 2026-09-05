/*
    RegisterUZinitializationV2_1.sql

    Version 2.1 SQL Server database structure for data retrieved from the
    Register of Financial Statements of the Slovak Republic (RegisterUZ).

    Design principles
    -----------------
    - The database inherits the SQL Server default collation.
    - No SQL Server system-versioned temporal tables are used.
    - Normalized tables contain the current accepted state.
    - Raw API payload versions are immutable and retained for traceability.
    - PayloadSha256 verifies the exact stored response bytes.
    - CanonicalSha256 identifies semantic versions while ignoring JSON object
      property order, whitespace and order of relationship-ID sets.
    - Observation timestamps are distinct from source modification dates.
    - A financial report belongs to exactly one financial statement or
      exactly one annual report, never both.
    - Source deletions are represented by status; rows are not physically
      deleted by synchronization.

    The script is non-destructive and can be run again. It creates missing
    objects but does not alter or drop existing objects.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF DB_ID(N'RegisterUZ') IS NULL
BEGIN
    CREATE DATABASE [RegisterUZ];
END;
GO

USE [RegisterUZ];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE [name] = N'Registry')
    EXEC(N'CREATE SCHEMA [Registry] AUTHORIZATION [dbo];');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE [name] = N'Reference')
    EXEC(N'CREATE SCHEMA [Reference] AUTHORIZATION [dbo];');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE [name] = N'Templates')
    EXEC(N'CREATE SCHEMA [Templates] AUTHORIZATION [dbo];');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE [name] = N'Reporting')
    EXEC(N'CREATE SCHEMA [Reporting] AUTHORIZATION [dbo];');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE [name] = N'Raw')
    EXEC(N'CREATE SCHEMA [Raw] AUTHORIZATION [dbo];');
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE [name] = N'Sync')
    EXEC(N'CREATE SCHEMA [Sync] AUTHORIZATION [dbo];');
GO

/* V2 is a clean initialization script, not an in-place V1 migration. */
IF OBJECT_ID(N'[Raw].[PayloadVersion]', N'U') IS NOT NULL
   AND COL_LENGTH(N'Raw.PayloadVersion', N'CanonicalSha256') IS NULL
BEGIN
    THROW 50002,
        'RegisterUZ V1 detected. Recreate the experimental database before running initialization V2.',
        1;
END;
GO

/* -------------------------------------------------------------------------
   Synchronization lookups
   ------------------------------------------------------------------------- */

IF OBJECT_ID(N'[Sync].[ObjectType]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sync].[ObjectType]
    (
        [ObjectTypeId] tinyint NOT NULL,
        [Code] varchar(30) NOT NULL,
        [ApiListPath] varchar(200) NOT NULL,
        [ApiDetailPath] varchar(200) NOT NULL,
        CONSTRAINT [PK_Sync_ObjectType]
            PRIMARY KEY CLUSTERED ([ObjectTypeId]),
        CONSTRAINT [UQ_Sync_ObjectType_Code]
            UNIQUE ([Code])
    );
END;
GO

INSERT INTO [Sync].[ObjectType]
    ([ObjectTypeId], [Code], [ApiListPath], [ApiDetailPath])
SELECT v.[ObjectTypeId], v.[Code], v.[ApiListPath], v.[ApiDetailPath]
FROM
(
    VALUES
        (CONVERT(tinyint, 1), 'AccountingEntity',
         '/api/uctovne-jednotky', '/api/uctovna-jednotka'),
        (CONVERT(tinyint, 2), 'FinancialStatement',
         '/api/uctovne-zavierky', '/api/uctovna-zavierka'),
        (CONVERT(tinyint, 3), 'FinancialReport',
         '/api/uctovne-vykazy', '/api/uctovny-vykaz'),
        (CONVERT(tinyint, 4), 'AnnualReport',
         '/api/vyrocne-spravy', '/api/vyrocna-sprava')
) v ([ObjectTypeId], [Code], [ApiListPath], [ApiDetailPath])
WHERE NOT EXISTS
(
    SELECT 1
    FROM [Sync].[ObjectType] t
    WHERE t.[ObjectTypeId] = v.[ObjectTypeId]
       OR t.[Code] = v.[Code]
);
GO

/* -------------------------------------------------------------------------
   Reference classifications
   SourceCode is the identifier returned in accounting-entity details.
   ------------------------------------------------------------------------- */

IF OBJECT_ID(N'[Reference].[LegalForm]', N'U') IS NULL
BEGIN
    CREATE TABLE [Reference].[LegalForm]
    (
        [SourceCode] varchar(100) NOT NULL,
        [TitleSk] nvarchar(250) NULL,
        [TitleEn] nvarchar(250) NULL,
        [IsDeleted] bit NOT NULL
            CONSTRAINT [DF_Reference_LegalForm_IsDeleted] DEFAULT (0),
        [SourceLastModifiedDate] date NULL,
        [FirstObservedAtUtc] datetime2(3) NOT NULL,
        [LastObservedAtUtc] datetime2(3) NOT NULL,
        [CreatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reference_LegalForm_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reference_LegalForm_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Reference_LegalForm] PRIMARY KEY CLUSTERED ([SourceCode]),
        CONSTRAINT [CK_Reference_LegalForm_ObservationOrder]
            CHECK ([LastObservedAtUtc] >= [FirstObservedAtUtc])
    );
END;
GO

IF OBJECT_ID(N'[Reference].[SkNace]', N'U') IS NULL
BEGIN
    CREATE TABLE [Reference].[SkNace]
    (
        [SourceCode] varchar(100) NOT NULL,
        [TitleSk] nvarchar(250) NULL,
        [TitleEn] nvarchar(250) NULL,
        [IsDeleted] bit NOT NULL
            CONSTRAINT [DF_Reference_SkNace_IsDeleted] DEFAULT (0),
        [SourceLastModifiedDate] date NULL,
        [FirstObservedAtUtc] datetime2(3) NOT NULL,
        [LastObservedAtUtc] datetime2(3) NOT NULL,
        [CreatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reference_SkNace_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reference_SkNace_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Reference_SkNace] PRIMARY KEY CLUSTERED ([SourceCode]),
        CONSTRAINT [CK_Reference_SkNace_ObservationOrder]
            CHECK ([LastObservedAtUtc] >= [FirstObservedAtUtc])
    );
END;
GO

IF OBJECT_ID(N'[Reference].[OrganizationSize]', N'U') IS NULL
BEGIN
    CREATE TABLE [Reference].[OrganizationSize]
    (
        [SourceCode] varchar(100) NOT NULL,
        [TitleSk] nvarchar(250) NULL,
        [TitleEn] nvarchar(250) NULL,
        [IsDeleted] bit NOT NULL
            CONSTRAINT [DF_Reference_OrganizationSize_IsDeleted] DEFAULT (0),
        [SourceLastModifiedDate] date NULL,
        [FirstObservedAtUtc] datetime2(3) NOT NULL,
        [LastObservedAtUtc] datetime2(3) NOT NULL,
        [CreatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reference_OrganizationSize_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reference_OrganizationSize_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Reference_OrganizationSize] PRIMARY KEY CLUSTERED ([SourceCode]),
        CONSTRAINT [CK_Reference_OrganizationSize_ObservationOrder]
            CHECK ([LastObservedAtUtc] >= [FirstObservedAtUtc])
    );
END;
GO

IF OBJECT_ID(N'[Reference].[OwnershipType]', N'U') IS NULL
BEGIN
    CREATE TABLE [Reference].[OwnershipType]
    (
        [SourceCode] varchar(100) NOT NULL,
        [TitleSk] nvarchar(250) NULL,
        [TitleEn] nvarchar(250) NULL,
        [IsDeleted] bit NOT NULL
            CONSTRAINT [DF_Reference_OwnershipType_IsDeleted] DEFAULT (0),
        [SourceLastModifiedDate] date NULL,
        [FirstObservedAtUtc] datetime2(3) NOT NULL,
        [LastObservedAtUtc] datetime2(3) NOT NULL,
        [CreatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reference_OwnershipType_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reference_OwnershipType_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Reference_OwnershipType] PRIMARY KEY CLUSTERED ([SourceCode]),
        CONSTRAINT [CK_Reference_OwnershipType_ObservationOrder]
            CHECK ([LastObservedAtUtc] >= [FirstObservedAtUtc])
    );
END;
GO

IF OBJECT_ID(N'[Reference].[Location]', N'U') IS NULL
BEGIN
    CREATE TABLE [Reference].[Location]
    (
        [SourceCode] varchar(100) NOT NULL,
        [TitleSk] nvarchar(250) NULL,
        [TitleEn] nvarchar(250) NULL,
        [ParentSourceCode] varchar(100) NULL,
        [IsDeleted] bit NOT NULL
            CONSTRAINT [DF_Reference_Location_IsDeleted] DEFAULT (0),
        [SourceLastModifiedDate] date NULL,
        [FirstObservedAtUtc] datetime2(3) NOT NULL,
        [LastObservedAtUtc] datetime2(3) NOT NULL,
        [CreatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reference_Location_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reference_Location_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Reference_Location] PRIMARY KEY CLUSTERED ([SourceCode]),
        CONSTRAINT [FK_Reference_Location_Parent]
            FOREIGN KEY ([ParentSourceCode])
            REFERENCES [Reference].[Location] ([SourceCode]),
        CONSTRAINT [CK_Reference_Location_NotOwnParent]
            CHECK ([ParentSourceCode] IS NULL OR [ParentSourceCode] <> [SourceCode]),
        CONSTRAINT [CK_Reference_Location_ObservationOrder]
            CHECK ([LastObservedAtUtc] >= [FirstObservedAtUtc])
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Reference].[Location]')
      AND [name] = N'IX_Reference_Location_ParentSourceCode'
)
    CREATE INDEX [IX_Reference_Location_ParentSourceCode]
        ON [Reference].[Location] ([ParentSourceCode]);
GO

/* -------------------------------------------------------------------------
   Official RegisterUZ financial-report templates
   ------------------------------------------------------------------------- */

IF OBJECT_ID(N'[Templates].[FinancialReportTemplate]', N'U') IS NULL
BEGIN
    CREATE TABLE [Templates].[FinancialReportTemplate]
    (
        [RegisterUzTemplateId] bigint NOT NULL,
        [Name] nvarchar(255) NULL,
        [MinistrySpecification] nvarchar(100) NULL,
        [ValidFrom] date NULL,
        [ValidTo] date NULL,
        [IsDeleted] bit NOT NULL
            CONSTRAINT [DF_Templates_FinancialReportTemplate_IsDeleted] DEFAULT (0),
        [FirstObservedAtUtc] datetime2(3) NOT NULL,
        [LastObservedAtUtc] datetime2(3) NOT NULL,
        [CreatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Templates_FinancialReportTemplate_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Templates_FinancialReportTemplate_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Templates_FinancialReportTemplate]
            PRIMARY KEY CLUSTERED ([RegisterUzTemplateId]),
        CONSTRAINT [CK_Templates_FinancialReportTemplate_Id]
            CHECK ([RegisterUzTemplateId] > 0),
        CONSTRAINT [CK_Templates_FinancialReportTemplate_Validity]
            CHECK ([ValidFrom] IS NULL OR [ValidTo] IS NULL OR [ValidTo] >= [ValidFrom]),
        CONSTRAINT [CK_Templates_FinancialReportTemplate_ObservationOrder]
            CHECK ([LastObservedAtUtc] >= [FirstObservedAtUtc])
    );
END;
GO

IF OBJECT_ID(N'[Templates].[TemplateTable]', N'U') IS NULL
BEGIN
    CREATE TABLE [Templates].[TemplateTable]
    (
        [TemplateTableId] bigint IDENTITY(1,1) NOT NULL,
        [RegisterUzTemplateId] bigint NOT NULL,
        [TableOrdinal] int NOT NULL,
        [NameSk] nvarchar(250) NULL,
        [NameEn] nvarchar(250) NULL,
        [NumberOfColumns] int NULL,
        [NumberOfDataColumns] int NULL,
        [CreatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Templates_TemplateTable_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Templates_TemplateTable_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Templates_TemplateTable]
            PRIMARY KEY CLUSTERED ([TemplateTableId]),
        CONSTRAINT [FK_Templates_TemplateTable_Template]
            FOREIGN KEY ([RegisterUzTemplateId])
            REFERENCES [Templates].[FinancialReportTemplate] ([RegisterUzTemplateId]),
        CONSTRAINT [UQ_Templates_TemplateTable_Template_Ordinal]
            UNIQUE ([RegisterUzTemplateId], [TableOrdinal]),
        CONSTRAINT [CK_Templates_TemplateTable_Ordinal]
            CHECK ([TableOrdinal] >= 0),
        CONSTRAINT [CK_Templates_TemplateTable_ColumnCounts]
            CHECK
            (
                ([NumberOfColumns] IS NULL OR [NumberOfColumns] >= 0)
                AND ([NumberOfDataColumns] IS NULL OR [NumberOfDataColumns] >= 0)
                AND ([NumberOfColumns] IS NULL OR [NumberOfDataColumns] IS NULL
                     OR [NumberOfDataColumns] <= [NumberOfColumns])
            )
    );
END;
GO

IF OBJECT_ID(N'[Templates].[TemplateHeader]', N'U') IS NULL
BEGIN
    CREATE TABLE [Templates].[TemplateHeader]
    (
        [TemplateHeaderId] bigint IDENTITY(1,1) NOT NULL,
        [TemplateTableId] bigint NOT NULL,
        [HeaderOrdinal] int NOT NULL,
        [TextSk] nvarchar(max) NULL,
        [TextEn] nvarchar(max) NULL,
        [RowPosition] int NULL,
        [ColumnPosition] int NULL,
        [ColumnSpan] int NULL,
        [RowSpan] int NULL,
        CONSTRAINT [PK_Templates_TemplateHeader]
            PRIMARY KEY CLUSTERED ([TemplateHeaderId]),
        CONSTRAINT [FK_Templates_TemplateHeader_Table]
            FOREIGN KEY ([TemplateTableId])
            REFERENCES [Templates].[TemplateTable] ([TemplateTableId]),
        CONSTRAINT [UQ_Templates_TemplateHeader_Table_Ordinal]
            UNIQUE ([TemplateTableId], [HeaderOrdinal]),
        CONSTRAINT [CK_Templates_TemplateHeader_Positions]
            CHECK
            (
                [HeaderOrdinal] >= 0
                AND ([RowPosition] IS NULL OR [RowPosition] >= 0)
                AND ([ColumnPosition] IS NULL OR [ColumnPosition] >= 0)
                AND ([ColumnSpan] IS NULL OR [ColumnSpan] >= 1)
                AND ([RowSpan] IS NULL OR [RowSpan] >= 1)
            )
    );
END;
GO

IF OBJECT_ID(N'[Templates].[TemplateRow]', N'U') IS NULL
BEGIN
    CREATE TABLE [Templates].[TemplateRow]
    (
        [TemplateRowId] bigint IDENTITY(1,1) NOT NULL,
        [TemplateTableId] bigint NOT NULL,
        [RowOrdinal] int NOT NULL,
        [RowNumber] int NULL,
        [Designation] nvarchar(100) NULL,
        [TextSk] nvarchar(max) NULL,
        [TextEn] nvarchar(max) NULL,
        CONSTRAINT [PK_Templates_TemplateRow]
            PRIMARY KEY CLUSTERED ([TemplateRowId]),
        CONSTRAINT [FK_Templates_TemplateRow_Table]
            FOREIGN KEY ([TemplateTableId])
            REFERENCES [Templates].[TemplateTable] ([TemplateTableId]),
        CONSTRAINT [UQ_Templates_TemplateRow_Table_Ordinal]
            UNIQUE ([TemplateTableId], [RowOrdinal]),
        CONSTRAINT [CK_Templates_TemplateRow_Ordinal]
            CHECK ([RowOrdinal] >= 0)
    );
END;
GO

/* -------------------------------------------------------------------------
   Registry
   ------------------------------------------------------------------------- */

IF OBJECT_ID(N'[Registry].[AccountingEntity]', N'U') IS NULL
BEGIN
    CREATE TABLE [Registry].[AccountingEntity]
    (
        [RegisterUzEntityId] bigint NOT NULL,
        [Ico] varchar(20) NULL,
        [Dic] varchar(20) NULL,
        [Sid] varchar(20) NULL,
        [Name] nvarchar(500) NULL,
        [City] nvarchar(200) NULL,
        [Street] nvarchar(500) NULL,
        [PostalCode] varchar(20) NULL,
        [EstablishedDate] date NULL,
        [CancellationDate] date NULL,
        [LegalFormCode] varchar(100) NULL,
        [SkNaceCode] varchar(100) NULL,
        [OrganizationSizeCode] varchar(100) NULL,
        [OwnershipTypeCode] varchar(100) NULL,
        [RegionCode] varchar(100) NULL,
        [DistrictCode] varchar(100) NULL,
        [RegisteredOfficeCode] varchar(100) NULL,
        [IsConsolidated] bit NULL,
        [DataSourceCode] varchar(30) NULL,
        [SourceLastModifiedDate] date NULL,
        [SourceStatus] nvarchar(30) NULL,
        [IsDeleted] bit NOT NULL
            CONSTRAINT [DF_Registry_AccountingEntity_IsDeleted] DEFAULT (0),
        [FirstObservedAtUtc] datetime2(3) NOT NULL,
        [LastObservedAtUtc] datetime2(3) NOT NULL,
        [LastDetailRetrievedAtUtc] datetime2(3) NOT NULL,
        [CurrentPayloadVersionId] bigint NULL,
        [CreatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Registry_AccountingEntity_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Registry_AccountingEntity_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Registry_AccountingEntity]
            PRIMARY KEY CLUSTERED ([RegisterUzEntityId]),
        CONSTRAINT [FK_Registry_AccountingEntity_LegalForm]
            FOREIGN KEY ([LegalFormCode]) REFERENCES [Reference].[LegalForm] ([SourceCode]),
        CONSTRAINT [FK_Registry_AccountingEntity_SkNace]
            FOREIGN KEY ([SkNaceCode]) REFERENCES [Reference].[SkNace] ([SourceCode]),
        CONSTRAINT [FK_Registry_AccountingEntity_OrganizationSize]
            FOREIGN KEY ([OrganizationSizeCode]) REFERENCES [Reference].[OrganizationSize] ([SourceCode]),
        CONSTRAINT [FK_Registry_AccountingEntity_OwnershipType]
            FOREIGN KEY ([OwnershipTypeCode]) REFERENCES [Reference].[OwnershipType] ([SourceCode]),
        CONSTRAINT [FK_Registry_AccountingEntity_Region]
            FOREIGN KEY ([RegionCode]) REFERENCES [Reference].[Location] ([SourceCode]),
        CONSTRAINT [FK_Registry_AccountingEntity_District]
            FOREIGN KEY ([DistrictCode]) REFERENCES [Reference].[Location] ([SourceCode]),
        CONSTRAINT [CK_Registry_AccountingEntity_Id]
            CHECK ([RegisterUzEntityId] > 0),
        CONSTRAINT [CK_Registry_AccountingEntity_ObservationOrder]
            CHECK
            (
                [LastObservedAtUtc] >= [FirstObservedAtUtc]
                AND [LastDetailRetrievedAtUtc] >= [FirstObservedAtUtc]
            )
    );
END;
GO

/* RegisteredOfficeCode (sidlo) is retained exactly as supplied by the API,
   but the large sidla catalog is intentionally not mirrored. */
IF EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE [parent_object_id] = OBJECT_ID(N'[Registry].[AccountingEntity]')
      AND [name] = N'FK_Registry_AccountingEntity_RegisteredOffice'
)
    ALTER TABLE [Registry].[AccountingEntity]
        DROP CONSTRAINT [FK_Registry_AccountingEntity_RegisteredOffice];
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Registry].[AccountingEntity]')
      AND [name] = N'UX_Registry_AccountingEntity_Ico'
)
    CREATE UNIQUE INDEX [UX_Registry_AccountingEntity_Ico]
        ON [Registry].[AccountingEntity] ([Ico])
        WHERE [Ico] IS NOT NULL AND [IsDeleted] = 0;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Registry].[AccountingEntity]')
      AND [name] = N'IX_Registry_AccountingEntity_SourceLastModifiedDate'
)
    CREATE INDEX [IX_Registry_AccountingEntity_SourceLastModifiedDate]
        ON [Registry].[AccountingEntity] ([SourceLastModifiedDate], [RegisterUzEntityId]);
GO

/* -------------------------------------------------------------------------
   Reporting graph
   ------------------------------------------------------------------------- */

IF OBJECT_ID(N'[Reporting].[FinancialStatement]', N'U') IS NULL
BEGIN
    CREATE TABLE [Reporting].[FinancialStatement]
    (
        [RegisterUzStatementId] bigint NOT NULL,
        [RegisterUzEntityId] bigint NOT NULL,
        [PeriodFrom] char(7) NULL,
        [PeriodTo] char(7) NULL,
        [SubmissionDate] date NULL,
        [PreparationDate] date NULL,
        [ApprovalDate] date NULL,
        [AssemblyDate] date NULL,
        [AuditorReportAttachmentDate] date NULL,
        [FundName] nvarchar(500) NULL,
        [LeiCode] varchar(20) NULL,
        [IsConsolidated] bit NULL,
        [IsConsolidatedCentralGovernment] bit NULL,
        [IsSummaryPublicAdministration] bit NULL,
        [StatementType] nvarchar(100) NULL,
        [DataSourceCode] varchar(30) NULL,
        [SourceLastModifiedDate] date NULL,
        [SourceStatus] nvarchar(30) NULL,
        [IsDeleted] bit NOT NULL
            CONSTRAINT [DF_Reporting_FinancialStatement_IsDeleted] DEFAULT (0),
        [FirstObservedAtUtc] datetime2(3) NOT NULL,
        [LastObservedAtUtc] datetime2(3) NOT NULL,
        [LastDetailRetrievedAtUtc] datetime2(3) NOT NULL,
        [CurrentPayloadVersionId] bigint NULL,
        [CreatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reporting_FinancialStatement_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reporting_FinancialStatement_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Reporting_FinancialStatement]
            PRIMARY KEY CLUSTERED ([RegisterUzStatementId]),
        CONSTRAINT [FK_Reporting_FinancialStatement_Entity]
            FOREIGN KEY ([RegisterUzEntityId])
            REFERENCES [Registry].[AccountingEntity] ([RegisterUzEntityId]),
        CONSTRAINT [CK_Reporting_FinancialStatement_Id]
            CHECK ([RegisterUzStatementId] > 0),
        CONSTRAINT [CK_Reporting_FinancialStatement_Period]
            CHECK
            (
                ([PeriodFrom] IS NULL OR [PeriodFrom] LIKE '[12][0-9][0-9][0-9]-[01][0-9]')
                AND ([PeriodTo] IS NULL OR [PeriodTo] LIKE '[12][0-9][0-9][0-9]-[01][0-9]')
            ),
        CONSTRAINT [CK_Reporting_FinancialStatement_ObservationOrder]
            CHECK
            (
                [LastObservedAtUtc] >= [FirstObservedAtUtc]
                AND [LastDetailRetrievedAtUtc] >= [FirstObservedAtUtc]
            )
    );
END;
GO

IF OBJECT_ID(N'[Reporting].[AnnualReport]', N'U') IS NULL
BEGIN
    CREATE TABLE [Reporting].[AnnualReport]
    (
        [RegisterUzAnnualReportId] bigint NOT NULL,
        [RegisterUzEntityId] bigint NOT NULL,
        [EntityNameAtSubmission] nvarchar(500) NULL,
        [AnnualReportType] nvarchar(100) NULL,
        [FundName] nvarchar(500) NULL,
        [LeiCode] varchar(20) NULL,
        [PeriodFrom] char(7) NULL,
        [PeriodTo] char(7) NULL,
        [SubmissionDate] date NULL,
        [AssemblyDate] date NULL,
        [DataAvailability] nvarchar(30) NULL,
        [DataSourceCode] varchar(30) NULL,
        [SourceLastModifiedDate] date NULL,
        [SourceStatus] nvarchar(30) NULL,
        [IsDeleted] bit NOT NULL
            CONSTRAINT [DF_Reporting_AnnualReport_IsDeleted] DEFAULT (0),
        [FirstObservedAtUtc] datetime2(3) NOT NULL,
        [LastObservedAtUtc] datetime2(3) NOT NULL,
        [LastDetailRetrievedAtUtc] datetime2(3) NOT NULL,
        [CurrentPayloadVersionId] bigint NULL,
        [CreatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reporting_AnnualReport_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reporting_AnnualReport_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Reporting_AnnualReport]
            PRIMARY KEY CLUSTERED ([RegisterUzAnnualReportId]),
        CONSTRAINT [FK_Reporting_AnnualReport_Entity]
            FOREIGN KEY ([RegisterUzEntityId])
            REFERENCES [Registry].[AccountingEntity] ([RegisterUzEntityId]),
        CONSTRAINT [CK_Reporting_AnnualReport_Id]
            CHECK ([RegisterUzAnnualReportId] > 0),
        CONSTRAINT [CK_Reporting_AnnualReport_Period]
            CHECK
            (
                ([PeriodFrom] IS NULL OR [PeriodFrom] LIKE '[12][0-9][0-9][0-9]-[01][0-9]')
                AND ([PeriodTo] IS NULL OR [PeriodTo] LIKE '[12][0-9][0-9][0-9]-[01][0-9]')
            ),
        CONSTRAINT [CK_Reporting_AnnualReport_ObservationOrder]
            CHECK
            (
                [LastObservedAtUtc] >= [FirstObservedAtUtc]
                AND [LastDetailRetrievedAtUtc] >= [FirstObservedAtUtc]
            )
    );
END;
GO

IF OBJECT_ID(N'[Reporting].[FinancialReport]', N'U') IS NULL
BEGIN
    CREATE TABLE [Reporting].[FinancialReport]
    (
        [RegisterUzFinancialReportId] bigint NOT NULL,
        [RegisterUzStatementId] bigint NULL,
        [RegisterUzAnnualReportId] bigint NULL,
        [RegisterUzTemplateId] bigint NULL,
        [CurrencyCode] varchar(9) NULL,
        [TaxOfficeCode] varchar(3) NULL,
        [DataAvailability] nvarchar(30) NULL,
        [DataSourceCode] varchar(30) NULL,
        [SourceLastModifiedDate] date NULL,
        [SourceStatus] nvarchar(30) NULL,
        [IsDeleted] bit NOT NULL
            CONSTRAINT [DF_Reporting_FinancialReport_IsDeleted] DEFAULT (0),
        [FirstObservedAtUtc] datetime2(3) NOT NULL,
        [LastObservedAtUtc] datetime2(3) NOT NULL,
        [LastDetailRetrievedAtUtc] datetime2(3) NOT NULL,
        [CurrentPayloadVersionId] bigint NULL,
        [CreatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reporting_FinancialReport_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Reporting_FinancialReport_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Reporting_FinancialReport]
            PRIMARY KEY CLUSTERED ([RegisterUzFinancialReportId]),
        CONSTRAINT [FK_Reporting_FinancialReport_Statement]
            FOREIGN KEY ([RegisterUzStatementId])
            REFERENCES [Reporting].[FinancialStatement] ([RegisterUzStatementId]),
        CONSTRAINT [FK_Reporting_FinancialReport_AnnualReport]
            FOREIGN KEY ([RegisterUzAnnualReportId])
            REFERENCES [Reporting].[AnnualReport] ([RegisterUzAnnualReportId]),
        CONSTRAINT [FK_Reporting_FinancialReport_Template]
            FOREIGN KEY ([RegisterUzTemplateId])
            REFERENCES [Templates].[FinancialReportTemplate] ([RegisterUzTemplateId]),
        CONSTRAINT [CK_Reporting_FinancialReport_Id]
            CHECK ([RegisterUzFinancialReportId] > 0),
        CONSTRAINT [CK_Reporting_FinancialReport_ExactlyOneParent]
            CHECK
            (
                ([RegisterUzStatementId] IS NOT NULL AND [RegisterUzAnnualReportId] IS NULL)
                OR
                ([RegisterUzStatementId] IS NULL AND [RegisterUzAnnualReportId] IS NOT NULL)
            ),
        CONSTRAINT [CK_Reporting_FinancialReport_ObservationOrder]
            CHECK
            (
                [LastObservedAtUtc] >= [FirstObservedAtUtc]
                AND [LastDetailRetrievedAtUtc] >= [FirstObservedAtUtc]
            )
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Reporting].[FinancialStatement]')
      AND [name] = N'IX_Reporting_FinancialStatement_Entity_Period'
)
    CREATE INDEX [IX_Reporting_FinancialStatement_Entity_Period]
        ON [Reporting].[FinancialStatement]
           ([RegisterUzEntityId], [PeriodTo], [RegisterUzStatementId]);
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Reporting].[AnnualReport]')
      AND [name] = N'IX_Reporting_AnnualReport_Entity_Period'
)
    CREATE INDEX [IX_Reporting_AnnualReport_Entity_Period]
        ON [Reporting].[AnnualReport]
           ([RegisterUzEntityId], [PeriodTo], [RegisterUzAnnualReportId]);
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Reporting].[FinancialReport]')
      AND [name] = N'IX_Reporting_FinancialReport_Statement'
)
    CREATE INDEX [IX_Reporting_FinancialReport_Statement]
        ON [Reporting].[FinancialReport]
           ([RegisterUzStatementId], [RegisterUzFinancialReportId])
        WHERE [RegisterUzStatementId] IS NOT NULL;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Reporting].[FinancialReport]')
      AND [name] = N'IX_Reporting_FinancialReport_AnnualReport'
)
    CREATE INDEX [IX_Reporting_FinancialReport_AnnualReport]
        ON [Reporting].[FinancialReport]
           ([RegisterUzAnnualReportId], [RegisterUzFinancialReportId])
        WHERE [RegisterUzAnnualReportId] IS NOT NULL;
GO

IF OBJECT_ID(N'[Reporting].[FinancialReportTitlePage]', N'U') IS NULL
BEGIN
    CREATE TABLE [Reporting].[FinancialReportTitlePage]
    (
        [RegisterUzFinancialReportId] bigint NOT NULL,
        [EntityName] nvarchar(500) NULL,
        [Ico] varchar(20) NULL,
        [Dic] varchar(20) NULL,
        [Sid] varchar(20) NULL,
        [Address] nvarchar(max) NULL,
        [LegalFormCode] varchar(100) NULL,
        [SkNaceCode] varchar(100) NULL,
        [ReportType] nvarchar(100) NULL,
        [IsConsolidated] bit NULL,
        [IsConsolidatedCentralGovernment] bit NULL,
        [IsSummaryPublicAdministration] bit NULL,
        [EntityType] nvarchar(100) NULL,
        [CommercialRegister] nvarchar(max) NULL,
        [FundName] nvarchar(500) NULL,
        [LeiCode] varchar(20) NULL,
        [PeriodFrom] char(7) NULL,
        [PeriodTo] char(7) NULL,
        [PreviousPeriodFrom] char(7) NULL,
        [PreviousPeriodTo] char(7) NULL,
        [CompletionDate] date NULL,
        [ApprovalDate] date NULL,
        [PreparationDate] date NULL,
        [AssemblyDate] date NULL,
        [AuditorReportAttachmentDate] date NULL,
        CONSTRAINT [PK_Reporting_FinancialReportTitlePage]
            PRIMARY KEY CLUSTERED ([RegisterUzFinancialReportId]),
        CONSTRAINT [FK_Reporting_FinancialReportTitlePage_Report]
            FOREIGN KEY ([RegisterUzFinancialReportId])
            REFERENCES [Reporting].[FinancialReport] ([RegisterUzFinancialReportId])
            ON DELETE CASCADE
    );
END;
GO

IF OBJECT_ID(N'[Reporting].[FinancialReportTable]', N'U') IS NULL
BEGIN
    CREATE TABLE [Reporting].[FinancialReportTable]
    (
        [FinancialReportTableId] bigint IDENTITY(1,1) NOT NULL,
        [RegisterUzFinancialReportId] bigint NOT NULL,
        [TemplateTableId] bigint NULL,
        /* Zero-based position of this table in the API's obsah.tabulky array. */
        [TableOrdinal] int NOT NULL,
        [NameSk] nvarchar(250) NULL,
        [NameEn] nvarchar(250) NULL,
        CONSTRAINT [PK_Reporting_FinancialReportTable]
            PRIMARY KEY CLUSTERED ([FinancialReportTableId]),
        CONSTRAINT [FK_Reporting_FinancialReportTable_Report]
            FOREIGN KEY ([RegisterUzFinancialReportId])
            REFERENCES [Reporting].[FinancialReport] ([RegisterUzFinancialReportId])
            ON DELETE CASCADE,
        CONSTRAINT [FK_Reporting_FinancialReportTable_TemplateTable]
            FOREIGN KEY ([TemplateTableId])
            REFERENCES [Templates].[TemplateTable] ([TemplateTableId]),
        CONSTRAINT [UQ_Reporting_FinancialReportTable_Report_Ordinal]
            UNIQUE ([RegisterUzFinancialReportId], [TableOrdinal]),
        CONSTRAINT [CK_Reporting_FinancialReportTable_Ordinal]
            CHECK ([TableOrdinal] >= 0)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Reporting].[FinancialReportTable]')
      AND [name] = N'UX_Reporting_FinancialReportTable_Report_TemplateTable'
)
    CREATE UNIQUE INDEX [UX_Reporting_FinancialReportTable_Report_TemplateTable]
        ON [Reporting].[FinancialReportTable]
           ([RegisterUzFinancialReportId], [TemplateTableId])
        WHERE [TemplateTableId] IS NOT NULL;
GO

IF OBJECT_ID(N'[Reporting].[FinancialReportValue]', N'U') IS NULL
BEGIN
    CREATE TABLE [Reporting].[FinancialReportValue]
    (
        [FinancialReportTableId] bigint NOT NULL,
        /* Zero-based position in the API's flattened table data array. */
        [ValueOrdinal] int NOT NULL,
        /* Derived physical positions; neither is a displayed report row number. */
        [RowOrdinal] int NOT NULL,
        [DataColumnOrdinal] int NOT NULL,
        [NumericValue] decimal(38, 10) NULL,
        [SourceValue] nvarchar(100) NULL,
        CONSTRAINT [PK_Reporting_FinancialReportValue]
            PRIMARY KEY CLUSTERED ([FinancialReportTableId], [ValueOrdinal]),
        CONSTRAINT [FK_Reporting_FinancialReportValue_Table]
            FOREIGN KEY ([FinancialReportTableId])
            REFERENCES [Reporting].[FinancialReportTable] ([FinancialReportTableId])
            ON DELETE CASCADE,
        CONSTRAINT [UQ_Reporting_FinancialReportValue_Coordinate]
            UNIQUE ([FinancialReportTableId], [RowOrdinal], [DataColumnOrdinal]),
        CONSTRAINT [CK_Reporting_FinancialReportValue_Ordinals]
            CHECK
            (
                [ValueOrdinal] >= 0
                AND [RowOrdinal] >= 0
                AND [DataColumnOrdinal] >= 0
            )
    );
END;
GO

IF OBJECT_ID(N'[Reporting].[FinancialReportAttachment]', N'U') IS NULL
BEGIN
    CREATE TABLE [Reporting].[FinancialReportAttachment]
    (
        [RegisterUzAttachmentId] bigint NOT NULL,
        [RegisterUzFinancialReportId] bigint NOT NULL,
        [FileName] nvarchar(255) NULL,
        [MimeType] varchar(100) NULL,
        [FileSizeBytes] bigint NULL,
        [PageCount] int NULL,
        [DigestSha256] binary(32) NULL,
        [LanguageCode] varchar(10) NULL,
        [FirstObservedAtUtc] datetime2(3) NOT NULL,
        [LastObservedAtUtc] datetime2(3) NOT NULL,
        CONSTRAINT [PK_Reporting_FinancialReportAttachment]
            PRIMARY KEY CLUSTERED ([RegisterUzAttachmentId]),
        CONSTRAINT [FK_Reporting_FinancialReportAttachment_Report]
            FOREIGN KEY ([RegisterUzFinancialReportId])
            REFERENCES [Reporting].[FinancialReport] ([RegisterUzFinancialReportId])
            ON DELETE CASCADE,
        CONSTRAINT [CK_Reporting_FinancialReportAttachment_Values]
            CHECK
            (
                [RegisterUzAttachmentId] > 0
                AND ([FileSizeBytes] IS NULL OR [FileSizeBytes] >= 0)
                AND ([PageCount] IS NULL OR [PageCount] >= 0)
                AND [LastObservedAtUtc] >= [FirstObservedAtUtc]
            )
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Reporting].[FinancialReportAttachment]')
      AND [name] = N'IX_Reporting_FinancialReportAttachment_Report'
)
    CREATE INDEX [IX_Reporting_FinancialReportAttachment_Report]
        ON [Reporting].[FinancialReportAttachment]
           ([RegisterUzFinancialReportId], [RegisterUzAttachmentId]);
GO

IF OBJECT_ID(N'[Reporting].[AnnualReportAttachment]', N'U') IS NULL
BEGIN
    CREATE TABLE [Reporting].[AnnualReportAttachment]
    (
        [RegisterUzAttachmentId] bigint NOT NULL,
        [RegisterUzAnnualReportId] bigint NOT NULL,
        [FileName] nvarchar(255) NULL,
        [MimeType] varchar(100) NULL,
        [FileSizeBytes] bigint NULL,
        [DigestSha256] binary(32) NULL,
        [LanguageCode] varchar(10) NULL,
        [FirstObservedAtUtc] datetime2(3) NOT NULL,
        [LastObservedAtUtc] datetime2(3) NOT NULL,
        CONSTRAINT [PK_Reporting_AnnualReportAttachment]
            PRIMARY KEY CLUSTERED ([RegisterUzAttachmentId]),
        CONSTRAINT [FK_Reporting_AnnualReportAttachment_Report]
            FOREIGN KEY ([RegisterUzAnnualReportId])
            REFERENCES [Reporting].[AnnualReport] ([RegisterUzAnnualReportId])
            ON DELETE CASCADE,
        CONSTRAINT [CK_Reporting_AnnualReportAttachment_Values]
            CHECK
            (
                [RegisterUzAttachmentId] > 0
                AND ([FileSizeBytes] IS NULL OR [FileSizeBytes] >= 0)
                AND [LastObservedAtUtc] >= [FirstObservedAtUtc]
            )
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Reporting].[AnnualReportAttachment]')
      AND [name] = N'IX_Reporting_AnnualReportAttachment_Report'
)
    CREATE INDEX [IX_Reporting_AnnualReportAttachment_Report]
        ON [Reporting].[AnnualReportAttachment]
           ([RegisterUzAnnualReportId], [RegisterUzAttachmentId]);
GO

/* -------------------------------------------------------------------------
   Synchronization execution and observation data
   ------------------------------------------------------------------------- */

IF OBJECT_ID(N'[Sync].[Run]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sync].[Run]
    (
        [SyncRunId] bigint IDENTITY(1,1) NOT NULL,
        [RunType] varchar(30) NOT NULL,
        [RequestedBy] nvarchar(200) NULL,
        [StartedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Sync_Run_StartedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [CompletedAtUtc] datetime2(3) NULL,
        [Status] varchar(20) NOT NULL
            CONSTRAINT [DF_Sync_Run_Status] DEFAULT ('Running'),
        [ObservedIdCount] bigint NOT NULL
            CONSTRAINT [DF_Sync_Run_ObservedIdCount] DEFAULT (0),
        [DetailRequestCount] bigint NOT NULL
            CONSTRAINT [DF_Sync_Run_DetailRequestCount] DEFAULT (0),
        [InsertedObjectCount] bigint NOT NULL
            CONSTRAINT [DF_Sync_Run_InsertedObjectCount] DEFAULT (0),
        [UpdatedObjectCount] bigint NOT NULL
            CONSTRAINT [DF_Sync_Run_UpdatedObjectCount] DEFAULT (0),
        [UnchangedObjectCount] bigint NOT NULL
            CONSTRAINT [DF_Sync_Run_UnchangedObjectCount] DEFAULT (0),
        [DeletedObjectCount] bigint NOT NULL
            CONSTRAINT [DF_Sync_Run_DeletedObjectCount] DEFAULT (0),
        [ErrorCount] bigint NOT NULL
            CONSTRAINT [DF_Sync_Run_ErrorCount] DEFAULT (0),
        [CatalogObservationCount] bigint NOT NULL
            CONSTRAINT [DF_Sync_Run_CatalogObservationCount] DEFAULT (0),
        [CatalogInsertedCount] bigint NOT NULL
            CONSTRAINT [DF_Sync_Run_CatalogInsertedCount] DEFAULT (0),
        [CatalogUpdatedCount] bigint NOT NULL
            CONSTRAINT [DF_Sync_Run_CatalogUpdatedCount] DEFAULT (0),
        [CatalogRemovedCount] bigint NOT NULL
            CONSTRAINT [DF_Sync_Run_CatalogRemovedCount] DEFAULT (0),
        [CatalogReviewRequiredCount] bigint NOT NULL
            CONSTRAINT [DF_Sync_Run_CatalogReviewRequiredCount] DEFAULT (0),
        [Notes] nvarchar(max) NULL,
        CONSTRAINT [PK_Sync_Run] PRIMARY KEY CLUSTERED ([SyncRunId]),
        CONSTRAINT [CK_Sync_Run_Status]
            CHECK ([Status] IN ('Running', 'Completed', 'CompletedWithErrors', 'Failed', 'Cancelled')),
        CONSTRAINT [CK_Sync_Run_TimeOrder]
            CHECK ([CompletedAtUtc] IS NULL OR [CompletedAtUtc] >= [StartedAtUtc]),
        CONSTRAINT [CK_Sync_Run_Counts]
            CHECK
            (
                [ObservedIdCount] >= 0 AND [DetailRequestCount] >= 0
                AND [InsertedObjectCount] >= 0 AND [UpdatedObjectCount] >= 0
                AND [UnchangedObjectCount] >= 0 AND [DeletedObjectCount] >= 0
                AND [ErrorCount] >= 0 AND [CatalogObservationCount] >= 0
                AND [CatalogInsertedCount] >= 0 AND [CatalogUpdatedCount] >= 0
                AND [CatalogRemovedCount] >= 0 AND [CatalogReviewRequiredCount] >= 0
            )
    );
END;
GO

/* The following ALTER blocks make the V2 script safely rerunnable against a
   V2 database created before catalog synchronization was introduced. */
IF COL_LENGTH(N'Sync.Run', N'CatalogObservationCount') IS NULL
    ALTER TABLE [Sync].[Run] ADD [CatalogObservationCount] bigint NOT NULL
        CONSTRAINT [DF_Sync_Run_CatalogObservationCount] DEFAULT (0) WITH VALUES;
IF COL_LENGTH(N'Sync.Run', N'CatalogInsertedCount') IS NULL
    ALTER TABLE [Sync].[Run] ADD [CatalogInsertedCount] bigint NOT NULL
        CONSTRAINT [DF_Sync_Run_CatalogInsertedCount] DEFAULT (0) WITH VALUES;
IF COL_LENGTH(N'Sync.Run', N'CatalogUpdatedCount') IS NULL
    ALTER TABLE [Sync].[Run] ADD [CatalogUpdatedCount] bigint NOT NULL
        CONSTRAINT [DF_Sync_Run_CatalogUpdatedCount] DEFAULT (0) WITH VALUES;
IF COL_LENGTH(N'Sync.Run', N'CatalogRemovedCount') IS NULL
    ALTER TABLE [Sync].[Run] ADD [CatalogRemovedCount] bigint NOT NULL
        CONSTRAINT [DF_Sync_Run_CatalogRemovedCount] DEFAULT (0) WITH VALUES;
IF COL_LENGTH(N'Sync.Run', N'CatalogReviewRequiredCount') IS NULL
    ALTER TABLE [Sync].[Run] ADD [CatalogReviewRequiredCount] bigint NOT NULL
        CONSTRAINT [DF_Sync_Run_CatalogReviewRequiredCount] DEFAULT (0) WITH VALUES;
GO

IF OBJECT_ID(N'[Sync].[CatalogObservation]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sync].[CatalogObservation]
    (
        [CatalogObservationId] bigint IDENTITY(1,1) NOT NULL,
        [SyncRunId] bigint NOT NULL,
        [CatalogCode] varchar(40) NOT NULL,
        [RetrievedAtUtc] datetime2(3) NOT NULL,
        [HttpStatusCode] int NOT NULL,
        [RecordCount] int NOT NULL,
        [PayloadSha256] binary(32) NOT NULL,
        [CanonicalSha256] binary(32) NOT NULL,
        [PayloadCompressed] varbinary(max) NOT NULL,
        [CompressionCode] varchar(10) NOT NULL
            CONSTRAINT [DF_Sync_CatalogObservation_CompressionCode] DEFAULT ('GZIP'),
        [UncompressedLengthBytes] bigint NOT NULL,
        [ApiVersion] varchar(50) NULL,
        [HasChanged] bit NOT NULL,
        CONSTRAINT [PK_Sync_CatalogObservation]
            PRIMARY KEY CLUSTERED ([CatalogObservationId]),
        CONSTRAINT [FK_Sync_CatalogObservation_Run]
            FOREIGN KEY ([SyncRunId]) REFERENCES [Sync].[Run] ([SyncRunId]),
        CONSTRAINT [UQ_Sync_CatalogObservation_Run_Catalog]
            UNIQUE ([SyncRunId], [CatalogCode]),
        CONSTRAINT [CK_Sync_CatalogObservation_Values]
            CHECK
            (
                [RecordCount] >= 0 AND [UncompressedLengthBytes] >= 0
                AND [HttpStatusCode] BETWEEN 100 AND 599
                AND [CompressionCode] IN ('GZIP', 'NONE')
            )
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Sync].[CatalogObservation]')
      AND [name] = N'IX_Sync_CatalogObservation_Catalog_Latest'
)
    CREATE INDEX [IX_Sync_CatalogObservation_Catalog_Latest]
        ON [Sync].[CatalogObservation] ([CatalogCode], [CatalogObservationId] DESC)
        INCLUDE ([SyncRunId], [RetrievedAtUtc], [CanonicalSha256], [RecordCount], [HasChanged]);
GO

IF OBJECT_ID(N'[Sync].[CatalogItemState]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sync].[CatalogItemState]
    (
        [CatalogCode] varchar(40) NOT NULL,
        [SourceObjectKey] varchar(100) NOT NULL,
        [CanonicalSha256] binary(32) NOT NULL,
        [FirstObservedAtUtc] datetime2(3) NOT NULL,
        [LastObservedAtUtc] datetime2(3) NOT NULL,
        [FirstObservedInRunId] bigint NOT NULL,
        [LastObservedInRunId] bigint NOT NULL,
        [IsPresent] bit NOT NULL,
        CONSTRAINT [PK_Sync_CatalogItemState]
            PRIMARY KEY CLUSTERED ([CatalogCode], [SourceObjectKey]),
        CONSTRAINT [FK_Sync_CatalogItemState_FirstRun]
            FOREIGN KEY ([FirstObservedInRunId]) REFERENCES [Sync].[Run] ([SyncRunId]),
        CONSTRAINT [FK_Sync_CatalogItemState_LastRun]
            FOREIGN KEY ([LastObservedInRunId]) REFERENCES [Sync].[Run] ([SyncRunId]),
        CONSTRAINT [CK_Sync_CatalogItemState_TimeOrder]
            CHECK ([LastObservedAtUtc] >= [FirstObservedAtUtc])
    );
END;
GO

IF OBJECT_ID(N'[Sync].[CatalogChange]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sync].[CatalogChange]
    (
        [CatalogChangeId] bigint IDENTITY(1,1) NOT NULL,
        [SyncRunId] bigint NOT NULL,
        [CatalogCode] varchar(40) NOT NULL,
        [SourceObjectKey] varchar(100) NOT NULL,
        [ChangeType] varchar(20) NOT NULL,
        [ChangeScope] varchar(20) NOT NULL,
        [OldCanonicalSha256] binary(32) NULL,
        [NewCanonicalSha256] binary(32) NULL,
        [ChangeDescription] nvarchar(max) NULL,
        [RequiresReview] bit NOT NULL,
        [ReviewedAtUtc] datetime2(3) NULL,
        [ReviewedBy] nvarchar(200) NULL,
        [PublishedToAuditAddInAtUtc] datetime2(3) NULL,
        [PublishedBy] nvarchar(200) NULL,
        CONSTRAINT [PK_Sync_CatalogChange]
            PRIMARY KEY CLUSTERED ([CatalogChangeId]),
        CONSTRAINT [FK_Sync_CatalogChange_Run]
            FOREIGN KEY ([SyncRunId]) REFERENCES [Sync].[Run] ([SyncRunId]),
        CONSTRAINT [UQ_Sync_CatalogChange_Run_Item]
            UNIQUE ([SyncRunId], [CatalogCode], [SourceObjectKey]),
        CONSTRAINT [CK_Sync_CatalogChange_Type]
            CHECK ([ChangeType] IN ('Inserted', 'Updated', 'Removed', 'Reappeared')),
        CONSTRAINT [CK_Sync_CatalogChange_Scope]
            CHECK ([ChangeScope] IN ('Metadata', 'Caption', 'Structure')),
        CONSTRAINT [CK_Sync_CatalogChange_Review]
            CHECK
            (
                ([ReviewedAtUtc] IS NULL AND [ReviewedBy] IS NULL)
                OR ([ReviewedAtUtc] IS NOT NULL AND [ReviewedBy] IS NOT NULL)
            ),
        CONSTRAINT [CK_Sync_CatalogChange_Publication]
            CHECK
            (
                ([PublishedToAuditAddInAtUtc] IS NULL AND [PublishedBy] IS NULL)
                OR ([PublishedToAuditAddInAtUtc] IS NOT NULL AND [PublishedBy] IS NOT NULL)
            )
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Sync].[CatalogChange]')
      AND [name] = N'IX_Sync_CatalogChange_PendingReview'
)
    CREATE INDEX [IX_Sync_CatalogChange_PendingReview]
        ON [Sync].[CatalogChange] ([RequiresReview], [ReviewedAtUtc], [SyncRunId])
        INCLUDE ([CatalogCode], [SourceObjectKey], [ChangeType], [ChangeScope])
        WHERE [RequiresReview] = 1 AND [ReviewedAtUtc] IS NULL;
GO

IF OBJECT_ID(N'[Sync].[LoadTarget]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sync].[LoadTarget]
    (
        [LoadTargetId] bigint IDENTITY(1,1) NOT NULL,
        [Ico] varchar(20) NOT NULL,
        [IsEnabled] bit NOT NULL
            CONSTRAINT [DF_Sync_LoadTarget_IsEnabled] DEFAULT (1),
        [RequestedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Sync_LoadTarget_RequestedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [RequestedBy] nvarchar(200) NULL,
        [LastAttemptAtUtc] datetime2(3) NULL,
        [LastSuccessfulLoadAtUtc] datetime2(3) NULL,
        [LastStatus] varchar(30) NULL,
        [LastError] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Sync_LoadTarget_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Sync_LoadTarget_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Sync_LoadTarget] PRIMARY KEY CLUSTERED ([LoadTargetId]),
        CONSTRAINT [UQ_Sync_LoadTarget_Ico] UNIQUE ([Ico])
    );
END;
GO

IF OBJECT_ID(N'[Sync].[ObservedObject]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sync].[ObservedObject]
    (
        [ObjectTypeId] tinyint NOT NULL,
        [RegisterUzObjectId] bigint NOT NULL,
        [FirstObservedAtUtc] datetime2(3) NOT NULL,
        [LastObservedAtUtc] datetime2(3) NOT NULL,
        [FirstObservedInRunId] bigint NULL,
        [LastObservedInRunId] bigint NULL,
        [ObservationCount] bigint NOT NULL
            CONSTRAINT [DF_Sync_ObservedObject_ObservationCount] DEFAULT (1),
        [LastDetailAttemptAtUtc] datetime2(3) NULL,
        [LastDetailRetrievedAtUtc] datetime2(3) NULL,
        [LastDetailStatus] varchar(30) NULL,
        [SourceLastModifiedDate] date NULL,
        [IsDeleted] bit NOT NULL
            CONSTRAINT [DF_Sync_ObservedObject_IsDeleted] DEFAULT (0),
        CONSTRAINT [PK_Sync_ObservedObject]
            PRIMARY KEY CLUSTERED ([ObjectTypeId], [RegisterUzObjectId]),
        CONSTRAINT [FK_Sync_ObservedObject_ObjectType]
            FOREIGN KEY ([ObjectTypeId]) REFERENCES [Sync].[ObjectType] ([ObjectTypeId]),
        CONSTRAINT [FK_Sync_ObservedObject_FirstRun]
            FOREIGN KEY ([FirstObservedInRunId]) REFERENCES [Sync].[Run] ([SyncRunId]),
        CONSTRAINT [FK_Sync_ObservedObject_LastRun]
            FOREIGN KEY ([LastObservedInRunId]) REFERENCES [Sync].[Run] ([SyncRunId]),
        CONSTRAINT [CK_Sync_ObservedObject_Values]
            CHECK
            (
                [RegisterUzObjectId] > 0
                AND [LastObservedAtUtc] >= [FirstObservedAtUtc]
                AND [ObservationCount] >= 1
            )
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Sync].[ObservedObject]')
      AND [name] = N'IX_Sync_ObservedObject_DetailPending'
)
    CREATE INDEX [IX_Sync_ObservedObject_DetailPending]
        ON [Sync].[ObservedObject]
           ([ObjectTypeId], [LastDetailRetrievedAtUtc], [LastObservedAtUtc])
        INCLUDE ([RegisterUzObjectId], [SourceLastModifiedDate], [IsDeleted]);
GO

IF OBJECT_ID(N'[Sync].[ChangeFeedCheckpoint]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sync].[ChangeFeedCheckpoint]
    (
        [ObjectTypeId] tinyint NOT NULL,
        [WindowFromDate] date NOT NULL,
        [WindowToDate] date NOT NULL,
        [ContinueAfterId] bigint NULL,
        [Status] varchar(20) NOT NULL
            CONSTRAINT [DF_Sync_ChangeFeedCheckpoint_Status] DEFAULT ('Pending'),
        [LastRunId] bigint NULL,
        [LastPageRetrievedAtUtc] datetime2(3) NULL,
        [CompletedAtUtc] datetime2(3) NULL,
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Sync_ChangeFeedCheckpoint_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Sync_ChangeFeedCheckpoint]
            PRIMARY KEY CLUSTERED ([ObjectTypeId]),
        CONSTRAINT [FK_Sync_ChangeFeedCheckpoint_ObjectType]
            FOREIGN KEY ([ObjectTypeId]) REFERENCES [Sync].[ObjectType] ([ObjectTypeId]),
        CONSTRAINT [FK_Sync_ChangeFeedCheckpoint_Run]
            FOREIGN KEY ([LastRunId]) REFERENCES [Sync].[Run] ([SyncRunId]),
        CONSTRAINT [CK_Sync_ChangeFeedCheckpoint_Window]
            CHECK ([WindowToDate] >= [WindowFromDate]),
        CONSTRAINT [CK_Sync_ChangeFeedCheckpoint_Status]
            CHECK ([Status] IN ('Pending', 'Running', 'Completed', 'Failed')),
        CONSTRAINT [CK_Sync_ChangeFeedCheckpoint_ContinueAfterId]
            CHECK ([ContinueAfterId] IS NULL OR [ContinueAfterId] > 0)
    );
END;
GO

IF OBJECT_ID(N'[Sync].[Request]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sync].[Request]
    (
        [SyncRequestId] bigint IDENTITY(1,1) NOT NULL,
        [SyncRunId] bigint NOT NULL,
        [ObjectTypeId] tinyint NULL,
        [RequestKind] varchar(20) NOT NULL,
        [RegisterUzObjectId] bigint NULL,
        [RequestUri] nvarchar(2000) NOT NULL,
        [RequestedAtUtc] datetime2(3) NOT NULL,
        [CompletedAtUtc] datetime2(3) NULL,
        [HttpStatusCode] int NULL,
        [ApiVersion] varchar(50) NULL,
        [ResponseBytes] bigint NULL,
        [AttemptNumber] int NOT NULL
            CONSTRAINT [DF_Sync_Request_AttemptNumber] DEFAULT (1),
        [Succeeded] bit NULL,
        [ErrorMessage] nvarchar(max) NULL,
        CONSTRAINT [PK_Sync_Request] PRIMARY KEY CLUSTERED ([SyncRequestId]),
        CONSTRAINT [FK_Sync_Request_Run]
            FOREIGN KEY ([SyncRunId]) REFERENCES [Sync].[Run] ([SyncRunId]),
        CONSTRAINT [FK_Sync_Request_ObjectType]
            FOREIGN KEY ([ObjectTypeId]) REFERENCES [Sync].[ObjectType] ([ObjectTypeId]),
        CONSTRAINT [CK_Sync_Request_Values]
            CHECK
            (
                [AttemptNumber] >= 1
                AND ([RegisterUzObjectId] IS NULL OR [RegisterUzObjectId] > 0)
                AND ([HttpStatusCode] IS NULL OR [HttpStatusCode] BETWEEN 100 AND 599)
                AND ([ResponseBytes] IS NULL OR [ResponseBytes] >= 0)
                AND ([CompletedAtUtc] IS NULL OR [CompletedAtUtc] >= [RequestedAtUtc])
            )
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Sync].[Request]')
      AND [name] = N'IX_Sync_Request_Run'
)
    CREATE INDEX [IX_Sync_Request_Run]
        ON [Sync].[Request] ([SyncRunId], [SyncRequestId]);
GO

IF OBJECT_ID(N'[Sync].[Error]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sync].[Error]
    (
        [SyncErrorId] bigint IDENTITY(1,1) NOT NULL,
        [SyncRunId] bigint NULL,
        [SyncRequestId] bigint NULL,
        [ObjectTypeId] tinyint NULL,
        [RegisterUzObjectId] bigint NULL,
        [ErrorStage] varchar(30) NOT NULL,
        [ErrorCode] varchar(100) NULL,
        [Message] nvarchar(max) NOT NULL,
        [Details] nvarchar(max) NULL,
        [OccurredAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Sync_Error_OccurredAtUtc] DEFAULT (SYSUTCDATETIME()),
        [ResolvedAtUtc] datetime2(3) NULL,
        [Resolution] nvarchar(max) NULL,
        CONSTRAINT [PK_Sync_Error] PRIMARY KEY CLUSTERED ([SyncErrorId]),
        CONSTRAINT [FK_Sync_Error_Run]
            FOREIGN KEY ([SyncRunId]) REFERENCES [Sync].[Run] ([SyncRunId]),
        CONSTRAINT [FK_Sync_Error_Request]
            FOREIGN KEY ([SyncRequestId]) REFERENCES [Sync].[Request] ([SyncRequestId]),
        CONSTRAINT [FK_Sync_Error_ObjectType]
            FOREIGN KEY ([ObjectTypeId]) REFERENCES [Sync].[ObjectType] ([ObjectTypeId]),
        CONSTRAINT [CK_Sync_Error_ResolutionTime]
            CHECK ([ResolvedAtUtc] IS NULL OR [ResolvedAtUtc] >= [OccurredAtUtc])
    );
END;
GO

/* -------------------------------------------------------------------------
   Immutable raw API payload versions

   PayloadCompressed contains UTF-8 JSON compressed by the application.
   CompressionCode identifies the algorithm, initially GZIP.
   ------------------------------------------------------------------------- */

IF OBJECT_ID(N'[Raw].[PayloadVersion]', N'U') IS NULL
BEGIN
    CREATE TABLE [Raw].[PayloadVersion]
    (
        [PayloadVersionId] bigint IDENTITY(1,1) NOT NULL,
        [ObjectTypeId] tinyint NOT NULL,
        [RegisterUzObjectId] bigint NOT NULL,
        [PayloadSha256] binary(32) NOT NULL,
        [CanonicalSha256] binary(32) NOT NULL,
        [PayloadCompressed] varbinary(max) NOT NULL,
        [CompressionCode] varchar(10) NOT NULL
            CONSTRAINT [DF_Raw_PayloadVersion_CompressionCode] DEFAULT ('GZIP'),
        [UncompressedLengthBytes] bigint NOT NULL,
        [RetrievedAtUtc] datetime2(3) NOT NULL,
        [FirstObservedAtUtc] datetime2(3) NOT NULL,
        [LastObservedAtUtc] datetime2(3) NOT NULL,
        [SourceLastModifiedDate] date NULL,
        [SourceStatus] nvarchar(30) NULL,
        [IsDeleted] bit NOT NULL
            CONSTRAINT [DF_Raw_PayloadVersion_IsDeleted] DEFAULT (0),
        [HttpStatusCode] int NOT NULL,
        [ApiVersion] varchar(50) NULL,
        [SyncRunId] bigint NULL,
        [SyncRequestId] bigint NULL,
        [ValidationStatus] varchar(20) NOT NULL
            CONSTRAINT [DF_Raw_PayloadVersion_ValidationStatus] DEFAULT ('Pending'),
        [ValidationMessage] nvarchar(max) NULL,
        [NormalizedAtUtc] datetime2(3) NULL,
        CONSTRAINT [PK_Raw_PayloadVersion]
            PRIMARY KEY CLUSTERED ([PayloadVersionId]),
        CONSTRAINT [FK_Raw_PayloadVersion_ObjectType]
            FOREIGN KEY ([ObjectTypeId]) REFERENCES [Sync].[ObjectType] ([ObjectTypeId]),
        CONSTRAINT [FK_Raw_PayloadVersion_Run]
            FOREIGN KEY ([SyncRunId]) REFERENCES [Sync].[Run] ([SyncRunId]),
        CONSTRAINT [FK_Raw_PayloadVersion_Request]
            FOREIGN KEY ([SyncRequestId]) REFERENCES [Sync].[Request] ([SyncRequestId]),
        CONSTRAINT [UQ_Raw_PayloadVersion_Object_CanonicalHash]
            UNIQUE ([ObjectTypeId], [RegisterUzObjectId], [CanonicalSha256]),
        CONSTRAINT [CK_Raw_PayloadVersion_Values]
            CHECK
            (
                [RegisterUzObjectId] > 0
                AND [UncompressedLengthBytes] >= 0
                AND [LastObservedAtUtc] >= [FirstObservedAtUtc]
                AND [RetrievedAtUtc] >= [FirstObservedAtUtc]
                AND [HttpStatusCode] BETWEEN 100 AND 599
            ),
        CONSTRAINT [CK_Raw_PayloadVersion_Compression]
            CHECK ([CompressionCode] IN ('GZIP', 'NONE')),
        CONSTRAINT [CK_Raw_PayloadVersion_ValidationStatus]
            CHECK ([ValidationStatus] IN ('Pending', 'Valid', 'Invalid', 'Failed'))
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Raw].[PayloadVersion]')
      AND [name] = N'IX_Raw_PayloadVersion_Object_Latest'
)
    CREATE INDEX [IX_Raw_PayloadVersion_Object_Latest]
        ON [Raw].[PayloadVersion]
           ([ObjectTypeId], [RegisterUzObjectId], [RetrievedAtUtc] DESC)
        INCLUDE
           ([PayloadVersionId], [PayloadSha256], [CanonicalSha256], [SourceLastModifiedDate],
            [IsDeleted], [ValidationStatus], [NormalizedAtUtc]);
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE [object_id] = OBJECT_ID(N'[Raw].[PayloadVersion]')
      AND [name] = N'IX_Raw_PayloadVersion_NormalizationPending'
)
    CREATE INDEX [IX_Raw_PayloadVersion_NormalizationPending]
        ON [Raw].[PayloadVersion]
           ([ValidationStatus], [NormalizedAtUtc], [PayloadVersionId])
        WHERE [ValidationStatus] = 'Valid' AND [NormalizedAtUtc] IS NULL;
GO

/* CurrentPayloadVersionId is intentionally added after Raw.PayloadVersion
   so the circular current-state/history references can be enforced. */

IF NOT EXISTS
(
    SELECT 1 FROM sys.foreign_keys
    WHERE [name] = N'FK_Registry_AccountingEntity_CurrentPayloadVersion'
)
    ALTER TABLE [Registry].[AccountingEntity]
        ADD CONSTRAINT [FK_Registry_AccountingEntity_CurrentPayloadVersion]
        FOREIGN KEY ([CurrentPayloadVersionId])
        REFERENCES [Raw].[PayloadVersion] ([PayloadVersionId]);
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.foreign_keys
    WHERE [name] = N'FK_Reporting_FinancialStatement_CurrentPayloadVersion'
)
    ALTER TABLE [Reporting].[FinancialStatement]
        ADD CONSTRAINT [FK_Reporting_FinancialStatement_CurrentPayloadVersion]
        FOREIGN KEY ([CurrentPayloadVersionId])
        REFERENCES [Raw].[PayloadVersion] ([PayloadVersionId]);
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.foreign_keys
    WHERE [name] = N'FK_Reporting_AnnualReport_CurrentPayloadVersion'
)
    ALTER TABLE [Reporting].[AnnualReport]
        ADD CONSTRAINT [FK_Reporting_AnnualReport_CurrentPayloadVersion]
        FOREIGN KEY ([CurrentPayloadVersionId])
        REFERENCES [Raw].[PayloadVersion] ([PayloadVersionId]);
GO

IF NOT EXISTS
(
    SELECT 1 FROM sys.foreign_keys
    WHERE [name] = N'FK_Reporting_FinancialReport_CurrentPayloadVersion'
)
    ALTER TABLE [Reporting].[FinancialReport]
        ADD CONSTRAINT [FK_Reporting_FinancialReport_CurrentPayloadVersion]
        FOREIGN KEY ([CurrentPayloadVersionId])
        REFERENCES [Raw].[PayloadVersion] ([PayloadVersionId]);
GO

/* -------------------------------------------------------------------------
   Database metadata
   ------------------------------------------------------------------------- */

IF OBJECT_ID(N'[dbo].[DatabaseMetadata]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DatabaseMetadata]
    (
        [MetadataKey] varchar(100) NOT NULL,
        [MetadataValue] nvarchar(max) NOT NULL,
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_DatabaseMetadata_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_DatabaseMetadata] PRIMARY KEY CLUSTERED ([MetadataKey])
    );
END;
GO

IF EXISTS
(
    SELECT 1 FROM [dbo].[DatabaseMetadata]
    WHERE [MetadataKey] = 'SchemaVersion'
)
    UPDATE [dbo].[DatabaseMetadata]
    SET [MetadataValue] = N'2.1', [UpdatedAtUtc] = SYSUTCDATETIME()
    WHERE [MetadataKey] = 'SchemaVersion';
ELSE
    INSERT INTO [dbo].[DatabaseMetadata]
        ([MetadataKey], [MetadataValue])
    VALUES
        ('SchemaVersion', N'2.1');
GO

IF NOT EXISTS
(
    SELECT 1 FROM [dbo].[DatabaseMetadata]
    WHERE [MetadataKey] = 'DatabasePurpose'
)
    INSERT INTO [dbo].[DatabaseMetadata]
        ([MetadataKey], [MetadataValue])
    VALUES
        ('DatabasePurpose', N'RegisterUZ source mirror and selective synchronization store');
GO

PRINT N'RegisterUZ database initialization V2.1 completed successfully.';
GO
