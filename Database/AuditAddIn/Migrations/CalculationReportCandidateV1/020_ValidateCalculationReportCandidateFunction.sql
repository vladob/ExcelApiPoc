/*
    Non-mutating structural and operational validation for
    Accounts.GetCalculationReportCandidates.
*/

USE [AuditAddIn];
GO

IF OBJECT_ID(N'[Accounts].[GetCalculationReportCandidates]', N'IF') IS NULL
    THROW 52130, 'Accounts.GetCalculationReportCandidates does not exist as an inline table-valued function.', 1;

IF EXISTS
(
    SELECT 1
    FROM [Accounts].[GetCalculationReportCandidates]
         ('__NO_SUCH_ICO__', 2024)
)
    THROW 52131, 'A nonexistent IČO unexpectedly returned a calculation-report candidate.', 1;

IF EXISTS
(
    SELECT 1
    FROM [Accounts].[GetCalculationReportCandidates]
         ('00322792', 2024) AS c
    WHERE c.[PeriodFrom] <> '2024-01'
       OR c.[PeriodTo] <> '2024-12'
       OR c.[StatementType] <> N'Riadna'
       OR c.[DataAvailability] <> N'Verejné'
)
    THROW 52132, 'The candidate function returned a report outside the requested audited fiscal period or status.', 1;

IF EXISTS
(
    SELECT 1
    FROM [RegisterUZ].[Registry].[AccountingEntity] AS ae
    WHERE ae.[Ico] = '00322792'
      AND ae.[IsDeleted] = 0
)
BEGIN
    IF
    (
        SELECT COUNT_BIG(*)
        FROM [Accounts].[GetCalculationReportCandidates]
             ('00322792', 2024)
    ) <> 1
        THROW 52133, 'IČO 00322792 must resolve to exactly one mapped report candidate for 2024.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM [Accounts].[GetCalculationReportCandidates]
             ('00322792', 2024)
        WHERE [RegisterUzTemplateId] = 690
          AND [FrameworkCode] = N'GOV_LOCAL'
          AND [CalculationImplemented] = 1
    )
        THROW 52134, 'IČO 00322792 did not resolve to the accepted template-690 calculation candidate for 2024.', 1;
END
ELSE
    PRINT 'Known template-690 probe skipped because IČO 00322792 is not loaded in RegisterUZ.';

SELECT
    [RegisterUzEntityId],
    [Ico],
    [EntityName],
    [RegisterUzStatementId],
    [RegisterUzFinancialReportId],
    [RegisterUzTemplateId],
    [FrameworkCode],
    [CalculationImplemented]
FROM [Accounts].[GetCalculationReportCandidates]
     ('00322792', 2024)
ORDER BY
    [RegisterUzFinancialReportId];

PRINT 'Calculation-report candidate validation completed.';
GO
