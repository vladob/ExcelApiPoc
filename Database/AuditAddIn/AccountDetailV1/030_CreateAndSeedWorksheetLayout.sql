/*
    Account detail helper, step 3 of 3. The JSON is a rendering contract, not
    executable Excel formulas. Edit this definition by creating a new version;
    the add-in must validate supported schema versions and field keys.
*/
USE [AuditAddIn];
GO

IF OBJECT_ID(N'Settings.Users', N'U') IS NULL
    THROW 53300, 'Run step 010 before step 030.', 1;
GO

IF OBJECT_ID(N'Settings.WorksheetLayouts', N'U') IS NULL
BEGIN
    CREATE TABLE [Settings].[WorksheetLayouts]
    (
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Settings_WorksheetLayouts] PRIMARY KEY,
        [LayoutCode] varchar(50) NOT NULL,
        [OwnerUserId] int NULL, -- NULL: shared layout; otherwise a private copy.
        [DisplayName] nvarchar(200) NOT NULL,
        CONSTRAINT [FK_Settings_WorksheetLayouts_Owner] FOREIGN KEY ([OwnerUserId]) REFERENCES [Settings].[Users] ([Id])
    );
    CREATE UNIQUE INDEX [UX_Settings_WorksheetLayouts_Shared]
        ON [Settings].[WorksheetLayouts] ([LayoutCode]) WHERE [OwnerUserId] IS NULL;
    CREATE UNIQUE INDEX [UX_Settings_WorksheetLayouts_Private]
        ON [Settings].[WorksheetLayouts] ([OwnerUserId], [LayoutCode]) WHERE [OwnerUserId] IS NOT NULL;
END;
GO

IF OBJECT_ID(N'Settings.WorksheetLayoutVersions', N'U') IS NULL
BEGIN
    CREATE TABLE [Settings].[WorksheetLayoutVersions]
    (
        [LayoutId] int NOT NULL,
        [VersionNo] int NOT NULL,
        [DefinitionJson] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2(0) NOT NULL CONSTRAINT [DF_Settings_LayoutVersion_Created] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Settings_WorksheetLayoutVersions] PRIMARY KEY ([LayoutId], [VersionNo]),
        CONSTRAINT [FK_Settings_WorksheetLayoutVersions_Layout] FOREIGN KEY ([LayoutId]) REFERENCES [Settings].[WorksheetLayouts] ([Id]),
        CONSTRAINT [CK_Settings_WorksheetLayoutVersions_Version] CHECK ([VersionNo] > 0),
        CONSTRAINT [CK_Settings_WorksheetLayoutVersions_Json] CHECK (ISJSON([DefinitionJson]) = 1)
    );
END;
GO

IF OBJECT_ID(N'Settings.WorksheetLayoutDefaults', N'U') IS NULL
BEGIN
    CREATE TABLE [Settings].[WorksheetLayoutDefaults]
    (
        [LayoutCode] varchar(50) NOT NULL CONSTRAINT [PK_Settings_WorksheetLayoutDefaults] PRIMARY KEY,
        [LayoutId] int NOT NULL,
        [VersionNo] int NOT NULL,
        CONSTRAINT [FK_Settings_WorksheetLayoutDefaults_Version] FOREIGN KEY ([LayoutId], [VersionNo])
            REFERENCES [Settings].[WorksheetLayoutVersions] ([LayoutId], [VersionNo])
    );
END;
GO

IF OBJECT_ID(N'Settings.UserWorksheetLayouts', N'U') IS NULL
BEGIN
    CREATE TABLE [Settings].[UserWorksheetLayouts]
    (
        [UserId] int NOT NULL,
        [LayoutCode] varchar(50) NOT NULL,
        [LayoutId] int NOT NULL,
        [VersionNo] int NOT NULL,
        CONSTRAINT [PK_Settings_UserWorksheetLayouts] PRIMARY KEY ([UserId], [LayoutCode]),
        CONSTRAINT [FK_Settings_UserWorksheetLayouts_User] FOREIGN KEY ([UserId]) REFERENCES [Settings].[Users] ([Id]),
        CONSTRAINT [FK_Settings_UserWorksheetLayouts_Version] FOREIGN KEY ([LayoutId], [VersionNo])
            REFERENCES [Settings].[WorksheetLayoutVersions] ([LayoutId], [VersionNo])
    );
END;
GO

DECLARE @LayoutId int;
SELECT @LayoutId = [Id] FROM [Settings].[WorksheetLayouts]
WHERE [LayoutCode] = 'ACCOUNT_DETAIL' AND [OwnerUserId] IS NULL;

IF @LayoutId IS NULL
BEGIN
    INSERT [Settings].[WorksheetLayouts] ([LayoutCode], [OwnerUserId], [DisplayName])
    VALUES ('ACCOUNT_DETAIL', NULL, N'Detail syntetického účtu');
    SET @LayoutId = CONVERT(int, SCOPE_IDENTITY());
END;

DECLARE @Definition nvarchar(max) = N'{
  "schemaVersion": 1,
  "sheetKind": "ACCOUNT_DETAIL",
  "navigationCell": "A1",
  "commentRow": 2,
  "countRow": 3,
  "titleRow": 4,
  "detailTable": {
    "headerRow": 5,
    "style": "TableStyleMedium2",
    "columns": [
      { "key": "document", "captionSk": "Doklad", "width": 16 },
      { "key": "syntheticAccount", "captionSk": "Syntetický účet", "width": 17 },
      { "key": "analyticalAccount", "captionSk": "Analytický účet", "width": 17 },
      { "key": "description", "captionSk": "Popis operácie", "width": 48 },
      { "key": "debit", "captionSk": "MD", "width": 18, "format": "amount" },
      { "key": "credit", "captionSk": "Dal", "width": 18, "format": "amount" },
      { "key": "date", "captionSk": "Dátum", "width": 16, "format": "date" },
      { "key": "note", "captionSk": "Poznámka", "width": 40 }
    ]
  },
  "textSection": {
    "placement": "afterDetailTable",
    "gapRows": 2,
    "categoryOrder": [
      "Valuation",
      "ExistenceAndOwnership",
      "CompletenessOfAccounting",
      "AccuratePeriodDetermination"
    ]
  },
  "print": { "orientation": "landscape", "startRow": 4, "repeatHeaderRow": 5 }
}';

IF ISJSON(@Definition) <> 1
    THROW 53301, 'Invalid account detail layout JSON.', 1;

IF NOT EXISTS (SELECT 1 FROM [Settings].[WorksheetLayoutVersions]
               WHERE [LayoutId] = @LayoutId AND [VersionNo] = 1)
    INSERT [Settings].[WorksheetLayoutVersions] ([LayoutId], [VersionNo], [DefinitionJson])
    VALUES (@LayoutId, 1, @Definition);

IF NOT EXISTS (SELECT 1 FROM [Settings].[WorksheetLayoutDefaults] WHERE [LayoutCode] = 'ACCOUNT_DETAIL')
    INSERT [Settings].[WorksheetLayoutDefaults] ([LayoutCode], [LayoutId], [VersionNo])
    VALUES ('ACCOUNT_DETAIL', @LayoutId, 1);

-- A user with no assignment receives the shared default. To give a user a
-- private layout, insert a private WorksheetLayouts row and its new version,
-- then insert/update UserWorksheetLayouts for that UserId and LayoutCode.
GO
