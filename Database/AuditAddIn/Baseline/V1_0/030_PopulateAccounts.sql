/* Reviewed production accounts are initially promoted from official reference data. */
USE [AuditAddIn];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

DECLARE @AccountLevel tinyint = 1;

WHILE @AccountLevel <= 3
BEGIN
    INSERT INTO [Accounts].[Accounts]
    (
        [AccountFrameworkVersionId], [AccountCode], [AccountName_sk], [AccountLevel], [ParentAccountId], [SortOrder]
    )
    SELECT
        o.[AccountFrameworkVersionId], o.[AccountCode], o.[AccountName_sk], o.[AccountLevel], p.[Id], o.[SortOrder]
    FROM [Accounts].[OfficialAccounts] AS o
    LEFT JOIN [Accounts].[Accounts] AS p
        ON p.[AccountFrameworkVersionId] = o.[AccountFrameworkVersionId]
       AND p.[AccountCode] = o.[ParentAccountCode]
    LEFT JOIN [Accounts].[Accounts] AS e
        ON e.[AccountFrameworkVersionId] = o.[AccountFrameworkVersionId]
       AND e.[AccountCode] = o.[AccountCode]
    WHERE o.[AccountLevel] = @AccountLevel
      AND e.[Id] IS NULL;

    SET @AccountLevel += 1;
END;

;WITH [NewData] AS
(
    SELECT N'GOV_LOCAL' AS [FrameworkCode], N'2023-01-01' AS [VersionCode], 2 AS [AccountLevel], N'75' AS [FromAccountCode], N'79' AS [ToAccountCode], N'7' AS [ParentAccountCode], N'Podsúvahové účty' AS [AccountName_sk], 1 AS [SortOrder]
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', 2, N'90', N'98', N'9', N'Vnútroorganizačné účtovníctvo', 2
)
INSERT INTO [Accounts].[AccountRanges]
(
    [AccountFrameworkVersionId], [AccountLevel], [FromAccountCode], [ToAccountCode], [ParentAccountId], [AccountName_sk], [SortOrder]
)
SELECT
    afv.[Id], n.[AccountLevel], n.[FromAccountCode], n.[ToAccountCode], p.[Id], n.[AccountName_sk], n.[SortOrder]
FROM [NewData] AS n
INNER JOIN [Accounts].[AccountFramework] AS af ON af.[Code] = n.[FrameworkCode]
INNER JOIN [Accounts].[AccountFrameworkVersion] AS afv ON afv.[AccountFrameworkId] = af.[Id] AND afv.[VersionCode] = n.[VersionCode]
LEFT JOIN [Accounts].[Accounts] AS p ON p.[AccountFrameworkVersionId] = afv.[Id] AND p.[AccountCode] = n.[ParentAccountCode]
LEFT JOIN [Accounts].[AccountRanges] AS e ON e.[AccountFrameworkVersionId] = afv.[Id] AND e.[AccountLevel] = n.[AccountLevel] AND e.[FromAccountCode] = n.[FromAccountCode] AND e.[ToAccountCode] = n.[ToAccountCode]
WHERE e.[Id] IS NULL;

COMMIT TRANSACTION;
PRINT '030 production-account population completed.';
GO
