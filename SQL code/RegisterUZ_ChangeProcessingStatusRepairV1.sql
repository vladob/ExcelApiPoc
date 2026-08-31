/*
    Repair work rows created for package-only observations before their
    initial status respected ChangeObservationCount.
*/

USE [RegisterUZ];
GO

SET XACT_ABORT ON;

UPDATE w
SET [Status] = 'Resolved',
    [UpdatedAtUtc] = SYSUTCDATETIME()
FROM [Sync].[ObservedObjectWork] w
JOIN [Sync].[ObservedObject] o
  ON o.[ObjectTypeId] = w.[ObjectTypeId]
 AND o.[RegisterUzObjectId] = w.[RegisterUzObjectId]
WHERE w.[Status] = 'Pending'
  AND w.[AcknowledgedObservationCount] = 0
  AND w.[AttemptCount] = 0
  AND o.[ChangeObservationCount] = 0;

DECLARE @RepairedWorkItemCount int = @@ROWCOUNT;

SELECT @RepairedWorkItemCount AS [RepairedWorkItemCount];

SELECT
    w.[ObjectTypeId],
    w.[Status],
    COUNT_BIG(*) AS [WorkItemCount]
FROM [Sync].[ObservedObjectWork] w
GROUP BY w.[ObjectTypeId], w.[Status]
ORDER BY w.[ObjectTypeId], w.[Status];

PRINT 'RegisterUZ package-only work statuses repaired.';
