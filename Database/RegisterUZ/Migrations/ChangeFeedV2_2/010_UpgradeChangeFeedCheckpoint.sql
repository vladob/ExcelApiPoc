/*
    RegisterUZ change-feed checkpoint migration V2.2.

    The original date-only window model is replaced with the exact UTC model
    recommended by the RegisterUZ API. This migration intentionally refuses
    to discard an existing checkpoint.
*/

USE [RegisterUZ];
GO

SET XACT_ABORT ON;

IF OBJECT_ID(N'[Sync].[ChangeFeedCheckpoint]', N'U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM [Sync].[ChangeFeedCheckpoint])
    THROW 51520, 'ChangeFeedCheckpoint contains data. Migrate it explicitly before applying V2.2.', 1;
GO

IF OBJECT_ID(N'[Sync].[ChangeFeedCheckpoint]', N'U') IS NOT NULL
    DROP TABLE [Sync].[ChangeFeedCheckpoint];
GO

CREATE TABLE [Sync].[ChangeFeedCheckpoint]
(
    [ObjectTypeId] tinyint NOT NULL,
    [ChangedSinceUtc] datetime2(0) NOT NULL,
    [ScanStartedAtUtc] datetime2(0) NULL,
    [ContinueAfterId] bigint NULL,
    [PageSize] int NOT NULL,
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
    CONSTRAINT [CK_Sync_ChangeFeedCheckpoint_Status]
        CHECK ([Status] IN ('Pending', 'Running', 'Paused', 'Completed', 'Failed')),
    CONSTRAINT [CK_Sync_ChangeFeedCheckpoint_ContinueAfterId]
        CHECK ([ContinueAfterId] IS NULL OR [ContinueAfterId] > 0),
    CONSTRAINT [CK_Sync_ChangeFeedCheckpoint_PageSize]
        CHECK ([PageSize] BETWEEN 1 AND 10000),
    CONSTRAINT [CK_Sync_ChangeFeedCheckpoint_ScanState]
        CHECK
        (
            ([Status] = 'Completed' AND [ScanStartedAtUtc] IS NULL AND [ContinueAfterId] IS NULL)
            OR ([Status] IN ('Running', 'Paused', 'Failed') AND [ScanStartedAtUtc] IS NOT NULL)
            OR ([Status] = 'Pending' AND [ScanStartedAtUtc] IS NULL)
        )
);
GO

CREATE INDEX [IX_Sync_ChangeFeedCheckpoint_Status]
    ON [Sync].[ChangeFeedCheckpoint] ([Status], [ObjectTypeId])
    INCLUDE ([ChangedSinceUtc], [ScanStartedAtUtc], [ContinueAfterId], [PageSize]);
GO

SELECT
    c.[name] AS [ColumnName], t.[name] AS [DataType],
    c.[max_length] AS [MaxLength], c.[is_nullable] AS [IsNullable]
FROM sys.columns c
JOIN sys.types t ON t.[user_type_id] = c.[user_type_id]
WHERE c.[object_id] = OBJECT_ID(N'[Sync].[ChangeFeedCheckpoint]')
ORDER BY c.[column_id];

PRINT 'RegisterUZ change-feed checkpoint V2.2 created.';
