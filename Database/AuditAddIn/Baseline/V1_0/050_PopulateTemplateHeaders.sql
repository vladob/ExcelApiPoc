/* Template headers; generated in bounded CTE batches for reliable compilation. */
USE [AuditAddIn];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

;WITH [NewData] AS
(
    SELECT 802 AS [TableErpId], N'4' AS [Text_sk], NULL AS [Text_en], 2 AS [RowPosition], 4 AS [ColumnPosition], 1 AS [RowSpan], 1 AS [ColumnSpan], 7 AS [HeaderOrdinal]
    UNION ALL SELECT 802, N'Záväzky', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 802, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 802, N'Za bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 802, N'Za bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 802, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 802, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 802, N'3', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 902, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 902, N'STRANA PASÍV', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 902, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 902, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 902, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 902, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 902, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 902, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 902, N'3', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 902, N'4', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 1701, NULL, NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 1701, N'Strana aktív', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 1701, N'č.r.', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 1701, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 1701, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 1, 1, 4
    UNION ALL SELECT 1701, NULL, NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 1701, N'Brutto', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 1701, N'Korekcia', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 1701, N'Netto', NULL, 2, 6, 1, 1, 8
    UNION ALL SELECT 1701, N'Netto', NULL, 2, 7, 1, 1, 9
    UNION ALL SELECT 1701, NULL, NULL, 3, 1, 1, 1, 10
    UNION ALL SELECT 1701, N'a', NULL, 3, 2, 1, 1, 11
    UNION ALL SELECT 1701, N'b', NULL, 3, 3, 1, 1, 12
    UNION ALL SELECT 1701, N'1', NULL, 3, 4, 1, 1, 13
    UNION ALL SELECT 1701, N'2', NULL, 3, 5, 1, 1, 14
    UNION ALL SELECT 1701, N'3', NULL, 3, 6, 1, 1, 15
    UNION ALL SELECT 1701, N'4', NULL, 3, 7, 1, 1, 16
    UNION ALL SELECT 1801, N'Číslo účtu', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 1801, N'Náklady', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 1801, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 1801, N'Činnosť', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 1801, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 1801, N'Hlavná nezdaňovaná', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 1801, N'Podnikateľská zdaňovaná', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 1801, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 1801, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 1801, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 1801, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 1801, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 1801, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 1801, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 1801, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 6201, N'Príjmy', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 6201, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 6201, N'Za bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 6201, N'a', NULL, 2, 1, 1, 1, 3
    UNION ALL SELECT 6201, N'b', NULL, 2, 2, 1, 1, 4
    UNION ALL SELECT 6201, N'1', NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 8101, N'Majetok', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 8101, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 8101, N'Za bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 8101, N'Za bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 8101, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 8101, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 8101, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 8101, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 52202, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 52202, N'STRANA PASÍV', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 52202, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 52202, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 52202, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 52202, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 52202, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 52202, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 52202, N'5', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 52202, N'6', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 54102, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 54102, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 54102, N'Bežné účtovné obdobie', NULL, 1, 4, 2, 1, 2
    UNION ALL SELECT 54102, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 2, 1, 3
    UNION ALL SELECT 54102, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 54102, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 54102, N'a', NULL, 3, 1, 1, 1, 6
    UNION ALL SELECT 54102, N'b', NULL, 3, 2, 1, 1, 7
    UNION ALL SELECT 54102, N'c', NULL, 3, 3, 1, 1, 8
    UNION ALL SELECT 54102, N'5', NULL, 3, 4, 1, 1, 9
    UNION ALL SELECT 54102, N'6', NULL, 3, 5, 1, 1, 10
    UNION ALL SELECT 68703, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 68703, N'Text', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 68703, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 68703, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 68703, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 68703, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 68703, N'1', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 68703, N'2', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 68703, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 68703, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 69002, N'Označenie', N'Designation', 1, 1, 1, 1, 0
    UNION ALL SELECT 69002, N'STRANA PASÍV', N'LIABILITIES AND EQUITY', 1, 2, 1, 1, 1
    UNION ALL SELECT 69002, N'Číslo riadku', N'Line No.', 1, 3, 1, 1, 2
    UNION ALL SELECT 69002, N'20xx', N'20xx', 1, 4, 1, 1, 3
    UNION ALL SELECT 69002, N'20xx-1', N'20xx-1', 1, 5, 1, 1, 4
    UNION ALL SELECT 69002, N'a', N'a', 2, 1, 1, 1, 5
    UNION ALL SELECT 69002, N'b', N'b', 2, 2, 1, 1, 6
    UNION ALL SELECT 69002, N'c', N'c', 2, 3, 1, 1, 7
    UNION ALL SELECT 69002, N'5', N'5', 2, 4, 1, 1, 8
    UNION ALL SELECT 69002, N'6', N'6', 2, 5, 1, 1, 9
    UNION ALL SELECT 72303, N'Označnie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 72303, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 72303, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 72303, N'Predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 72303, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 72303, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 72303, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 72303, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 72702, N'Číslo účtu alebo skupiny', N'Account or group number', 1, 1, 2, 1, 0
    UNION ALL SELECT 72702, N'Výnosy, daň z príjmov a výsledok hospodárenia', N'Revenues, income tax, and net profit/loss', 1, 2, 2, 1, 1
    UNION ALL SELECT 72702, N'Číslo riadku', N'Line No.', 1, 3, 2, 1, 2
    UNION ALL SELECT 72702, N'20xx', N'20xx', 1, 4, 1, 3, 3
    UNION ALL SELECT 72702, N'20xx-1', N'20xx-1', 1, 7, 2, 1, 4
    UNION ALL SELECT 72702, N'Hlavná činnosť', N'Core activity', 2, 4, 1, 1, 5
    UNION ALL SELECT 72702, N'Podnikateľská činnosť', N'Business activity', 2, 5, 1, 1, 6
    UNION ALL SELECT 72702, N'Spolu', N'Total', 2, 6, 1, 1, 7
    UNION ALL SELECT 72702, N'a', N'a', 3, 1, 1, 1, 8
    UNION ALL SELECT 72702, N'b', N'b', 3, 2, 1, 1, 9
    UNION ALL SELECT 72702, N'c', N'c', 3, 3, 1, 1, 10
    UNION ALL SELECT 72702, N'1', N'1', 3, 4, 1, 1, 11
    UNION ALL SELECT 72702, N'2', N'2', 3, 5, 1, 1, 12
    UNION ALL SELECT 72702, N'3', N'3', 3, 6, 1, 1, 13
    UNION ALL SELECT 72702, N'4', N'4', 3, 7, 1, 1, 14
    UNION ALL SELECT 94202, N'Pasíva', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 94202, N'č. r.', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 94202, N'Stav', NULL, 1, 3, 1, 5, 2
    UNION ALL SELECT 94202, N'Spolu', NULL, 2, 3, 1, 1, 3
    UNION ALL SELECT 94202, N'ŽP a AZ - ŽP', NULL, 2, 4, 1, 1, 4
    UNION ALL SELECT 94202, N'NP a AZ - NP', NULL, 2, 5, 1, 1, 5
    UNION ALL SELECT 94202, N'AZ - ŽP', NULL, 2, 6, 1, 1, 6
    UNION ALL SELECT 94202, N'AZ - NP', NULL, 2, 7, 1, 1, 7
    UNION ALL SELECT 94202, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 94202, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 94202, N'3', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 94202, N'4', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 94202, N'5', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 94202, N'6', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 94202, N'7', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 94204, N'Pasíva', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 94204, N'č. r.', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 94204, N'Stav', NULL, 1, 3, 1, 5, 2
    UNION ALL SELECT 94204, N'Spolu', NULL, 2, 3, 1, 1, 3
    UNION ALL SELECT 94204, N'ŽP a AZ - ŽP', NULL, 2, 4, 1, 1, 4
    UNION ALL SELECT 94204, N'NP a AZ - NP', NULL, 2, 5, 1, 1, 5
    UNION ALL SELECT 94204, N'AZ - ŽP', NULL, 2, 6, 1, 1, 6
    UNION ALL SELECT 94204, N'AZ - NP', NULL, 2, 7, 1, 1, 7
    UNION ALL SELECT 94204, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 94204, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 94204, N'3', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 94204, N'4', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 94204, N'5', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 94204, N'6', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 94204, N'7', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 102103, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 102103, N'Položka číslo', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 102103, N'Bežné účtovné obdobie', NULL, 1, 4, 2, 1, 2
    UNION ALL SELECT 102103, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 2, 1, 3
    UNION ALL SELECT 102103, N'Číslo riadku', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 102103, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 102103, N'a', NULL, 3, 1, 1, 1, 6
    UNION ALL SELECT 102103, N'b', NULL, 3, 2, 1, 1, 7
    UNION ALL SELECT 102103, N'c', NULL, 3, 3, 1, 1, 8
    UNION ALL SELECT 102103, N'1', NULL, 3, 4, 1, 1, 9
    UNION ALL SELECT 102103, N'2', NULL, 3, 5, 1, 1, 10
    UNION ALL SELECT 110102, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 110102, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 110102, N'Bežné účtovné obdobie', NULL, 1, 4, 2, 1, 2
    UNION ALL SELECT 110102, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 2, 1, 3
    UNION ALL SELECT 110102, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 110102, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 110102, N'a', NULL, 3, 1, 1, 1, 6
    UNION ALL SELECT 110102, N'b', NULL, 3, 2, 1, 1, 7
    UNION ALL SELECT 110102, N'c', NULL, 3, 3, 1, 1, 8
    UNION ALL SELECT 110102, N'5', NULL, 3, 4, 1, 1, 9
    UNION ALL SELECT 110102, N'6', NULL, 3, 5, 1, 1, 10
    UNION ALL SELECT 118003, N'Číslo účtu', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 118003, N'Náklady', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 118003, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 118003, N'Činnosť', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 118003, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 118003, N'Hlavná nezdaňovaná', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 118003, N'Podnikateľská zdaňovaná', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 118003, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 118003, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 118003, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 118003, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 118003, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 118003, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 118003, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 118003, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 518101, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 518101, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 518101, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 518101, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 518101, N'a', NULL, 2, 1, 1, 1, 4
)
INSERT INTO [Template].[Headers]
(
    [TableId], [Text_sk], [Text_en], [RowPosition], [ColumnPosition], [RowSpan], [ColumnSpan], [HeaderOrdinal]
)
SELECT
    t.[Id], n.[Text_sk], n.[Text_en], n.[RowPosition], n.[ColumnPosition], n.[RowSpan], n.[ColumnSpan], n.[HeaderOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Headers] AS e ON e.[TableId] = t.[Id] AND e.[HeaderOrdinal] = n.[HeaderOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 518101 AS [TableErpId], N'b' AS [Text_sk], NULL AS [Text_en], 2 AS [RowPosition], 2 AS [ColumnPosition], 1 AS [RowSpan], 1 AS [ColumnSpan], 5 AS [HeaderOrdinal]
    UNION ALL SELECT 518101, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 518101, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 518306, N'Číslo riadku', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 518306, N'Názov položky', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 518306, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 518306, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 518402, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 518402, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 518402, N'Bežné účtovné obdobie', NULL, 1, 4, 2, 1, 2
    UNION ALL SELECT 518402, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 2, 1, 3
    UNION ALL SELECT 518402, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 518402, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 518402, N'6', NULL, 3, 5, 1, 1, 10
    UNION ALL SELECT 518402, N'c', NULL, 3, 3, 1, 1, 8
    UNION ALL SELECT 518402, N'5', NULL, 3, 4, 1, 1, 9
    UNION ALL SELECT 518402, N'a', NULL, 3, 1, 1, 1, 6
    UNION ALL SELECT 518402, N'b', NULL, 3, 2, 1, 1, 7
    UNION ALL SELECT 102, N'Číslo účtu alebo skupiny', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 102, N'Výnosy, daň z príjmov a výsledok hospodárenia', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 102, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 102, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 102, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 102, N'Hlavná činnosť', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 102, N'Podnikateľská činnosť', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 102, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 102, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 102, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 102, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 102, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 102, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 102, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 102, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 601, N'Riadok', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 601, N'Položky', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 601, N'Ročný rozpočet', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 601, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 601, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 1201, N'Číslo účtu alebo skupiny', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 1201, N'Náklady', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 1201, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 1201, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 1201, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 1201, N'Hlavná činnosť', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 1201, N'Podnikateľská činnosť', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 1201, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 1201, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 1201, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 1201, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 1201, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 1201, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 1201, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 1201, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 1502, N'ZÁVÄZKY', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 1502, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 1502, N'Účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 1502, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 1502, NULL, NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 1502, NULL, NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 1502, N'3', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 1502, N'4', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 1901, N'Číslo účtu', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 1901, N'Náklady', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 1901, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 1901, N'Činnosť', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 1901, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 1901, N'Hlavná nezdaňovaná', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 1901, N'Podnikateľská zdaňovaná', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 1901, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 1901, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 1901, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 1901, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 1901, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 1901, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 1901, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 1901, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 6101, N'Príjmy', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 6101, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 6101, N'Za bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 6101, N'a', NULL, 2, 1, 1, 1, 3
    UNION ALL SELECT 6101, N'b', NULL, 2, 2, 1, 1, 4
    UNION ALL SELECT 6101, N'1', NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 8202, N'Záväzky', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 8202, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 8202, N'Za bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 8202, N'Za bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 8202, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 8202, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 8202, N'3', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 8202, N'4', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 38503, N'Číslo účtu', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 38503, N'Náklady', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 38503, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 38503, N'Činnosť', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 38503, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 38503, N'Hlavná nezdaňovaná', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 38503, N'Podnikateľská zdaňovaná', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 38503, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 38503, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 38503, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 38503, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 38503, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 38503, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 38503, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 38503, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 66102, N'Názov položky', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 66102, N'Číslo riadku', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 66102, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 66102, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 66102, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 66102, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 66102, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 66102, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 68401, N'STRANA AKTÍV', N'ASSETS', 1, 2, 2, 1, 1
    UNION ALL SELECT 68401, N'Číslo riadku', N'Line No.', 1, 3, 2, 1, 2
    UNION ALL SELECT 68401, N'20xx', N'20xx', 1, 4, 1, 1, 3
    UNION ALL SELECT 68401, N'20xx-1', N'20xx-1', 1, 5, 1, 1, 4
    UNION ALL SELECT 68401, N'Netto', N'Net', 2, 4, 1, 1, 5
    UNION ALL SELECT 68401, N'Netto', N'Net', 2, 5, 1, 1, 6
    UNION ALL SELECT 68401, N'a', N'a', 3, 1, 1, 1, 7
    UNION ALL SELECT 68401, N'b', N'b', 3, 2, 1, 1, 8
    UNION ALL SELECT 68401, N'c', N'c', 3, 3, 1, 1, 9
    UNION ALL SELECT 68401, N'1', N'1', 3, 4, 1, 1, 10
    UNION ALL SELECT 68401, N'2', N'2', 3, 5, 1, 1, 11
    UNION ALL SELECT 68401, N'Označenie', N'Designation', 1, 1, 2, 1, 0
    UNION ALL SELECT 69601, N'Číslo účtu alebo skupiny', N'Account or group number', 1, 1, 2, 1, 0
    UNION ALL SELECT 69601, N'Náklady', N'Expenses', 1, 2, 2, 1, 1
    UNION ALL SELECT 69601, N'Číslo riadku', N'Line No.', 1, 3, 2, 1, 2
    UNION ALL SELECT 69601, N'20xx', N'20xx', 1, 4, 1, 3, 3
    UNION ALL SELECT 69601, N'20xx-1', N'20xx-1', 1, 7, 2, 1, 4
    UNION ALL SELECT 69601, N'Hlavná činnosť', N'Core activity', 2, 4, 1, 1, 5
    UNION ALL SELECT 69601, N'Podnikateľská činnosť', N'Business activity', 2, 5, 1, 1, 6
    UNION ALL SELECT 69601, N'Spolu', N'Total', 2, 6, 1, 1, 7
    UNION ALL SELECT 69601, N'a', N'a', 3, 1, 1, 1, 8
    UNION ALL SELECT 69601, N'b', N'b', 3, 2, 1, 1, 9
    UNION ALL SELECT 69601, N'c', N'c', 3, 3, 1, 1, 10
    UNION ALL SELECT 69601, N'1', N'1', 3, 4, 1, 1, 11
    UNION ALL SELECT 69601, N'2', N'2', 3, 5, 1, 1, 12
    UNION ALL SELECT 69601, N'3', N'3', 3, 6, 1, 1, 13
    UNION ALL SELECT 69601, N'4', N'4', 3, 7, 1, 1, 14
    UNION ALL SELECT 71604, N'Záväzky', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 71604, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 71604, N'Za bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 71604, N'Za bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 71604, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 71604, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 71604, N'3', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 71604, N'4', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 73302, N'Názov položky', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 73302, N'Číslo riadku', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 73302, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 73302, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 73302, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 73302, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 73302, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 73302, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 94101, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 94101, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 94101, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 94101, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 94101, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 94101, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 94101, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 94101, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 94301, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 94301, N'Názov položky', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 94301, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 94301, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 94301, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 94301, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 94301, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 94301, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 94301, N'1', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 94301, N'2', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 100103, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 100103, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 100103, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 2
    UNION ALL SELECT 100103, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 3
    UNION ALL SELECT 100103, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 100103, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 100103, N'Základňa', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 100103, N'Medzisúčet', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 100103, N'Výsledok', NULL, 2, 6, 1, 1, 8
    UNION ALL SELECT 100103, N'a', NULL, 3, 1, 1, 1, 9
    UNION ALL SELECT 100103, N'b', NULL, 3, 2, 1, 1, 10
    UNION ALL SELECT 100103, N'c', NULL, 3, 3, 1, 1, 11
    UNION ALL SELECT 100103, N'1', NULL, 3, 4, 1, 1, 12
    UNION ALL SELECT 100103, N'2', NULL, 3, 5, 1, 1, 13
    UNION ALL SELECT 100103, N'3', NULL, 3, 6, 1, 1, 14
    UNION ALL SELECT 100103, N'4', NULL, 3, 7, 1, 1, 15
    UNION ALL SELECT 112101, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 112101, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 112101, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 112101, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 112101, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 112101, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 112101, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 112101, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 118001, NULL, NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 118001, N'Strana aktív', NULL, 1, 2, 2, 1, 1
)
INSERT INTO [Template].[Headers]
(
    [TableId], [Text_sk], [Text_en], [RowPosition], [ColumnPosition], [RowSpan], [ColumnSpan], [HeaderOrdinal]
)
SELECT
    t.[Id], n.[Text_sk], n.[Text_en], n.[RowPosition], n.[ColumnPosition], n.[RowSpan], n.[ColumnSpan], n.[HeaderOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Headers] AS e ON e.[TableId] = t.[Id] AND e.[HeaderOrdinal] = n.[HeaderOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 118001 AS [TableErpId], N'č.r.' AS [Text_sk], NULL AS [Text_en], 1 AS [RowPosition], 3 AS [ColumnPosition], 1 AS [RowSpan], 1 AS [ColumnSpan], 2 AS [HeaderOrdinal]
    UNION ALL SELECT 118001, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 118001, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 1, 1, 4
    UNION ALL SELECT 118001, NULL, NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 118001, N'Brutto', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 118001, N'Korekcia', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 118001, N'Netto', NULL, 2, 6, 1, 1, 8
    UNION ALL SELECT 118001, N'Netto', NULL, 2, 7, 1, 1, 9
    UNION ALL SELECT 118001, NULL, NULL, 3, 1, 1, 1, 10
    UNION ALL SELECT 118001, N'a', NULL, 3, 2, 1, 1, 11
    UNION ALL SELECT 118001, N'1', NULL, 3, 4, 1, 1, 13
    UNION ALL SELECT 118001, N'2', NULL, 3, 5, 1, 1, 14
    UNION ALL SELECT 118001, N'3', NULL, 3, 6, 1, 1, 15
    UNION ALL SELECT 118001, N'4', NULL, 3, 7, 1, 1, 16
    UNION ALL SELECT 118001, N'b', NULL, 3, 3, 1, 1, 12
    UNION ALL SELECT 518104, N'Číslo riadku', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 518104, N'Počet zamestnancov', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 518104, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 518104, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 518301, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 518301, N'Názov položky', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 518301, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 518301, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 518301, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 518301, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 518301, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 518301, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 518301, N'1', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 518301, N'2', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 1002, N'Číslo účtu alebo skupiny', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 1002, N'Výnosy, daň z príjmov a výsledok hospodárenia', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 1002, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 1002, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 1002, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 1002, N'Hlavná činnosť', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 1002, N'Podnikateľská činnosť', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 1002, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 1002, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 1002, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 1002, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 1002, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 1002, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 1002, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 1002, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 1102, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 1102, N'STRANA PASÍV', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 1102, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 1102, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 1102, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 1102, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 1102, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 1102, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 1102, N'3', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 1102, N'4', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 1501, N'MAJETOK', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 1501, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 1501, N'Účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 1501, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 1501, NULL, NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 1501, NULL, NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 1501, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 1501, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 1601, N'MAJETOK', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 1601, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 1601, N'Účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 1601, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 1601, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 1601, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 1601, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 1601, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 8201, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 8201, N'Za bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 8201, N'Za bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 8201, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 8201, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 8201, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 8201, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 8201, N'Majetok', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 38301, N'PRÍJMY', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 38301, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 38301, N'Nezdaňovaná činnosť', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 38301, N'Zdaňovaná činnosť', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 38301, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 38301, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 38301, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 38301, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 38504, N'Číslo účtu', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 38504, N'Výnosy', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 38504, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 38504, N'Činnosť', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 38504, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 38504, N'Hlavná nezdaňovaná', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 38504, N'Podnikateľská zdaňovaná', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 38504, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 38504, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 38504, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 38504, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 38504, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 38504, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 38504, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 38504, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 52102, N'Číslo účtu alebo skupiny', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 52102, N'Výnosy, daň z príjmov a výsledok hospodárenia', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 52102, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 52102, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 52102, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 52102, N'Hlavná činnosť', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 52102, N'Podnikateľská činnosť', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 52102, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 52102, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 52102, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 52102, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 52102, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 52102, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 52102, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 52102, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 69602, N'Číslo účtu alebo skupiny', N'Account or group number', 1, 1, 2, 1, 0
    UNION ALL SELECT 69602, N'Výnosy, daň z príjmov a výsledok hospodárenia', N'Revenues, income tax, and net profit/loss', 1, 2, 2, 1, 1
    UNION ALL SELECT 69602, N'Číslo riadku', N'Line No.', 1, 3, 2, 1, 2
    UNION ALL SELECT 69602, N'20xx', N'20xx', 1, 4, 1, 3, 3
    UNION ALL SELECT 69602, N'20xx-1', N'20xx-1', 1, 7, 2, 1, 4
    UNION ALL SELECT 69602, N'Hlavná činnosť', N'Core activity', 2, 4, 1, 1, 5
    UNION ALL SELECT 69602, N'Podnikateľská činnosť', N'Business activity', 2, 5, 1, 1, 6
    UNION ALL SELECT 69602, N'Spolu', N'Total', 2, 6, 1, 1, 7
    UNION ALL SELECT 69602, N'a', N'a', 3, 1, 1, 1, 8
    UNION ALL SELECT 69602, N'b', N'b', 3, 2, 1, 1, 9
    UNION ALL SELECT 69602, N'c', N'c', 3, 3, 1, 1, 10
    UNION ALL SELECT 69602, N'1', N'1', 3, 4, 1, 1, 11
    UNION ALL SELECT 69602, N'2', N'2', 3, 5, 1, 1, 12
    UNION ALL SELECT 69602, N'3', N'3', 3, 6, 1, 1, 13
    UNION ALL SELECT 69602, N'4', N'4', 3, 7, 1, 1, 14
    UNION ALL SELECT 69902, N'Označenie', N'Designation', 1, 1, 1, 1, 0
    UNION ALL SELECT 69902, N'STRANA PASÍV', N'LIABILITIES AND EQUITY', 1, 2, 1, 1, 1
    UNION ALL SELECT 69902, N'Číslo riadku', N'Line No.', 1, 3, 1, 1, 2
    UNION ALL SELECT 69902, N'Bežné účtovné obdobie', N'Current accounting period', 1, 4, 1, 1, 3
    UNION ALL SELECT 69902, N'Bezprostredne predchádzajúce účtovné obdobie', N'Preceding accounting period', 1, 5, 1, 1, 4
    UNION ALL SELECT 69902, N'a', N'a', 2, 1, 1, 1, 5
    UNION ALL SELECT 69902, N'b', N'b', 2, 2, 1, 1, 6
    UNION ALL SELECT 69902, N'c', N'c', 2, 3, 1, 1, 7
    UNION ALL SELECT 69902, N'4', N'4', 2, 4, 1, 1, 8
    UNION ALL SELECT 69902, N'5', N'5', 2, 5, 1, 1, 9
    UNION ALL SELECT 71601, N'Príjmy', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 71601, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 71601, N'Za bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 71601, N'a', NULL, 2, 1, 1, 1, 3
    UNION ALL SELECT 71601, N'b', NULL, 2, 2, 1, 1, 4
    UNION ALL SELECT 71601, N'1', NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 71603, N'Majetok', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 71603, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 71603, N'Za bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 71603, N'Za bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 71603, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 71603, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 71603, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 71603, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 72301, N'Označenie', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 72301, N'POLOŽKA', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 72301, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 3, 2
    UNION ALL SELECT 72301, N'Predchádzajúce účtovné obdobie', NULL, 1, 6, 2, 1, 3
    UNION ALL SELECT 72301, N'Brutto', NULL, 2, 3, 1, 1, 4
    UNION ALL SELECT 72301, N'Korekcia', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 72301, N'Netto', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 72301, N'a', NULL, 3, 1, 1, 1, 7
    UNION ALL SELECT 72301, N'b', NULL, 3, 2, 1, 1, 8
    UNION ALL SELECT 72301, N'1', NULL, 3, 3, 1, 1, 9
    UNION ALL SELECT 72301, N'2', NULL, 3, 4, 1, 1, 10
    UNION ALL SELECT 72301, N'3', NULL, 3, 5, 1, 1, 11
    UNION ALL SELECT 72301, N'4', NULL, 3, 6, 1, 1, 12
    UNION ALL SELECT 94206, N'Označenie podsúvahovej položky', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 94206, N'č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 94206, N'Stav', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 94206, N'Charakteristika podsúvahovej položky', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 94206, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 94206, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 94206, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 94206, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 94208, NULL, NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 94208, N'č. r.', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 94208, N'Spolu', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 94208, N'Životné poistenie', NULL, 1, 4, 1, 2, 3
    UNION ALL SELECT 94208, N'Neživotné poistenie', NULL, 1, 6, 1, 1, 4
    UNION ALL SELECT 94208, N'Aktívne zaistenie', NULL, 1, 7, 1, 3, 5
    UNION ALL SELECT 94208, N'Spolu', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 94208, N'B9', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 94208, N'Spolu', NULL, 2, 6, 1, 1, 8
    UNION ALL SELECT 94208, N'Spolu', NULL, 2, 7, 1, 1, 9
    UNION ALL SELECT 94208, N'ŽP', NULL, 2, 8, 1, 1, 10
    UNION ALL SELECT 94208, N'NP', NULL, 2, 9, 1, 1, 11
    UNION ALL SELECT 94208, N'a', NULL, 3, 1, 1, 1, 12
    UNION ALL SELECT 94208, N'b', NULL, 3, 2, 1, 1, 13
    UNION ALL SELECT 94208, N'1', NULL, 3, 3, 1, 1, 14
    UNION ALL SELECT 94208, N'2', NULL, 3, 4, 1, 1, 15
    UNION ALL SELECT 94208, N'3', NULL, 3, 5, 1, 1, 16
    UNION ALL SELECT 94208, N'4', NULL, 3, 6, 1, 1, 17
    UNION ALL SELECT 94208, N'5', NULL, 3, 7, 1, 1, 18
    UNION ALL SELECT 94208, N'6', NULL, 3, 8, 1, 1, 19
    UNION ALL SELECT 94208, N'7', NULL, 3, 9, 1, 1, 20
    UNION ALL SELECT 94302, N'Pohľadávky po lehote splatnosti', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 94302, N'Číslo riadku', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 94302, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
)
INSERT INTO [Template].[Headers]
(
    [TableId], [Text_sk], [Text_en], [RowPosition], [ColumnPosition], [RowSpan], [ColumnSpan], [HeaderOrdinal]
)
SELECT
    t.[Id], n.[Text_sk], n.[Text_en], n.[RowPosition], n.[ColumnPosition], n.[RowSpan], n.[ColumnSpan], n.[HeaderOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Headers] AS e ON e.[TableId] = t.[Id] AND e.[HeaderOrdinal] = n.[HeaderOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 94302 AS [TableErpId], N'Bezprostredne predchádzajúce účtovné obdobie' AS [Text_sk], NULL AS [Text_en], 1 AS [RowPosition], 4 AS [ColumnPosition], 1 AS [RowSpan], 1 AS [ColumnSpan], 3 AS [HeaderOrdinal]
    UNION ALL SELECT 94302, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 94302, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 94302, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 94302, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 100102, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 100102, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 100102, N'Bežné účtovné obdobie', NULL, 1, 4, 2, 1, 2
    UNION ALL SELECT 100102, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 2, 1, 3
    UNION ALL SELECT 100102, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 100102, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 100102, N'a', NULL, 3, 1, 1, 1, 6
    UNION ALL SELECT 100102, N'b', NULL, 3, 2, 1, 1, 7
    UNION ALL SELECT 100102, N'c', NULL, 3, 3, 1, 1, 8
    UNION ALL SELECT 100102, N'5', NULL, 3, 4, 1, 1, 9
    UNION ALL SELECT 100102, N'6', NULL, 3, 5, 1, 1, 10
    UNION ALL SELECT 102101, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 102101, N'Položka číslo', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 102101, N'Bežné účtovné obdobie', NULL, 1, 4, 2, 1, 2
    UNION ALL SELECT 102101, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 2, 1, 3
    UNION ALL SELECT 102101, N'Číslo riadku', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 102101, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 102101, N'a', NULL, 3, 1, 1, 1, 6
    UNION ALL SELECT 102101, N'b', NULL, 3, 2, 1, 1, 7
    UNION ALL SELECT 102101, N'c', NULL, 3, 3, 1, 1, 8
    UNION ALL SELECT 102101, N'1', NULL, 3, 4, 1, 1, 9
    UNION ALL SELECT 102101, N'2', NULL, 3, 5, 1, 1, 10
    UNION ALL SELECT 518103, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 518103, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 518103, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 518103, N'Predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 518103, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 518103, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 518103, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 518103, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 518201, N'Oprávky a opravné položky', NULL, 1, 5, 3, 1, 4
    UNION ALL SELECT 518201, N'Stav netto', NULL, 1, 6, 1, 4, 5
    UNION ALL SELECT 518201, N'Spolu', NULL, 2, 6, 2, 1, 6
    UNION ALL SELECT 518201, N'ŽP', NULL, 2, 7, 1, 2, 7
    UNION ALL SELECT 518201, N'NP', NULL, 2, 9, 2, 1, 8
    UNION ALL SELECT 518201, N'Spolu ŽP', NULL, 3, 7, 1, 1, 9
    UNION ALL SELECT 518201, N'z toho: VFA', NULL, 3, 8, 1, 1, 10
    UNION ALL SELECT 518201, N'a', NULL, 4, 1, 1, 1, 11
    UNION ALL SELECT 518201, N'b', NULL, 4, 2, 1, 1, 12
    UNION ALL SELECT 518201, N'2', NULL, 4, 3, 1, 1, 13
    UNION ALL SELECT 518201, N'3', NULL, 4, 4, 1, 1, 14
    UNION ALL SELECT 518201, N'4', NULL, 4, 5, 1, 1, 15
    UNION ALL SELECT 518201, N'5', NULL, 4, 6, 1, 1, 16
    UNION ALL SELECT 518201, N'6', NULL, 4, 7, 1, 1, 17
    UNION ALL SELECT 518201, N'7', NULL, 4, 8, 1, 1, 18
    UNION ALL SELECT 518201, N'8', NULL, 4, 9, 1, 1, 19
    UNION ALL SELECT 518201, N'označenie z dôvodu kontrolných súčtov', NULL, 1, 1, 3, 1, 0
    UNION ALL SELECT 518201, N'SK', N'EN', 1, 2, 3, 1, 1
    UNION ALL SELECT 518201, N'č. r.', NULL, 1, 3, 3, 1, 2
    UNION ALL SELECT 518201, N'Stav brutto', NULL, 1, 4, 3, 1, 3
    UNION ALL SELECT 518302, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 518302, N'Pohľadávky po lehote splatnosti', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 518302, N'Číslo riadku', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 518302, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 518302, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 518302, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 518302, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 518302, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 518304, N'Názov položky', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 518304, N'Číslo riadku', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 518304, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 518304, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 518304, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 518304, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 518304, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 518304, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 901, N'Označenie', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 901, N'STRANA AKTÍV', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 901, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 901, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 901, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 901, N'Netto', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 901, N'Netto', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 901, N'a', NULL, 3, 1, 1, 1, 7
    UNION ALL SELECT 901, N'b', NULL, 3, 2, 1, 1, 8
    UNION ALL SELECT 901, N'c', NULL, 3, 3, 1, 1, 9
    UNION ALL SELECT 901, N'1', NULL, 3, 4, 1, 1, 10
    UNION ALL SELECT 901, N'2', NULL, 3, 5, 1, 1, 11
    UNION ALL SELECT 1001, N'Číslo účtu alebo skupiny', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 1001, N'Náklady', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 1001, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 1001, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 1001, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 1001, N'Hlavná činnosť', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 1001, N'Podnikateľská činnosť', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 1001, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 1001, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 1001, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 1001, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 1001, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 1001, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 1001, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 1001, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 1702, N'Strana pasív', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 1702, N'č.r.', NULL, 1, 3, 1, 1, 1
    UNION ALL SELECT 1702, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 2
    UNION ALL SELECT 1702, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 3
    UNION ALL SELECT 1702, N'a', NULL, 2, 1, 1, 2, 4
    UNION ALL SELECT 1702, N'b', NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 1702, N'5', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 1702, N'6', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 1802, N'Výnosy', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 1802, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 1802, N'Činnosť', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 1802, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 1802, N'Hlavná nezdaňovaná', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 1802, N'Podnikateľská zdaňovaná', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 1802, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 1802, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 1802, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 1802, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 1802, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 1802, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 1802, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 1802, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 1802, N'Číslo účtu', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 6102, N'Výdavky', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 6102, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 6102, N'Za bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 6102, N'a', NULL, 2, 1, 1, 1, 3
    UNION ALL SELECT 6102, N'b', NULL, 2, 2, 1, 1, 4
    UNION ALL SELECT 6102, N'2', NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 6202, N'Výdavky', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 6202, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 6202, N'Za bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 6202, N'a', NULL, 2, 1, 1, 1, 3
    UNION ALL SELECT 6202, N'b', NULL, 2, 2, 1, 1, 4
    UNION ALL SELECT 6202, N'2', NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 8102, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 8102, N'3', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 8102, N'4', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 8102, N'Záväzky', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 8102, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 8102, N'Za bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 8102, N'Za bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 8102, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 52201, N'Označenie', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 52201, N'STRANA AKTÍV', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 52201, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 52201, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 52201, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 1, 1, 4
    UNION ALL SELECT 52201, N'Brutto', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 52201, N'Korekcia', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 52201, N'Netto', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 52201, N'Netto', NULL, 2, 7, 1, 1, 8
    UNION ALL SELECT 52201, N'a', NULL, 3, 1, 1, 1, 9
    UNION ALL SELECT 52201, N'b', NULL, 3, 2, 1, 1, 10
    UNION ALL SELECT 52201, N'c', NULL, 3, 3, 1, 1, 11
    UNION ALL SELECT 52201, N'1', NULL, 3, 4, 1, 1, 12
    UNION ALL SELECT 52201, N'2', NULL, 3, 5, 1, 1, 13
    UNION ALL SELECT 52201, N'3', NULL, 3, 6, 1, 1, 14
    UNION ALL SELECT 52201, N'4', NULL, 3, 7, 1, 1, 15
    UNION ALL SELECT 54101, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 54101, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 2
    UNION ALL SELECT 54101, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 3
    UNION ALL SELECT 54101, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 54101, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 54101, N'Brutto', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 54101, N'Korekcia', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 54101, N'Netto', NULL, 2, 6, 1, 1, 8
    UNION ALL SELECT 54101, N'a', NULL, 3, 1, 1, 1, 9
    UNION ALL SELECT 54101, N'b', NULL, 3, 2, 1, 1, 10
    UNION ALL SELECT 54101, N'c', NULL, 3, 3, 1, 1, 11
    UNION ALL SELECT 54101, N'1', NULL, 3, 4, 1, 1, 12
    UNION ALL SELECT 54101, N'2', NULL, 3, 5, 1, 1, 13
    UNION ALL SELECT 54101, N'3', NULL, 3, 6, 1, 1, 14
    UNION ALL SELECT 54101, N'4', NULL, 3, 7, 1, 1, 15
    UNION ALL SELECT 54101, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 68702, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 68702, N'STRANA PASÍV', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 68702, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 68702, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 68702, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 68702, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 68702, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 68702, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 68702, N'3', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 68702, N'4', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 69001, N'Označenie', N'Designation', 1, 1, 2, 1, 0
    UNION ALL SELECT 69001, N'STRANA AKTÍV', N'ASSETS', 1, 2, 2, 1, 1
    UNION ALL SELECT 69001, N'Číslo riadku', N'Line No.', 1, 3, 2, 1, 2
    UNION ALL SELECT 69001, N'20xx', N'20xx', 1, 4, 1, 3, 3
    UNION ALL SELECT 69001, N'20xx-1', N'20xx-1', 1, 7, 1, 1, 4
    UNION ALL SELECT 69001, N'Brutto', N'Gross', 2, 4, 1, 1, 5
    UNION ALL SELECT 69001, N'Korekcia', N'Correction', 2, 5, 1, 1, 6
    UNION ALL SELECT 69001, N'Netto', N'Net', 2, 6, 1, 1, 7
    UNION ALL SELECT 69001, N'Netto', N'Net', 2, 7, 1, 1, 8
    UNION ALL SELECT 69001, N'a', N'a', 3, 1, 1, 1, 9
    UNION ALL SELECT 69001, N'b', N'b', 3, 2, 1, 1, 10
    UNION ALL SELECT 69001, N'c', N'c', 3, 3, 1, 1, 11
    UNION ALL SELECT 69001, N'1', N'1', 3, 4, 1, 1, 12
    UNION ALL SELECT 69001, N'2', N'2', 3, 5, 1, 1, 13
    UNION ALL SELECT 69001, N'3', N'3', 3, 6, 1, 1, 14
    UNION ALL SELECT 69001, N'4', N'4', 3, 7, 1, 1, 15
    UNION ALL SELECT 72302, N'Označenie', NULL, 1, 1, 1, 1, 0
)
INSERT INTO [Template].[Headers]
(
    [TableId], [Text_sk], [Text_en], [RowPosition], [ColumnPosition], [RowSpan], [ColumnSpan], [HeaderOrdinal]
)
SELECT
    t.[Id], n.[Text_sk], n.[Text_en], n.[RowPosition], n.[ColumnPosition], n.[RowSpan], n.[ColumnSpan], n.[HeaderOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Headers] AS e ON e.[TableId] = t.[Id] AND e.[HeaderOrdinal] = n.[HeaderOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 72302 AS [TableErpId], N'POLOŽKA' AS [Text_sk], NULL AS [Text_en], 1 AS [RowPosition], 2 AS [ColumnPosition], 1 AS [RowSpan], 1 AS [ColumnSpan], 1 AS [HeaderOrdinal]
    UNION ALL SELECT 72302, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 72302, N'Predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 72302, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 72302, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 72302, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 72302, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 72701, N'Číslo účtu alebo skupiny', N'Account or group number', 1, 1, 2, 1, 0
    UNION ALL SELECT 72701, N'Náklady', N'Expenses', 1, 2, 2, 1, 1
    UNION ALL SELECT 72701, N'Číslo riadku', N'Line No.', 1, 3, 2, 1, 2
    UNION ALL SELECT 72701, N'20xx', N'20xx', 1, 4, 1, 3, 3
    UNION ALL SELECT 72701, N'20xx-1', N'20xx-1', 1, 7, 2, 1, 4
    UNION ALL SELECT 72701, N'Hlavná činnosť', N'Core activity', 2, 4, 1, 1, 5
    UNION ALL SELECT 72701, N'Podnikateľská činnosť', N'Business activity', 2, 5, 1, 1, 6
    UNION ALL SELECT 72701, N'Spolu', N'Total', 2, 6, 1, 1, 7
    UNION ALL SELECT 72701, N'a', N'a', 3, 1, 1, 1, 8
    UNION ALL SELECT 72701, N'b', N'b', 3, 2, 1, 1, 9
    UNION ALL SELECT 72701, N'c', N'c', 3, 3, 1, 1, 10
    UNION ALL SELECT 72701, N'1', N'1', 3, 4, 1, 1, 11
    UNION ALL SELECT 72701, N'2', N'2', 3, 5, 1, 1, 12
    UNION ALL SELECT 72701, N'3', N'3', 3, 6, 1, 1, 13
    UNION ALL SELECT 72701, N'4', N'4', 3, 7, 1, 1, 14
    UNION ALL SELECT 94203, N'č. r.', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 94203, N'Stav brutto', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 94203, N'Oprávky a opravné položky', NULL, 1, 4, 2, 1, 3
    UNION ALL SELECT 94203, N'Stav (netto)', NULL, 1, 5, 1, 5, 4
    UNION ALL SELECT 94203, N'Spolu', NULL, 2, 5, 1, 1, 5
    UNION ALL SELECT 94203, N'ŽP a AZ - ŽP', NULL, 2, 6, 1, 1, 6
    UNION ALL SELECT 94203, N'NP a AZ - NP', NULL, 2, 7, 1, 1, 7
    UNION ALL SELECT 94203, N'AZ - ŽP', NULL, 2, 8, 1, 1, 8
    UNION ALL SELECT 94203, N'AZ - NP', NULL, 2, 9, 1, 1, 9
    UNION ALL SELECT 94203, N'a', NULL, 3, 1, 1, 1, 10
    UNION ALL SELECT 94203, N'b', NULL, 3, 2, 1, 1, 11
    UNION ALL SELECT 94203, N'1', NULL, 3, 3, 1, 1, 12
    UNION ALL SELECT 94203, N'2', NULL, 3, 4, 1, 1, 13
    UNION ALL SELECT 94203, N'3', NULL, 3, 5, 1, 1, 14
    UNION ALL SELECT 94203, N'4', NULL, 3, 6, 1, 1, 15
    UNION ALL SELECT 94203, N'5', NULL, 3, 7, 1, 1, 16
    UNION ALL SELECT 94203, N'6', NULL, 3, 8, 1, 1, 17
    UNION ALL SELECT 94203, N'7', NULL, 3, 9, 1, 1, 18
    UNION ALL SELECT 94203, N'Aktíva', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 94205, N'č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 94205, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 94205, N'e', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 94205, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 94205, N'Pohľadávky po lehote splatnosti', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 94205, N'Prírastok/ Úbytok', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 94205, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 94205, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 94205, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 94205, N'd', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 110101, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 110101, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 2
    UNION ALL SELECT 110101, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 3
    UNION ALL SELECT 110101, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 110101, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 110101, N'Brutto', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 110101, N'Korekcia', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 110101, N'Netto', NULL, 2, 6, 1, 1, 8
    UNION ALL SELECT 110101, N'a', NULL, 3, 1, 1, 1, 9
    UNION ALL SELECT 110101, N'b', NULL, 3, 2, 1, 1, 10
    UNION ALL SELECT 110101, N'c', NULL, 3, 3, 1, 1, 11
    UNION ALL SELECT 110101, N'1', NULL, 3, 4, 1, 1, 12
    UNION ALL SELECT 110101, N'2', NULL, 3, 5, 1, 1, 13
    UNION ALL SELECT 110101, N'3', NULL, 3, 6, 1, 1, 14
    UNION ALL SELECT 110101, N'4', NULL, 3, 7, 1, 1, 15
    UNION ALL SELECT 110101, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 110103, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 110103, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 110103, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 110103, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 2
    UNION ALL SELECT 110103, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 3
    UNION ALL SELECT 110103, N'a', NULL, 3, 1, 1, 1, 9
    UNION ALL SELECT 110103, N'Výsledok', NULL, 2, 6, 1, 1, 8
    UNION ALL SELECT 110103, N'Medzisúčet', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 110103, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 110103, N'Základňa', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 110103, N'b', NULL, 3, 2, 1, 1, 10
    UNION ALL SELECT 110103, N'c', NULL, 3, 3, 1, 1, 11
    UNION ALL SELECT 110103, N'1', NULL, 3, 4, 1, 1, 12
    UNION ALL SELECT 110103, N'2', NULL, 3, 5, 1, 1, 13
    UNION ALL SELECT 110103, N'3', NULL, 3, 6, 1, 1, 14
    UNION ALL SELECT 110103, N'4', NULL, 3, 7, 1, 1, 15
    UNION ALL SELECT 118004, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 118004, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 118004, N'Číslo účtu', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 118004, N'Výnosy', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 118004, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 118004, N'Činnosť', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 118004, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 118004, N'Hlavná nezdaňovaná', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 118004, N'Podnikateľská zdaňovaná', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 118004, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 118004, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 118004, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 118004, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 118004, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 118004, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 518102, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 518102, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 518102, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 518102, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 518102, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 518102, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 518102, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 518102, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 518305, N'Číslo riadku', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 518305, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 518305, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 518305, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 518305, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 518305, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 518305, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 518305, N'Daň z príjmov', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 518401, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 518401, N'Brutto', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 518401, N'Korekcia', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 518401, N'Netto', NULL, 2, 6, 1, 1, 8
    UNION ALL SELECT 518401, N'a', NULL, 3, 1, 1, 1, 9
    UNION ALL SELECT 518401, N'b', NULL, 3, 2, 1, 1, 10
    UNION ALL SELECT 518401, N'c', NULL, 3, 3, 1, 1, 11
    UNION ALL SELECT 518401, N'1', NULL, 3, 4, 1, 1, 12
    UNION ALL SELECT 518401, N'2', NULL, 3, 5, 1, 1, 13
    UNION ALL SELECT 518401, N'3', NULL, 3, 6, 1, 1, 14
    UNION ALL SELECT 518401, N'4', NULL, 3, 7, 1, 1, 15
    UNION ALL SELECT 518401, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 518401, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 518401, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 2
    UNION ALL SELECT 518401, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 3
    UNION ALL SELECT 518401, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 518403, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 518403, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 518403, N'Bežné účtovné obdobie', NULL, 1, 4, 2, 1, 2
    UNION ALL SELECT 518403, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 2, 1, 3
    UNION ALL SELECT 518403, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 518403, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 518403, N'a', NULL, 3, 1, 1, 1, 6
    UNION ALL SELECT 518403, N'b', NULL, 3, 2, 1, 1, 7
    UNION ALL SELECT 518403, N'c', NULL, 3, 3, 1, 1, 8
    UNION ALL SELECT 518403, N'1', NULL, 3, 4, 1, 1, 9
    UNION ALL SELECT 518403, N'2', NULL, 3, 5, 1, 1, 10
    UNION ALL SELECT 401, N'Riadok', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 401, N'Položky', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 401, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 401, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 501, N'Riadok', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 501, N'Položky', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 501, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 501, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 501, NULL, NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 501, NULL, NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 501, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 501, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 1202, N'Číslo účtu alebo skupiny', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 1202, N'Výnosy, daň z príjmov a výsledok hospodárenia', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 1202, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 1202, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 1202, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 1202, N'Hlavná činnosť', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 1202, N'Podnikateľská činnosť', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 1202, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 1202, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 1202, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 1202, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 1202, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 1202, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 1202, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 1202, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 1301, N'PRÍJMY', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 1301, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 1301, N'Nezdaňovaná činnosť', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 1301, N'Zdaňovaná činnosť', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 1301, NULL, NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 1301, NULL, NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 1301, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 1301, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 1402, N'VÝDAVKY', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 1402, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 1402, N'Nezdaňovaná činnosť', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 1402, N'Zdaňovaná činnosť', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 1402, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 1402, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 1402, N'3', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 1402, N'4', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 2102, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 2102, N'STRANA PASÍV', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 2102, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 2102, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 2102, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 2102, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 2102, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 2102, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 2102, N'4', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 2102, N'5', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 2201, N'Označenie', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 2201, N'Text', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 2201, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 2201, N'Skutočnosť', NULL, 1, 4, 1, 2, 3
    UNION ALL SELECT 2201, N'bežné účtovné obdobie', NULL, 2, 4, 1, 1, 4
    UNION ALL SELECT 2201, N'bezprostredne predchádzajúce účtovné obdobie', NULL, 2, 5, 1, 1, 5
)
INSERT INTO [Template].[Headers]
(
    [TableId], [Text_sk], [Text_en], [RowPosition], [ColumnPosition], [RowSpan], [ColumnSpan], [HeaderOrdinal]
)
SELECT
    t.[Id], n.[Text_sk], n.[Text_en], n.[RowPosition], n.[ColumnPosition], n.[RowSpan], n.[ColumnSpan], n.[HeaderOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Headers] AS e ON e.[TableId] = t.[Id] AND e.[HeaderOrdinal] = n.[HeaderOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 2201 AS [TableErpId], N'a' AS [Text_sk], NULL AS [Text_en], 3 AS [RowPosition], 1 AS [ColumnPosition], 1 AS [RowSpan], 1 AS [ColumnSpan], 6 AS [HeaderOrdinal]
    UNION ALL SELECT 2201, N'b', NULL, 3, 2, 1, 1, 7
    UNION ALL SELECT 2201, N'c', NULL, 3, 3, 1, 1, 8
    UNION ALL SELECT 2201, N'1', NULL, 3, 4, 1, 1, 9
    UNION ALL SELECT 2201, N'2', NULL, 3, 5, 1, 1, 10
    UNION ALL SELECT 38501, NULL, NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 38501, N'Strana aktív', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 38501, N'č.r.', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 38501, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 38501, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 1, 1, 4
    UNION ALL SELECT 38501, NULL, NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 38501, N'Brutto', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 38501, N'Korekcia', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 38501, N'Netto', NULL, 2, 6, 1, 1, 8
    UNION ALL SELECT 38501, N'Netto', NULL, 2, 7, 1, 1, 9
    UNION ALL SELECT 38501, NULL, NULL, 3, 1, 1, 1, 10
    UNION ALL SELECT 38501, N'a', NULL, 3, 2, 1, 1, 11
    UNION ALL SELECT 38501, N'b', NULL, 3, 3, 1, 1, 12
    UNION ALL SELECT 38501, N'1', NULL, 3, 4, 1, 1, 13
    UNION ALL SELECT 38501, N'2', NULL, 3, 5, 1, 1, 14
    UNION ALL SELECT 38501, N'3', NULL, 3, 6, 1, 1, 15
    UNION ALL SELECT 38501, N'4', NULL, 3, 7, 1, 1, 16
    UNION ALL SELECT 38502, N'Strana pasív', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 38502, N'č.r.', NULL, 1, 3, 1, 1, 1
    UNION ALL SELECT 38502, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 2
    UNION ALL SELECT 38502, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 3
    UNION ALL SELECT 38502, N'a', NULL, 2, 1, 1, 2, 4
    UNION ALL SELECT 38502, N'b', NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 38502, N'5', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 38502, N'6', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 66202, N'POLOŽKA', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 66202, N'Číslo riadku', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 66202, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 66202, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 2, 1, 3
    UNION ALL SELECT 66202, N'Netto', NULL, 2, 3, 1, 1, 4
    UNION ALL SELECT 66202, N'a', NULL, 3, 1, 1, 1, 5
    UNION ALL SELECT 66202, N'b', NULL, 3, 2, 1, 1, 6
    UNION ALL SELECT 66202, N'3', NULL, 3, 3, 1, 1, 7
    UNION ALL SELECT 66202, N'4', NULL, 3, 4, 1, 1, 8
    UNION ALL SELECT 66302, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 66302, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 66302, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 66302, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 66302, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 66302, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 66302, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 66302, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 71101, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 71101, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 71101, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 71101, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 71101, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 71101, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 71101, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 71101, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 71102, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 71102, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 71102, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 71102, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 71102, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 71102, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 71102, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 71102, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 80102, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 80102, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 80102, N'Bežné účtovné obdobie', NULL, 1, 4, 2, 1, 2
    UNION ALL SELECT 80102, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 2, 1, 3
    UNION ALL SELECT 80102, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 80102, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 80102, N'a', NULL, 3, 1, 1, 1, 6
    UNION ALL SELECT 80102, N'b', NULL, 3, 2, 1, 1, 7
    UNION ALL SELECT 80102, N'c', NULL, 3, 3, 1, 1, 8
    UNION ALL SELECT 80102, N'5', NULL, 3, 4, 1, 1, 9
    UNION ALL SELECT 80102, N'6', NULL, 3, 5, 1, 1, 10
    UNION ALL SELECT 80103, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 80103, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 80103, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 2
    UNION ALL SELECT 80103, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 3
    UNION ALL SELECT 80103, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 80103, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 80103, N'Základňa', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 80103, N'Medzisúčet', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 80103, N'Výsledok', NULL, 2, 6, 1, 1, 8
    UNION ALL SELECT 80103, N'a', NULL, 3, 1, 1, 1, 9
    UNION ALL SELECT 80103, N'b', NULL, 3, 2, 1, 1, 10
    UNION ALL SELECT 80103, N'c', NULL, 3, 3, 1, 1, 11
    UNION ALL SELECT 80103, N'1', NULL, 3, 4, 1, 1, 12
    UNION ALL SELECT 80103, N'2', NULL, 3, 5, 1, 1, 13
    UNION ALL SELECT 80103, N'3', NULL, 3, 6, 1, 1, 14
    UNION ALL SELECT 80103, N'4', NULL, 3, 7, 1, 1, 15
    UNION ALL SELECT 94303, N'Záväzky po lehote splatnosti', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 94303, N'Číslo riadku', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 94303, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 94303, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 94303, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 94303, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 94303, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 94303, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 100101, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 100101, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 100101, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 2
    UNION ALL SELECT 100101, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 3
    UNION ALL SELECT 100101, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 100101, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 100101, N'Brutto', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 100101, N'Korekcia', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 100101, N'Netto', NULL, 2, 6, 1, 1, 8
    UNION ALL SELECT 100101, N'a', NULL, 3, 1, 1, 1, 9
    UNION ALL SELECT 100101, N'b', NULL, 3, 2, 1, 1, 10
    UNION ALL SELECT 100101, N'c', NULL, 3, 3, 1, 1, 11
    UNION ALL SELECT 100101, N'1', NULL, 3, 4, 1, 1, 12
    UNION ALL SELECT 100101, N'2', NULL, 3, 5, 1, 1, 13
    UNION ALL SELECT 100101, N'3', NULL, 3, 6, 1, 1, 14
    UNION ALL SELECT 100101, N'4', NULL, 3, 7, 1, 1, 15
    UNION ALL SELECT 114201, N'Číslo účtu', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 114201, N'Náklady', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 114201, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 114201, N'Účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 114201, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 114201, N'Nezdaňovaná činnosť', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 114201, N'Zdaňovaná činnosť', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 114201, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 114201, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 114201, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 114201, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 114201, N'7', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 114201, N'8', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 114201, N'9', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 114201, N'10', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 114202, N'Číslo účtu', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 114202, N'Výnosy', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 114202, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 114202, N'Účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 114202, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 114202, N'Nezdaňovaná činnosť', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 114202, N'Zdaňovaná činnosť', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 114202, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 114202, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 114202, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 114202, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 114202, N'7', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 114202, N'8', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 114202, N'9', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 114202, N'10', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 518205, N'č. r.', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 518205, N'Názov položky', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 518205, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 518205, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 518205, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 518205, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 518205, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 518205, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 518206, N'Číslo riadku', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 518206, N'Dane, poplatky a odvody', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 518206, N'Hodnota', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 518206, N'a', NULL, 2, 1, 1, 1, 3
    UNION ALL SELECT 518206, N'b', NULL, 2, 2, 1, 1, 4
    UNION ALL SELECT 518206, N'1', NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 301, N'Označenie', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 301, N'STRANA AKTÍV', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 301, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 301, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 301, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 1, 1, 4
    UNION ALL SELECT 301, N'Brutto', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 301, N'Korekcia', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 301, N'Netto', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 301, N'Netto', NULL, 2, 7, 1, 1, 8
    UNION ALL SELECT 301, N'a', NULL, 3, 1, 1, 1, 9
    UNION ALL SELECT 301, N'b', NULL, 3, 2, 1, 1, 10
    UNION ALL SELECT 301, N'c', NULL, 3, 3, 1, 1, 11
    UNION ALL SELECT 301, N'1', NULL, 3, 4, 1, 1, 12
    UNION ALL SELECT 301, N'2', NULL, 3, 5, 1, 1, 13
    UNION ALL SELECT 301, N'3', NULL, 3, 6, 1, 1, 14
    UNION ALL SELECT 301, N'4', NULL, 3, 7, 1, 1, 15
    UNION ALL SELECT 302, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 302, N'STRANA PASÍV', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 302, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 302, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 302, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 302, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 302, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 302, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 302, N'5', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 302, N'6', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 1302, N'VÝDAVKY', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 1302, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 1302, N'Nezdaňovaná činnosť', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 1302, N'Zdaňovaná činnosť', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 1302, NULL, NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 1302, NULL, NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 1302, N'3', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 1302, N'4', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 1401, N'PRÍJMY', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 1401, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 1401, N'Nezdaňovaná činnosť', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 1401, N'Zdaňovaná činnosť', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 1401, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 1401, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 1401, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 1401, N'2', NULL, 2, 4, 1, 1, 7
)
INSERT INTO [Template].[Headers]
(
    [TableId], [Text_sk], [Text_en], [RowPosition], [ColumnPosition], [RowSpan], [ColumnSpan], [HeaderOrdinal]
)
SELECT
    t.[Id], n.[Text_sk], n.[Text_en], n.[RowPosition], n.[ColumnPosition], n.[RowSpan], n.[ColumnSpan], n.[HeaderOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Headers] AS e ON e.[TableId] = t.[Id] AND e.[HeaderOrdinal] = n.[HeaderOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 2101 AS [TableErpId], N'Označenie' AS [Text_sk], NULL AS [Text_en], 1 AS [RowPosition], 1 AS [ColumnPosition], 1 AS [RowSpan], 1 AS [ColumnSpan], 0 AS [HeaderOrdinal]
    UNION ALL SELECT 2101, N'STRANA AKTÍV', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 2101, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 2101, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 2101, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 1, 1, 4
    UNION ALL SELECT 2101, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 2101, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 2101, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 2101, N'Brutto - časť 1', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 2101, N'Korekcia - časť 2', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 2101, N'Netto 2', NULL, 2, 6, 1, 1, 10
    UNION ALL SELECT 2101, N'Netto 3', NULL, 2, 7, 1, 1, 11
    UNION ALL SELECT 2701, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 2701, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 2701, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 2
    UNION ALL SELECT 2701, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 3
    UNION ALL SELECT 2701, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 2701, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 2701, N'Základňa', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 2701, N'Medzisúčet', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 2701, N'Výsledok', NULL, 2, 6, 1, 1, 8
    UNION ALL SELECT 2701, N'a', NULL, 3, 1, 1, 1, 9
    UNION ALL SELECT 2701, N'b', NULL, 3, 2, 1, 1, 10
    UNION ALL SELECT 2701, N'c', NULL, 3, 3, 1, 1, 11
    UNION ALL SELECT 2701, N'1', NULL, 3, 4, 1, 1, 12
    UNION ALL SELECT 2701, N'2', NULL, 3, 5, 1, 1, 13
    UNION ALL SELECT 2701, N'3', NULL, 3, 6, 1, 1, 14
    UNION ALL SELECT 2701, N'4', NULL, 3, 7, 1, 1, 15
    UNION ALL SELECT 38303, N'MAJETOK', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 38303, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 38303, N'Účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 38303, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 38303, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 38303, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 38303, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 38303, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 38304, N'ZÁVÄZKY', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 38304, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 38304, N'Účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 38304, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 38304, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 38304, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 38304, N'3', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 38304, N'4', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 66203, N'POLOŽKA', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 66203, N'Číslo riadku', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 66203, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 3, 2
    UNION ALL SELECT 66203, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 6, 2, 1, 3
    UNION ALL SELECT 66203, N'Životné poistenie', NULL, 2, 3, 1, 1, 4
    UNION ALL SELECT 66203, N'Neživotné poistenie', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 66203, N'Spolu', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 66203, N'a', NULL, 3, 1, 1, 1, 7
    UNION ALL SELECT 66203, N'b', NULL, 3, 2, 1, 1, 8
    UNION ALL SELECT 66203, N'1', NULL, 3, 3, 1, 1, 9
    UNION ALL SELECT 66203, N'2', NULL, 3, 4, 1, 1, 10
    UNION ALL SELECT 66203, N'3', NULL, 3, 5, 1, 1, 11
    UNION ALL SELECT 66203, N'4', NULL, 3, 6, 1, 1, 12
    UNION ALL SELECT 66301, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 66301, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 66301, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 66301, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 66301, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 66301, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 66301, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 66301, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 69903, N'Označenie', N'Designation', 1, 1, 2, 1, 0
    UNION ALL SELECT 69903, N'Text', N'Text', 1, 2, 2, 1, 1
    UNION ALL SELECT 69903, N'Číslo riadku', N'Line No.', 1, 3, 2, 1, 2
    UNION ALL SELECT 69903, N'Skutočnosť', N'Actual data', 1, 4, 1, 2, 3
    UNION ALL SELECT 69903, N'Bežné účtovné obdobie', N'Current accounting period', 2, 4, 1, 1, 4
    UNION ALL SELECT 69903, N'Bezprostredne predchádzajúce účtovné obdobie', N'Preceding accounting period', 2, 5, 1, 1, 5
    UNION ALL SELECT 69903, N'a', N'a', 3, 1, 1, 1, 6
    UNION ALL SELECT 69903, N'b', N'b', 3, 2, 1, 1, 7
    UNION ALL SELECT 69903, N'c', N'c', 3, 3, 1, 1, 8
    UNION ALL SELECT 69903, N'1', N'1', 3, 4, 1, 1, 9
    UNION ALL SELECT 69903, N'2', N'2', 3, 5, 1, 1, 10
    UNION ALL SELECT 71103, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 71103, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 71103, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 71103, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 71103, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 71103, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 71103, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 71103, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 73803, N'POLOŽKA', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 73803, N'Číslo riadku', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 73803, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 3, 2
    UNION ALL SELECT 73803, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 6, 2, 1, 3
    UNION ALL SELECT 73803, N'Životné poistenie', NULL, 2, 3, 1, 1, 4
    UNION ALL SELECT 73803, N'Neživotné poistenie', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 73803, N'Spolu', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 73803, N'a', NULL, 3, 1, 1, 1, 7
    UNION ALL SELECT 73803, N'b', NULL, 3, 2, 1, 1, 8
    UNION ALL SELECT 73803, N'1', NULL, 3, 3, 1, 1, 9
    UNION ALL SELECT 73803, N'2', NULL, 3, 4, 1, 1, 10
    UNION ALL SELECT 73803, N'3', NULL, 3, 5, 1, 1, 11
    UNION ALL SELECT 73803, N'4', NULL, 3, 6, 1, 1, 12
    UNION ALL SELECT 80101, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 80101, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 80101, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 2
    UNION ALL SELECT 80101, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 3
    UNION ALL SELECT 80101, N'Netto', NULL, 2, 6, 1, 1, 8
    UNION ALL SELECT 80101, N'a', NULL, 3, 1, 1, 1, 9
    UNION ALL SELECT 80101, N'b', NULL, 3, 2, 1, 1, 10
    UNION ALL SELECT 80101, N'c', NULL, 3, 3, 1, 1, 11
    UNION ALL SELECT 80101, N'1', NULL, 3, 4, 1, 1, 12
    UNION ALL SELECT 80101, N'2', NULL, 3, 5, 1, 1, 13
    UNION ALL SELECT 80101, N'3', NULL, 3, 6, 1, 1, 14
    UNION ALL SELECT 80101, N'4', NULL, 3, 7, 1, 1, 15
    UNION ALL SELECT 80101, N'Korekcia', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 80101, N'Brutto', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 80101, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 80101, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 94304, N'Názov položky', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 94304, N'Číslo riadku', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 94304, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 94304, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 94304, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 94304, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 94304, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 94304, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 94305, N'Daň z príjmov', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 94305, N'Číslo riadku', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 94305, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 94305, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 94305, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 94305, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 94305, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 94305, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 114102, N'STRANA PASÍV', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 114102, N'Číslo riadku', NULL, 1, 3, 1, 1, 1
    UNION ALL SELECT 114102, N'Účtovné obdobie', NULL, 1, 4, 1, 1, 2
    UNION ALL SELECT 114102, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 3
    UNION ALL SELECT 114102, N'a', NULL, 2, 1, 1, 2, 4
    UNION ALL SELECT 114102, N'b', NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 114102, N'5', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 114102, N'6', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 116401, N'PRÍJMY', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 116401, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 116401, N'Nezdaňovaná činnosť', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 116401, N'Zdaňovaná činnosť', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 116401, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 116401, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 116401, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 116401, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 116402, N'VÝDAVKY', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 116402, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 116402, N'Nezdaňovaná činnosť', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 116402, N'Zdaňovaná činnosť', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 116402, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 116402, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 116402, N'3', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 116402, N'4', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 518203, N'označenie z dôvodu kontrolných súčtov', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 518203, N'SK', N'EN', 1, 2, 2, 1, 1
    UNION ALL SELECT 518203, N'č. r.', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 518203, N'Spolu', NULL, 1, 4, 2, 1, 3
    UNION ALL SELECT 518203, N'Životné poistenie', NULL, 1, 5, 1, 4, 4
    UNION ALL SELECT 518203, N'Neživotné poistenie', NULL, 1, 9, 1, 3, 5
    UNION ALL SELECT 518203, N'Spolu ŽP', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 518203, N'GMM', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 518203, N'VFA', NULL, 2, 7, 1, 1, 8
    UNION ALL SELECT 518203, N'PAA', NULL, 2, 8, 1, 1, 9
    UNION ALL SELECT 518203, N'Spolu NP', NULL, 2, 9, 1, 1, 10
    UNION ALL SELECT 518203, N'GMM', NULL, 2, 10, 1, 1, 11
    UNION ALL SELECT 518203, N'PAA', NULL, 2, 11, 1, 1, 12
    UNION ALL SELECT 518203, N'a', NULL, 3, 1, 1, 1, 13
    UNION ALL SELECT 518203, N'b', NULL, 3, 2, 1, 1, 14
    UNION ALL SELECT 518203, N'2', NULL, 3, 3, 1, 1, 15
    UNION ALL SELECT 518203, N'3', NULL, 3, 4, 1, 1, 16
    UNION ALL SELECT 518203, N'4', NULL, 3, 5, 1, 1, 17
    UNION ALL SELECT 518203, N'5', NULL, 3, 6, 1, 1, 18
    UNION ALL SELECT 518203, N'6', NULL, 3, 7, 1, 1, 19
    UNION ALL SELECT 518203, N'7', NULL, 3, 8, 1, 1, 20
    UNION ALL SELECT 518203, N'8', NULL, 3, 9, 1, 1, 21
    UNION ALL SELECT 518203, N'9', NULL, 3, 10, 1, 1, 22
    UNION ALL SELECT 518203, N'10', NULL, 3, 11, 1, 1, 23
    UNION ALL SELECT 518204, N'označenie z dôvodu kontrolných súčtov', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 518204, N'SK', N'EN', 1, 2, 2, 1, 1
    UNION ALL SELECT 518204, N'Spolu ŽP', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 518204, N'GMM', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 518204, N'VFA', NULL, 2, 7, 1, 1, 8
    UNION ALL SELECT 518204, N'PAA', NULL, 2, 8, 1, 1, 9
    UNION ALL SELECT 518204, N'Spolu NP', NULL, 2, 9, 1, 1, 10
    UNION ALL SELECT 518204, N'GMM', NULL, 2, 10, 1, 1, 11
    UNION ALL SELECT 518204, N'PAA', NULL, 2, 11, 1, 1, 12
    UNION ALL SELECT 518204, N'a', NULL, 3, 1, 1, 1, 13
    UNION ALL SELECT 518204, N'b', NULL, 3, 2, 1, 1, 14
    UNION ALL SELECT 518204, N'2', NULL, 3, 3, 1, 1, 15
    UNION ALL SELECT 518204, N'3', NULL, 3, 4, 1, 1, 16
    UNION ALL SELECT 518204, N'4', NULL, 3, 5, 1, 1, 17
    UNION ALL SELECT 518204, N'5', NULL, 3, 6, 1, 1, 18
    UNION ALL SELECT 518204, N'6', NULL, 3, 7, 1, 1, 19
    UNION ALL SELECT 518204, N'7', NULL, 3, 8, 1, 1, 20
    UNION ALL SELECT 518204, N'8', NULL, 3, 9, 1, 1, 21
    UNION ALL SELECT 518204, N'9', NULL, 3, 10, 1, 1, 22
    UNION ALL SELECT 518204, N'10', NULL, 3, 11, 1, 1, 23
    UNION ALL SELECT 518204, N'č. r.', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 518204, N'Spolu', NULL, 1, 4, 2, 1, 3
    UNION ALL SELECT 518204, N'Životné poistenie', NULL, 1, 5, 1, 4, 4
)
INSERT INTO [Template].[Headers]
(
    [TableId], [Text_sk], [Text_en], [RowPosition], [ColumnPosition], [RowSpan], [ColumnSpan], [HeaderOrdinal]
)
SELECT
    t.[Id], n.[Text_sk], n.[Text_en], n.[RowPosition], n.[ColumnPosition], n.[RowSpan], n.[ColumnSpan], n.[HeaderOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Headers] AS e ON e.[TableId] = t.[Id] AND e.[HeaderOrdinal] = n.[HeaderOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 518204 AS [TableErpId], N'Neživotné poistenie' AS [Text_sk], NULL AS [Text_en], 1 AS [RowPosition], 9 AS [ColumnPosition], 1 AS [RowSpan], 3 AS [ColumnSpan], 5 AS [HeaderOrdinal]
    UNION ALL SELECT 202, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 202, N'STRANA PASÍV', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 202, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 202, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 202, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 202, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 202, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 202, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 202, N'5', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 202, N'6', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 702, N'Výdavky', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 702, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 702, N'Za bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 702, N'a', NULL, 2, 1, 1, 1, 3
    UNION ALL SELECT 702, N'b', NULL, 2, 2, 1, 1, 4
    UNION ALL SELECT 702, N'2', NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 1101, N'Označenie', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 1101, N'STRANA AKTÍV', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 1101, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 1101, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 1101, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 1101, N'Netto', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 1101, N'Netto', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 1101, N'a', NULL, 3, 1, 1, 1, 7
    UNION ALL SELECT 1101, N'b', NULL, 3, 2, 1, 1, 8
    UNION ALL SELECT 1101, N'c', NULL, 3, 3, 1, 1, 9
    UNION ALL SELECT 1101, N'1', NULL, 3, 4, 1, 1, 10
    UNION ALL SELECT 1101, N'2', NULL, 3, 5, 1, 1, 11
    UNION ALL SELECT 1602, N'ZÁVÄZKY', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 1602, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 1602, N'Účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 1602, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 1602, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 1602, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 1602, N'3', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 1602, N'4', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 2001, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 2001, N'STRANA AKTÍV', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 2001, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 2001, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 2001, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 1, 1, 4
    UNION ALL SELECT 2001, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 2001, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 2001, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 2001, N'Brutto - časť 1', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 2001, N'Korekcia - časť 2', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 2001, N'Netto 2', NULL, 2, 6, 1, 1, 10
    UNION ALL SELECT 2001, N'Netto 3', NULL, 2, 7, 1, 1, 11
    UNION ALL SELECT 2902, N'STRANA PASÍV', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 2902, N'Číslo riadku', NULL, 1, 3, 1, 1, 1
    UNION ALL SELECT 2902, N'Účtovné obdobie', NULL, 1, 4, 1, 1, 2
    UNION ALL SELECT 2902, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 3
    UNION ALL SELECT 2902, N'a', NULL, 2, 1, 1, 2, 4
    UNION ALL SELECT 2902, N'b', NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 2902, N'5', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 2902, N'6', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 38302, N'VÝDAVKY', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 38302, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 38302, N'Nezdaňovaná činnosť', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 38302, N'Zdaňovaná činnosť', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 38302, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 38302, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 38302, N'3', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 38302, N'4', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 52101, N'Číslo účtu alebo skupiny', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 52101, N'Náklady', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 52101, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 52101, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 52101, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 52101, N'Hlavná činnosť', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 52101, N'Podnikateľská činnosť', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 52101, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 52101, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 52101, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 52101, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 52101, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 52101, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 52101, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 52101, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 54202, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 54202, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 54202, N'Bežné účtovné obdobie', NULL, 1, 4, 2, 1, 2
    UNION ALL SELECT 54202, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 2, 1, 3
    UNION ALL SELECT 54202, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 54202, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 54202, N'a', NULL, 3, 1, 1, 1, 6
    UNION ALL SELECT 54202, N'b', NULL, 3, 2, 1, 1, 7
    UNION ALL SELECT 54202, N'c', NULL, 3, 3, 1, 1, 8
    UNION ALL SELECT 54202, N'5', NULL, 3, 4, 1, 1, 9
    UNION ALL SELECT 54202, N'6', NULL, 3, 5, 1, 1, 10
    UNION ALL SELECT 68701, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 68701, N'STRANA AKTÍV', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 68701, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 68701, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 68701, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 68701, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 68701, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 68701, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 68701, N'Netto 1', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 68701, N'Netto 2', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 69901, N'Brutto - časť 1', N'Gross - Part 1', 2, 4, 1, 1, 8
    UNION ALL SELECT 69901, N'Korekcia - časť 2', N'Correction-Part 2', 2, 5, 1, 1, 9
    UNION ALL SELECT 69901, N'Netto 2', N'Net 2', 2, 6, 1, 1, 10
    UNION ALL SELECT 69901, N'Netto 3', N'Net 3', 2, 7, 1, 1, 11
    UNION ALL SELECT 69901, N'Označenie', N'Designation', 1, 1, 1, 1, 0
    UNION ALL SELECT 69901, N'STRANA AKTÍV', N'ASSETS', 1, 2, 1, 1, 1
    UNION ALL SELECT 69901, N'Číslo riadku', N'Line No.', 1, 3, 1, 1, 2
    UNION ALL SELECT 69901, N'Bežné účtovné obdobie', N'Current accounting period', 1, 4, 1, 3, 3
    UNION ALL SELECT 69901, N'Bezprostredne predchádzajúce účtovné obdobie', N'Preceding accounting period', 1, 7, 1, 1, 4
    UNION ALL SELECT 69901, N'a', N'a', 2, 1, 1, 1, 5
    UNION ALL SELECT 69901, N'b', N'b', 2, 2, 1, 1, 6
    UNION ALL SELECT 69901, N'c', N'c', 2, 3, 1, 1, 7
    UNION ALL SELECT 71602, N'Výdavky', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 71602, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 71602, N'Za bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 71602, N'a', NULL, 2, 1, 1, 1, 3
    UNION ALL SELECT 71602, N'b', NULL, 2, 2, 1, 1, 4
    UNION ALL SELECT 71602, N'2', NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 73802, N'POLOŽKA', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 73802, N'Číslo riadku', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 73802, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 73802, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 2, 1, 3
    UNION ALL SELECT 73802, N'Netto', NULL, 2, 3, 1, 1, 4
    UNION ALL SELECT 73802, N'a', NULL, 3, 1, 1, 1, 5
    UNION ALL SELECT 73802, N'b', NULL, 3, 2, 1, 1, 6
    UNION ALL SELECT 73802, N'3', NULL, 3, 3, 1, 1, 7
    UNION ALL SELECT 73802, N'4', NULL, 3, 4, 1, 1, 8
    UNION ALL SELECT 94103, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 94103, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 94103, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 94103, N'Predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 94103, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 94103, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 94103, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 94103, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 94207, NULL, NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 94207, N'č. r.', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 94207, N'Spolu', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 94207, N'Životné poistenie', NULL, 1, 4, 1, 2, 3
    UNION ALL SELECT 94207, N'Neživotné poistenie', NULL, 1, 6, 1, 1, 4
    UNION ALL SELECT 94207, N'Aktívne zaistenie', NULL, 1, 7, 1, 3, 5
    UNION ALL SELECT 94207, N'Spolu', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 94207, N'B9', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 94207, N'Spolu', NULL, 2, 6, 1, 1, 8
    UNION ALL SELECT 94207, N'Spolu', NULL, 2, 7, 1, 1, 9
    UNION ALL SELECT 94207, N'ŽP', NULL, 2, 8, 1, 1, 10
    UNION ALL SELECT 94207, N'NP', NULL, 2, 9, 1, 1, 11
    UNION ALL SELECT 94207, N'a', NULL, 3, 1, 1, 1, 12
    UNION ALL SELECT 94207, N'b', NULL, 3, 2, 1, 1, 13
    UNION ALL SELECT 94207, N'1', NULL, 3, 3, 1, 1, 14
    UNION ALL SELECT 94207, N'2', NULL, 3, 4, 1, 1, 15
    UNION ALL SELECT 94207, N'3', NULL, 3, 5, 1, 1, 16
    UNION ALL SELECT 94207, N'4', NULL, 3, 6, 1, 1, 17
    UNION ALL SELECT 94207, N'5', NULL, 3, 7, 1, 1, 18
    UNION ALL SELECT 94207, N'6', NULL, 3, 8, 1, 1, 19
    UNION ALL SELECT 94207, N'7', NULL, 3, 9, 1, 1, 20
    UNION ALL SELECT 102102, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 102102, N'Položka číslo', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 102102, N'Bežné účtovné obdobie', NULL, 1, 4, 2, 1, 2
    UNION ALL SELECT 102102, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 2, 1, 3
    UNION ALL SELECT 102102, N'Číslo riadku', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 102102, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 102102, N'a', NULL, 3, 1, 1, 1, 6
    UNION ALL SELECT 102102, N'b', NULL, 3, 2, 1, 1, 7
    UNION ALL SELECT 102102, N'c', NULL, 3, 3, 1, 1, 8
    UNION ALL SELECT 102102, N'3', NULL, 3, 4, 1, 1, 9
    UNION ALL SELECT 102102, N'4', NULL, 3, 5, 1, 1, 10
    UNION ALL SELECT 112103, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 112103, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 112103, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 112103, N'Predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 112103, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 112103, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 112103, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 112103, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 116403, N'MAJETOK', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 116403, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 116403, N'Účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 116403, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 116403, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 116403, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 116403, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 116403, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 518202, N'4', NULL, 4, 5, 1, 1, 15
    UNION ALL SELECT 518202, N'5', NULL, 4, 6, 1, 1, 16
    UNION ALL SELECT 518202, N'6', NULL, 4, 7, 1, 1, 17
    UNION ALL SELECT 518202, N'7', NULL, 4, 8, 1, 1, 18
    UNION ALL SELECT 518202, N'8', NULL, 4, 9, 1, 1, 19
    UNION ALL SELECT 518202, N'a', NULL, 4, 1, 1, 1, 11
    UNION ALL SELECT 518202, N'b', NULL, 4, 2, 1, 1, 12
    UNION ALL SELECT 518202, N'2', NULL, 4, 3, 1, 1, 13
    UNION ALL SELECT 518202, N'3', NULL, 4, 4, 1, 1, 14
    UNION ALL SELECT 518202, N'označenie z dôvodu kontrolných súčtov', NULL, 1, 1, 3, 1, 0
    UNION ALL SELECT 518202, N'SK', N'EN', 1, 2, 3, 1, 1
    UNION ALL SELECT 518202, N'č. r.', NULL, 1, 3, 3, 1, 2
    UNION ALL SELECT 518202, N'Stav brutto', NULL, 1, 4, 3, 1, 3
    UNION ALL SELECT 518202, N'Oprávky a opravné položky', NULL, 1, 5, 3, 1, 4
    UNION ALL SELECT 518202, N'Stav netto', NULL, 1, 6, 1, 4, 5
    UNION ALL SELECT 518202, N'Spolu', NULL, 2, 6, 2, 1, 6
)
INSERT INTO [Template].[Headers]
(
    [TableId], [Text_sk], [Text_en], [RowPosition], [ColumnPosition], [RowSpan], [ColumnSpan], [HeaderOrdinal]
)
SELECT
    t.[Id], n.[Text_sk], n.[Text_en], n.[RowPosition], n.[ColumnPosition], n.[RowSpan], n.[ColumnSpan], n.[HeaderOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Headers] AS e ON e.[TableId] = t.[Id] AND e.[HeaderOrdinal] = n.[HeaderOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 518202 AS [TableErpId], N'ŽP' AS [Text_sk], NULL AS [Text_en], 2 AS [RowPosition], 7 AS [ColumnPosition], 1 AS [RowSpan], 2 AS [ColumnSpan], 7 AS [HeaderOrdinal]
    UNION ALL SELECT 518202, N'NP', NULL, 2, 9, 2, 1, 8
    UNION ALL SELECT 518202, N'Spolu ŽP', NULL, 3, 7, 1, 1, 9
    UNION ALL SELECT 518202, N'z toho: VFA', NULL, 3, 8, 1, 1, 10
    UNION ALL SELECT 518303, N'Záväzky po lehote splatnosti', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 518303, N'Číslo riadku', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 518303, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 518303, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 518303, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 518303, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 518303, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 518303, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 101, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 101, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 101, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 101, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 101, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 101, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 101, N'Číslo účtu alebo skupiny', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 101, N'Náklady', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 101, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 101, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 101, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 101, N'Hlavná činnosť', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 101, N'Podnikateľská činnosť', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 101, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 101, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 201, N'Korekcia', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 201, N'Netto', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 201, N'Netto', NULL, 2, 7, 1, 1, 8
    UNION ALL SELECT 201, N'a', NULL, 3, 1, 1, 1, 9
    UNION ALL SELECT 201, N'b', NULL, 3, 2, 1, 1, 10
    UNION ALL SELECT 201, N'c', NULL, 3, 3, 1, 1, 11
    UNION ALL SELECT 201, N'1', NULL, 3, 4, 1, 1, 12
    UNION ALL SELECT 201, N'2', NULL, 3, 5, 1, 1, 13
    UNION ALL SELECT 201, N'3', NULL, 3, 6, 1, 1, 14
    UNION ALL SELECT 201, N'4', NULL, 3, 7, 1, 1, 15
    UNION ALL SELECT 201, N'Označenie', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 201, N'STRANA AKTÍV', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 201, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 201, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 201, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 1, 1, 4
    UNION ALL SELECT 201, N'Brutto', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 701, N'Príjmy', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 701, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 701, N'Za bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 701, N'a', NULL, 2, 1, 1, 1, 3
    UNION ALL SELECT 701, N'b', NULL, 2, 2, 1, 1, 4
    UNION ALL SELECT 701, N'1', NULL, 2, 3, 1, 1, 5
    UNION ALL SELECT 801, N'Majetok', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 801, N'Riadok', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 801, N'Za bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 801, N'Za bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 801, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 801, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 801, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 801, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 1902, N'Číslo účtu', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 1902, N'Hlavná nezdaňovaná', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 1902, N'Podnikateľská zdaňovaná', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 1902, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 1902, N'Činnosť', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 1902, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 1902, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 1902, N'Výnosy', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 1902, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 1902, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 1902, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 1902, N'1', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 1902, N'2', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 1902, N'3', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 1902, N'4', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 2002, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 2002, N'STRANA PASÍV', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 2002, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 2002, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 2002, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 2002, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 2002, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 2002, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 2002, N'4', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 2002, N'5', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 2901, N'STRANA AKTÍV', NULL, 1, 1, 2, 2, 0
    UNION ALL SELECT 2901, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 2901, N'Účtovné obdobie', NULL, 1, 4, 1, 3, 2
    UNION ALL SELECT 2901, N'Bezprostrredne predchádzajúce účtovné obdobie', NULL, 1, 7, 1, 1, 3
    UNION ALL SELECT 2901, N'Brutto', NULL, 2, 4, 1, 1, 4
    UNION ALL SELECT 2901, N'Korekcia', NULL, 2, 5, 1, 1, 5
    UNION ALL SELECT 2901, N'Netto', NULL, 2, 6, 1, 1, 6
    UNION ALL SELECT 2901, N'Netto', NULL, 2, 7, 1, 1, 7
    UNION ALL SELECT 2901, N'a', NULL, 3, 1, 1, 2, 8
    UNION ALL SELECT 2901, N'b', NULL, 3, 3, 1, 1, 9
    UNION ALL SELECT 2901, N'1', NULL, 3, 4, 1, 1, 10
    UNION ALL SELECT 2901, N'2', NULL, 3, 5, 1, 1, 11
    UNION ALL SELECT 2901, N'3', NULL, 3, 6, 1, 1, 12
    UNION ALL SELECT 2901, N'4', NULL, 3, 7, 1, 1, 13
    UNION ALL SELECT 3001, N'10', NULL, 3, 7, 1, 1, 14
    UNION ALL SELECT 3001, N'Číslo účtu', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 3001, N'Náklady', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 3001, N'Číslo riadku', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 3001, N'Účtovné obdobie', NULL, 1, 4, 1, 3, 3
    UNION ALL SELECT 3001, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 4
    UNION ALL SELECT 3001, N'Nezdaňovaná činnosť', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 3001, N'Zdaňovaná činnosť', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 3001, N'Spolu', NULL, 2, 6, 1, 1, 7
    UNION ALL SELECT 3001, N'a', NULL, 3, 1, 1, 1, 8
    UNION ALL SELECT 3001, N'b', NULL, 3, 2, 1, 1, 9
    UNION ALL SELECT 3001, N'c', NULL, 3, 3, 1, 1, 10
    UNION ALL SELECT 3001, N'7', NULL, 3, 4, 1, 1, 11
    UNION ALL SELECT 3001, N'8', NULL, 3, 5, 1, 1, 12
    UNION ALL SELECT 3001, N'9', NULL, 3, 6, 1, 1, 13
    UNION ALL SELECT 54201, N'Položka', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 54201, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 54201, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 3, 2
    UNION ALL SELECT 54201, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 7, 2, 1, 3
    UNION ALL SELECT 54201, N'Číslo', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 54201, N'Názov', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 54201, N'Brutto', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 54201, N'Korekcia', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 54201, N'Netto', NULL, 2, 6, 1, 1, 8
    UNION ALL SELECT 54201, N'a', NULL, 3, 1, 1, 1, 9
    UNION ALL SELECT 54201, N'b', NULL, 3, 2, 1, 1, 10
    UNION ALL SELECT 54201, N'c', NULL, 3, 3, 1, 1, 11
    UNION ALL SELECT 54201, N'1', NULL, 3, 4, 1, 1, 12
    UNION ALL SELECT 54201, N'2', NULL, 3, 5, 1, 1, 13
    UNION ALL SELECT 54201, N'3', NULL, 3, 6, 1, 1, 14
    UNION ALL SELECT 54201, N'4', NULL, 3, 7, 1, 1, 15
    UNION ALL SELECT 66101, N'2', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 66101, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 66101, N'Názov položky', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 66101, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 66101, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 66101, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 66101, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 66101, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 66101, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 66101, N'1', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 66201, N'POLOŽKA', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 66201, N'Číslo riadku', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 66201, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 3, 2
    UNION ALL SELECT 66201, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 6, 2, 1, 3
    UNION ALL SELECT 66201, N'Brutto', NULL, 2, 3, 1, 1, 4
    UNION ALL SELECT 66201, N'Korekcia', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 66201, N'Netto', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 66201, N'a', NULL, 3, 1, 1, 1, 7
    UNION ALL SELECT 66201, N'b', NULL, 3, 2, 1, 1, 8
    UNION ALL SELECT 66201, N'1', NULL, 3, 3, 1, 1, 9
    UNION ALL SELECT 66201, N'2', NULL, 3, 4, 1, 1, 10
    UNION ALL SELECT 66201, N'3', NULL, 3, 5, 1, 1, 11
    UNION ALL SELECT 66201, N'4', NULL, 3, 6, 1, 1, 12
    UNION ALL SELECT 66303, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 66303, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 66303, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 66303, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 66303, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 66303, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 66303, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 66303, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 68402, N'Označenie', N'Designation', 1, 1, 1, 1, 0
    UNION ALL SELECT 68402, N'STRANA PASÍV', N'LIABILITIES AND EQUITY', 1, 2, 1, 1, 1
    UNION ALL SELECT 68402, N'Číslo riadku', N'Line No.', 1, 3, 1, 1, 2
    UNION ALL SELECT 68402, N'20xx', N'20xx', 1, 4, 1, 1, 3
    UNION ALL SELECT 68402, N'20xx-1', N'20xx-1', 1, 5, 1, 1, 4
    UNION ALL SELECT 68402, N'a', N'a', 2, 1, 1, 1, 5
    UNION ALL SELECT 68402, N'b', N'b', 2, 2, 1, 1, 6
    UNION ALL SELECT 68402, N'c', N'c', 2, 3, 1, 1, 7
    UNION ALL SELECT 68402, N'3', N'3', 2, 4, 1, 1, 8
    UNION ALL SELECT 68402, N'4', N'4', 2, 5, 1, 1, 9
    UNION ALL SELECT 73301, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 73301, N'Názov položky', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 73301, N'Číslo riadku', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 73301, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 73301, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 4
    UNION ALL SELECT 73301, N'a', NULL, 2, 1, 1, 1, 5
    UNION ALL SELECT 73301, N'b', NULL, 2, 2, 1, 1, 6
    UNION ALL SELECT 73301, N'c', NULL, 2, 3, 1, 1, 7
    UNION ALL SELECT 73301, N'1', NULL, 2, 4, 1, 1, 8
    UNION ALL SELECT 73301, N'2', NULL, 2, 5, 1, 1, 9
    UNION ALL SELECT 73801, N'POLOŽKA', NULL, 1, 1, 2, 1, 0
    UNION ALL SELECT 73801, N'Číslo riadku', NULL, 1, 2, 2, 1, 1
    UNION ALL SELECT 73801, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 3, 2
    UNION ALL SELECT 73801, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 6, 2, 1, 3
    UNION ALL SELECT 73801, N'Brutto', NULL, 2, 3, 1, 1, 4
    UNION ALL SELECT 73801, N'Korekcia', NULL, 2, 4, 1, 1, 5
    UNION ALL SELECT 73801, N'Netto', NULL, 2, 5, 1, 1, 6
    UNION ALL SELECT 73801, N'a', NULL, 3, 1, 1, 1, 7
    UNION ALL SELECT 73801, N'b', NULL, 3, 2, 1, 1, 8
    UNION ALL SELECT 73801, N'2', NULL, 3, 4, 1, 1, 10
    UNION ALL SELECT 73801, N'1', NULL, 3, 3, 1, 1, 9
    UNION ALL SELECT 73801, N'3', NULL, 3, 5, 1, 1, 11
    UNION ALL SELECT 73801, N'4', NULL, 3, 6, 1, 1, 12
    UNION ALL SELECT 94102, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 94102, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 94102, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 94102, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 94102, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 94102, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 94102, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 94102, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 94201, N'Aktíva', NULL, 1, 1, 2, 1, 0
)
INSERT INTO [Template].[Headers]
(
    [TableId], [Text_sk], [Text_en], [RowPosition], [ColumnPosition], [RowSpan], [ColumnSpan], [HeaderOrdinal]
)
SELECT
    t.[Id], n.[Text_sk], n.[Text_en], n.[RowPosition], n.[ColumnPosition], n.[RowSpan], n.[ColumnSpan], n.[HeaderOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Headers] AS e ON e.[TableId] = t.[Id] AND e.[HeaderOrdinal] = n.[HeaderOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 94201 AS [TableErpId], N'č. r.' AS [Text_sk], NULL AS [Text_en], 1 AS [RowPosition], 2 AS [ColumnPosition], 2 AS [RowSpan], 1 AS [ColumnSpan], 1 AS [HeaderOrdinal]
    UNION ALL SELECT 94201, N'Stav brutto', NULL, 1, 3, 2, 1, 2
    UNION ALL SELECT 94201, N'Oprávky a opravné položky', NULL, 1, 4, 2, 1, 3
    UNION ALL SELECT 94201, N'Stav (netto)', NULL, 1, 5, 1, 5, 4
    UNION ALL SELECT 94201, N'Spolu', NULL, 2, 5, 1, 1, 5
    UNION ALL SELECT 94201, N'ŽP a AZ - ŽP', NULL, 2, 6, 1, 1, 6
    UNION ALL SELECT 94201, N'NP a AZ - NP', NULL, 2, 7, 1, 1, 7
    UNION ALL SELECT 94201, N'AZ - ŽP', NULL, 2, 8, 1, 1, 8
    UNION ALL SELECT 94201, N'AZ - NP', NULL, 2, 9, 1, 1, 9
    UNION ALL SELECT 94201, N'a', NULL, 3, 1, 1, 1, 10
    UNION ALL SELECT 94201, N'b', NULL, 3, 2, 1, 1, 11
    UNION ALL SELECT 94201, N'1', NULL, 3, 3, 1, 1, 12
    UNION ALL SELECT 94201, N'2', NULL, 3, 4, 1, 1, 13
    UNION ALL SELECT 94201, N'3', NULL, 3, 5, 1, 1, 14
    UNION ALL SELECT 94201, N'4', NULL, 3, 6, 1, 1, 15
    UNION ALL SELECT 94201, N'5', NULL, 3, 7, 1, 1, 16
    UNION ALL SELECT 94201, N'6', NULL, 3, 8, 1, 1, 17
    UNION ALL SELECT 94201, N'7', NULL, 3, 9, 1, 1, 18
    UNION ALL SELECT 112102, N'Označenie', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 112102, N'POLOŽKA', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 112102, N'Bežné účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 112102, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 112102, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 112102, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 112102, N'1', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 112102, N'2', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 114101, N'STRANA AKTÍV', NULL, 1, 1, 2, 2, 0
    UNION ALL SELECT 114101, N'Číslo riadku', NULL, 1, 3, 2, 1, 1
    UNION ALL SELECT 114101, N'Účtovné obdobie', NULL, 1, 4, 1, 3, 2
    UNION ALL SELECT 114101, N'Bezprostrredne predchádzajúce účtovné obdobie', NULL, 1, 7, 1, 1, 3
    UNION ALL SELECT 114101, N'Brutto', NULL, 2, 4, 1, 1, 4
    UNION ALL SELECT 114101, N'Korekcia', NULL, 2, 5, 1, 1, 5
    UNION ALL SELECT 114101, N'Netto', NULL, 2, 6, 1, 1, 6
    UNION ALL SELECT 114101, N'Netto', NULL, 2, 7, 1, 1, 7
    UNION ALL SELECT 114101, N'a', NULL, 3, 1, 1, 2, 8
    UNION ALL SELECT 114101, N'b', NULL, 3, 3, 1, 1, 9
    UNION ALL SELECT 114101, N'1', NULL, 3, 4, 1, 1, 10
    UNION ALL SELECT 114101, N'2', NULL, 3, 5, 1, 1, 11
    UNION ALL SELECT 114101, N'3', NULL, 3, 6, 1, 1, 12
    UNION ALL SELECT 114101, N'4', NULL, 3, 7, 1, 1, 13
    UNION ALL SELECT 116404, N'ZÁVÄZKY', NULL, 1, 1, 1, 1, 0
    UNION ALL SELECT 116404, N'Č. r.', NULL, 1, 2, 1, 1, 1
    UNION ALL SELECT 116404, N'Účtovné obdobie', NULL, 1, 3, 1, 1, 2
    UNION ALL SELECT 116404, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 4, 1, 1, 3
    UNION ALL SELECT 116404, N'a', NULL, 2, 1, 1, 1, 4
    UNION ALL SELECT 116404, N'b', NULL, 2, 2, 1, 1, 5
    UNION ALL SELECT 116404, N'3', NULL, 2, 3, 1, 1, 6
    UNION ALL SELECT 116404, N'4', NULL, 2, 4, 1, 1, 7
    UNION ALL SELECT 118002, N'a', NULL, 2, 1, 1, 2, 4
    UNION ALL SELECT 118002, N'Strana pasív', NULL, 1, 1, 1, 2, 0
    UNION ALL SELECT 118002, N'č.r.', NULL, 1, 3, 1, 1, 1
    UNION ALL SELECT 118002, N'Bežné účtovné obdobie', NULL, 1, 4, 1, 1, 2
    UNION ALL SELECT 118002, N'Bezprostredne predchádzajúce účtovné obdobie', NULL, 1, 5, 1, 1, 3
    UNION ALL SELECT 118002, N'5', NULL, 2, 4, 1, 1, 6
    UNION ALL SELECT 118002, N'6', NULL, 2, 5, 1, 1, 7
    UNION ALL SELECT 118002, N'b', NULL, 2, 3, 1, 1, 5
)
INSERT INTO [Template].[Headers]
(
    [TableId], [Text_sk], [Text_en], [RowPosition], [ColumnPosition], [RowSpan], [ColumnSpan], [HeaderOrdinal]
)
SELECT
    t.[Id], n.[Text_sk], n.[Text_en], n.[RowPosition], n.[ColumnPosition], n.[RowSpan], n.[ColumnSpan], n.[HeaderOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Headers] AS e ON e.[TableId] = t.[Id] AND e.[HeaderOrdinal] = n.[HeaderOrdinal]
WHERE e.[Id] IS NULL;

COMMIT TRANSACTION;
PRINT '050 template-header population completed.';
GO
