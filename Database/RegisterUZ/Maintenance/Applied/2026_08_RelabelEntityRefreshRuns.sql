/*
    Relabel package runs created by the entity-refresh queue before the
    application began writing EntityRefresh directly.
*/

USE [RegisterUZ];
GO

SET XACT_ABORT ON;

UPDATE r
SET [RunType] = 'EntityRefresh'
FROM [Sync].[Run] r
JOIN
(
    SELECT DISTINCT [LastSyncRunId]
    FROM [Sync].[EntityRefreshQueue]
    WHERE [LastSyncRunId] IS NOT NULL
) q ON q.[LastSyncRunId] = r.[SyncRunId]
WHERE r.[RunType] = 'SingleIco';

DECLARE @RelabeledRunCount int = @@ROWCOUNT;

SELECT
    @RelabeledRunCount AS [RelabeledRunCount];

SELECT
    r.[RunType],
    COUNT_BIG(*) AS [RunCount]
FROM [Sync].[Run] r
GROUP BY r.[RunType]
ORDER BY r.[RunType];

PRINT 'RegisterUZ entity-refresh run types corrected.';
