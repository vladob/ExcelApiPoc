USE [AuditAddIn];
GO
-- Run before deploying the API version that reads CanAccessMkSoft.
-- Existing and new users are denied until explicitly enabled by an administrator.
IF COL_LENGTH(N'Settings.Users', N'CanAccessMkSoft') IS NULL
BEGIN
    ALTER TABLE [Settings].[Users]
        ADD [CanAccessMkSoft] bit NOT NULL
            CONSTRAINT [DF_Settings_Users_CanAccessMkSoft] DEFAULT (0) WITH VALUES;
END;
GO
-- Explicit grants (run separately for approved Settings.Users IDs):
-- UPDATE [Settings].[Users] SET [CanAccessMkSoft] = 1 WHERE [Id] IN (1);
-- Revoke: UPDATE [Settings].[Users] SET [CanAccessMkSoft] = 0 WHERE [Id] = 1;
