/*
    RegisterUZ Stage 2: durable observation resolution and entity refresh queue.

    Existing observations whose latest source was a ChangeFeed run are left
    pending. Observations last written by a package load are acknowledged as
    already materialized in the mirror.
*/

USE [RegisterUZ];
GO

SET XACT_ABORT ON;
GO

IF COL_LENGTH(N'Sync.ObservedObject', N'ChangeObservationCount') IS NULL
BEGIN
    ALTER TABLE [Sync].[ObservedObject]
        ADD [ChangeObservationCount] bigint NOT NULL
            CONSTRAINT [DF_Sync_ObservedObject_ChangeObservationCount] DEFAULT (0);

    /*
        This migration follows the first verified change-feed scan. Its IDs
        were each returned once. Preserve them as pending without treating
        package-load observations as new change notifications.
    */
    UPDATE o
    SET [ChangeObservationCount] = 1
    FROM [Sync].[ObservedObject] o
    JOIN [Sync].[Run] r ON r.[SyncRunId] = o.[LastObservedInRunId]
    WHERE r.[RunType] = 'ChangeFeed';
END;
GO

IF OBJECT_ID(N'[Sync].[ObservedObjectWork]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sync].[ObservedObjectWork]
    (
        [ObjectTypeId] tinyint NOT NULL,
        [RegisterUzObjectId] bigint NOT NULL,
        [AcknowledgedObservationCount] bigint NOT NULL
            CONSTRAINT [DF_Sync_ObservedObjectWork_AcknowledgedCount] DEFAULT (0),
        [ClaimObservationCount] bigint NULL,
        [ResolvedEntityId] bigint NULL,
        [Status] varchar(20) NOT NULL
            CONSTRAINT [DF_Sync_ObservedObjectWork_Status] DEFAULT ('Pending'),
        [LeaseToken] uniqueidentifier NULL,
        [LeaseExpiresAtUtc] datetime2(3) NULL,
        [AttemptCount] int NOT NULL
            CONSTRAINT [DF_Sync_ObservedObjectWork_AttemptCount] DEFAULT (0),
        [LastAttemptAtUtc] datetime2(3) NULL,
        [LastCompletedAtUtc] datetime2(3) NULL,
        [LastError] nvarchar(4000) NULL,
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Sync_ObservedObjectWork_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Sync_ObservedObjectWork]
            PRIMARY KEY CLUSTERED ([ObjectTypeId], [RegisterUzObjectId]),
        CONSTRAINT [FK_Sync_ObservedObjectWork_ObservedObject]
            FOREIGN KEY ([ObjectTypeId], [RegisterUzObjectId])
            REFERENCES [Sync].[ObservedObject] ([ObjectTypeId], [RegisterUzObjectId]),
        CONSTRAINT [CK_Sync_ObservedObjectWork_Status]
            CHECK ([Status] IN ('Pending', 'Resolving', 'Resolved', 'Failed')),
        CONSTRAINT [CK_Sync_ObservedObjectWork_Counts]
            CHECK ([AcknowledgedObservationCount] >= 0 AND
                   ([ClaimObservationCount] IS NULL OR [ClaimObservationCount] >= 0)),
        CONSTRAINT [CK_Sync_ObservedObjectWork_Lease]
            CHECK (([Status] = 'Resolving' AND [LeaseToken] IS NOT NULL AND [LeaseExpiresAtUtc] IS NOT NULL
                    AND [ClaimObservationCount] IS NOT NULL)
                   OR ([Status] <> 'Resolving' AND [LeaseToken] IS NULL AND [LeaseExpiresAtUtc] IS NULL
                       AND [ClaimObservationCount] IS NULL))
    );

    CREATE INDEX [IX_Sync_ObservedObjectWork_Claim]
        ON [Sync].[ObservedObjectWork] ([Status], [LeaseExpiresAtUtc], [ObjectTypeId], [RegisterUzObjectId])
        INCLUDE ([AcknowledgedObservationCount], [ClaimObservationCount]);
END;
GO

