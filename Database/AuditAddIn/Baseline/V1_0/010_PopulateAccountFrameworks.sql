/* Framework masters and whole-framework effective versions. */
USE [AuditAddIn];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

;WITH [NewData] AS
(
    SELECT N'GOV_LOCAL' AS [Code], N'Rámcová účtová osnova pre obce, vyššie územné celky a nimi zriadené rozpočtové organizácie a príspevkové organizácie' AS [Name_sk], CONVERT(datetime2(0), '2026-08-27T16:28:02') AS [CreatedAtUtc]
    UNION ALL SELECT N'PROFIT', N'Rámcová účtová osnova pre ziskovú sféru', CONVERT(datetime2(0), '2026-09-03T18:51:02')
    UNION ALL SELECT N'NONPROFIT', N'Rámcová účtová osnova pre neziskovú sféru', CONVERT(datetime2(0), '2026-09-03T18:51:02')
)
INSERT INTO [Accounts].[AccountFramework]
(
    [Code], [Name_sk], [CreatedAtUtc]
)
SELECT
    n.[Code], n.[Name_sk], n.[CreatedAtUtc]
FROM [NewData] AS n
LEFT JOIN [Accounts].[AccountFramework] AS e ON e.[Code] = n.[Code]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT N'GOV_LOCAL' AS [FrameworkCode], N'2023-01-01' AS [VersionCode], CONVERT(date, '2023-01-01') AS [ValidFrom], NULL AS [ValidTo], N'Opatrenie Ministerstva financií Slovenskej republiky č. MF/014454/2022-36' AS [LegalReference], N'https://static.slov-lex.sk/pdf/prilohy/SK/OP/2022/33/20230101_5489967-2.pdf' AS [SourceUrl], 'A12322B93DC11D487736FD4CA04065943FB57961A03B8D3E532A8ACC748AD5BA' AS [SourceSha256], CONVERT(datetime2(0), '2026-08-27T16:28:10') AS [CreatedAtUtc]
    UNION ALL SELECT N'PROFIT', N'2022-01-01', CONVERT(date, '2023-01-01'), NULL, N'Book1', N'scan', NULL, CONVERT(datetime2(0), '2026-09-03T18:51:02')
    UNION ALL SELECT N'NONPROFIT', N'2022-01-01', CONVERT(date, '2023-01-01'), NULL, N'Book2', N'scan', NULL, CONVERT(datetime2(0), '2026-09-03T18:51:02')
)
INSERT INTO [Accounts].[AccountFrameworkVersion]
(
    [AccountFrameworkId], [VersionCode], [ValidFrom], [ValidTo], [LegalReference], [SourceUrl], [SourceSha256], [CreatedAtUtc]
)
SELECT
    af.[Id], n.[VersionCode], n.[ValidFrom], n.[ValidTo], n.[LegalReference], n.[SourceUrl], n.[SourceSha256], n.[CreatedAtUtc]
FROM [NewData] AS n
INNER JOIN [Accounts].[AccountFramework] AS af ON af.[Code] = n.[FrameworkCode]
LEFT JOIN [Accounts].[AccountFrameworkVersion] AS e ON e.[AccountFrameworkId] = af.[Id] AND e.[VersionCode] = n.[VersionCode]
WHERE e.[Id] IS NULL;

COMMIT TRANSACTION;
PRINT '010 framework population completed.';
GO
