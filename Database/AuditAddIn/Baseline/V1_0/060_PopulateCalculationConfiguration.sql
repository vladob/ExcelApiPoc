/* GOV_LOCAL calculation configuration; wording remains in Accounts.Accounts. */
USE [AuditAddIn];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

;WITH [NewData] AS
(
    SELECT N'ClosingDebit' AS [Code], N'Closing debit balance' AS [Description]
    UNION ALL SELECT N'ClosingCredit', N'Closing credit balance'
    UNION ALL SELECT N'ClosingNetto', N'Net closing balance'
)
INSERT INTO [Accounts].[ValueSource]
(
    [Code], [Description]
)
SELECT
    n.[Code], n.[Description]
FROM [NewData] AS n
LEFT JOIN [Accounts].[ValueSource] AS e ON e.[Code] = n.[Code]
WHERE e.[Code] IS NULL;

;WITH [NewData] AS
(
    SELECT N'GOV_LOCAL' AS [FrameworkCode], N'2023-01-01' AS [FrameworkVersionCode], N'GOV' AS [AccountingModelCode],
           N'GOV_LOCAL-2023-01' AS [Code], N'Initial normalized calculation configuration for template 690' AS [Description],
           CONVERT(date, '2023-01-01') AS [ValidFrom], CONVERT(date, NULL) AS [ValidTo]
)
INSERT INTO [Accounts].[CalculationConfigurationVersion]
(
    [AccountFrameworkVersionId], [AccountingModelCode], [Code], [Description], [ValidFrom], [ValidTo]
)
SELECT
    afv.[Id], n.[AccountingModelCode], n.[Code], n.[Description], n.[ValidFrom], n.[ValidTo]
FROM [NewData] AS n
INNER JOIN [Accounts].[AccountFramework] AS af ON af.[Code] = n.[FrameworkCode]
INNER JOIN [Accounts].[AccountFrameworkVersion] AS afv ON afv.[AccountFrameworkId] = af.[Id] AND afv.[VersionCode] = n.[FrameworkVersionCode]
LEFT JOIN [Accounts].[CalculationConfigurationVersion] AS e ON e.[AccountFrameworkVersionId] = afv.[Id] AND e.[Code] = n.[Code]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT N'GOV_LOCAL' AS [FrameworkCode], N'2023-01-01' AS [VersionCode], N'01' AS [AccountCode], NULL AS [Legend], NULL AS [AssetsValueSourceCode], NULL AS [LiabilitiesValueSourceCode]
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'012', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'013', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'014', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'018', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'019', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'02', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'021', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'022', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'023', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'025', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'026', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'028', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'029', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'03', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'031', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'032', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'033', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'04', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'041', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'042', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'043', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'05', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'051', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'052', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'06', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'061', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'062', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'063', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'065', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'066', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'067', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'069', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'07', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'072', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'073', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'074', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'078', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'079', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'08', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'081', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'082', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'083', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'085', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'086', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'088', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'089', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'09', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'091', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'092', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'093', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'094', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'095', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'096', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'11', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'112', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'119', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'12', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'121', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'122', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'123', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'124', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'13', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'131', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'132', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'133', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'139', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'19', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'191', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'192', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'193', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'194', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'195', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'196', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'21', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'211', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'213', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'22', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'221', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'222', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'223', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'224', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'225', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'23', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'231', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'232', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'24', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'241', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'249', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'25', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'251', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'253', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'255', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'256', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'257', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'259', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'26', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'261', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'27', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'271', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'272', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'273', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'274', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'275', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'277', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'29', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'291', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'31', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'311', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'312', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'313', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'314', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'315', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'316', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'317', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'318', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'319', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'32', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'321', N'S P', NULL, N'ClosingNetto'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'322', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'323', N'S P', NULL, N'ClosingNetto'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'324', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'325', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'326', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'33', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'331', N'S P', NULL, N'ClosingNetto'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'333', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'335', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'336', N'S P', NULL, N'ClosingNetto'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'34', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'341', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'342', N'S P', NULL, N'ClosingNetto'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'343', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'345', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'35', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'351', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'352', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'353', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'354', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'355', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'356', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'357', N'S P', NULL, N'ClosingNetto'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'358', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'359', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'36', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'367', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'368', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'369', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'37', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'371', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'372', N'S P', NULL, N'ClosingNetto'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'373', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'374', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'375', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'376', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'377', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'378', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'379', N'S P', NULL, N'ClosingNetto'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'38', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'381', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'382', N'S A', N'ClosingNetto', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'383', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'384', N'S P', NULL, N'ClosingNetto'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'385', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'39', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'391', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'395', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'396', N'S A', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'41', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'414', N'S P', NULL, N'ClosingNetto'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'415', N'S P', NULL, N'ClosingNetto'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'42', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'421', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'427', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'428', N'S P', NULL, N'ClosingNetto'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'43', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'431', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'45', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'451', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'459', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'46', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'461', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'47', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'472', N'S P', NULL, N'ClosingNetto'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'473', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'474', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'475', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'476', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'478', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'479', N'S P', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'50', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'501', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'502', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'503', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'504', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'51', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'511', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'512', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'513', N'V N', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'518', N'V D', N'ClosingDebit', NULL
)
INSERT INTO [Accounts].[AccountCalculationRules]
(
    [CalculationConfigurationVersionId], [AccountId], [Legend], [AssetsValueSourceCode], [LiabilitiesValueSourceCode]
)
SELECT
    ccv.[Id], a.[Id], n.[Legend], n.[AssetsValueSourceCode], n.[LiabilitiesValueSourceCode]