IF OBJECT_ID(N'[Sync].[EntityRefreshQueue]', N'U') IS NULL
BEGIN
    CREATE TABLE [Sync].[EntityRefreshQueue]
    (
        [RegisterUzEntityId] bigint NOT NULL,
        [RequestedGeneration] bigint NOT NULL,
        [CompletedGeneration] bigint NOT NULL
            CONSTRAINT [DF_Sync_EntityRefreshQueue_CompletedGeneration] DEFAULT (0),
        [ClaimGeneration] bigint NULL,
        [Status] varchar(20) NOT NULL
            CONSTRAINT [DF_Sync_EntityRefreshQueue_Status] DEFAULT ('Pending'),
        [LeaseToken] uniqueidentifier NULL,
        [LeaseExpiresAtUtc] datetime2(3) NULL,
        [AttemptCount] int NOT NULL
            CONSTRAINT [DF_Sync_EntityRefreshQueue_AttemptCount] DEFAULT (0),
        [LastAttemptAtUtc] datetime2(3) NULL,
        [LastCompletedAtUtc] datetime2(3) NULL,
        [LastSyncRunId] bigint NULL,
        [LastError] nvarchar(4000) NULL,
        [CreatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Sync_EntityRefreshQueue_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        [UpdatedAtUtc] datetime2(3) NOT NULL
            CONSTRAINT [DF_Sync_EntityRefreshQueue_UpdatedAtUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_Sync_EntityRefreshQueue]
            PRIMARY KEY CLUSTERED ([RegisterUzEntityId]),
        CONSTRAINT [FK_Sync_EntityRefreshQueue_Run]
            FOREIGN KEY ([LastSyncRunId]) REFERENCES [Sync].[Run] ([SyncRunId]),
        CONSTRAINT [CK_Sync_EntityRefreshQueue_Status]
            CHECK ([Status] IN ('Pending', 'Refreshing', 'Completed', 'Failed')),
        CONSTRAINT [CK_Sync_EntityRefreshQueue_Generations]
            CHECK ([RequestedGeneration] > 0 AND [CompletedGeneration] >= 0 AND
                   [CompletedGeneration] <= [RequestedGeneration] AND
                   ([ClaimGeneration] IS NULL OR
                    ([ClaimGeneration] > [CompletedGeneration] AND
                     [ClaimGeneration] <= [RequestedGeneration]))),
        CONSTRAINT [CK_Sync_EntityRefreshQueue_Lease]
            CHECK (([Status] = 'Refreshing' AND [LeaseToken] IS NOT NULL AND
                    [LeaseExpiresAtUtc] IS NOT NULL AND [ClaimGeneration] IS NOT NULL)
                   OR ([Status] <> 'Refreshing' AND [LeaseToken] IS NULL AND
                       [LeaseExpiresAtUtc] IS NULL AND [ClaimGeneration] IS NULL))
    );

    CREATE INDEX [IX_Sync_EntityRefreshQueue_Claim]
        ON [Sync].[EntityRefreshQueue] ([Status], [LeaseExpiresAtUtc], [RegisterUzEntityId])
        INCLUDE ([RequestedGeneration], [CompletedGeneration], [ClaimGeneration]);
END;
GO

INSERT INTO [Sync].[ObservedObjectWork]
(
    [ObjectTypeId], [RegisterUzObjectId], [AcknowledgedObservationCount], [Status]
)
SELECT
    o.[ObjectTypeId],
    o.[RegisterUzObjectId],
    CASE WHEN o.[ChangeObservationCount] > 0 THEN 0 ELSE o.[ChangeObservationCount] END,
    CASE WHEN o.[ChangeObservationCount] > 0 THEN 'Pending' ELSE 'Resolved' END
FROM [Sync].[ObservedObject] o
WHERE NOT EXISTS
(
    SELECT 1
    FROM [Sync].[ObservedObjectWork] w
    WHERE w.[ObjectTypeId] = o.[ObjectTypeId]
      AND w.[RegisterUzObjectId] = o.[RegisterUzObjectId]
);
GO

SELECT
    w.[Status],
    COUNT_BIG(*) AS [WorkItemCount]
FROM [Sync].[ObservedObjectWork] w
GROUP BY w.[Status]
ORDER BY w.[Status];

PRINT 'RegisterUZ change processing V1 created.';
