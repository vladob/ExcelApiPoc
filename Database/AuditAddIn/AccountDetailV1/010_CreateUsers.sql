/*
    Account detail helper, step 1 of 3. Run in AuditAddIn.
    Set the test auditor's actual name and email before running. Keys are created
    by SQL Server, never stored in source control. Save the returned keys securely;
    this script does not display existing keys on subsequent runs.
*/
USE [AuditAddIn];
GO

IF SCHEMA_ID(N'Settings') IS NULL EXEC(N'CREATE SCHEMA [Settings]');
GO

IF OBJECT_ID(N'Settings.Users', N'U') IS NULL
BEGIN
    CREATE TABLE [Settings].[Users]
    (
        [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Settings_Users] PRIMARY KEY,
        [DisplayName] nvarchar(256) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [ApiKey] varchar(64) COLLATE Latin1_General_100_BIN2 NOT NULL,
        [IsActive] bit NOT NULL CONSTRAINT [DF_Settings_Users_IsActive] DEFAULT (1),
        [CreatedAtUtc] datetime2(0) NOT NULL CONSTRAINT [DF_Settings_Users_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [UQ_Settings_Users_Email] UNIQUE ([Email]),
        CONSTRAINT [UQ_Settings_Users_ApiKey] UNIQUE ([ApiKey]),
        CONSTRAINT [CK_Settings_Users_ApiKey] CHECK (LEN([ApiKey]) = 64 AND [ApiKey] NOT LIKE '%[^0-9A-F]%')
    );
END;
GO

DECLARE @AuditorDisplayName nvarchar(256) = N''; -- Required: test auditor's name.
DECLARE @AuditorEmail nvarchar(256) = N'';       -- Required: test auditor's email.

IF NULLIF(TRIM(@AuditorDisplayName), N'') IS NULL
    OR NULLIF(TRIM(@AuditorEmail), N'') IS NULL
    THROW 53100, 'Set @AuditorDisplayName and @AuditorEmail before running step 010.', 1;

DECLARE @SeedUsers table ([DisplayName] nvarchar(256), [Email] nvarchar(256));
INSERT @SeedUsers ([DisplayName], [Email]) VALUES
    (N'Vlado Bošnjaković', N'bosnjakovic.vlado@gmail.com'),
    (@AuditorDisplayName, @AuditorEmail);

IF (SELECT COUNT(DISTINCT [Email]) FROM @SeedUsers) <> 2
    THROW 53101, 'The two seed users must have different email addresses.', 1;

DECLARE @NewKeys table ([DisplayName] nvarchar(256), [Email] nvarchar(256), [ApiKey] varchar(64));

INSERT [Settings].[Users] ([DisplayName], [Email], [ApiKey])
OUTPUT inserted.[DisplayName], inserted.[Email], inserted.[ApiKey]
    INTO @NewKeys ([DisplayName], [Email], [ApiKey])
SELECT s.[DisplayName], s.[Email], CONVERT(varchar(64), CRYPT_GEN_RANDOM(32), 2)
FROM @SeedUsers AS s
WHERE NOT EXISTS (SELECT 1 FROM [Settings].[Users] AS u WHERE u.[Email] = s.[Email]);

-- This result contains credentials. Do not include it in logs or commit it.
SELECT [DisplayName], [Email], [ApiKey] FROM @NewKeys ORDER BY [Email];
GO