FROM [NewData] AS n
INNER JOIN [Accounts].[AccountFramework] AS af ON af.[Code] = n.[FrameworkCode]
INNER JOIN [Accounts].[AccountFrameworkVersion] AS afv ON afv.[AccountFrameworkId] = af.[Id] AND afv.[VersionCode] = n.[VersionCode]
INNER JOIN [Accounts].[CalculationConfigurationVersion] AS ccv ON ccv.[AccountFrameworkVersionId] = afv.[Id] AND ccv.[ValidTo] IS NULL
INNER JOIN [Accounts].[Accounts] AS a ON a.[AccountFrameworkVersionId] = afv.[Id] AND a.[AccountCode] = n.[AccountCode]
LEFT JOIN [Accounts].[AccountCalculationRules] AS e ON e.[CalculationConfigurationVersionId] = ccv.[Id] AND e.[AccountId] = a.[Id]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT N'GOV_LOCAL' AS [FrameworkCode], N'2023-01-01' AS [VersionCode], N'52' AS [AccountCode], NULL AS [Legend], NULL AS [AssetsValueSourceCode], NULL AS [LiabilitiesValueSourceCode]
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'521', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'524', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'525', N'V N', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'527', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'528', N'V N', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'53', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'531', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'532', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'538', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'54', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'541', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'542', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'544', N'V N', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'545', N'V N', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'546', N'V N', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'548', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'549', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'55', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'551', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'552', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'553', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'554', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'555', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'557', N'V N', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'558', N'V N', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'559', N'V N', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'56', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'561', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'562', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'563', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'564', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'566', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'567', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'568', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'569', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'57', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'58', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'581', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'582', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'583', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'584', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'585', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'586', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'587', N'V D', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'588', N'V N', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'589', N'V N', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'59', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'591', N'V N', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'595', N'V N', N'ClosingDebit', NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'60', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'601', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'602', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'604', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'61', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'611', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'612', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'613', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'614', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'62', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'621', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'622', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'623', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'624', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'63', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'631', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'632', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'633', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'64', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'641', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'642', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'644', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'645', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'646', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'648', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'65', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'652', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'653', N'V N', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'654', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'655', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'657', N'V N', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'658', N'V N', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'659', N'V N', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'66', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'661', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'662', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'663', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'664', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'665', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'666', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'667', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'668', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'68', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'681', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'682', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'683', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'684', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'685', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'686', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'687', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'688', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'689', N'V N', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'69', NULL, NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'691', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'692', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'693', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'694', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'695', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'696', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'697', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'698', N'V D', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'699', N'V N', NULL, N'ClosingCredit'
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'701', N'Z', NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'702', N'Z', NULL, NULL
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'710', N'Z', NULL, NULL
)
INSERT INTO [Accounts].[AccountCalculationRules]
(
    [CalculationConfigurationVersionId], [AccountId], [Legend], [AssetsValueSourceCode], [LiabilitiesValueSourceCode]
)
SELECT
    ccv.[Id], a.[Id], n.[Legend], n.[AssetsValueSourceCode], n.[LiabilitiesValueSourceCode]
FROM [NewData] AS n
INNER JOIN [Accounts].[AccountFramework] AS af ON af.[Code] = n.[FrameworkCode]
INNER JOIN [Accounts].[AccountFrameworkVersion] AS afv ON afv.[AccountFrameworkId] = af.[Id] AND afv.[VersionCode] = n.[VersionCode]
INNER JOIN [Accounts].[CalculationConfigurationVersion] AS ccv ON ccv.[AccountFrameworkVersionId] = afv.[Id] AND ccv.[ValidTo] IS NULL
INNER JOIN [Accounts].[Accounts] AS a ON a.[AccountFrameworkVersionId] = afv.[Id] AND a.[AccountCode] = n.[AccountCode]
LEFT JOIN [Accounts].[AccountCalculationRules] AS e ON e.[CalculationConfigurationVersionId] = ccv.[Id] AND e.[AccountId] = a.[Id]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 690 AS [TemplateErpId], N'GOV_LOCAL' AS [FrameworkCode], N'2023-01-01' AS [FrameworkVersionCode], N'GOV_LOCAL-2023-01' AS [ConfigurationCode]
)
INSERT INTO [Accounts].[TemplateFrameworkVersion]
(
    [TemplateId], [AccountFrameworkVersionId], [CalculationConfigurationVersionId]
)
SELECT
    t.[Id], afv.[Id], ccv.[Id]
FROM [NewData] AS n
INNER JOIN [Template].[Templates] AS t ON t.[ErpId] = n.[TemplateErpId]
INNER JOIN [Accounts].[AccountFramework] AS af ON af.[Code] = n.[FrameworkCode]
INNER JOIN [Accounts].[AccountFrameworkVersion] AS afv ON afv.[AccountFrameworkId] = af.[Id] AND afv.[VersionCode] = n.[FrameworkVersionCode]
INNER JOIN [Accounts].[CalculationConfigurationVersion] AS ccv ON ccv.[AccountFrameworkVersionId] = afv.[Id] AND ccv.[Code] = n.[ConfigurationCode]
LEFT JOIN [Accounts].[TemplateFrameworkVersion] AS e ON e.[TemplateId] = t.[Id] AND e.[CalculationConfigurationVersionId] = ccv.[Id]
WHERE e.[Id] IS NULL;

COMMIT TRANSACTION;
PRINT '060 calculation-configuration population completed.';
GO
