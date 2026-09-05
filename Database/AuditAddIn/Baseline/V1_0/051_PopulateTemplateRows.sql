/* Template rows; generated in bounded CTE batches for reliable compilation. */
USE [AuditAddIn];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

;WITH [NewData] AS
(
    SELECT 802 AS [TableErpId], 16 AS [RowNumber], NULL AS [Designation], N'Rezervy' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 0 AS [RowOrdinal]
    UNION ALL SELECT 802, 17, NULL, N'Záväzky', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 802, 18, NULL, N'Úvery', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 802, 19, NULL, N'Opravná položka k odplatne nadobudnutému majetku (pasívna)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 802, 20, NULL, N'Záväzky celkom súčet (r. 16 až 19)', NULL, 1, NULL, NULL, 4
    UNION ALL SELECT 802, 21, NULL, N'Rozdiel majetku a záväzkov (r. 15 - r. 20)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 902, 118, NULL, N'VLASTNÉ IMANIE A ZÁVÄZKY r. 119 + r. 130 + r. 185 + r. 188', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 902, 119, N'A.', N'Vlastné imanie r. 120 + r. 123 + r. 126 + r. 129', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 902, 120, N'A.I.', N'Oceňovacie rozdiely súčet (r. 121 + r. 122)', NULL, 1, NULL, NULL, 2
    UNION ALL SELECT 902, 121, N'A.I.1.', N'Oceňovacie rozdiely z precenenia majetku a záväzkov (+/– 414)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 902, 122, N'2.', N'Oceňovacie rozdiely z kapitálových účastín (+/– 415)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 902, 123, N'A.II.', N'Fondy súčet (r. 124 + r. 125)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 902, 124, N'A.II.1.', N'Zákonný rezervný fond (421)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 902, 125, N'2.', N'Ostatné fondy (427)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 902, 126, N'A.III.', N'Výsledok hospodárenia (+/-) súčet (r. 127 až 128)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 902, 127, N'A.III.1.', N'Nevysporiadaný výsledok hospodárenia minulých rokov (+/– 428)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 902, 128, N'2.', N'Výsledok hospodárenia za účtovné obdobie (+/–) r. 001 - (r.120 + r.123 + r.127 + r.129 + r. 130 + r. 185 + r. 188)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 902, 129, N'A.IV.', N'Podiely iných učtovných jednotiek', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 902, 130, N'B.', N'Záväzky súčet r. 131 + r. 136 + r. 144 + r. 156 + r. 178', NULL, 1, NULL, NULL, 12
    UNION ALL SELECT 902, 131, N'B.I.', N'Rezervy súčet (r. 132 až 135)', NULL, 1, NULL, NULL, 13
    UNION ALL SELECT 902, 132, N'B.I.1.', N'Rezervy zákonné dlhodobé (451AÚ)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 902, 133, N'2.', N'Ostatné rezervy (459AÚ)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 902, 134, N'3.', N'Rezervy zákonné krátkodobé (323AÚ, 451AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 902, 135, N'4.', N'Ostatné krátkodobé rezervy (323AÚ, 459AÚ)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 902, 136, N'B.II.', N'Zúčtovanie medzi subjektami verejnej správy súčet (r. 137 až r. 143)', NULL, 1, NULL, NULL, 18
    UNION ALL SELECT 902, 137, N'B.II.1.', N'Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa (351)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 902, 138, N'2.', N'Zúčtovanie transferov štátneho rozpočtu (353)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 902, 139, N'3.', N'Zúčtovanie transferov rozpočtu obce a vyššieho územného celku (355)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 902, 140, N'4.', N'Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku (356)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 902, 141, N'5.', N'Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku (357)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 902, 142, N'6.', N'Zúčtovanie transferov zo štátneho rozpočtu iným subjektom (358)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 902, 143, N'7.', N'Zúčtovanie transferov medzi subjektami verejnej správy (359)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 902, 144, N'B.III.', N'Dlhodobé záväzky súčet (r. 145 až 153 + r. 155)', NULL, 1, NULL, NULL, 26
    UNION ALL SELECT 902, 145, N'B.III.1.', N'Ostatné dlhodobé záväzky (479AÚ)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 902, 146, N'2.', N'Dlhodobé prijaté preddavky (475AÚ)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 902, 147, N'3.', N'Dlhodobé zmenky na úhradu (478AÚ)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 902, 148, N'4.', N'Záväzky zo sociálneho fondu (472)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 902, 149, N'5.', N'Záväzky z nájmu (474AÚ)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 902, 150, N'6.', N'Dlhodobé nevyfakturované dodávky (476AÚ)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 902, 151, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 902, 152, N'8.', N'Predané opcie (377AÚ)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 902, 153, N'9.', N'Iné záväzky (379AÚ)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 902, 154, NULL, N'z toho: odložený daňový záväzok', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 902, 155, N'10.', N'Vydané dlhopisy dlhodobé (473AÚ ) - (255AÚ)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 902, 156, N'B.IV.', N'Krátkodobé záväzky súčet (r. 157 až 177)', NULL, 1, NULL, NULL, 38
    UNION ALL SELECT 902, 157, N'B.IV.1.', N'Dodávatelia (321)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 902, 158, N'2.', N'Zmenky na úhradu (322, 478AÚ)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 902, 159, N'3.', N'Prijaté preddavky (324, 475AÚ)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 902, 160, N'4.', N'Ostatné záväzky (325, 479AÚ)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 902, 161, N'5.', N'Nevyfakturované dodávky (326, 476AÚ)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 902, 162, N'6.', N'Záväzky z nájmu (474AÚ)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 902, 163, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 902, 164, N'8.', N'Predané opcie (377AÚ)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 902, 165, N'9.', N'Iné záväzky (379AÚ)', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 902, 166, N'10.', N'Záväzky z upísaných nesplatených cenných papierov a vkladov (367)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 902, 167, N'11.', N'Záväzky voči združeniu (368)', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 902, 168, N'12.', N'Zamestnanci (331)', NULL, 0, N'Krátkodobé záväzky - Dan z pridanej hodnoty', NULL, 50
    UNION ALL SELECT 902, 169, N'13.', N'Ostatné záväzky voči zamestnancom (333)', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 902, 170, N'14.', N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia (336)', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 902, 171, N'15.', N'Daň z príjmov (341)', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 902, 172, N'16.', N'Ostatné priame dane (342)', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 902, 173, N'17.', N'Daň z pridanej hodnoty (343)', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 902, 174, N'18.', N'Ostatné dane a poplatky (345)', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 902, 175, N'19.', N'Spojovací účet pri združení (396AÚ)', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 902, 176, N'20.', N'Zúčtovanie s Európskymi spoločenstvami (371AÚ)', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 902, 177, N'21.', N'Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy (372AÚ)', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 902, 178, N'B.V.', N'Bankové úvery a výpomoci súčet (r. 179 až 184)', NULL, 1, NULL, NULL, 60
    UNION ALL SELECT 902, 179, N'B.V.1.', N'Bankové úvery dlhodobé (461AÚ )', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 902, 180, N'2.', N'Bežné bankové úvery (461AÚ, 221AÚ, 231, 232)', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 902, 181, N'3.', N'Vydané dlhopisy krátkodobé (473AÚ, 241 ) - (255AÚ)', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 902, 182, N'4.', N'Ostatné krátkodobé finančné výpomoci (249)', NULL, 0, NULL, NULL, 64
    UNION ALL SELECT 902, 183, N'5.', N'Prijaté návratné finančné výpomoci od subjektov verejnej správy dlhodobé (273AÚ)', NULL, 0, NULL, NULL, 65
    UNION ALL SELECT 902, 184, N'6.', N'Prijaté návratné finančné výpomoci od subjektov verejnej správy krátkodobé (273AÚ)', NULL, 0, NULL, NULL, 66
    UNION ALL SELECT 902, 185, N'C.', N'Časové rozlíšenie súčet (r. 186 + r. 187)', NULL, 1, NULL, NULL, 67
    UNION ALL SELECT 902, 186, N'C.1.', N'Výdavky budúcich období (383)', NULL, 0, NULL, NULL, 68
    UNION ALL SELECT 902, 187, N'2.', N'Výnosy budúcich období (384)', NULL, 0, NULL, NULL, 69
    UNION ALL SELECT 902, 188, N'D.', N'Vzťahy k účtom klientov štátnej pokladnice (účtová skupina 20)', NULL, 0, NULL, NULL, 70
    UNION ALL SELECT 902, 999, NULL, N'KONTROLNÉ ČÍSLO súčet (r. 118 až 188)', NULL, 1, NULL, NULL, 71
    UNION ALL SELECT 52202, 115, NULL, N'VLASTNÉ IMANIE A ZÁVÄZKY r. 116 + r. 126 + r. 180 + r. 183', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 52202, 116, N'A.', N'Vlastné imanie r. 117 + r. 120 + r. 123', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 52202, 117, N'A.I.', N'Oceňovacie rozdiely súčet (r. 118 + r. 119)', NULL, 1, NULL, NULL, 2
    UNION ALL SELECT 52202, 118, N'A.I.1.', N'Oceňovacie rozdiely z precenenia majetku a záväzkov (+/– 414)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 52202, 119, N'2.', N'Oceňovacie rozdiely z kapitálových účastín (+/– 415)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 52202, 120, N'A.II.', N'Fondy súčet (r. 121 + r. 122)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 52202, 121, N'A.II.1.', N'Zákonný rezervný fond (421)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 52202, 122, N'2.', N'Ostatné fondy (427)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 52202, 123, N'A.III.', N'Výsledok hospodárenia (+/-) súčet (r. 124 až 125)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 52202, 124, N'A.III.1.', N'Nevysporiadaný výsledok hospodárenia minulých rokov (+/– 428)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 52202, 125, N'2.', N'Výsledok hospodárenia za účtovné obdobie (+/–) r. 001 - (r. 117 + r. 120 +r.124+ r. 126 + r. 180 + r. 183)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 52202, 126, N'B.', N'Záväzky súčet r. 127 + r. 132 + r. 140 + r. 151 + r. 173', NULL, 1, NULL, NULL, 11
    UNION ALL SELECT 52202, 127, N'B.I.', N'Rezervy súčet (r. 128 až 131)', NULL, 1, NULL, NULL, 12
    UNION ALL SELECT 52202, 128, N'B.I.1.', N'Rezervy zákonné dlhodobé (451AÚ)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 52202, 129, N'2.', N'Ostatné rezervy (459AÚ)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 52202, 130, N'3.', N'Rezervy zákonné krátkodobé (323AÚ, 451AÚ)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 52202, 131, N'4.', N'Ostatné krátkodobé rezervy (323AÚ, 459AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 52202, 132, N'B.II.', N'Zúčtovanie medzi subjektami verejnej správy súčet (r. 133 až r. 139)', NULL, 1, NULL, NULL, 17
    UNION ALL SELECT 52202, 133, N'B.II.1.', N'Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa (351)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 52202, 134, N'2.', N'Zúčtovanie transferov štátneho rozpočtu (353)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 52202, 135, N'3.', N'Zúčtovanie transferov rozpočtu obce a vyššieho územného celku (355)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 52202, 136, N'4.', N'Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku (356)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 52202, 137, N'5.', N'Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku (357)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 52202, 138, N'6.', N'Zúčtovanie transferov zo štátneho rozpočtu iným subjektom (358)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 52202, 139, N'7.', N'Zúčtovanie transferov medzi subjektami verejnej správy (359)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 52202, 140, N'B.III.', N'Dlhodobé záväzky súčet (r. 141 až 150)', NULL, 1, NULL, NULL, 25
    UNION ALL SELECT 52202, 141, N'B.III.1.', N'Ostatné dlhodobé záväzky (479AÚ)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 52202, 142, N'2.', N'Dlhodobé prijaté preddavky (475AÚ)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 52202, 143, N'3.', N'Dlhodobé zmenky na úhradu (478AÚ)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 52202, 144, N'4.', N'Záväzky zo sociálneho fondu (472)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 52202, 145, N'5.', N'Záväzky z nájmu (474AÚ)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 52202, 146, N'6.', N'Dlhodobé nevyfakturované dodávky (476AÚ)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 52202, 147, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 52202, 148, N'8.', N'Predané opcie (377AÚ)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 52202, 149, N'9.', N'Iné záväzky (379AÚ)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 52202, 150, N'10.', N'Vydané dlhopisy dlhodobé (473AÚ) - (255AÚ)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 52202, 151, N'B.IV.', N'Krátkodobé záväzky súčet (r. 152 až 172)', NULL, 1, NULL, NULL, 36
    UNION ALL SELECT 52202, 152, N'B.IV.1.', N'Dodávatelia (321)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 1701, 16, NULL, N'Základné stádo a ťažné zvieratá 026 - (086 + 092AÚ)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 1701, 17, NULL, N'Drobný dlhodobý hmotný majetok 028 - (088 + 092AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 1701, 18, NULL, N'Ostatný dlhodobý hmotný majetok 029 - (089 +092AÚ)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 1701, 19, NULL, N'Obstaranie dlhodobého hmotného majetku (042 - 094)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 1701, 20, NULL, N'Poskytnuté preddavky na dlhodobý hmotný majetok (052 - 095AÚ)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 1701, 21, N'3.', N'Dlhodobý finančný majetok r. 022 až r. 028', NULL, 1, NULL, NULL, 20
    UNION ALL SELECT 1701, 22, NULL, N'Podielové cenné papiere a podiely v obchodných spoločnostiach v ovládanej osobe (061- 096 AÚ)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 1701, 23, NULL, N'Podielové cenné papiere a podiely v obchodných spoločnostiach s podstatným vplyvom (062 - 096 AÚ)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 1701, 24, NULL, N'Dlhové cenné papiere držané do splatnosti (065 - 096 AÚ)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 1701, 25, NULL, N'Pôžičky podnikom v skupine a ostatné pôžičky (066 + 067) - 096 AÚ', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 1701, 26, NULL, N'Ostatný dlhodobý finančný majetok (069 - 096 AÚ)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 1701, 27, NULL, N'Obstaranie dlhodobého finančného majetku (043 - 096 AÚ)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 1701, 28, NULL, N'Poskytnuté preddavky na dlhodobý finančný majetok (053 - 096 AÚ)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 1701, 991, NULL, N'Kontrolné číslo r. 001 až r. 028', NULL, 1, NULL, NULL, 28
    UNION ALL SELECT 1701, 29, N'B.', N'OBEŽNÝ MAJETOK SPOLU r. 030+ r. 037+ r. 042 + r. 051', NULL, 1, NULL, NULL, 29
    UNION ALL SELECT 1701, 30, N'1.', N'Zásoby r. 031 až r. 036', NULL, 1, NULL, NULL, 30
    UNION ALL SELECT 1701, 31, NULL, N'Materiál (112 + 119) - 191', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 1701, 32, NULL, N'Nedokončená výroba a polotovary vlastnej výroby (121+122)-(192+193)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 1701, 33, NULL, N'Výrobky (123 - 194)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 1701, 34, NULL, N'Zvieratá (124 - 195)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 1701, 35, NULL, N'Tovar (132 + 139) - 196', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 1701, 36, NULL, N'Poskytnuté prevádzkové preddavky na zásoby (314 AÚ - 391 AÚ)', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 1701, 37, N'2.', N'Dlhodobé pohľadávky r. 038 až r. 041', NULL, 1, NULL, NULL, 37
    UNION ALL SELECT 1701, 38, NULL, N'Pohľadávky z obchodného styku (311 AÚ až 314 AÚ) - 391 AÚ', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 1701, 39, NULL, N'Ostatné pohľadávky (315 AÚ - 391AÚ)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 1701, 40, NULL, N'Pohľadávky voči účastníkom združení (358AÚ - 391AÚ)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 1701, 41, NULL, N'Iné pohľadávky ( 335 AÚ + 373 AÚ + 375 AÚ + 378AÚ) - 391AÚ', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 1701, 42, N'3.', N'Krátkodobé pohľadávky r. 043 až r. 050', NULL, 1, NULL, NULL, 42
    UNION ALL SELECT 1701, 43, NULL, N'Pohľadávky z obchodného styku (311AÚ až 314 AÚ) - 391AÚ', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 1701, 44, NULL, N'Ostatné pohľadávky (315 AÚ - 391 AÚ)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 1701, 45, NULL, N'Zúčtovanie so Sociálnou poisťovňou a zdravotnými poisťovňami (336 )', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 1701, 46, NULL, N'Daňové pohľadávky (341 až 345)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 1701, 47, NULL, N'Pohľadávky z dôvodu finančných vzťahov k štátnemu rozpočtu a rozpočtom územnej samosprávy (346+ 348)', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 1701, 48, NULL, N'Pohľadávky voči účastníkom združení (358 AÚ - 391AÚ)', NULL, 0, NULL, NULL, 48
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 1701 AS [TableErpId], 49 AS [RowNumber], NULL AS [Designation], N'Spojovací účet pri združení (396 - 391AÚ)' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 49 AS [RowOrdinal]
    UNION ALL SELECT 1701, 50, NULL, N'Iné pohľadávky (335AÚ + 373AÚ + 375AÚ + 378AÚ) - 391AÚ', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 1701, 51, N'4.', N'Finančné účty r. 052 až r. 056', NULL, 1, NULL, NULL, 51
    UNION ALL SELECT 1701, 52, NULL, N'Pokladnica (211 + 213)', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 1701, 53, NULL, N'Bankové účty (221 AÚ + 261)', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 1701, 54, NULL, N'Bankové účty s dobou viazanosti dlhšou ako jeden rok (221 AÚ)', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 1701, 55, NULL, N'Krátkodobý finančný majetok (251+ 253 + 255 + 256 + 257) - 291AÚ', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 1701, 56, NULL, N'Obstaranie krátkodobého finančného majetku (259 - 291AÚ)', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 1701, 57, N'C.', N'ČASOVÉ ROZLÍŠENIE SPOLU r. 058 a r. 059', NULL, 1, NULL, NULL, 57
    UNION ALL SELECT 1701, 58, N'1.', N'Náklady budúcich období (381)', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 1701, 59, NULL, N'Príjmy budúcich období (385)', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 1701, 60, NULL, N'MAJETOK SPOLU r. 001 + r. 029 + r. 057', NULL, 1, NULL, NULL, 60
    UNION ALL SELECT 1701, 992, NULL, N'Kontrolné číslo r. 029 až r. 060', NULL, 1, NULL, NULL, 61
    UNION ALL SELECT 1801, 1, N'501', N'Spotreba materiálu', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 1801, 2, N'502', N'Spotreba energie', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 1801, 3, N'504', N'Predaný tovar', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1801, 4, N'511', N'Opravy a udržiavanie', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1801, 5, N'512', N'Cestovné', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1801, 6, N'513', N'Náklady na reprezentáciu', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1801, 7, N'518', N'Ostatné služby', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1801, 8, N'521', N'Mzdové náklady', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1801, 9, N'524', N'Zákonné sociálne poistenie a zdravotné poistenie', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 1801, 10, N'525', N'Ostatné sociálne poistenie', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 1801, 11, N'527', N'Zákonné sociálne náklady', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 1801, 12, N'528', N'Ostatné sociálne náklady', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 1801, 13, N'531', N'Daň z motorových vozidiel', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 1801, 14, N'532', N'Daň z nehnuteľností', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 1801, 15, N'538', N'Ostatné dane a poplatky', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 1801, 16, N'541', N'Zmluvné pokuty a penále', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 1801, 17, N'542', N'Ostatné pokuty a penále', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 1801, 18, N'543', N'Odpísanie pohľadávky', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 1801, 19, N'544', N'Úroky', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 1801, 20, N'545', N'Kurzové straty', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 1801, 21, N'546', N'Dary', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 1801, 22, N'547', N'Osobitné náklady', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 1801, 23, N'548', N'Manká a škody', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 1801, 24, N'549', N'Iné ostatné náklady', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 1801, 25, N'551', N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 1801, 26, N'552', N'Zostatková cena predaného dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 1801, 27, N'553', N'Predané cenné papiere', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 1801, 28, N'554', N'Predaný materiál', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 1801, 29, N'555', N'Náklady na krátkodobý finančný majetok', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 1801, 30, N'556', N'Tvorba fondov', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 1801, 31, N'557', N'Náklady na precenenie cenných papierov', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 1801, 32, N'558', N'Tvorba a zúčtovanie opravných položiek', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 1801, 33, N'559', N'Tvorba a zúčtovanie zákonných opravných položiek', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 1801, 34, N'561', N'Poskytnuté príspevky organizačným zložkám', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 1801, 35, N'562', N'Poskytnuté príspevky iným účtovným jednotkám', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 1801, 36, N'563', N'Poskytnuté príspevky fyzickým osobám', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 1801, 37, N'567', N'Poskytnuté príspevky z verejnej zbierky', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 1801, 38, NULL, N'Účtová trieda 5 spolu r. 01 až r. 37', NULL, 1, NULL, NULL, 37
    UNION ALL SELECT 1801, 994, NULL, N'Kontrolné číslo r. 01 až r. 38', NULL, 1, NULL, NULL, 38
    UNION ALL SELECT 54102, 58, N'A.', N'Vlastné imanie', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 54102, 59, N'I.', N'Základné imanie, z toho', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 54102, 60, N'1.', N'upísané základné imanie splatené', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 54102, 61, N'2.', N'Vlastné akcie (-)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 54102, 62, N'II.', N'Emisné ážio', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 54102, 63, N'III.', N'Oceňovacie rozdiely z ocenenia majetku a záväzkov', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 54102, 64, N'IV.', N'Rezervné fondy a ostatné fondy tvorené zo zisku', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 54102, 65, N'1.', N'Ostatné kapitálové fondy', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 54102, 66, N'V.', N'Výsledok hospodárenia minulých rokov', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 54102, 67, N'VI.', N'Výsledok hospodárenia bežného účtovného obdobia', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 54102, 68, N'B.', N'Podriadené pasíva', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 54102, 69, N'C.', N'Technické rezervy', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 54102, 70, N'1.', N'Technická rezerva na poistné budúcich období', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 54102, 71, N'1a.', N'Hrubá výška', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 54102, 72, N'1b.', N'Výška zaistenia (-)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 54102, 73, N'3.', N'Technická rezerva na poistné plnenie', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 54102, 74, N'3a.', N'Hrubá výška', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 16
    UNION ALL SELECT 54102, 75, N'3b.', N'Výška zaistenia (-)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 54102, 76, N'4.', N'Technická rezerva na poistné prémie a zľavy', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 54102, 77, N'4a.', N'Hrubá výška', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 54102, 78, N'4b.', N'Výška zaistenia (-)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 54102, 79, N'6.', N'Iné technické rezervy', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 54102, 80, N'6a.', N'Hrubá výška', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 54102, 81, N'6b.', N'Výška zaistenia (-)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 54102, 82, N'E.', N'Ostatné rezervy', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 54102, 83, N'G.', N'Záväzky, z toho', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 54102, 84, N'I.A.', N'z verejného zdravotného poistenia, z toho', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 54102, 85, N'1.', N'voči poisteným, z toho', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 54102, 86, N'1a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 54102, 87, N'1b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 54102, 88, N'2.', N'voči poskytovateľom zdravotnej starostlivosti', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 54102, 89, N'2a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 54102, 90, N'2b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 54102, 91, N'3.', N'voči inej zdravotnej poisťovni, z toho', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 54102, 92, N'3a.', N'z prerozdelenia poistného', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 54102, 93, N'4.', N'voči Úradu pre dohľad nad zdravotnou starostlivosťou', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 54102, 94, N'5.', N'voči Ministerstvu zdravotníctva Slovenskej republiky', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 54102, 95, N'I.B.', N'z individuálneho zdravotného poistenia', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 52202, 153, N'2.', N'Zmenky na úhradu (322, 478AÚ)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 52202, 154, N'3.', N'Prijaté preddavky (324, 475AÚ)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 52202, 155, N'4.', N'Ostatné záväzky (325, 479AÚ)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 52202, 156, N'5.', N'Nevyfakturované dodávky (326, 476AÚ)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 52202, 157, N'6.', N'Záväzky z nájmu (474AÚ)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 52202, 158, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 52202, 159, N'8.', N'Predané opcie (377AÚ)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 52202, 160, N'9.', N'Iné záväzky (379AÚ)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 52202, 161, N'10.', N'Záväzky z upísaných nesplatených cenných papierov a vkladov (367)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 52202, 162, N'11.', N'Záväzky voči združeniu (368)', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 52202, 163, N'12.', N'Zamestnanci (331)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 52202, 164, N'13.', N'Ostatné záväzky voči zamestnancom (333)', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 52202, 165, N'14.', N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia (336)', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 52202, 166, N'15.', N'Daň z príjmov (341)', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 52202, 167, N'16.', N'Ostatné priame dane (342)', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 52202, 168, N'17.', N'Daň z pridanej hodnoty (343)', NULL, 0, N'Krátkodobé záväzky - Dan z pridanej hodnoty', NULL, 53
    UNION ALL SELECT 52202, 169, N'18.', N'Ostatné dane a poplatky (345)', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 52202, 170, N'19.', N'Spojovací účet pri združení (396AÚ)', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 52202, 171, N'20.', N'Zúčtovanie s Európskou úniou (371AÚ)', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 52202, 172, N'21.', N'Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy (372AÚ)', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 52202, 173, N'B.V.', N'Bankové úvery a výpomoci súčet (r. 174 až 179)', NULL, 1, NULL, NULL, 58
    UNION ALL SELECT 52202, 174, N'B.V.1.', N'Bankové úvery dlhodobé (461AÚ)', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 52202, 175, N'2.', N'Bežné bankové úvery (461AÚ, 221AÚ, 231, 232)', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 52202, 176, N'3.', N'Vydané dlhopisy krátkodobé (473AÚ, 241) - (255AÚ)', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 52202, 177, N'4.', N'Ostatné krátkodobé finančné výpomoci (249)', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 52202, 178, N'5.', N'Prijaté návratné finančné výpomoci od subjektov verejnej správy dlhodobé (273AÚ)', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 52202, 179, N'6.', N'Prijaté návratné finančné výpomoci od subjektov verejnej správy krátkodobé (273AÚ)', NULL, 0, NULL, NULL, 64
    UNION ALL SELECT 52202, 180, N'C.', N'Časové rozlíšenie súčet (r. 181 + r. 182)', NULL, 1, NULL, NULL, 65
    UNION ALL SELECT 52202, 181, N'C.1.', N'Výdavky budúcich období (383)', NULL, 0, NULL, NULL, 66
    UNION ALL SELECT 52202, 182, N'2.', N'Výnosy budúcich období (384)', NULL, 0, NULL, NULL, 67
    UNION ALL SELECT 52202, 183, N'D.', N'Vzťahy k účtom klientov štátnej pokladnice (účtová skupina 20)', NULL, 0, NULL, NULL, 68
    UNION ALL SELECT 52202, 999, NULL, N'KONTROLNÉ ČÍSLO súčet (r. 115 až 183)', NULL, 1, NULL, NULL, 69
    UNION ALL SELECT 1701, 1, N'A.', N'NEOBEŽNÝ MAJETOK SPOLU r. 002 + r. 009 + r. 021', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 1701, 2, N'1.', N'Dlhodobý nehmotný majetok r. 003 až r. 008', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 1701, 3, NULL, N'Nehmotné výsledky z vývojovej a obdobnej činnosti 012-(072+091AÚ)', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1701, 4, NULL, N'Softvér 013 - (073+091AÚ)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1701, 5, NULL, N'Oceniteľné práva 014 - (074 + 091AÚ)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1701, 6, NULL, N'Ostatný dlhodobý nehmotný majetok (018+ 019)-(078 + 079 + 091 AÚ)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1701, 7, NULL, N'Obstaranie dlhodobého nehmotného majetku (041-093)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1701, 8, NULL, N'Poskytnuté preddavky na dlhodobý nehmotný majetok (051- 095AÚ)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1701, 9, N'2.', N'Dlhodobý hmotný majetok r. 010 až r. 020', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 1701, 10, NULL, N'Pozemky (031)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 1701, 11, NULL, N'Umelecké diela a zbierky (032)', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 1701, 12, NULL, N'Stavby 021 - (081 - 092AÚ)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 1701, 13, NULL, N'Samostatné hnuteľné veci a súbory hnuteľných vecí 022 - (082 + 092AÚ)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 1701, 14, NULL, N'Dopravné prostriedky 023 - (083 + 092AÚ)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 1701, 15, NULL, N'Pestovateľské celky trvalých porastov 025 - (085 + 092AÚ)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 6201, 1, NULL, N'Predaj tovaru', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 6201, 2, NULL, N'Predaj výrobkov a služieb', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 6201, 3, NULL, N'Ostatné príjmy', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 6201, 4, NULL, N'Príjmy celkom súčet (r. 01 až 03)', NULL, 1, NULL, NULL, 3
    UNION ALL SELECT 518306, 1, N'1.', N'Priemerný prepočítaný stav zamestnancov', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 518306, 2, N'2.', N'Stav zamestnancov ku dňu, ku ktorému sa zostavuje účtovná závierka', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 110102, 63, N'2.', N'Rezervný fond na vlastné akcie', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 110102, 64, N'V.', N'Výsledok hospodárenia minulých rokov', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 110102, 65, N'VI.', N'Výsledok hospodárenia bežného účtovného obdobia', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 110102, 66, N'B.', N'Podriadené pasíva', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 110102, 67, N'C.', N'Technické rezervy', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 110102, 68, N'1.', N'Technická rezerva na poistné budúcich období', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 110102, 69, N'1a.', N'Hrubá výška', NULL, 0, NULL, NULL, 13
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 110102 AS [TableErpId], 70 AS [RowNumber], N'1b.' AS [Designation], N'Výška zaistenia (–)' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 14 AS [RowOrdinal]
    UNION ALL SELECT 110102, 71, N'3.', N'Technická rezerva na poistné plnenie', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 110102, 72, N'3a.', N'Hrubá výška', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 110102, 73, N'3b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 110102, 74, N'4.', N'Technická rezerva na poistné prémie a zľavy', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 18
    UNION ALL SELECT 110102, 75, N'4a.', N'Hrubá výška', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 110102, 76, N'4b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 110102, 77, N'6.', N'Iné technické rezervy', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 110102, 78, N'6a.', N'Hrubá výška', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 110102, 79, N'6b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 110102, 80, N'E.', N'Ostatné rezervy', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 110102, 81, N'G.', N'Záväzky, z toho', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 110102, 82, N'I.', N'z verejného zdravotného poistenia, z toho', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 110102, 83, N'1.', N'voči poisteným, z toho', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 110102, 84, N'1a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 110102, 85, N'1b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 110102, 86, N'2.', N'voči poskytovateľom zdravotnej starostlivosti', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 110102, 87, N'2a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 110102, 88, N'2b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 110102, 89, N'3.', N'voči inej zdravotnej poisťovni, z toho', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 110102, 90, N'3a.', N'z prerozdelenia poistného', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 110102, 91, N'4.', N'voči Úradu pre dohľad nad zdravotnou starostlivosťou', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 54102, 96, N'1.', N'voči poisteným', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 54102, 97, N'2.', N'voči sprostredkovateľom poistenia', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 54102, 98, N'3.', N'voči poskytovateľom zdravotnej starostlivosti', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 54102, 99, N'II.', N'zo zaistenia', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 54102, 100, N'III.', N'pôžičky zaručené dlhopisom, z toho', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 54102, 101, N'1.', N'v konvertibilnej mene', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 54102, 102, N'2.', N'krátkodobé pôžičky', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 54102, 103, N'3.', N'dlhodobé pôžičky', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 54102, 104, N'IV.', N'bankové úvery, z toho', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 54102, 105, N'1.', N'krátkodobé úvery', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 54102, 106, N'V.', N'ostatné záväzky, z toho', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 54102, 107, N'1.', N'z daní', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 54102, 108, N'2.', N'zo sociálneho poistenia a zdravotného poistenia', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 54102, 109, N'3.', N'z finančného prenájmu', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 54102, 110, N'H.', N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 54102, 111, NULL, N'PASÍVA spolu', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 54102, 999, NULL, N'Kontrolné číslo', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 518402, 103, N'2a.', N'z toho zo sociálneho poistenia a zdravotného poistenia', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 518402, 104, N'3.', N'z finančného prenájmu', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 518402, 105, N'4.', N'z dotácií zo štátneho rozpočtu a ostatné dotácie', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 518402, 106, N'H.', N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 518402, 107, NULL, N'PASÍVA spolu', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 518402, 999, NULL, N'Kontrolné číslo', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 110102, 56, N'A.', N'Vlastné imanie', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 110102, 57, N'I.', N'Základné imanie, z toho', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 110102, 58, N'1.', N'upísané základné imanie splatené', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 110102, 59, N'II.', N'Emisné ážio', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 110102, 60, N'III.', N'Oceňovacie rozdiely z ocenenia majetku a záväzkov', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 110102, 61, N'IV.', N'Rezervné fondy a ostatné fondy tvorené zo zisku', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 110102, 62, N'1.', N'Ostatné kapitálové fondy', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 68703, 13, N'E.', N'Dane a poplatky (účtová skupina 53)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 68703, 14, N'F.', N'Odpisy a opravné položky k dlhodobému nehmotnému majetku a dlhodobému hmotnému majetku (551, (+/-) 553)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 68703, 15, N'G.', N'Zostatková cena predaného dlhodobého majetku a predaného materiálu (541, 542)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 68703, 16, N'H.', N'Opravné položky k pohľadávkam (+/- 547)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 68703, 17, N'I.', N'Ostatné náklady na hospodársku činnosť (543, 544, 545, 546, 548, 549, 555, 557)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 68703, 18, N'**', N'Výsledok hospodárenia z hospodárskej činnosti (+/-) (r. 01 - r. 08)', NULL, 1, NULL, NULL, 17
    UNION ALL SELECT 68703, 19, N'*', N'Pridaná hodnota (r. 02 - r. 09) + (r. 03 + r. 04 + r. 05) - (r. 10 + r. 11)', NULL, 1, NULL, NULL, 18
    UNION ALL SELECT 68703, 20, N'*', N'Výnosy z finančnej činnosti spolu súčet (r. 21 až r. 26)', NULL, 1, NULL, NULL, 19
    UNION ALL SELECT 68703, 21, N'VII.', N'Tržby z predaja cenných papierov a podielov (661)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 68703, 22, N'VIII.', N'Výnosy z dlhodobého finančného majetku (665)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 68703, 23, N'IX.', N'Výnosy z krátkodobého finančného majetku (666)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 68703, 24, N'X.', N'Výnosové úroky (662)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 68703, 25, N'XI.', N'Kurzové zisky (663)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 68703, 26, N'XII.', N'Ostatné výnosy z finančnej činnosti (668)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 68703, 27, N'*', N'Náklady na finančnú činnosť spolu súčet (r. 28 až r. 33)', NULL, 1, NULL, NULL, 26
    UNION ALL SELECT 68703, 28, N'J.', N'Predané cenné papiere a podiely (561)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 68703, 29, N'K.', N'Náklady na krátkodobý finančný majetok (566)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 68703, 30, N'L.', N'Opravné položky k finančnému majetku (+/-) (565)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 68703, 31, N'M.', N'Nákladové úroky (562)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 68703, 32, N'N.', N'Kurzové straty (563)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 68703, 33, N'O.', N'Ostatné náklady na finančnú činnosť (568, 569)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 518402, 56, N'A.', N'Vlastné imanie', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 518402, 57, N'I.', N'Základné imanie, z toho', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 518402, 58, NULL, N'upísané základné imanie splatené', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 518402, 59, N'II.', N'Emisné ážio', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 518402, 60, N'III.', N'Oceňovacie rozdiely z ocenenia majetku a záväzkov', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 518402, 61, N'IV.', N'Rezervné fondy, kapitálový fond tvorený z príspevkov akcionárov a ostatné fondy tvorené zo zisku', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 518402, 62, N'1.', N'Ostatné kapitálové fondy', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 518402, 63, N'2.', N'Rezervný fond na vlastné akcie', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 518402, 64, N'V.', N'Výsledok hospodárenia minulých rokov', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 518402, 65, N'VI.', N'Výsledok hospodárenia bežného účtovného obdobia', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 518402, 66, N'B.', N'Podriadené pasíva', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 518402, 67, N'C.', N'Technické rezervy', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 518402, 68, N'1.', N'Technická rezerva na poistné budúcich období', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 518402, 69, N'1a.', N'Hrubá výška', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 518402, 70, N'1b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 518402, 71, N'3.', N'Technická rezerva na poistné plnenie', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 518402, 72, N'3a.', N'Hrubá výška', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 518402, 73, N'3b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 518402, 74, N'4.', N'Technická rezerva na poistné prémie a zľavy', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 18
    UNION ALL SELECT 518402, 75, N'4a.', N'Hrubá výška', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 518402, 76, N'4b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 518402, 77, N'5.', N'Technická rezerva na prerozdeľovanie poistného', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 518402, 78, N'6.', N'Iné technické rezervy', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 518402, 79, N'6a.', N'Hrubá výška', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 518402, 80, N'6b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 518402, 81, N'E.', N'Ostatné rezervy', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 518402, 82, N'G.', N'Záväzky, z toho', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 518402, 83, N'I.', N'z verejného zdravotného poistenia, z toho', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 518402, 84, N'1.', N'voči poisteným, z toho', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 518402, 85, N'1a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 518402, 86, N'1b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 518402, 87, N'2.', N'voči poskytovateľom zdravotnej starostlivosti, z toho', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 518402, 88, N'2a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 518402, 89, N'2b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 518402, 90, N'3.', N'voči inej zdravotnej poisťovni, z toho', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 518402, 91, N'3a.', N'z prerozdelenia poistného', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 518402, 92, N'4.', N'voči Úradu pre dohľad nad zdravotnou starostlivosťou', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 518402, 93, N'5.', N'voči Ministerstvu zdravotníctva Slovenskej republiky', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 518402, 94, N'II.', N'pôžičky zaručené dlhopisom, z toho', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 518402, 95, N'1.', N'v konvertibilnej mene', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 518402, 96, N'2.', N'krátkodobé pôžičky', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 518402, 97, N'3.', N'dlhodobé pôžičky', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 518402, 98, N'III.', N'bankové úvery, z toho', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 518402, 99, NULL, N'krátkodobé úvery', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 518402, 100, N'IV.', N'ostatné záväzky, z toho', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 518402, 101, N'1.', N'z daní', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 518402, 102, N'2.', N'záväzky voči zamestnancom celkom, z toho', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 68703, 1, N'*', N'Výnosy z hospodárskej činnosti spolu súčet (r. 02 až r. 07)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 68703, 2, N'I.', N'Tržby z predaja tovaru (604, 607)', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 68703, 3, N'II.', N'Tržby z predaja vlastných výrobkov a služieb (601, 602, 606)', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 68703, 4, N'III.', N'Zmena stavu vnútroorganizačných zásob (+/-) (účtová skupina 61)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 68703, 5, N'IV.', N'Aktivácia (účtová skupina 62)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 68703, 6, N'V.', N'Tržby z predaja dlhodobého nehmotného majetku, dlhodobého hmotného majetku a materiálu (641, 642)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 68703, 7, N'VI.', N'Ostatné výnosy z hospodárskej činnosti (644, 645, 646, 648, 655, 657)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 68703, 8, N'*', N'Náklady na hospodársku činnosť spolu súčet (r. 09 až r. 17)', NULL, 1, NULL, NULL, 7
    UNION ALL SELECT 68703, 9, N'A.', N'Náklady vynaložené na obstaranie predaného tovaru (504, (+/- ) 505A, 507)', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 68703, 10, N'B.', N'Spotreba materiálu, energie a ostatných neskladovateľných dodávok (501, 502, 503, (+/-) 505A)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 68703, 11, N'C.', N'Služby (účtová skupina 51)', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 68703, 12, N'D.', N'Osobné náklady (účtová skupina 52)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 69002, 115, NULL, N'VLASTNÉ IMANIE A ZÁVÄZKY r. 116 + r. 126 + r. 180 + r. 183', N'TOTAL EQUITY AND LIABILITIES line 116 + line 126 + line 180 + line 183', 1, N'VLASTNÉ IMANIE A ZÁVÄZKY', N'r.115 - VLASTNÉ IMANIE A ZÁVÄZKY r. 116 + r. 126 + r. 180 + r. 183', 0
    UNION ALL SELECT 69002, 116, N'A.', N'Vlastné imanie r. 117 + r. 120 + r. 123', N'Equity line 117 + line 120 + line 123', 1, N'Vlastné imanie', N'r.116 - Vlastné imanie r. 117 + r. 120 + r. 123', 1
    UNION ALL SELECT 69002, 117, N'A.I.', N'Oceňovacie rozdiely súčet (r. 118 + r. 119)', N'Differences from revaluation - total (lines 118 to 119)', 1, N'Oceňovacie rozdiely súčet', N'r.117 - Oceňovacie rozdiely súčet (r. 118 + r. 119)', 2
    UNION ALL SELECT 69002, 118, N'A.I.1.', N'Oceňovacie rozdiely z precenenia majetku a záväzkov (+/– 414)', N'Differences from revaluation of assets and liabilities (+/- 414)', 0, N'Oceňovacie rozdiely z precenenia majetku a záväzkov', N'r.118 - Oceňovacie rozdiely z precenenia majetku a záväzkov (+/– 414)', 3
    UNION ALL SELECT 69002, 119, N'2.', N'Oceňovacie rozdiely z kapitálových účastín (+/– 415)', N'Investment revaluation reserves (+/- 415)', 0, N'Oceňovacie rozdiely z kapitálových účastín', N'r.119 - Oceňovacie rozdiely z kapitálových účastín (+/– 415)', 4
    UNION ALL SELECT 69002, 120, N'A.II.', N'Fondy súčet (r. 121 + r. 122)', N'Funds - total (lines 121 to 122)', 1, N'Fondy súčet', N'r.120 - Fondy súčet (r. 121 + r. 122)', 5
    UNION ALL SELECT 69002, 121, N'A.II.1.', N'Zákonný rezervný fond (421)', N'Statutory reserve fund (421)', 0, N'Zákonný rezervný fond', N'r.121 - Zákonný rezervný fond (421)', 6
    UNION ALL SELECT 69002, 122, N'2.', N'Ostatné fondy (427)', N'Other funds (427)', 0, N'Ostatné fondy', N'r.122 - Ostatné fondy (427)', 7
    UNION ALL SELECT 69002, 123, N'A.III.', N'Výsledok hospodárenia (+/-) súčet (r. 124 až 125)', N'Net profit or loss (+/-) total (lines 124 to 125)', 1, N'Výsledok hospodárenia', N'r.123 - Výsledok hospodárenia (+/-) súčet (r. 124 až 125)', 8
    UNION ALL SELECT 69002, 124, N'A.III.1.', N'Nevysporiadaný výsledok hospodárenia minulých rokov (+/– 428)', N'Retained earnings or accumulated losses from previous years (+/- 428)', 0, N'Nevysporiadaný výsledok hospodárenia minulých rokov', N'r.124 - Nevysporiadaný výsledok hospodárenia minulých rokov (+/– 428)', 9
    UNION ALL SELECT 69002, 125, N'2.', N'Výsledok hospodárenia za účtovné obdobie (+/–) r. 001 - (r. 117 + r. 120 +r.124+ r. 126 + r. 180 + r. 183)', N'Net profit/loss for the accounting period  (+/-) line 001 - (line 117 + line 120 + line 124 + line 126 + line 180 + line 183)', 1, N'Výsledok hospodárenia za účtovné obdobie', N'r.125 - Výsledok hospodárenia za účtovné obdobie (+/–) r. 001 - (r. 117 + r. 120 +r.124+ r. 126 + r. 180 + r. 183)', 10
    UNION ALL SELECT 69002, 126, N'B.', N'Záväzky súčet r. 127 + r. 132 + r. 140 + r. 151 + r. 173', N'Liabilities - line 127 + line 132 + line 140 + line 151 + line 173', 1, N'Záväzky súčet', N'r.126 - Záväzky súčet r. 127 + r. 132 + r. 140 + r. 151 + r. 173', 11
    UNION ALL SELECT 69002, 127, N'B.I.', N'Rezervy súčet (r. 128 až 131)', N'Provisions - total (lines 128 to 131)', 1, N'Rezervy súčet', N'r.127 - Rezervy súčet (r. 128 až 131)', 12
    UNION ALL SELECT 69002, 128, N'B.I.1.', N'Rezervy zákonné dlhodobé (451AÚ)', N'Legal provisions - long-term (451A)', 0, N'Rezervy zákonné dlhodobé', N'r.128 - Rezervy zákonné dlhodobé (451AÚ)', 13
    UNION ALL SELECT 69002, 129, N'2.', N'Ostatné rezervy (459AÚ)', N'Other provisions (459A)', 0, N'Ostatné rezervy', N'r.129 - Ostatné rezervy (459AÚ)', 14
    UNION ALL SELECT 69002, 130, N'3.', N'Rezervy zákonné krátkodobé (323AÚ, 451AÚ)', N'Legal provisions - short-term (323A, 451A)', 0, N'Rezervy zákonné krátkodobé', N'r.130 - Rezervy zákonné krátkodobé (323AÚ, 451AÚ)', 15
    UNION ALL SELECT 69002, 131, N'4.', N'Ostatné krátkodobé rezervy (323AÚ, 459AÚ)', N'Other short-term provisions (323A, 459A)', 0, N'Ostatné krátkodobé rezervy', N'r.131 - Ostatné krátkodobé rezervy (323AÚ, 459AÚ)', 16
    UNION ALL SELECT 69002, 132, N'B.II.', N'Zúčtovanie medzi subjektami verejnej správy súčet (r. 133 až r. 139)', N'Clearance between the public administration entities - total (lines 133 to 139)', 1, N'Zúčtovanie medzi subjektami verejnej správy súčet', N'r.132 - Zúčtovanie medzi subjektami verejnej správy súčet (r. 133 až r. 139)', 17
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 69002 AS [TableErpId], 133 AS [RowNumber], N'B.II.1.' AS [Designation], N'Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa (351)' AS [Text_sk], N'Clearing of state-funded organisation''s contributions to founder''s budget (351)' AS [Text_en], 0 AS [IsSumRow], N'Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa' AS [Category_sk], N'r.133 - Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa (351)' AS [MappingCaption_sk], 18 AS [RowOrdinal]
    UNION ALL SELECT 69002, 134, N'2.', N'Zúčtovanie transferov štátneho rozpočtu (353)', N'Clearing of state budget transfers (353)', 0, N'Zúčtovanie transferov štátneho rozpočtu', N'r.134 - Zúčtovanie transferov štátneho rozpočtu (353)', 19
    UNION ALL SELECT 69002, 135, N'3.', N'Zúčtovanie transferov rozpočtu obce a vyššieho územného celku (355)', N'Clearing of transfers of the budget of municipalities and higher territorial units (355)', 0, N'Zúčtovanie transferov rozpočtu obce a vyššieho územného celku', N'r.135 - Zúčtovanie transferov rozpočtu obce a vyššieho územného celku (355)', 20
    UNION ALL SELECT 69002, 136, N'4.', N'Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku (356)', N'Clearing of transfers from state budget within consolidated unit (356)', 0, N'Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku', N'r.136 - Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku (356)', 21
    UNION ALL SELECT 69002, 137, N'5.', N'Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku (357)', N'Other clearing of the budget of municipalities and higher territorial units (357)', 0, N'Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku', N'r.137 - Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku (357)', 22
    UNION ALL SELECT 69002, 138, N'6.', N'Zúčtovanie transferov zo štátneho rozpočtu iným subjektom (358)', N'Clearing of transfers from state budget to other entities (358)', 0, N'Zúčtovanie transferov zo štátneho rozpočtu iným subjektom', N'r.138 - Zúčtovanie transferov zo štátneho rozpočtu iným subjektom (358)', 23
    UNION ALL SELECT 69002, 139, N'7.', N'Zúčtovanie transferov medzi subjektami verejnej správy a iné zúčtovania (359)', N'Clearance of transfers between the public administration entities and other clearance transactions (359)', 0, N'Zúčtovanie transferov medzi subjektami verejnej správy a iné zúčtovania', N'r.139 - Zúčtovanie transferov medzi subjektami verejnej správy a iné zúčtovania (359)', 24
    UNION ALL SELECT 69002, 140, N'B.III.', N'Dlhodobé záväzky súčet (r. 141 až 150)', N'Non-current liabilities - total (lines 141 to 150)', 1, N'Dlhodobé záväzky súčet', N'r.140 - Dlhodobé záväzky súčet (r. 141 až 150)', 25
    UNION ALL SELECT 69002, 141, N'B.III.1.', N'Ostatné dlhodobé záväzky (479AÚ)', N'Other non-current liabilities (479A)', 0, N'Ostatné dlhodobé záväzky', N'r.141 - Ostatné dlhodobé záväzky (479AÚ)', 26
    UNION ALL SELECT 69002, 142, N'2.', N'Dlhodobé prijaté preddavky (475AÚ)', N'Long-term advance payments received (475A)', 0, N'Dlhodobé prijaté preddavky', N'r.142 - Dlhodobé prijaté preddavky (475AÚ)', 27
    UNION ALL SELECT 69002, 143, N'3.', N'Dlhodobé zmenky na úhradu (478AÚ)', N'Long-term bills of exchange to be paid (478A)', 0, N'Dlhodobé zmenky na úhradu', N'r.143 - Dlhodobé zmenky na úhradu (478AÚ)', 28
    UNION ALL SELECT 69002, 144, N'4.', N'Záväzky zo sociálneho fondu (472)', N'Liabilities related to social fund (472)', 0, N'Záväzky zo sociálneho fondu', N'r.144 - Záväzky zo sociálneho fondu (472)', 29
    UNION ALL SELECT 69002, 145, N'5.', N'Záväzky z nájmu (474AÚ)', N'Liabilities under leasing contracts (474A)', 0, N'Záväzky z nájmu', N'r.145 - Dlhodobé záväzky / Záväzky z nájmu (474AÚ)', 30
    UNION ALL SELECT 69002, 146, N'6.', N'Dlhodobé nevyfakturované dodávky (476AÚ)', N'Unbilled long-term supplies (476A)', 0, N'Dlhodobé nevyfakturované dodávky', N'r.146 - Dlhodobé nevyfakturované dodávky (476AÚ)', 31
    UNION ALL SELECT 69002, 147, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ)', N'Receivables and liabilities from fixed term transactions (373A)', 0, N'Pohľadávky a záväzky z pevných termínových operácií', N'r.147 - Dlhodobé záväzky / Pohľadávky a záväzky z pevných termínových operácií (373AÚ)', 32
    UNION ALL SELECT 69002, 148, N'8.', N'Predané opcie (377AÚ)', N'Options sold (377A)', 0, N'Predané opcie', N'r.148 - Dlhodobé záväzky / Predané opcie (377AÚ)', 33
    UNION ALL SELECT 69002, 149, N'9.', N'Iné záväzky (379AÚ)', N'Other liabilities (379A)', 0, N'Iné záväzky', N'r.149 - Dlhodobé záväzky / Iné záväzky (379AÚ)', 34
    UNION ALL SELECT 69002, 150, N'10.', N'Vydané dlhopisy dlhodobé (473AÚ) - (255AÚ)', N'Bonds issued (473A ) - (255A)', 0, N'Vydané dlhopisy dlhodobé', N'r.150 - Vydané dlhopisy dlhodobé (473AÚ) - (255AÚ)', 35
    UNION ALL SELECT 69002, 151, N'B.IV.', N'Krátkodobé záväzky súčet (r. 152 až 172)', N'Current liabilities - total (lines 152 to 172)', 1, N'Dan z príjmov', N'r.151 - Krátkodobé záväzky súčet (r. 152 až 172)', 36
    UNION ALL SELECT 69002, 152, N'B.IV.1.', N'Dodávatelia (321)', N'Suppliers (321)', 0, N'Dodávatelia', N'r.152 - Dodávatelia (321)', 37
    UNION ALL SELECT 69002, 153, N'2.', N'Zmenky na úhradu (322, 478AÚ)', N'Bills of exchange to be paid (322, 478A)', 0, N'Zmenky na úhradu', N'r.153 - Zmenky na úhradu (322, 478AÚ)', 38
    UNION ALL SELECT 69002, 154, N'3.', N'Prijaté preddavky (324, 475AÚ)', N'Advance payments received (324, 475A)', 0, N'Prijaté preddavky', N'r.154 - Prijaté preddavky (324, 475AÚ)', 39
    UNION ALL SELECT 69002, 155, N'4.', N'Ostatné záväzky (325, 479AÚ)', N'Other liabilities (325, 479A)', 0, N'Ostatné záväzky', N'r.155 - Ostatné záväzky (325, 479AÚ)', 40
    UNION ALL SELECT 69002, 156, N'5.', N'Nevyfakturované dodávky (326, 476AÚ)', N'Unbilled supplies (326, 476A)', 0, N'Nevyfakturované dodávky', N'r.156 - Nevyfakturované dodávky (326, 476AÚ)', 41
    UNION ALL SELECT 69002, 157, N'6.', N'Záväzky z nájmu (474AÚ)', N'Liabilities under leasing contracts (474A)', 0, N'Záväzky z nájmu', N'r.157 - Krátkodobé záväzky / Záväzky z nájmu (474AÚ)', 42
    UNION ALL SELECT 69002, 158, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ)', N'Receivables and liabilities from fixed term transactions (373A)', 0, N'Pohľadávky a záväzky z pevných termínových operácií', N'r.158 - Krátkodobé záväzky / Pohľadávky a záväzky z pevných termínových operácií (373AÚ)', 43
    UNION ALL SELECT 69002, 159, N'8.', N'Predané opcie (377AÚ)', N'Options sold (377A)', 0, N'Predané opcie', N'r.159 - Krátkodobé záväzky / Predané opcie (377AÚ)', 44
    UNION ALL SELECT 69002, 160, N'9.', N'Iné záväzky (379AÚ)', N'Other liabilities (379A)', 0, N'Iné záväzky', N'r.160 - Krátkodobé záväzky / Iné záväzky (379AÚ)', 45
    UNION ALL SELECT 69002, 161, N'10.', N'Záväzky z upísaných nesplatených cenných papierov a vkladov (367)', N'Liabilities out of subscribed unpaid securities and contributions (367)', 0, N'Záväzky z upísaných nesplatených cenných papierov a vkladov', N'r.161 - Záväzky z upísaných nesplatených cenných papierov a vkladov (367)', 46
    UNION ALL SELECT 69002, 162, N'11.', N'Záväzky voči združeniu (368)', N'Liabilities to participants in association (368)', 0, N'Záväzky voči združeniu', N'r.162 - Záväzky voči združeniu (368)', 47
    UNION ALL SELECT 69002, 163, N'12.', N'Zamestnanci (331)', N'Employees (331)', 0, N'Zamestnanci', N'r.163 - Zamestnanci (331)', 48
    UNION ALL SELECT 69002, 164, N'13.', N'Ostatné záväzky voči zamestnancom (333)', N'Other liabilities to employees (333)', 0, N'Ostatné záväzky voči zamestnancom', N'r.164 - Ostatné záväzky voči zamestnancom (333)', 49
    UNION ALL SELECT 69002, 165, N'14.', N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia (336)', N'Clearing with social and health insurance institutions (336)', 0, N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia', N'r.165 - Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia (336)', 50
    UNION ALL SELECT 69002, 166, N'15.', N'Daň z príjmov (341)', N'Income tax (341)', 0, N'Daň z príjmov', N'r.166 - Daň z príjmov (341)', 51
    UNION ALL SELECT 69002, 167, N'16.', N'Ostatné priame dane (342)', N'Other direct taxes (342)', 0, N'Ostatné priame dane', N'r.167 - Ostatné priame dane (342)', 52
    UNION ALL SELECT 69002, 168, N'17.', N'Daň z pridanej hodnoty (343)', N'Value added tax (343)', 0, N'Dan z pridanej hodnoty', N'r.168 - Daň z pridanej hodnoty (343)', 53
    UNION ALL SELECT 69002, 169, N'18.', N'Ostatné dane a poplatky (345)', N'Other taxes and fees (345)', 0, N'Ostatné dane a poplatky', N'r.169 - Ostatné dane a poplatky (345)', 54
    UNION ALL SELECT 69002, 170, N'19.', N'Spojovací účet pri združení (396AÚ)', N'Control account at association (396A)', 0, N'Spojovací účet pri združení', N'r.170 - Krátkodobé záväzky / Spojovací účet pri združení (396AÚ)', 55
    UNION ALL SELECT 69002, 171, N'20.', N'Zúčtovanie s Európskou úniou (371AÚ)', N'Clearing with the European Union (371A)', 0, N'Zúčtovanie s Európskou úniou', N'r.171 - Zúčtovanie s Európskou úniou (371AÚ)', 56
    UNION ALL SELECT 69002, 172, N'21.', N'Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy (372AÚ)', N'Transfers and other clearance with entities outside public administration (372A)', 0, N'Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy', N'r.172 - Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy (372AÚ)', 57
    UNION ALL SELECT 69002, 173, N'B.V.', N'Bankové úvery a výpomoci súčet (r. 174 až 179)', N'Bank loans and assistances - total (lines 174 to 179)', 1, N'Bankové úvery a výpomoci súčet', N'r.173 - Bankové úvery a výpomoci súčet (r. 174 až 179)', 58
    UNION ALL SELECT 69002, 174, N'B.V.1.', N'Bankové úvery dlhodobé (461AÚ)', N'Long-term bank loans (461A)', 0, N'Bankové úvery dlhodobé', N'r.174 - Bankové úvery dlhodobé (461AÚ)', 59
    UNION ALL SELECT 72702, 93, N'653', N'Zúčtovanie ostatných rezerv z prevádzkovej činnosti', N'Clearing of other provisions out of operations', 0, NULL, NULL, 28
    UNION ALL SELECT 72702, 94, N'657', N'Zúčtovanie zákonných opravných položiek z prevádzkovej činnosti', N'Clearing of legal adjusting entries out of operations', 0, NULL, NULL, 29
    UNION ALL SELECT 72702, 95, N'658', N'Zúčtovanie ostatných opravných položiek z prevádzkovej činnosti', N'Clearing of other adjusting entries out of operations', 0, NULL, NULL, 30
    UNION ALL SELECT 72702, 96, NULL, N'Zúčtovanie rezerv a opravných položiek z finančnej činnosti (r. 097 + r. 098)', N'Clearing of provisions and adjusting entries to financial activities - total (lines 097 to 098)', 1, NULL, NULL, 31
    UNION ALL SELECT 72702, 97, N'654', N'Zúčtovanie rezerv z finančnej činnosti', N'Clearing of provisions out of financial activity', 0, NULL, NULL, 32
    UNION ALL SELECT 72702, 98, N'659', N'Zúčtovanie opravných položiek z finančnej činnosti', N'Clearing of adjusting entries out of financial activity', 0, NULL, NULL, 33
    UNION ALL SELECT 72702, 99, N'655', N'Zúčtovanie komplexných nákladov budúcich období', N'Clearing of complex deferred expenses', 0, NULL, NULL, 34
    UNION ALL SELECT 72702, 100, N'66', N'Finančné výnosy (r. 101 až r. 108)', N'Financial revenues - total (lines 101 to 108)', 1, NULL, NULL, 35
    UNION ALL SELECT 72702, 101, N'661', N'Tržby z predaja cenných papierov a podielov', N'Revenues from the sale of securities and shares', 0, NULL, NULL, 36
    UNION ALL SELECT 72702, 102, N'662', N'Úroky', N'Interest income', 0, NULL, NULL, 37
    UNION ALL SELECT 72702, 103, N'663', N'Kurzové zisky', N'Exchange rate gains', 0, NULL, NULL, 38
    UNION ALL SELECT 72702, 104, N'664', N'Výnosy z precenenia cenných papierov', N'Revenue from securities revaluation', 0, NULL, NULL, 39
    UNION ALL SELECT 72702, 105, N'665', N'Výnosy z dlhodobého finančného majetku', N'Revenues from non-current financial assets', 0, NULL, NULL, 40
    UNION ALL SELECT 72702, 106, N'666', N'Výnosy z krátkodobého finančného majetku', N'Revenues from current financial assets', 0, NULL, NULL, 41
    UNION ALL SELECT 72702, 107, N'667', N'Výnosy z derivátových operácií', N'Revenues from derivative transactions', 0, NULL, NULL, 42
    UNION ALL SELECT 72702, 108, N'668', N'Ostatné finančné výnosy', N'Other financial revenues', 0, NULL, NULL, 43
    UNION ALL SELECT 72702, 109, N'67', N'Mimoriadne výnosy (r. 110 až r. 113)', N'Extraordinary revenues - total (lines 110 to 113)', 1, NULL, NULL, 44
    UNION ALL SELECT 72702, 110, N'672', N'Náhrady škôd', N'Compensation of damages', 0, NULL, NULL, 45
    UNION ALL SELECT 72702, 111, N'674', N'Zúčtovanie rezerv', N'Accounting for provisions', 0, NULL, NULL, 46
    UNION ALL SELECT 72702, 112, N'678', N'Ostatné mimoriadne výnosy', N'Other extraordinary revenues', 0, NULL, NULL, 47
    UNION ALL SELECT 72702, 113, N'679', N'Zúčtovanie opravných položiek', N'Clearing of adjusting entries', 0, NULL, NULL, 48
    UNION ALL SELECT 72702, 114, N'68', N'Výnosy z transferov a rozpočtových príjmov v štátnych rozpočtových organizáciách a príspevkových organizáciách (r. 115 až r. 123)', N'Revenues from transfers and budgetary revenues in state-funded and state-subsidized organisations - total (lines 115 to 123)', 1, NULL, NULL, 49
    UNION ALL SELECT 72702, 115, N'681', N'Výnosy z bežných transferov zo štátneho rozpočtu', N'Revenues from current transfers from state budget', 0, NULL, NULL, 50
    UNION ALL SELECT 72702, 116, N'682', N'Výnosy z kapitálových transferov zo štátneho rozpočtu', N'Revenues from capital transfers from state budget', 0, NULL, NULL, 51
    UNION ALL SELECT 72702, 117, N'683', N'Výnosy z bežných transferov od ostatných subjektov verejnej správy', N'Revenues from current transfers from other entities of general government', 0, NULL, NULL, 52
    UNION ALL SELECT 72702, 118, N'684', N'Výnosy z kapitálových transferov od ostatných subjektov verejnej správy', N'Revenues from capital transfers from other entities of general government', 0, NULL, NULL, 53
    UNION ALL SELECT 72702, 119, N'685', N'Výnosy z bežných transferov od Európskej únie', N'Revenues from current transfers from the European Union', 0, NULL, NULL, 54
    UNION ALL SELECT 72702, 120, N'686', N'Výnosy z kapitálových transferov od Európskej únie', N'Revenues from capital transfers from the European Union', 0, NULL, NULL, 55
    UNION ALL SELECT 72702, 121, N'687', N'Výnosy z bežných transferov od ostatných subjektov mimo verejnej správy', N'Revenues from current transfers from other entities outside of general government', 0, NULL, NULL, 56
    UNION ALL SELECT 72702, 122, N'688', N'Výnosy z kapitálových transferov od ostatných subjektov mimo verejnej správy', N'Revenues from capital transfers from other entities outside of general government', 0, NULL, NULL, 57
    UNION ALL SELECT 72702, 123, N'689', N'Výnosy z odvodu rozpočtových príjmov', N'Revenues from budgetary contributions', 0, NULL, NULL, 58
    UNION ALL SELECT 72702, 124, N'69', N'Výnosy z transferov a rozpočtových príjmov v obciach, vyšších územných celkoch a v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom (r. 125 až r. 133)', N'Revenue from transfers and budgetary revenue in municipalities, higher regional units and state-subsidized organisations founded by municipality and higher regional unit - total (lines 125 to 133)', 1, NULL, NULL, 59
    UNION ALL SELECT 72702, 125, N'691', N'Výnosy z bežných transferov z rozpočtu obce alebo z rozpočtu vyššieho územného celku v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom', N'Revenues from current transfers from the budget of municipality or higher regional unit in state-funded and state-subsidized organisations founded by municipality or higher regional unit', 0, NULL, NULL, 60
    UNION ALL SELECT 72702, 126, N'692', N'Výnosy z kapitálových transferov z rozpočtu obce alebo z rozpočtu vyššieho územného celku v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom', N'Revenues from capital transfers from the budget of municipality or higher regional unit in state-funded and state-subsidized organisations founded by municipality or higher regional unit', 0, NULL, NULL, 61
    UNION ALL SELECT 94204, 57, NULL, N'Vlastné imanie', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 94204, 58, NULL, N'Základné imanie', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 94204, 59, NULL, N'z toho: Upísané základné imanie splatené', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94204, 60, NULL, N'Vlastné akcie', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 94204, 61, NULL, N'Emisné ážio', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 94204, 62, NULL, N'Finančné zdroje poskytnuté pobočke zahraničnej poisťovne', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 94204, 63, NULL, N'Rezervné fondy a ostatné fondy tvorené zo zisku', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 94204, 64, NULL, N'z toho: rezervné fondy', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 94204, 65, NULL, N'Fond vyrovnávacej rezervy', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 94204, 66, NULL, N'Ostatné kapitálové fondy', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 94204, 67, NULL, N'Oceňovacie rozdiely', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 94204, 68, NULL, N'Z prepočtu zabezpečovacích derivátov', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 94204, 69, NULL, N'Z ocenenia finančných nástrojov na predaj', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 94204, 70, NULL, N'Z ocenenia pozemku a stavieb', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 94204, 71, NULL, N'Ostatné', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 94204, 72, NULL, N'Vlastnosti ľubovoľnej účasti', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 94204, 73, NULL, N'Hospodársky výsledok minulých rokov', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 94204, 74, NULL, N'Hospodársky výsledok vo schvaľovacom období', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 17
    UNION ALL SELECT 94204, 75, NULL, N'Hospodársky výsledok bežného obdobia', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 94204, 76, NULL, N'Záväzky', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 94204, 77, NULL, N'Podriadené záväzky', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 94204, 78, NULL, N'Prijaté úvery a pôžičky', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 94204, 79, NULL, N'z toho: vydané dlhové cenné papiere', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 94204, 80, NULL, N'úvery prijaté od spriaznených osôb', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 94204, 81, NULL, N'úvery od bánk', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 94204, 82, NULL, N'záväzky z finančného leasingu', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 94204, 83, NULL, N'Vklady pri pasívnom zaistení', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 94204, 84, NULL, N'Záporná reálna hodnota derivátových operácií na obchodovanie', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 94204, 85, NULL, N'Záporná reálna hodnota derivátových operácií na zabezpečenie', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 94204, 86, NULL, N'Rezervy na poistné zmluvy', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 94204, 87, NULL, N'Rezerva na poistné budúcich období', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 94204, 88, NULL, N'Rezerva na poistné plnenia', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 94204, 89, NULL, N'Rezerva na poistné prémie a zľavy', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 94204, 90, NULL, N'Rezerva na úhradu záväzkov voči Slovenskej kancelárii poisťovateľov vznikajúcich z činností podľa osobitného predpisu', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 94204, 91, NULL, N'Rezerva na životné poistenie', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 94204, 92, NULL, N'Ďalšie rezervy', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 94204, 93, NULL, N'Rezerva na krytie rizika z investovania finančných prostriedkov v mene poistených', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 94204, 94, NULL, N'Finančné záväzky z investičných zmlúv', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 94204, 95, NULL, N'z toho: Finančné záväzky z investičných zmlúv na krytie rizika v mene poistených vyplývajúce z investičných zmlúv', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 94204, 96, NULL, N'Netechnické rezervy', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 94204, 97, NULL, N'z toho: dlhodobé zamestnanecké pôžitky', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 94204, 98, NULL, N'Záväzky z poistenia a zaistenia', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 94204, 99, NULL, N'Voči poisteným', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 94204, 100, NULL, N'Zo spolupoistenia', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 94204, 101, NULL, N'Voči sprostredkovateľom', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 94204, 102, NULL, N'Voči zaisťovateľom', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 94204, 103, NULL, N'Ostatné záväzky z poistenia a zaistenia', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 94204, 104, NULL, N'Krátkodobé zamestnanecké pôžitky', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 94204, 105, NULL, N'Záväzky voči zamestnancom zo závislej činnosti', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 94204, 106, NULL, N'Zúčtovanie so Sociálnou poisťovnou a zdravotnými poisťovňami', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 94204, 107, NULL, N'Sociálna poisťovňa', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 94204, 108, NULL, N'Zdravotné poisťovne', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 94204, 109, NULL, N'Sociálny fond', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 94204, 110, NULL, N'Ostatné', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 94204, 111, NULL, N'Daňové záväzky', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 94204, 112, NULL, N'z toho: bežný daňový záväzok', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 94204, 113, NULL, N'odložený daňový záväzok', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 94204, 114, NULL, N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 94204, 115, NULL, N'Ostatné záväzky', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 94204, 116, NULL, N'z toho: prijaté preddavky', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 94204, 117, NULL, N'Pasíva spolu', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 72702, 65, N'60', N'Tržby za vlastné výkony a tovar (r. 066 až r. 068)', N'Revenues from own activity and merchandise - total (lines 066 to 068)', 1, NULL, NULL, 0
    UNION ALL SELECT 72702, 66, N'601', N'Tržby za vlastné výrobky', N'Revenues from the sale of own products', 0, NULL, NULL, 1
    UNION ALL SELECT 72702, 67, N'602', N'Tržby z predaja služieb', N'Revenues from the sale of services provided', 0, NULL, NULL, 2
    UNION ALL SELECT 72702, 68, N'604, 607', N'Tržby za tovar, Výnosy z nehnuteľnosti na predaj', N'Revenues from the sale of merchandise', 0, NULL, NULL, 3
    UNION ALL SELECT 72702, 69, N'61', N'Zmena stavu vnútroorganizačných zásob (r. 070 až r. 073)', N'Changes in internal inventory - total (lines 070 to 073) (+/-)', 1, NULL, NULL, 4
    UNION ALL SELECT 72702, 70, N'611', N'Zmena stavu nedokončenej výroby', N'Change in inventory of work in progress', 0, NULL, NULL, 5
    UNION ALL SELECT 72702, 71, N'612', N'Zmena stavu polotovarov', N'Change in inventory of semi-finished products', 0, NULL, NULL, 6
    UNION ALL SELECT 72702, 72, N'613', N'Zmena stavu výrobkov', N'Change in inventory of finished products', 0, NULL, NULL, 7
    UNION ALL SELECT 72702, 73, N'614', N'Zmena stavu zvierat', N'Change in animal inventory', 0, NULL, NULL, 8
    UNION ALL SELECT 72702, 74, N'62', N'Aktivácia (r. 075 až r. 078)', N'Capitalization - total (lines 075 to 078)', 1, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 9
    UNION ALL SELECT 72702, 75, N'621', N'Aktivácia materiálu a tovaru', N'Capitalization of material and merchandise', 0, NULL, NULL, 10
    UNION ALL SELECT 72702, 76, N'622', N'Aktivácia vnútroorganizačných služieb', N'Capitalization of internal services', 0, NULL, NULL, 11
    UNION ALL SELECT 72702, 77, N'623', N'Aktivácia dlhodobého nehmotného majetku', N'Capitalization of long-term intangible assets', 0, NULL, NULL, 12
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 72702 AS [TableErpId], 78 AS [RowNumber], N'624' AS [Designation], N'Aktivácia dlhodobého hmotného majetku' AS [Text_sk], N'Capitalization of long-term tangible assets' AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 13 AS [RowOrdinal]
    UNION ALL SELECT 72702, 79, N'63', N'Daňové a colné výnosy a výnosy z poplatkov (r. 080 až r. 082)', N'Tax and customs revenues and revenues from fees - total (lines 080 to 082)', 1, NULL, NULL, 14
    UNION ALL SELECT 72702, 80, N'631', N'Daňové a colné výnosy štátu', N'Tax and customs revenues of the state', 0, NULL, NULL, 15
    UNION ALL SELECT 72702, 81, N'632', N'Daňové výnosy samosprávy', N'Tax revenues of the local government', 0, NULL, NULL, 16
    UNION ALL SELECT 72702, 82, N'633', N'Výnosy z poplatkov', N'Revenues from fees', 0, NULL, NULL, 17
    UNION ALL SELECT 72702, 83, N'64', N'Ostatné výnosy z prevádzkovej činnosti (r. 084 až r. 089)', N'Other operating revenues - total (lines 084 to 089)', 1, NULL, NULL, 18
    UNION ALL SELECT 72702, 84, N'641', N'Tržby z predaja dlhodobého nehmotného majetku a dlhodobého hmotného majetku', N'Revenues from the sale of non-current intangible assets and non-current tangible assets', 0, NULL, NULL, 19
    UNION ALL SELECT 72702, 85, N'642', N'Tržby z predaja materiálu', N'Revenues from material sold', 0, NULL, NULL, 20
    UNION ALL SELECT 72702, 86, N'644', N'Zmluvné pokuty, penále a úroky z omeškania', N'Contractual fines, penalties, and interest on late payment', 0, NULL, NULL, 21
    UNION ALL SELECT 72702, 87, N'645', N'Ostatné pokuty, penále a úroky z omeškania', N'Other fines, penalties, and interest on late payment', 0, NULL, NULL, 22
    UNION ALL SELECT 72702, 88, N'646', N'Výnosy z odpísaných pohľadávok', N'Revenues from written off receivables', 0, NULL, NULL, 23
    UNION ALL SELECT 72702, 89, N'648', N'Ostatné výnosy z prevádzkovej činnosti', N'Other operating revenues', 0, NULL, NULL, 24
    UNION ALL SELECT 72702, 90, N'65', N'Zúčtovanie rezerv a opravných položiek z prevádzkovej činnosti a finančnej činnosti a zúčtovanie časového rozlíšenia (r. 091 + r. 096 +r. 099)', N'Clearing of provisions and adjusting entries to operating and financial activities, and clearing of accruals and deferrals - total (line 091 + line 091 + line 096 + line 099)', 1, NULL, NULL, 25
    UNION ALL SELECT 72702, 91, NULL, N'Zúčtovanie rezerv a opravných položiek z prevádzkovej činnosti (r. 092 až r. 095)', N'Clearing of provisions and adjusting entries to operating activities - total (lines 092 to 095)', 1, NULL, NULL, 26
    UNION ALL SELECT 72702, 92, N'652', N'Zúčtovanie zákonných rezerv z prevádzkovej činnosti', N'Clearing of legal provisions out of operations', 0, NULL, NULL, 27
    UNION ALL SELECT 68703, 34, N'**', N'Výsledok hospodárenia z finančnej činnosti (+/-) (r. 20 - r. 27)', NULL, 1, NULL, NULL, 33
    UNION ALL SELECT 68703, 35, N'**', N'Výsledok hospodárenia za účtovné obdobie pred zdanením (+/-) (r. 18 + r. 34)', NULL, 1, NULL, NULL, 34
    UNION ALL SELECT 68703, 36, N'P.', N'Daň z príjmov (591, 595)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 68703, 37, N'Q.', N'Prevod podielov na výsledku hospodárenia spoločníkom (+/-) (596)', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 68703, 38, N'***', N'Výsledok hospodárenia za účtovné obdobie po zdanení (+/-) (r. 35 - r. 36 - r. 37)', NULL, 1, NULL, NULL, 37
    UNION ALL SELECT 72702, 127, N'693', N'Výnosy samosprávy z bežných transferov zo štátneho rozpočtu a od iných subjektov verejnej správy', N'Revenues of the local government from current transfers from the state budget and from other entities of general government', 0, NULL, NULL, 62
    UNION ALL SELECT 72702, 128, N'694', N'Výnosy samosprávy z kapitálových transferov zo štátneho rozpočtu a od iných subjektov verejnej správy', N'Revenues of the local government from capital transfers from the state budget and from other entities of general government', 0, NULL, NULL, 63
    UNION ALL SELECT 72702, 129, N'695', N'Výnosy samosprávy z bežných transferov od Európskej únie', N'Revenues of the local government from current transfers from the European Union', 0, NULL, NULL, 64
    UNION ALL SELECT 72702, 130, N'696', N'Výnosy samosprávy z kapitálových transferov od Európskych spoločenstiev', N'Revenues of the local government from capital transfers from the European Union', 0, NULL, NULL, 65
    UNION ALL SELECT 72702, 131, N'697', N'Výnosy samosprávy z bežných transferov od ostatných subjektov mimo verejnej správy', N'Revenues of the local government from current transfers from other entities outside of general government', 0, NULL, NULL, 66
    UNION ALL SELECT 72702, 132, N'698', N'Výnosy samosprávy z kapitálových transferov od ostatných subjektov mimo verejnej správy', N'Revenues of the local government from capital transfers from other entities outside of general government', 0, NULL, NULL, 67
    UNION ALL SELECT 72702, 133, N'699', N'Výnosy samosprávy z odvodu rozpočtových príjmov', N'Revenues of the local government from budgetary contributions', 0, NULL, NULL, 68
    UNION ALL SELECT 72702, 134, NULL, N'Účtová trieda 6 súčet (r. 065 + r. 069 + r. 074 + r. 079 + r. 083 + r. 090 + r. 100 + r. 109 + r. 114 + r. 124)', N'Account class 6 - total (line 065 + line 069 + line 074 + line 079 + line 083 + line 090 + line 100 + line 109 + line 114 + line 124)', 1, NULL, NULL, 69
    UNION ALL SELECT 72702, 135, NULL, N'Výsledok hospodárenia pred zdanením (r. 134 mínus r. 064) (+/-)', N'Profit (loss) before tax (line 134 - line 064) (+/-)', 1, NULL, NULL, 70
    UNION ALL SELECT 72702, 136, N'591', N'Splatná daň z príjmov', N'Income tax - current', 0, NULL, NULL, 71
    UNION ALL SELECT 72702, 137, N'595', N'Dodatočne platená daň z príjmov', N'Supplementary income tax levies', 0, NULL, NULL, 72
    UNION ALL SELECT 72702, 138, NULL, N'Výsledok hospodárenia po zdanení r. 135 mínus (r. 136, r. 137) (+/-)', N'Profit/loss after tax (line 135 - line 136, line 137) (+/-)', 1, NULL, NULL, 73
    UNION ALL SELECT 69002, 175, N'2.', N'Bežné bankové úvery (461AÚ, 221AÚ, 231, 232)', N'Current bank loans (461A, 221A, 231, 232)', 0, N'Bežné bankové úvery', N'r.175 - Bežné bankové úvery (461AÚ, 221AÚ, 231, 232)', 60
    UNION ALL SELECT 69002, 176, N'3.', N'Vydané dlhopisy krátkodobé (473AÚ, 241) - (255AÚ)', N'Issued short-term bonds (473A, 241 ) - (255A)', 0, N'Vydané dlhopisy krátkodobé', N'r.176 - Vydané dlhopisy krátkodobé (473AÚ, 241) - (255AÚ)', 61
    UNION ALL SELECT 69002, 177, N'4.', N'Ostatné krátkodobé finančné výpomoci (249)', N'Other short-term financial assistance (249)', 0, N'Ostatné krátkodobé finančné výpomoci', N'r.177 - Ostatné krátkodobé finančné výpomoci (249)', 62
    UNION ALL SELECT 69002, 178, N'5.', N'Prijaté návratné finančné výpomoci od subjektov verejnej správy dlhodobé (273AÚ)', N'Repayable financial assistance accepted from entities of public administration - long-term (273A)', 0, N'Prijaté návratné finančné výpomoci od subjektov verejnej správy dlhodobé', N'r.178 - Prijaté návratné finančné výpomoci od subjektov verejnej správy dlhodobé (273AÚ)', 63
    UNION ALL SELECT 69002, 179, N'6.', N'Prijaté návratné finančné výpomoci od subjektov verejnej správy krátkodobé (273AÚ)', N'Repayable financial assistance accepted from entities of public administration - short-term (273A)', 0, N'Prijaté návratné finančné výpomoci od subjektov verejnej správy krátkodobé', N'r.179 - Prijaté návratné finančné výpomoci od subjektov verejnej správy krátkodobé (273AÚ)', 64
    UNION ALL SELECT 69002, 180, N'C.', N'Časové rozlíšenie súčet (r. 181 + r. 182)', N'Accruals and deferrals - total (lines 181 to 182)', 1, N'Časové rozlíšenie súčet', N'r.180 - Časové rozlíšenie súčet (r. 181 + r. 182)', 65
    UNION ALL SELECT 69002, 181, N'C.1.', N'Výdavky budúcich období (383)', N'Accrued expenses (383)', 0, N'Výdavky budúcich období', N'r.181 - Výdavky budúcich období (383)', 66
    UNION ALL SELECT 69002, 182, N'2.', N'Výnosy budúcich období (384)', N'Deferred income (384)', 0, N'Výnosy budúcich období', N'r.182 - Výnosy budúcich období (384)', 67
    UNION ALL SELECT 69002, 183, N'D.', N'Vzťahy k účtom klientov Štátnej pokladnice (účtová skupina 20)', N'Relationships to the State Treasury client accounts (account group 20)', 0, N'Vzťahy k účtom klientov Štátnej pokladnice', N'r.183 - Vzťahy k účtom klientov Štátnej pokladnice (účtová skupina 20)', 68
    UNION ALL SELECT 94202, 57, NULL, N'Vlastné imanie', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 94202, 58, NULL, N'Základné imanie', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 94202, 59, NULL, N'z toho: Upísané základné imanie splatené', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94202, 60, NULL, N'Vlastné akcie', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 94202, 61, NULL, N'Emisné ážio', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 94202, 62, NULL, N'Finančné zdroje poskytnuté pobočke zahraničnej poisťovne', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 94202, 63, NULL, N'Rezervné fondy a ostatné fondy tvorené zo zisku', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 94202, 64, NULL, N'z toho: rezervné fondy', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 94202, 65, NULL, N'Fond vyrovnávacej rezervy', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 94202, 66, NULL, N'Ostatné kapitálové fondy', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 94202, 67, NULL, N'Oceňovacie rozdiely', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 94202, 68, NULL, N'Z prepočtu zabezpečovacích derivátov', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 94202, 69, NULL, N'Z ocenenia finančných nástrojov na predaj', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 94202, 70, NULL, N'Z ocenenia pozemku a stavieb', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 94202, 71, NULL, N'Ostatné', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 94202, 72, NULL, N'Vlastnosti ľubovoľnej účasti', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 94202, 73, NULL, N'Hospodársky výsledok minulých rokov', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 94202, 74, NULL, N'Hospodársky výsledok vo schvaľovacom období', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 17
    UNION ALL SELECT 94202, 75, NULL, N'Hospodársky výsledok bežného obdobia', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 94202, 76, NULL, N'Záväzky', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 94202, 77, NULL, N'Podriadené záväzky', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 94202, 78, NULL, N'Prijaté úvery a pôžičky', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 94202, 79, NULL, N'z toho: vydané dlhové cenné papiere', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 94202, 80, NULL, N'úvery prijaté od spriaznených osôb', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 94202, 81, NULL, N'úvery od bánk', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 94202, 82, NULL, N'záväzky z finančného leasingu', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 94202, 83, NULL, N'Vklady pri pasívnom zaistení', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 94202, 84, NULL, N'Záporná reálna hodnota derivátových operácií na obchodovanie', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 94202, 85, NULL, N'Záporná reálna hodnota derivátových operácií na zabezpečenie', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 94202, 86, NULL, N'Rezervy na poistné zmluvy', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 94202, 87, NULL, N'Rezerva na poistné budúcich období', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 94202, 88, NULL, N'Rezerva na poistné plnenia', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 94202, 89, NULL, N'Rezerva na poistné prémie a zľavy', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 94202, 90, NULL, N'Rezerva na úhradu záväzkov voči Slovenskej kancelárii poisťovateľov vznikajúcich z činností podľa osobitného predpisu', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 94202, 91, NULL, N'Rezerva na životné poistenie', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 94202, 92, NULL, N'Ďalšie rezervy', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 94202, 93, NULL, N'Rezerva na krytie rizika z investovania finančných prostriedkov v mene poistených', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 94202, 94, NULL, N'Finančné záväzky z investičných zmlúv', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 94202, 95, NULL, N'z toho: Finančné záväzky z investičných zmlúv na krytie rizika v mene poistených vyplývajúce z investičných zmlúv', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 94202, 96, NULL, N'Netechnické rezervy', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 94202, 97, NULL, N'z toho: dlhodobé zamestnanecké pôžitky', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 94202, 98, NULL, N'Záväzky z poistenia a zaistenia', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 94202, 99, NULL, N'Voči poisteným', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 94202, 100, NULL, N'Zo spolupoistenia', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 94202, 101, NULL, N'Voči sprostredkovateľom', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 94202, 102, NULL, N'Voči zaisťovateľom', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 94202, 103, NULL, N'Ostatné záväzky z poistenia a zaistenia', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 94202, 104, NULL, N'Krátkodobé zamestnanecké pôžitky', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 94202, 105, NULL, N'Záväzky voči zamestnancom zo závislej činnosti', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 94202, 106, NULL, N'Zúčtovanie so Sociálnou poisťovnou a zdravotnými poisťovňami', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 94202, 107, NULL, N'Sociálna poisťovňa', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 94202, 108, NULL, N'Zdravotné poisťovne', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 94202, 109, NULL, N'Sociálny fond', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 94202, 110, NULL, N'Ostatné', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 94202, 111, NULL, N'Daňové záväzky', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 94202, 112, NULL, N'z toho: bežný daňový záväzok', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 94202, 113, NULL, N'odložený daňový záväzok', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 94202, 114, NULL, N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 94202, 115, NULL, N'Ostatné záväzky', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 94202, 116, NULL, N'z toho: prijaté preddavky', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 94202, 117, NULL, N'Pasíva spolu', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 110102, 92, N'5.', N'voči Ministerstvu zdravotníctva Slovenskej republiky', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 110102, 93, N'II.', N'pôžičky zaručené dlhopisom, z toho', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 110102, 94, N'1.', N'v konvertibilnej mene', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 110102, 95, N'2.', N'krátkodobé pôžičky', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 110102, 96, N'3.', N'dlhodobé pôžičky', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 110102, 97, N'III.', N'bankové úvery, z toho', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 110102, 98, N'1.', N'krátkodobé úvery', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 110102, 99, N'IV.', N'ostatné záväzky, z toho', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 110102, 100, N'1.', N'z daní', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 110102, 101, N'2.', N'záväzky voči zamestnancom celkom', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 110102, 102, N'2a.', N'z toho zo sociálneho poistenia a zdravotného poistenia', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 110102, 103, N'3.', N'z finančného prenájmu', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 110102, 104, N'4.', N'z dotácií zo štátneho rozpočtu a ostatné dotácie', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 110102, 105, N'H.', N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 110102, 106, NULL, N'PASÍVA spolu', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 110102, 999, NULL, N'Kontrolné číslo', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 8101, 1, NULL, N'Dlhodobý nehmotný majetok', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 8101, 2, NULL, N'Dlhodobý hmotný majetok', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 8101, 3, NULL, N'Dlhodobý finančný majetok', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 8101, 4, NULL, N'Zásoby celkom súčet (r. 05 až 07)', NULL, 1, NULL, NULL, 3
    UNION ALL SELECT 8101, 5, NULL, N'Materiál', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 8101, 6, NULL, N'Tovar', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 8101, 7, NULL, N'Nedokončená výroba, výrobky, zvieratá, ostatné', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 8101, 8, NULL, N'Pohľadávky', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 8101, 9, NULL, N'Krátkodobý finančný majetok súčet (r. 10 až 12)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 8101, 10, NULL, N'Peniaze a ceniny', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 8101, 11, NULL, N'Účty v bankách', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 8101, 12, NULL, N'Ostatný krátkodobý finančný majetok', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 8101, 13, NULL, N'Priebežné položky (+/-)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 8101, 14, NULL, N'Opravná položka k nadobudnutému majetku (aktívna)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 8101, 15, NULL, N'Majetok celkom r. 01 + r. 02 + r. 03 + r. 04 + r. 08 + r. 09 +/- r. 13 + r. 14', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 118003, 1, N'501', N'Spotreba materiálu', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 118003, 2, N'502', N'Spotreba energie', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 118003, 3, N'504', N'Predaný tovar', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 118003, 4, N'511', N'Opravy a udržiavanie', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 118003, 5, N'512', N'Cestovné', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 118003, 6, N'513', N'Náklady na reprezentáciu', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 118003, 7, N'518', N'Ostatné služby', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 118003, 8, N'521', N'Mzdové náklady', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 118003, 9, N'524', N'Zákonné sociálne poistenie a zdravotné poistenie', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 118003, 10, N'525', N'Ostatné sociálne poistenie', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 118003, 11, N'527', N'Zákonné sociálne náklady', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 118003, 12, N'528', N'Ostatné sociálne náklady', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 118003, 13, N'531', N'Daň z motorových vozidiel', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 118003, 14, N'532', N'Daň z nehnuteľností', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 118003, 15, N'538', N'Ostatné dane a poplatky', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 118003, 16, N'541', N'Zmluvné pokuty a penále', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 118003, 17, N'542', N'Ostatné pokuty a penále', NULL, 0, NULL, NULL, 16
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 118003 AS [TableErpId], 18 AS [RowNumber], N'543' AS [Designation], N'Odpísanie pohľadávky' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 17 AS [RowOrdinal]
    UNION ALL SELECT 118003, 19, N'544', N'Úroky', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 118003, 20, N'545', N'Kurzové straty', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 118003, 21, N'546', N'Dary', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 118003, 22, N'547', N'Osobitné náklady', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 118003, 23, N'548', N'Manká a škody', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 118003, 24, N'549', N'Iné ostatné náklady', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 118003, 25, N'551', N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 118003, 26, N'552', N'Zostatková cena predaného dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 118003, 27, N'553', N'Predané cenné papiere', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 118003, 28, N'554', N'Predaný materiál', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 118003, 29, N'555', N'Náklady na krátkodobý finančný majetok', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 118003, 30, N'556', N'Tvorba fondov', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 118003, 31, N'557', N'Náklady na precenenie cenných papierov', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 118003, 32, N'558', N'Tvorba a zúčtovanie opravných položiek', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 118003, 33, N'561', N'Poskytnuté príspevky organizačným zložkám', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 118003, 34, N'562', N'Poskytnuté príspevky iným účtovným jednotkám', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 118003, 35, N'563', N'Poskytnuté príspevky fyzickým osobám', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 118003, 36, N'565', N'Poskytnuté príspevky z podielu zaplatenej dane', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 118003, 37, N'567', N'Poskytnuté príspevky z verejnej zbierky', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 118003, 38, NULL, N'Účtová trieda 5 spolu r. 01 až r. 37', NULL, 1, NULL, NULL, 37
    UNION ALL SELECT 301, 29, N'2.', N'Pohľadávky z privatizácie (315 A)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 301, 30, N'3.', N'Iné pohľadávky (335A, 33XA, 373A, 375A, 376A, 378A)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 301, 31, N'B.II.', N'Krátkodobé pohľadávky súčet (r. 032 až 036)', NULL, 1, NULL, NULL, 30
    UNION ALL SELECT 301, 32, N'B.II.1', N'Pohľadávky z obchodného styku (311A, 312A, 313A, 314A, 316, 31XA)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 301, 33, N'2.', N'Pohľadávky z privatizácie (315 A)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 301, 34, N'3.', N'Sociálne zabezpečenie (336)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 301, 35, N'4.', N'Daňové pohľadávky (341, 342, 345)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 301, 36, N'5.', N'Iné pohľadávky (335A, 33XA, 373A, 375A, 376A, 378A)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 301, 37, N'B.III.', N'Finančné účty súčet (r . 038 až r. 045)', NULL, 1, NULL, NULL, 36
    UNION ALL SELECT 301, 38, N'B.III.1.', N'Peniaze (211, 213, 21X)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 301, 39, N'2.', N'Účty v bankách (221A, +/-261)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 301, 40, N'3.', N'Účty v bankách s dobou viazanosti dlhšou ako jeden rok 221A', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 301, 41, N'4.', N'Účty v bankách na prostriedky z privatizácie s dobou viazanosti dlhšou ako jeden rok (222 A)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 301, 42, N'5.', N'Účty v bankách na prostriedky z privatizácie s dobou viazanosti najviac jeden rok (222A)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 301, 43, N'6.', N'Krátkodobý finančný majetok (251, 253, 256, 257A, 25X)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 301, 44, N'7.', N'Krátkodobé cenné papiere a podiely na privatizáciu (257A)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 301, 45, N'8.', N'Obstarávaný krátkodobý finančný majetok (259)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 302, 46, NULL, N'Spolu vlastné imanie a záväzky r. 047 + r. 051', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 302, 47, N'A.', N'Vlastné imanie r. 048 až r. 050', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 302, 48, N'A.I.1.', N'Základné imanie (411)', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 302, 49, N'2.', N'Fond privatizácie (416)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 302, 50, N'3.', N'Fond osobitných prostriedkov (417)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 302, 51, N'B.', N'Záväzky r. 052 + r. 056 + r. 064 + r. 071', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 302, 52, N'B.I.', N'Rezervysúčet (r. 053 až r. 055)', NULL, 1, NULL, NULL, 6
    UNION ALL SELECT 302, 53, N'B.I.1.', N'Rezervy zákonné (451A)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 302, 54, N'2.', N'Ostatné dlhodobé rezervy (459 A, 45XA)', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 302, 55, N'3.', N'Krátkodobé rezervy (323, 32X, 451A, 459A, 45XA)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 302, 56, N'B.II.', N'Dlhodobé záväzky súčet (r. 057 až r. 063)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 302, 57, N'B.II.1.', N'Dlhodobé záväzky z obchodného styku (479A)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 302, 58, N'2.', N'Dlhodobé nevyfakturované dodávky (476A)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 302, 59, N'3.', N'Dlhodobé prijaté preddavky (475A)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 302, 60, N'4.', N'Dlhodobé zmenky na úhradu (478A)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 302, 61, N'5.', N'Vydané dlhopisy (473A)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 302, 62, N'6.', N'Záväzky zo sociálneho fondu (472)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 302, 63, N'7.', N'Ostatné dlhodobé záväzky ( 479A, 47XA, 373A, 377A)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 302, 64, N'B.III.', N'Krátkodobé záväzky súčet (r. 065 až r. 070)', NULL, 1, NULL, NULL, 18
    UNION ALL SELECT 302, 65, N'B.III.1.', N'Záväzky z obchodného styku (321, 322, 324, 325, 32X, 475A, 478A, 479A, 47XA)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 302, 66, N'2.', N'Nevyfakturované dodávky (326, 476A)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 302, 67, N'3.', N'Záväzky voči zamestnancom (331,333,33X,479A)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 302, 68, N'4.', N'Záväzky zo sociálneho zabezpečenia (336, 479A)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 302, 69, N'5.', N'Daňové záväzky (341, 342, 345, 34X)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 302, 70, N'6.', N'Ostatné záväzky ( 367,373A, 377A, 379A, 479A, 47X)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 302, 71, N'B.IV.', N'Bankové úvery a výpomoci súčet (r. 072 až r. 074)', NULL, 1, NULL, NULL, 25
    UNION ALL SELECT 302, 72, N'B.IV.1.', N'Bankové úvery dlhodobé (461A, 46XA)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 302, 73, N'2.', N'Bežné bankové úvery (221A, 231, 232, 23X, 461A, 46XA)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 302, 74, N'3.', N'Krátkodobé finančné výpomoci (241, 249, 24X, 473A)', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 28
    UNION ALL SELECT 2101, 16, N'5.', N'Základné stádo a ťažné zvieratá (026) - /086, 092A/', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 2101, 17, N'6.', N'Ostatný dlhodobý hmotný majetok (029, 02X, 032) - /089, 08X, 092A/', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 2101, 18, N'7.', N'Obstarávaný dlhodobý hmotný majetok (042) - 094', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 2101, 19, N'8.', N'Poskytnuté preddavky na dlhodobý hmotný majetok (052) - 095A', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 2101, 20, N'9.', N'Opravná položka k nadobudnutému majetku (+/- 097) +/- 098', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 2101, 21, N'A.III.', N'Dlhodobý finančný majetok súčet (r. 022 až r. 029)', NULL, 1, NULL, NULL, 20
    UNION ALL SELECT 2101, 22, N'A.III.1.', N'Podielové cenné papiere a podiely v dcérskej účtovnej jednotke (061) - 096A', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 2101, 23, N'2.', N'Podielové cenné papiere a podiely v spoločnosti s podstatným vplyvom (062) - 096A', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 2101, 24, N'3.', N'Ostatné dlhodobé cenné papiere a podiely (063, 065) - 096A', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 2101, 25, N'4.', N'Pôžičky účtovnej jednotke v konsolidovanom celku (066A) - 096A', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 2101, 26, N'5.', N'Ostatný dlhodobý finančný majetok (067A, 069, 06XA) - 096A', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 2101, 27, N'6.', N'Pôžičky s dobou splatnosti najviac jeden rok (066A, 067A, 06XA) - 096A', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 2101, 28, N'7.', N'Obstarávaný dlhodobý finančný majetok (043) - 096A', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 2101, 29, N'8.', N'Poskytnuté preddavky na dlhodobý finančný majetok (053) - 095A', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 2101, 30, N'B.', N'Obežný majetok r. 031 + r. 038 + r. 046 + r. 055', NULL, 1, NULL, NULL, 29
    UNION ALL SELECT 2101, 31, N'B.I.', N'Zásoby súčet (r. 032 až r. 037)', NULL, 1, NULL, NULL, 30
    UNION ALL SELECT 2101, 32, N'B.I.1.', N'Materiál (112, 119, 11X) - /191, 19X/', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 2101, 33, N'2.', N'Nedokončená výroba a polotovary vlastnej výroby (121, 122, 12X) - /192, 193, 19X/', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 2101, 34, N'3.', N'Výrobky (123) - 194', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 2101, 35, N'4.', N'Zvieratá (124) - 195', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 2101, 36, N'5.', N'Tovar (132, 13X, 139) - /196, 19X/', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 2101, 37, N'6.', N'Poskytnuté preddavky na zásoby (314A) - 391A', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 2101, 38, N'B.II.', N'Dlhodobé pohľadávky súčet (r. 039 až r. 045)', NULL, 1, NULL, NULL, 37
    UNION ALL SELECT 2101, 39, N'B.II.1.', N'Pohľadávky z obchodného styku (311A, 312A, 313A, 314A, 315A, 31XA) - 391A', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 2101, 40, N'2.', N'Čistá hodnota zákazky (316A)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 2101, 41, N'3.', N'Pohľadávky voči dcérskej účtovnej jednotke a materskej účtovnej jednotke (351A) - 391A', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 2101, 42, N'4.', N'Ostatné pohľadávky v rámci konsolidovaného celku (351A) - 391A', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 2101, 43, N'5.', N'Pohľadávky voči spoločníkom, členom a združeniu (354A, 355A, 358A, 35XA) - 391A', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 2101, 44, N'6.', N'Iné pohľadávky (335A, 33XA, 371A, 373A, 374A, 375A, 376A, 378A) - 391A', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 2101, 45, N'7.', N'Odložená daňová pohľadávka (481A)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 2101, 46, N'B.III.', N'Krátkodobé pohľadávky súčet (r. 047 až r. 054)', NULL, 1, NULL, NULL, 45
    UNION ALL SELECT 2101, 47, N'B.III.1.', N'Pohľadávky z obchodného styku (311A, 312A, 313A, 314A, 315A, 31XA) - 391A', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 2101, 48, N'2.', N'Čistá hodnota zákazky (316A)', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 2101, 49, N'3.', N'Pohľadávky voči dcérskej účtovnej jednotke a materskej účtovnej jednotke (351A) - 391A', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 2101, 50, N'4.', N'Ostatné pohľadávky v rámci konsolidovaného celku (351A) - 391A', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 2101, 51, N'5.', N'Pohľadávky voči spoločníkom, členom a združeniu (354A, 355A, 358A, 35XA, 398A) - 391A', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 2101, 52, N'6.', N'Sociálne poistenie (336) - 391A', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 2101, 53, N'7.', N'Daňové pohľadávky a dotácie (341, 342, 343, 345, 346, 347) - 391A', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 2101, 54, N'8.', N'Iné pohľadávky (335A, 33XA, 371A, 373A, 374A, 375A, 376A,378A) - 391A', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 2101, 55, N'B.IV.', N'Finančné účty súčet (r. 056 až r. 060)', NULL, 1, NULL, NULL, 54
    UNION ALL SELECT 2101, 56, N'B.IV.1.', N'Peniaze (211, 213, 21X)', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 2101, 57, N'2.', N'Účty v bankách (221A, 22X +/- 261)', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 2101, 58, N'3.', N'Účty v bankách s dobou viazanosti dlhšou ako jeden rok 22XA', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 2101, 59, N'4.', N'Krátkodobý finančný majetok (251, 253, 256, 257, 25X) - /291, 29X/', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 2101, 60, N'5.', N'Obstarávaný krátkodobý finančný majetok (259, 314A) - 291', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 2101, 61, N'C.', N'Časové rozlíšenie súčet (r. 062 až r. 065)', NULL, 1, NULL, NULL, 60
    UNION ALL SELECT 2101, 62, N'C.1.', N'Náklady budúcich období dlhodobé (381A, 382A)', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 2101, 63, N'2.', N'Náklady budúcich období krátkodobé (381A, 382A)', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 2101, 64, N'3.', N'Príjmy budúcich období dlhodobé (385A)', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 2101, 65, N'4.', N'Príjmy budúcich období krátkodobé (385A)', NULL, 0, NULL, NULL, 64
    UNION ALL SELECT 2701, 2, N'1.', N'Poistné v hrubej výške', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 2701, 3, N'2.', N'Prevedený výsledok z finančného umiestnenia z netechnického účtu', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 2701, 4, N'3.', N'Ostatné technické výnosy', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 2701, 5, N'4.', N'Náklady na poistné plnenia', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 2701, 6, N'4a.', N'Náklady na poistné plnenia v hrubej výške v tom', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 2701, 7, N'4aa.', N'Náklady na ambulantnú zdravotnú starostlivosť', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 2701, 8, N'4ab.', N'Náklady na ústavnú zdravotnú starostlivosť', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 2701, 9, N'4ac.', N'Náklady na lieky', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 2701, 10, N'4ad.', N'Náklady na zdravotnícke pomôcky', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 2701, 11, N'4ae.', N'Náklady na ostatné poistné plnenia', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 2701, 12, N'4b.', N'Nárok na úhradu nákladov od iných subjektov', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 2701, 13, N'4c.', N'Zmena stavu technickej rezervy na poistné plnenia v hrubej výške', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 2701, 14, N'5.', N'Zmena stavu iných technických rezerv', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 2701, 15, N'7.', N'Čistá výška prevádzkových nákladov', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 2701, 16, N'7a.', N'Obstarávacie náklady na poistné zmluvy', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 2701, 17, N'7b.', N'Správna réžia', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 2701, 18, N'8.', N'Ostatné technické náklady', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 2701, 19, N'10.', N'Výsledok technického účtu k neživotnému poisteniu A', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 2701, 20, N'I. B.', N'TECHNICKÝ ÚČET K NEŽIVOTNÉMU POISTENIU – INDIVIDUÁLNE ZDRAVOTNÉ POISTENIE', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 2701, 21, N'1.', N'Zaslúžené poistné, bez zaistenia', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 2701, 22, N'1a.', N'Poistné v hrubej výške', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 2701, 23, N'1b.', N'Poistné v hrubej výške postúpené zaisťovateľom', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 2701, 24, N'1c.', N'Zmena stavu technickej rezervy na poistné budúcich období', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 2701, 25, N'1d.', N'Podiel zaisťovateľov na tvorbe a použití technickej rezervy na poistné budúcich období', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 2701, 26, N'2.', N'Prevedený výsledok z finančného umiestnenia z netechnického účtu', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 2701, 27, N'3.', N'Ostatné technické výnosy, bez zaistenia', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 2701, 28, N'4.', N'Náklady na poistné plnenia, bez zaistenia', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 2701, 29, N'4a.', N'Náklady na poistné plnenia v hrubej výške', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 2701, 30, N'4aa.', N'Náklady na poistné plnenia postúpené zaisťovateľom', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 2701, 31, N'4b.', N'Zmena stavu technickej rezervy na poistné plnenia v hrubej výške', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 2701, 32, N'4ba.', N'Podiel zaisťovateľov na tvorbe a použití technickej rezervy na poistné plnenia', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 2701, 33, N'5.', N'Zmena stavu iných technických rezerv, bez zaistenia', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 2701, 34, N'6.', N'Prémie a zľavy, bez zaistenia', NULL, 0, NULL, NULL, 33
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 2701 AS [TableErpId], 35 AS [RowNumber], N'7.' AS [Designation], N'Čistá výška prevádzkových nákladov' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 34 AS [RowOrdinal]
    UNION ALL SELECT 2701, 36, N'7a.', N'Obstarávacie náklady na poistné zmluvy', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 2701, 37, N'7b.', N'Zmena stavu výšky prevedených obstarávacích nákladov na poistné zmluvy', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 2701, 38, N'7c.', N'Správna réžia', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 2701, 39, N'7d.', N'Provízie od zaisťovateľov a podiely na ziskoch', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 2701, 40, N'8.', N'Ostatné technické náklady, bez zaistenia', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 2701, 41, N'10.', N'Výsledok technického účtu k neživotnému poisteniu B', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 2701, 42, N'III.', N'NETECHNICKÝ ÚČET', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 2701, 43, N'1. A.', N'Výsledok technického účtu k neživotnému poisteniu A', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 2701, 44, N'1. B.', N'Výsledok technického účtu k neživotnému poisteniu B', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 2701, 45, N'3.', N'Výnosy z finančného umiestnenia', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 2701, 46, N'3a.', N'Výnosy z podielových cenných papierov a vkladov a v tom rozhodujúci vplyv', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 2701, 47, N'3b.', N'Výnosy z ostatného finančného umiestnenia a v tom rozhodujúci vplyv', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 2701, 48, N'3ba.', N'Výnosy z pozemkov a stavieb', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 2701, 49, N'3bb.', N'Výnosy z ostatných zložiek finančného umiestnenia', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 2701, 50, N'3c.', N'Použitie opravných položiek k finančnému umiestneniu', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 2701, 51, N'3d.', N'Výnosy z realizácie finančného umiestnenia', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 2701, 52, N'3e.', N'Prírastky hodnoty finančného umiestnenia', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 2701, 53, N'5.', N'Náklady na finančné umiestnenie', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 2701, 54, N'5a.', N'Náklady na finančné umiestnenie', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 2701, 55, N'5b.', N'Tvorba opravných položiek k finančnému umiestneniu', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 2701, 56, N'5c.', N'Náklady na realizáciu finančného umiestnenia', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 2701, 57, N'5d.', N'Úbytky hodnoty finančného umiestnenia', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 2701, 58, N'6.', N'Prevedené výnosy z finančného umiestnenia na technický účet', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 2701, 59, N'7.', N'Ostatné výnosy', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 2701, 60, N'8.', N'Ostatné náklady', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 2701, 61, N'8a.', N'Ostatné dane a poplatky', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 2701, 62, N'9.', N'Daň z príjmov z bežnej činnosti', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 2701, 63, N'10.', N'Výsledok hospodárenia z bežnej činnosti po zdanení', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 2701, 64, N'11.', N'Mimoriadne výnosy', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 2701, 65, N'12.', N'Mimoriadne náklady', NULL, 0, NULL, NULL, 64
    UNION ALL SELECT 2701, 66, N'13.', N'Mimoriadny výsledok hospodárenia', NULL, 0, NULL, NULL, 65
    UNION ALL SELECT 2701, 67, N'14.', N'Daň z príjmov z mimoriadnej činnosti', NULL, 0, NULL, NULL, 66
    UNION ALL SELECT 2701, 68, N'16.', N'Výsledok hospodárenia za účtovné obdobie', NULL, 0, NULL, NULL, 67
    UNION ALL SELECT 2701, 999, NULL, N'Kontrolné číslo', NULL, 0, NULL, NULL, 68
    UNION ALL SELECT 518203, 42, N'6.2.', N'Finančné výnosy alebo finančné náklady zo zaistných zmlúv', N'Insurance finance income or expenses from reinsurance contracts held', 0, NULL, NULL, 41
    UNION ALL SELECT 518203, 43, N'6.2.1.', N'Úrokový prírastok a efekt zmeny diskontnej sadzby', N'Accretion of interest & the effect of changes in interest rates', 0, NULL, NULL, 42
    UNION ALL SELECT 518203, 44, N'6.2.2.', N'Finančné riziká a dopad zmien finančných rizík vrátane kurzových rozdielov', N'The Effect of financial risk and changes in financial risk includ. FX differences', 0, NULL, NULL, 43
    UNION ALL SELECT 518203, 45, N'7.', N'Ostatné (v rámci finančného výsledku)', N'Other (finance result)', 0, NULL, NULL, 44
    UNION ALL SELECT 518203, 46, NULL, N'Finančný výsledok', N'Finance result (5) + (6) + (7)', 0, NULL, NULL, 45
    UNION ALL SELECT 518203, 47, N'8.', N'Ostatné výnosy', N'Other income', 0, NULL, NULL, 46
    UNION ALL SELECT 518203, 48, N'9.', N'Ostatné náklady', N'Other expenses', 0, NULL, NULL, 47
    UNION ALL SELECT 518203, 49, N'10.', N'Výsledok hospodárenia pred zdanením', N'Income before income taxes', 0, NULL, NULL, 48
    UNION ALL SELECT 518203, 50, N'11.', N'Dane', N'Taxes', 0, NULL, NULL, 49
    UNION ALL SELECT 518203, 51, N'12.', N'Výsledok hospodárenia po zdanení', N'Net income', 0, NULL, NULL, 50
    UNION ALL SELECT 518204, 1, N'1.', N'Výnosy z poistných služieb', N'Insurance Revenue', 0, NULL, NULL, 0
    UNION ALL SELECT 518204, 2, N'1.1.', N'Očakávané poistné plnenia a náklady na poistné zmluvy', N'Expected claims and other expenses', 0, NULL, NULL, 1
    UNION ALL SELECT 518204, 3, N'1.2.', N'Očakávané obstarávacie náklady', N'Recovery of acquisition cash flows', 0, NULL, NULL, 2
    UNION ALL SELECT 518204, 4, N'1.3.', N'Rozpustenie servisnej marže', N'Release of contractual service margin', 0, NULL, NULL, 3
    UNION ALL SELECT 518204, 5, N'1.4.', N'Zmena v rizikovej prirážke na nefinančné riziká', N'Change of risk adjustment', 0, NULL, NULL, 4
    UNION ALL SELECT 518204, 6, N'1.5.', N'Úprava poistného na základe skutočnosti', N'Premium experience adjustment', 0, NULL, NULL, 5
    UNION ALL SELECT 518204, 7, N'1.6.', N'Rozpustenie poistného PAA', N'Premium release PAA', 0, NULL, NULL, 6
    UNION ALL SELECT 518204, 8, N'2.', N'Náklady na poistné služby', N'Insurance service expenses', 0, NULL, NULL, 7
    UNION ALL SELECT 518204, 9, N'2.1.', N'Vzniknuté poistné plnenia a ostatné náklady na poistné služby', N'Incurred claims and other incurred insurance service expenses', 0, NULL, NULL, 8
    UNION ALL SELECT 518204, 10, N'2.1.1.', N'Vzniknuté poistné plnenia', N'Incurred claims', 0, NULL, NULL, 9
    UNION ALL SELECT 518204, 11, N'2.1.2.', N'Skutočné priamo a nepriamo priraditeľné náklady na poistné služby', N'Directly and indirectly attributable expenses', 0, NULL, NULL, 10
    UNION ALL SELECT 518204, 12, N'2.1.3.', N'Úprava o investičný komponent', N'Adjustment of investment component', 0, NULL, NULL, 11
    UNION ALL SELECT 518204, 13, N'2.2.', N'Amortizácia obstarávacích nákladov', N'Insurance acquisition cash flow amortization', 0, NULL, NULL, 12
    UNION ALL SELECT 518204, 14, N'2.3.', N'Zmena hodnoty poistných zmlúv na vzniknuté poistné udalosti', N'Changes in liability for incurred claims', 0, NULL, NULL, 13
    UNION ALL SELECT 518204, 15, N'2.4.', N'Straty na nevýhodných poistných zmluvách a ich zmeny', N'Losses on onerous groups of contracts and reversals of such losses', 0, NULL, NULL, 14
    UNION ALL SELECT 518204, 16, N'3.', N'Výnosy/náklady z pasívneho zaistenia', N'Income or expenses from reinsurance contracts held', 0, NULL, NULL, 15
    UNION ALL SELECT 518204, 17, N'3.1.', N'Očakávaný podiel zaisťovateľa na poistných plneniach a ostatných nákladoch zo zaistenia', N'Expected claims and other expenses recovery', 0, NULL, NULL, 16
    UNION ALL SELECT 518204, 18, N'3.2.', N'Zmena v rizikovej prirážke na nefinančné riziká', N'Change of risk adjustment', 0, NULL, NULL, 17
    UNION ALL SELECT 518204, 19, N'3.3.', N'Rozpustenie servisnej marže', N'Release of contractual service margin', 0, NULL, NULL, 18
    UNION ALL SELECT 518204, 20, N'3.4.', N'Náklady na zaistenie pre zaistné zmluvy oceňované PAA modelom', N'Reinsurance expenses contracts measured under the PAA', 0, NULL, NULL, 19
    UNION ALL SELECT 518204, 21, N'3.5.', N'Úprava postúpeného poistného o skutočnosť', N'Ceded premium experience adjustment', 0, NULL, NULL, 20
    UNION ALL SELECT 518204, 22, N'3.6.', N'Podiel zaisťovateľa na nákladoch na poistné plnenia a ostatných priraditeľných nákladoch', N'Claims recovered and other incurred attributable expenses', 0, NULL, NULL, 21
    UNION ALL SELECT 518204, 23, N'3.7.', N'Podiel zaisťovateľa na zmene hodnoty poistných zmlúv na vzniknuté poistné udalosti', N'Changes that relate to past service - adjustments to incurred claims', 0, NULL, NULL, 22
    UNION ALL SELECT 518204, 24, N'3.8.', N'Podiel zaisťovateľa na stratovom komponente a jeho zmeny', N'Loss recoveries and reversals of recoveries', 0, NULL, NULL, 23
    UNION ALL SELECT 518204, 25, N'3.9.', N'Zmena v riziku neplnenia zaisťovateľa', N'Effect of changes in the risk of reinsurers non-performance', 0, NULL, NULL, 24
    UNION ALL SELECT 518204, 26, N'4.', N'Ostatné (v rámci výsledku za poistné služby)', N'Other (insurance service result)', 0, NULL, NULL, 25
    UNION ALL SELECT 518204, 27, NULL, N'Výsledok za poistné služby', N'Insurance service result (1) + (2) + (3) + (4)', 0, NULL, NULL, 26
    UNION ALL SELECT 518204, 28, N'5.', N'Čistý investičný výsledok', N'Net investment result', 0, NULL, NULL, 27
    UNION ALL SELECT 518204, 29, N'5.1.', N'Čistý úrokový výnos z finančných aktív oceňovaných inak ako cez výsledok hospodárenia (AC, OCI)', N'Interest revenue from financial assets not measured at FVTPL (AC, OCI)', 0, NULL, NULL, 28
    UNION ALL SELECT 518204, 30, N'5.2.', N'Čistý výnos z finančných aktív oceňovaných cez výsledok hospodárenia (FVTPL)', N'Net gains on FVTPL investments', 0, NULL, NULL, 29
    UNION ALL SELECT 518204, 31, N'5.3.', N'Čisté straty z trvalého zníženia hodnoty', N'Net credit impairment losses', 0, NULL, NULL, 30
    UNION ALL SELECT 301, 1, NULL, N'Spolu majetok r. 002 + r. 026', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 301, 2, N'A.', N'Neobežný majetok r. 003 + r. 009 + r. 016', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 301, 3, N'A.I.', N'Dlhodobý nehmotný majetok súčet (r. 004 až r. 008)', NULL, 1, NULL, NULL, 2
    UNION ALL SELECT 301, 4, N'A.I.1.', N'Softvér (013) - /073/', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 301, 5, N'2.', N'Oceniteľné práva (014) - /074/', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 301, 6, N'3.', N'Ostatný dlhodobý nehmotný majetok (019, 01X) - /079, 07X,/', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 301, 7, N'4.', N'Obstarávaný dlhodobý nehmotný majetok (041)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 301, 8, N'5.', N'Poskytnuté preddavky na dlhodobý nehmotný majetok (051)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 301, 9, N'A.II.', N'Dlhodobý hmotný majetok súčet (r. 010 až r. 015)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 301, 10, N'A.II.1.', N'Pozemky (031)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 301, 11, N'2.', N'Stavby (021) - /081/', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 301, 12, N'3.', N'Samostatné hnuteľné veci a súbory hnuteľných vecí (022) - /082/', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 301, 13, N'4.', N'Ostatný dlhodobý hmotný majetok (029, 02X, 032) - /089, 08X/', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 301, 14, N'5.', N'Obstarávaný dlhodobý hmotný majetok (042)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 301, 15, N'6.', N'Poskytnuté preddavky na dlhodobý hmotný majetok (052)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 301, 16, N'A.III.', N'Dlhodobý finančný majetok súčet (r. 017 až r. 025)', NULL, 1, NULL, NULL, 15
    UNION ALL SELECT 301, 17, N'A.III.', N'1. Cenné papiere a podiely minimálne s 51% účasťou (061)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 301, 18, N'2.', N'Cenné papiere a podiely s viac ako 20% účasťou a menej ako 51% účasťou (062)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 301, 19, N'3.', N'Ostatné dlhodobé cenné papiere a podiely (063, 065)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 301, 20, N'4.', N'Ostatný dlhodobý finančný majetok (067A, 069, 06XA)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 301, 21, N'5.', N'Majetok fondu (064)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 301, 22, N'6.', N'Prenajatý majetok fondu (068)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 301, 23, N'7.', N'Poskytnuté preddavky na dlhodobý finančný majetok (053)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 301, 24, N'8.', N'Obstarávaný dlhodobý majetok (043)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 301, 25, N'9.', N'Poskytnuté preddavky na dlhodobý finančný majetok (053)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 301, 26, N'B.', N'Obežný majetok r. 027 + r. 031 + r. 037', NULL, 1, NULL, NULL, 25
    UNION ALL SELECT 301, 27, N'B.I.', N'Dlhodobé pohľadávky súčet (r. 028 až r. 030)', NULL, 1, NULL, NULL, 26
    UNION ALL SELECT 301, 28, N'B.I.1.', N'Pohľadávky z obchodného styku (311A, 312A, 313A, 314A, 316 ,31XA, )', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 1302, 17, NULL, N'Zásoby', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 1302, 18, NULL, N'Služby', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 1302, 19, NULL, N'Mzdy', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1302, 20, NULL, N'Platby do poistných fondov', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1302, 21, NULL, N'Prevádzková réžia', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1302, 22, NULL, N'Sociálny fond', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1302, 23, NULL, N'Ostatné', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1302, 24, NULL, N'Výdavky celkom (súčet r. 17 až r. 23)', NULL, 1, NULL, NULL, 7
    UNION ALL SELECT 1302, 25, NULL, N'Rozdiel príjmov a výdavkov (r. 16 - r. 24)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 1302, 26, NULL, N'Daň z príjmov', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 1401, 1, NULL, N'Z vkladu zriaďovateľa alebo zakladateľa', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 1401, 2, NULL, N'Z majetku', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 1401, 3, NULL, N'Z darov a príspevkov', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1401, 4, NULL, N'Z členských príspevkov', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1401, 5, NULL, N'Z podielu zaplatenej dane z príjmov', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1401, 6, NULL, N'Z verejných zbierok', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1401, 7, NULL, N'Z úverov a pôžičiek', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1401, 8, NULL, N'Z dedičstva', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1401, 9, NULL, N'Z organizovania podujatí', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 1401, 10, NULL, N'Z dotácií', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 1401, 11, NULL, N'Z likvidačného zostatku inej účtovnej jednotky', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 1401, 12, NULL, N'Z predaja majetku', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 1401, 13, NULL, N'Z poskytovania služieb a predaja vlastných výrobkov', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 1401, 14, NULL, N'Fond prevádzky, údržby a opráv', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 1401, 15, NULL, N'Ostatné', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 1401, 16, NULL, N'Príjmy celkom (súčet r. 01 až r. 15)', NULL, 1, NULL, NULL, 15
    UNION ALL SELECT 2101, 1, NULL, N'SPOLU MAJETOK r. 002 + r. 030 + r. 061', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 2101, 2, N'A.', N'Neobežný majetok r. 003 + r. 011 + r. 021', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 2101, 3, N'A.I.', N'Dlhodobý nehmotný majetok súčet (r. 004 až r. 010)', NULL, 1, NULL, NULL, 2
    UNION ALL SELECT 2101, 4, N'A.I.1.', N'Aktivované náklady na vývoj (012) - /072, 091A/', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 2101, 5, N'2.', N'Softvér (013) - /073, 091A/', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 2101, 6, N'3.', N'Oceniteľné práva (014) - /074, 091A/', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 2101, 7, N'4.', N'Goodwill (015) - /075, 091A/', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 2101, 8, N'5.', N'Ostatný dlhodobý nehmotný majetok (019, 01X) - /079, 07X, 091A/', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 2101, 9, N'6.', N'Obstarávaný dlhodobý nehmotný majetok (041) - 093', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 2101, 10, N'7.', N'Poskytnuté preddavky na dlhodobý nehmotný majetok (051) - 095A', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 2101, 11, N'A.II.', N'Dlhodobý hmotný majetok súčet (r. 012 až r. 020)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 2101, 12, N'A.II.1.', N'Pozemky (031) - 092A', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 2101, 13, N'2.', N'Stavby (021) - /081, 092A/', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 2101, 14, N'3.', N'Samostatné hnuteľné veci a súbory hnuteľných vecí (022) - /082, 092A/', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 2101, 15, N'4.', N'Pestovateľské celky trvalých porastov (025) - /085, 092A/', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 2701, 1, N'I. A.', N'TECHNICKÝ ÚČET K NEŽIVOTNÉMU POISTENIU - VEREJNÉ ZDRAVOTNÉ POISTENIE', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 518204, 51, N'12.', N'Výsledok hospodárenia po zdanení', N'Net income', 0, NULL, NULL, 50
    UNION ALL SELECT 114102, 96, N'3.', N'Záväzky voči združeniu (368)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 114102, 97, N'3.', N'Spojovací účet pri združení (396)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 114102, 98, N'3.', N'Iné záväzky (379 + 373AÚ + 952AÚ + 954AÚ + 959AÚ)', NULL, 0, NULL, NULL, 42
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 114102 AS [TableErpId], 99 AS [RowNumber], N'4.' AS [Designation], N'Bankové výpomoci a pôžičky súčet (r.100 až r.102)' AS [Text_sk], NULL AS [Text_en], 1 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 43 AS [RowOrdinal]
    UNION ALL SELECT 114102, 100, N'4.', N'Dlhodobé bankové úvery (951AÚ)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 114102, 101, N'4.', N'Bežné bankové úvery (231 + 232 + 951AÚ)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 114102, 102, N'4.', N'Iné krátkodobé finančné výpomoci (249)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 114102, 103, N'5.', N'Prechodné účty pasív súčet (r.104 + r.105)', NULL, 1, NULL, NULL, 47
    UNION ALL SELECT 114102, 104, N'5.', N'Výdavky budúcich období (383)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 114102, 105, N'5.', N'Výnosy budúcich období (384)', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 114102, 106, NULL, N'PASÍVA spolu súčet (r. 056 + r. 076)', NULL, 1, NULL, NULL, 50
    UNION ALL SELECT 518203, 1, N'1.', N'Výnosy z poistných služieb', N'Insurance Revenue', 0, NULL, NULL, 0
    UNION ALL SELECT 518203, 2, N'1.1.', N'Očakávané poistné plnenia a náklady na poistné zmluvy', N'Expected claims and other expenses', 0, NULL, NULL, 1
    UNION ALL SELECT 518203, 3, N'1.2.', N'Očakávané obstarávacie náklady', N'Recovery of acquisition cash flows', 0, NULL, NULL, 2
    UNION ALL SELECT 518203, 4, N'1.3.', N'Rozpustenie servisnej marže', N'Release of contractual service margin', 0, NULL, NULL, 3
    UNION ALL SELECT 518203, 5, N'1.4.', N'Zmena v rizikovej prirážke na nefinančné riziká', N'Change of risk adjustment', 0, NULL, NULL, 4
    UNION ALL SELECT 518203, 6, N'1.5.', N'Úprava poistného na základe skutočnosti', N'Premium experience adjustment', 0, NULL, NULL, 5
    UNION ALL SELECT 518203, 7, N'1.6.', N'Rozpustenie poistného PAA', N'Premium release PAA', 0, NULL, NULL, 6
    UNION ALL SELECT 518203, 8, N'2.', N'Náklady na poistné služby', N'Insurance service expenses', 0, NULL, NULL, 7
    UNION ALL SELECT 518203, 9, N'2.1.', N'Vzniknuté poistné plnenia a ostatné náklady na poistné služby', N'Incurred claims and other incurred insurance service expenses', 0, NULL, NULL, 8
    UNION ALL SELECT 518203, 10, N'2.1.1.', N'Vzniknuté poistné plnenia', N'Incurred claims', 0, NULL, NULL, 9
    UNION ALL SELECT 518203, 11, N'2.1.2.', N'Skutočné priamo a nepriamo priraditeľné náklady na poistné služby', N'Directly and indirectly attributable expenses', 0, NULL, NULL, 10
    UNION ALL SELECT 518203, 12, N'2.1.3.', N'Úprava o investičný komponent', N'Adjustment of investment component', 0, NULL, NULL, 11
    UNION ALL SELECT 518203, 13, N'2.2.', N'Amortizácia obstarávacích nákladov', N'Insurance acquisition cash flow amortization', 0, NULL, NULL, 12
    UNION ALL SELECT 518203, 14, N'2.3.', N'Zmena hodnoty poistných zmlúv na vzniknuté poistné udalosti', N'Changes in liability for incurred claims', 0, NULL, NULL, 13
    UNION ALL SELECT 518203, 15, N'2.4.', N'Straty na nevýhodných poistných zmluvách a ich zmeny', N'Losses on onerous groups of contracts and reversals of such losses', 0, NULL, NULL, 14
    UNION ALL SELECT 518203, 16, N'3.', N'Výnosy/náklady z pasívneho zaistenia', N'Income or expenses from reinsurance contracts held', 0, NULL, NULL, 15
    UNION ALL SELECT 518203, 17, N'3.1.', N'Očakávaný podiel zaisťovateľa na poistných plneniach a ostatných nákladoch zo zaistenia', N'Expected claims and other expenses recovery', 0, NULL, NULL, 16
    UNION ALL SELECT 518203, 18, N'3.2.', N'Zmena v rizikovej prirážke na nefinančné riziká', N'Change of risk adjustment', 0, NULL, NULL, 17
    UNION ALL SELECT 518203, 19, N'3.3.', N'Rozpustenie servisnej marže', N'Release of contractual service margin', 0, NULL, NULL, 18
    UNION ALL SELECT 518203, 20, N'3.4.', N'Náklady na zaistenie pre zaistné zmluvy oceňované PAA modelom', N'Reinsurance expenses contracts measured under the PAA', 0, NULL, NULL, 19
    UNION ALL SELECT 518203, 21, N'3.5.', N'Úprava postúpeného poistného o skutočnosť', N'Ceded premium experience adjustment', 0, NULL, NULL, 20
    UNION ALL SELECT 518203, 22, N'3.6.', N'Podiel zaisťovateľa na nákladoch na poistné plnenia a ostatných priraditeľných nákladoch', N'Claims recovered and other incurred attributable expenses', 0, NULL, NULL, 21
    UNION ALL SELECT 518203, 23, N'3.7.', N'Podiel zaisťovateľa na zmene hodnoty poistných zmlúv na vzniknuté poistné udalosti', N'Changes that relate to past service - adjustments to incurred claims', 0, NULL, NULL, 22
    UNION ALL SELECT 518203, 24, N'3.8.', N'Podiel zaisťovateľa na stratovom komponente a jeho zmeny', N'Loss recoveries and reversals of recoveries', 0, NULL, NULL, 23
    UNION ALL SELECT 518203, 25, N'3.9.', N'Zmena v riziku neplnenia zaisťovateľa', N'Effect of changes in the risk of reinsurers non-performance', 0, NULL, NULL, 24
    UNION ALL SELECT 518203, 26, N'4.', N'Ostatné (v rámci výsledku za poistné služby)', N'Other (insurance service result)', 0, NULL, NULL, 25
    UNION ALL SELECT 518203, 27, NULL, N'Výsledok za poistné služby', N'Insurance service result (1) + (2) + (3) + (4)', 0, NULL, NULL, 26
    UNION ALL SELECT 518203, 28, N'5.', N'Čistý investičný výsledok', N'Net investment result', 0, NULL, NULL, 27
    UNION ALL SELECT 518203, 29, N'5.1.', N'Čistý úrokový výnos z finančných aktív oceňovaných inak ako cez výsledok hospodárenia (AC, OCI)', N'Interest revenue from financial assets not measured at FVTPL (AC, OCI)', 0, NULL, NULL, 28
    UNION ALL SELECT 518203, 30, N'5.2.', N'Čistý výnos z finančných aktív oceňovaných cez výsledok hospodárenia (FVTPL)', N'Net gains on FVTPL investments', 0, NULL, NULL, 29
    UNION ALL SELECT 518203, 31, N'5.3.', N'Čisté straty z trvalého zníženia hodnoty', N'Net credit impairment losses', 0, NULL, NULL, 30
    UNION ALL SELECT 518203, 32, N'5.4.', N'Čistý výnos z dlhových CP oceňovaných FVOCI (pri predaji)', N'Net gains on investment in debt securities measured at FVOCI', 0, NULL, NULL, 31
    UNION ALL SELECT 518203, 33, N'5.5.', N'Čistý výnos z ukončenia vykazovania finančných aktív oceňovaných amortizovanou hodnotou (AC)', N'Net gains from the derecognition of financial assests measured at AC', 0, NULL, NULL, 32
    UNION ALL SELECT 518203, 34, N'5.6.', N'Čistá zmena záväzkov z investičných zmlúv', N'Net change in investment contract liabilities', 0, NULL, NULL, 33
    UNION ALL SELECT 518203, 35, N'5.7.', N'Čistý výnos z precenenia investičného majetku', N'Net gains from fair value adjustments to investment properties', 0, NULL, NULL, 34
    UNION ALL SELECT 518203, 36, N'5.8.', N'Opravná položka k očakávaným kreditným stratám', N'Expected credit loss allowance (OCI, AC)', 0, NULL, NULL, 35
    UNION ALL SELECT 518203, 37, N'6.', N'Čistý finančný výsledok z poistenia', N'Net insurance finance result (IFIE)', 0, NULL, NULL, 36
    UNION ALL SELECT 518203, 38, N'6.1.', N'Finančné výnosy alebo finančné náklady z poistných zmlúv', N'Insurance finance income or expenses from insurance contracts issued', 0, NULL, NULL, 37
    UNION ALL SELECT 518203, 39, N'6.1.1.', N'Úrokový prírastok a efekt zmeny diskontnej sadzby', N'Accretion of interest & the effect of changes in interest rates', 0, NULL, NULL, 38
    UNION ALL SELECT 518203, 40, N'6.1.2.', N'Finančné riziká a dopad zmien finančných rizík vrátane kurzových rozdielov', N'The Effect of financial risk and changes in financial risk includ. FX differences', 0, NULL, NULL, 39
    UNION ALL SELECT 518203, 41, N'6.1.3.', N'Zmena záväzku pre poistné zmluvy ocenené VFA modelom v dôsledku zmeny v reálnej hodnote podkladových aktív', N'Changes in the fair value of underlying assets of contracts measured under VFA (VFA mirroring)', 0, NULL, NULL, 40
    UNION ALL SELECT 114102, 56, NULL, N'A. Vlastné zdroje krytia majetku súčet (r. 057 + r. 062 + r. 073)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 114102, 57, N'1.', N'Fondy Sociálnej poisťovne súčet (r. 058 až r.061)', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 114102, 58, N'1.', N'Fond dlhodobého majetku (901)', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 114102, 59, N'1.', N'Fond prevádzkových prostriedkov (902)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 114102, 60, N'1.', N'Oceňovacie rozdiely z precenenia majetku a záväzkov (905)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 114102, 61, N'1.', N'Správny fond (+/- 914)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 114102, 62, N'2.', N'Osobitné fondy Sociálnej poisťovne súčet (r.063 až r.072)', NULL, 1, NULL, NULL, 6
    UNION ALL SELECT 518204, 32, N'5.4.', N'Čistý výnos z dlhových CP oceňovaných FVOCI (pri predaji)', N'Net gains on investment in debt securities measured at FVOCI', 0, NULL, NULL, 31
    UNION ALL SELECT 518204, 33, N'5.5.', N'Čistý výnos z ukončenia vykazovania finančných aktív oceňovaných amortizovanou hodnotou (AC)', N'Net gains from the derecognition of financial assests measured at AC', 0, NULL, NULL, 32
    UNION ALL SELECT 518204, 34, N'5.6.', N'Čistá zmena záväzkov z investičných zmlúv', N'Net change in investment contract liabilities', 0, NULL, NULL, 33
    UNION ALL SELECT 518204, 35, N'5.7.', N'Čistý výnos z precenenia investičného majetku', N'Net gains from fair value adjustments to investment properties', 0, NULL, NULL, 34
    UNION ALL SELECT 518204, 36, N'5.8.', N'Opravná položka k očakávaným kreditným stratám', N'Expected credit loss allowance (OCI, AC)', 0, NULL, NULL, 35
    UNION ALL SELECT 518204, 37, N'6.', N'Čistý finančný výsledok z poistenia', N'Net insurance finance result (IFIE)', 0, NULL, NULL, 36
    UNION ALL SELECT 518204, 38, N'6.1.', N'Finančné výnosy alebo finančné náklady z poistných zmlúv', N'Insurance finance income or expenses from insurance contracts issued', 0, NULL, NULL, 37
    UNION ALL SELECT 518204, 39, N'6.1.1.', N'Úrokový prírastok a efekt zmeny diskontnej sadzby', N'Accretion of interest & the effect of changes in interest rates', 0, NULL, NULL, 38
    UNION ALL SELECT 518204, 40, N'6.1.2.', N'Finančné riziká a dopad zmien finančných rizík vrátane kurzových rozdielov', N'The Effect of financial risk and changes in financial risk includ. FX differences', 0, NULL, NULL, 39
    UNION ALL SELECT 518204, 41, N'6.1.3.', N'Zmena záväzku pre poistné zmluvy ocenené VFA modelom v dôsledku zmeny v reálnej hodnote podkladových aktív', N'Changes in the fair value of underlying assets of contracts measured under VFA (VFA mirroring)', 0, NULL, NULL, 40
    UNION ALL SELECT 518204, 42, N'6.2.', N'Finančné výnosy alebo finančné náklady zo zaistných zmlúv', N'Insurance finance income or expenses from reinsurance contracts held', 0, NULL, NULL, 41
    UNION ALL SELECT 518204, 43, N'6.2.1.', N'Úrokový prírastok a efekt zmeny diskontnej sadzby', N'Accretion of interest & the effect of changes in interest rates', 0, NULL, NULL, 42
    UNION ALL SELECT 518204, 44, N'6.2.2.', N'Finančné riziká a dopad zmien finančných rizík vrátane kurzových rozdielov', N'The Effect of financial risk and changes in financial risk includ. FX differences', 0, NULL, NULL, 43
    UNION ALL SELECT 518204, 45, N'7.', N'Ostatné (v rámci finančného výsledku)', N'Other (finance result)', 0, NULL, NULL, 44
    UNION ALL SELECT 518204, 46, NULL, N'Finančný výsledok', N'Finance result (5) + (6) + (7)', 0, NULL, NULL, 45
    UNION ALL SELECT 518204, 47, N'8.', N'Ostatné výnosy', N'Other income', 0, NULL, NULL, 46
    UNION ALL SELECT 518204, 48, N'9.', N'Ostatné náklady', N'Other expenses', 0, NULL, NULL, 47
    UNION ALL SELECT 518204, 49, N'10.', N'Výsledok hospodárenia pred zdanením', N'Income before income taxes', 0, NULL, NULL, 48
    UNION ALL SELECT 518204, 50, N'11.', N'Dane', N'Taxes', 0, NULL, NULL, 49
    UNION ALL SELECT 66203, 1, NULL, N'Technické výnosy spolu', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 66203, 2, NULL, N'Čisté zaslúžené poistné', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 66203, 3, NULL, N'Predpísané poistné v hrubej výške', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 66203, 4, NULL, N'Podiel zaisťovateľa na predpísanom poistnom', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 66203, 5, NULL, N'Zmena stavu rezervy na poistné budúcich období v hrubej výške', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 66203, 6, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na poistné budúcich období', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 66203, 7, NULL, N'Ostatné technické výnosy z toho:', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 66203, 8, NULL, N'Provízie od zaisťovateľov', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 66203, 9, NULL, N'Provízie zo spoluzaistenia', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 66203, 10, NULL, N'Poplatky', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 66203, 11, NULL, N'Technické náklady spolu', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 66203, 12, NULL, N'Náklady na poistné plnenia', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 66203, 13, NULL, N'Náklady na poistné plnenia v hrubej výške', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 66203, 14, NULL, N'Podiel zaisťovateľa na nákladoch na poistné plnenia', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 66203, 15, NULL, N'Zmena stavu rezervy na poistné plnenia v hrubej výške', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 66203, 16, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na poistné plnenia', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 66203, 17, NULL, N'Zmena stavu ostatných rezerv', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 66203, 18, NULL, N'Zmena stavu rezervy na životné poistenie v hrubej výške', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 66203, 19, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na životné poistenie v hrubej výške', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 66203, 20, NULL, N'Zmena stavu rezervy na poistné prémie a zľavy v hrubej výške', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 66203, 21, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na poistné prémie a zľavy v hrubej výške', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 66203, 22, NULL, N'Zmena stavu rezervy na úhradu záväzkov voči SKP vznikajúcich z činnosti podľa osobitného predpisu', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 66203, 23, NULL, N'Zmena stavu ďalších rezerv v hrubej výške', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 66203, 24, NULL, N'Podiel zaisťovateľa na zmene stavu ďalších rezerv', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 66203, 25, NULL, N'Zmena stavu rezervy na krytie rizika z investovania finančných prostriedkov v mene poistených', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 66203, 26, NULL, N'Prevádzkové náklady', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 66203, 27, NULL, N'Obstarávacie náklady na poistné zmluvy z toho:', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 66203, 28, NULL, N'Provízie', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 66203, 29, NULL, N'Marketing', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 66203, 30, NULL, N'Správna réžia z toho:', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 66203, 31, NULL, N'Provízie', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 66203, 32, NULL, N'Ostatné technické náklady z toho:', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 66203, 33, NULL, N'Príspevky SKP', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 66203, 34, NULL, N'Príspevky MV SR', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 66203, 35, NULL, N'Technický výsledok', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 66203, 36, NULL, N'Finančné výnosy spolu', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 66203, 37, NULL, N'Výnosy z finančného majetku a investičného majetku, ktoré kryjú technické rezervy', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 66203, 38, NULL, N'Kde riziko z investovaných prostriedkov nesie poisťovňa', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 66203, 39, NULL, N'Kde riziko z investovaných prostriedkov nesie klient', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 66203, 40, NULL, N'Výnosy z finančného majetku a investičného majetku, ktoré nekryjú technické rezervy', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 66203, 41, NULL, N'Ostatné finančné výnosy', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 66203, 42, NULL, N'Finančné náklady spolu', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 66203, 43, NULL, N'Náklady na finančný majetok a investičný majetok, ktorý kryje technické rezervy', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 66203, 44, NULL, N'Kde riziko z investovaných prostriedkov nesie poisťovňa', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 66203, 45, NULL, N'Kde riziko z ivestovaných prostriedkov nesie klient', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 69903, 1, N'*', N'Čistý obrat (časť účt. tr. 6 podľa zákona)', N'Net turnover (part of account class 6 according to the Act)', 1, NULL, NULL, 0
    UNION ALL SELECT 69903, 2, N'**', N'Výnosy z hospodárskej činnosti spolu súčet (r. 03 až r. 09)', N'Operating income - total (lines 03 to 09)', 1, NULL, NULL, 1
    UNION ALL SELECT 69903, 3, N'I.', N'Tržby z predaja tovaru (604, 607)', N'Revenue from the sale of merchandise (604, 607)', 0, NULL, NULL, 2
    UNION ALL SELECT 69903, 4, N'II.', N'Tržby z predaja vlastných výrobkov (601)', N'Revenue from the sale of own products (601)', 0, NULL, NULL, 3
    UNION ALL SELECT 69903, 5, N'III.', N'Tržby z predaja služieb (602, 606)', N'Revenue from the sale of services (602, 606)', 0, NULL, NULL, 4
    UNION ALL SELECT 69903, 6, N'IV.', N'Zmeny stavu vnútroorganizačných zásob (+/-) (účtová skupina 61)', N'Changes in internal inventory (+/-) (account group 61)', 0, NULL, NULL, 5
    UNION ALL SELECT 69903, 7, N'V.', N'Aktivácia (účtová skupina 62)', N'Own work capitalized (account group 62)', 0, NULL, NULL, 6
    UNION ALL SELECT 69903, 8, N'VI.', N'Tržby z predaja dlhodobého nehmotného majetku, dlhodobého hmotného majetku a materiálu (641, 642)', N'Revenue from the sale of non-current intangible assets, property, plant and equipment, and raw materials (641, 642)', 0, NULL, NULL, 7
    UNION ALL SELECT 69903, 9, N'VII.', N'Ostatné výnosy z hospodárskej činnosti (644, 645, 646, 648, 655, 657)', N'Other operating income(644, 645, 646, 648, 655, 657)', 0, NULL, NULL, 8
    UNION ALL SELECT 69903, 10, N'**', N'Náklady na hospodársku činnosť spolu r. 11 + r. 12 + r. 13 + r.14 + r. 15 + r. 20 + r. 21 + r. 24 + r. 25 + r. 26', N'Operating expenses - total line 11 + line 12 + line 13 + line 14 + line 15 + line 20 + line 21 + line 24 + line 25 + line 26', 1, NULL, NULL, 9
    UNION ALL SELECT 69903, 11, N'A.', N'Náklady vynaložené na obstaranie predaného tovaru (504, 507)', N'Cost of merchandise sold (504, 507)', 0, NULL, NULL, 10
    UNION ALL SELECT 69903, 12, N'B.', N'Spotreba materiálu, energie a ostatných neskladovateľných dodávok (501, 502, 503)', N'Consumed raw materials, energy consumption, and consumption of other non-inventory supplies (501, 502, 503)', 0, NULL, NULL, 11
    UNION ALL SELECT 69903, 13, N'C', N'Opravné položky k zásobám (+/-) (505)', N'Value adjustments to inventory (+/-) (505)', 0, NULL, NULL, 12
    UNION ALL SELECT 69903, 14, N'D.', N'Služby (účtová skupina 51)', N'Services (account group 51)', 0, NULL, NULL, 13
    UNION ALL SELECT 69903, 15, N'E.', N'Osobné náklady (r. 16 až r. 19)', N'Personnel expenses - total (lines 16 to 19)', 1, NULL, NULL, 14
    UNION ALL SELECT 69903, 16, N'E.1.', N'Mzdové náklady (521, 522)', N'Wages and salaries (521, 522)', 0, NULL, NULL, 15
    UNION ALL SELECT 69903, 17, N'2.', N'Odmeny členom orgánov spoločnosti a družstva (523)', N'Remuneration of board members of company or cooperative (523)', 0, NULL, NULL, 16
    UNION ALL SELECT 69903, 18, N'3.', N'Náklady na sociálne poistenie (524, 525, 526)', N'Social security expenses (524, 525, 526)', 0, NULL, NULL, 17
    UNION ALL SELECT 69903, 19, N'4.', N'Sociálne náklady (527, 528)', N'Social expenses (527, 528)', 0, NULL, NULL, 18
    UNION ALL SELECT 69903, 20, N'F.', N'Dane a poplatky (účtová skupina 53)', N'Taxes and fees (account group 53)', 0, NULL, NULL, 19
    UNION ALL SELECT 69903, 21, N'G.', N'Odpisy a opravné položky k dlhodobému nehmotnému majetku a dlhodobému hmotnému majetku (r. 22 + r. 23)', N'Amortization and value adjustments to non-current intangible assets and depreciation and value adjustments to property, plant and equipment (line 22 + line 23)', 1, NULL, NULL, 20
    UNION ALL SELECT 69903, 22, N'G.1.', N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku (551)', N'Amortization of non-current intangible assets and depreciation of property, plant and equipment (551)', 0, NULL, NULL, 21
    UNION ALL SELECT 69903, 23, N'2.', N'Opravné položky k dlhodobému nehmotnému majetku a dlhodobému hmotnému majetku (+/-) (553)', N'Value adjustments to non-current intangible assets and property, plant and equipment (+/-) (553)', 0, NULL, NULL, 22
    UNION ALL SELECT 69903, 24, N'H.', N'Zostatková cena predaného dlhodobého majetku a predaného materiálu (541, 542)', N'Carrying value of non-current assets sold and raw materials sold (541, 542)', 0, NULL, NULL, 23
    UNION ALL SELECT 69903, 25, N'I.', N'Opravné položky k pohľadávkam (+/-) (547)', N'Value adjustments to receivables (+/-) (547)', 0, NULL, NULL, 24
    UNION ALL SELECT 69903, 26, N'J.', N'Ostatné náklady na hospodársku činnosť (543, 544, 545, 546, 548, 549, 555, 557)', N'Other operating expenses (543, 544, 545, 546, 548, 549, 555, 557)', 0, NULL, NULL, 25
    UNION ALL SELECT 69903, 27, N'***', N'Výsledok hospodárenia z hospodárskej činnosti (+/-) (r. 02 - r. 10)', N'Profit/loss from operations (+/-) (line 02 - line 10)', 1, NULL, NULL, 26
    UNION ALL SELECT 69903, 28, N'*', N'Pridaná hodnota (r. 03 + r. 04 + r. 05 + r. 06 + r. 07) - (r. 11 + r. 12 + r. 13 + r. 14)', N'Added value (line 03 + line 04 + line 05 + line 06 + line 07 ) - (line 11 + line 12 + line 13 + line 14)', 1, NULL, NULL, 27
    UNION ALL SELECT 69903, 29, N'**', N'Výnosy z finančnej činnosti spolu r. 30 + r. 31 + r. 35 + r. 39 + r. 42 + r. 43 + r. 44', N'Income from financial activities - total line 30 + line 31 + line 35 + line 39 + line 42 + line 43 + line 44', 1, NULL, NULL, 28
    UNION ALL SELECT 69903, 30, N'VIII.', N'Tržby z predaja cenných papierov a podielov (661)', N'Revenue from the sale of securities and shares (661)', 0, NULL, NULL, 29
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 69903 AS [TableErpId], 31 AS [RowNumber], N'IX.' AS [Designation], N'Výnosy z dlhodobého finančného majetku súčet (r. 32 až r. 34)' AS [Text_sk], N'Income from non-current financial assets (lines 32 to 34)' AS [Text_en], 1 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 30 AS [RowOrdinal]
    UNION ALL SELECT 69903, 32, N'IX.1.', N'Výnosy z cenných papierov a podielov od prepojených účtovných jednotiek (665A)', N'Income from securities and ownership interests in affiliated accounting entities (665A)', 0, NULL, NULL, 31
    UNION ALL SELECT 69903, 33, N'2.', N'Výnosy z cenných papierov a podielov v podielovej účasti okrem výnosov prepojených účtovných jednotiek (665A)', N'Income from securities and ownership interests within participating interest, except for income of affiliated accounting entities (665A )', 0, NULL, NULL, 32
    UNION ALL SELECT 69903, 34, N'3.', N'Ostatné výnosy z cenných papierov a podielov (665A)', N'Other income from securities and ownership interests (665A)', 0, NULL, NULL, 33
    UNION ALL SELECT 69903, 35, N'X.', N'Výnosy z krátkodobého finančného majetku súčet (r. 36 až r. 38)', N'Income from current financial assets - total (lines 36 to 38)', 1, NULL, NULL, 34
    UNION ALL SELECT 69903, 36, N'X.1.', N'Výnosy z krátkodobého finančného majetku od prepojených účtovných jednotiek (666A)', N'Income from current financial assets in affiliated accounting entities (666A)', 0, NULL, NULL, 35
    UNION ALL SELECT 69903, 37, N'2.', N'Výnosy z krátkodobého finančného majetku v podielovej účasti okrem výnosov prepojených účtovných jednotiek (666A)', N'Income from current financial assets within participating interest, except for income of affiliated accounting entities (666A)', 0, NULL, NULL, 36
    UNION ALL SELECT 69903, 38, N'3.', N'Ostatné výnosy z krátkodobého finančného majetku (666A)', N'Other income from current financial assets (666A)', 0, NULL, NULL, 37
    UNION ALL SELECT 73803, 3, NULL, N'Predpísané poistné v hrubej výške', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 73803, 4, NULL, N'Podiel zaisťovateľa na predpísanom poistnom', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 73803, 5, NULL, N'Zmena stavu rezervy na poistné budúcich období v hrubej výške', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 73803, 6, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na poistné budúcich období', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 73803, 7, NULL, N'Ostatné technické výnosy z toho:', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 73803, 8, NULL, N'Provízie od zaisťovateľov', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 73803, 9, NULL, N'Provízie zo spoluzaistenia', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 73803, 10, NULL, N'Poplatky', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 73803, 11, NULL, N'Technické náklady spolu', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 73803, 12, NULL, N'Náklady na poistné plnenia', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 73803, 13, NULL, N'Náklady na poistné plnenia v hrubej výške', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 73803, 14, NULL, N'Podiel zaisťovateľa na nákladoch na poistné plnenia', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 73803, 15, NULL, N'Zmena stavu rezervy na poistné plnenia v hrubej výške', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 73803, 16, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na poistné plnenia', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 73803, 17, NULL, N'Zmena stavu ostatných rezerv', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 73803, 18, NULL, N'Zmena stavu rezervy na životné poistenie v hrubej výške', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 73803, 19, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na životné poistenie v hrubej výške', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 73803, 20, NULL, N'Zmena stavu rezervy na poistné prémie a zľavy v hrubej výške', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 73803, 21, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na poistné prémie a zľavy v hrubej výške', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 73803, 22, NULL, N'Zmena stavu rezervy na úhradu záväzkov voči SKP vznikajúcich z činnosti podľa osobitného predpisu', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 73803, 23, NULL, N'Zmena stavu ďalších rezerv v hrubej výške', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 73803, 24, NULL, N'Podiel zaisťovateľa na zmene stavu ďalších rezerv', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 73803, 25, NULL, N'Zmena stavu rezervy na krytie rizika z investovania finančných prostriedkov v mene poistených', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 73803, 26, NULL, N'Prevádzkové náklady', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 73803, 27, NULL, N'Obstarávacie náklady na poistné zmluvy z toho:', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 73803, 28, NULL, N'Provízie', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 73803, 29, NULL, N'Marketing', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 73803, 30, NULL, N'Správna réžia z toho:', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 73803, 31, NULL, N'Provízie', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 73803, 32, NULL, N'Ostatné technické náklady z toho:', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 73803, 33, NULL, N'Príspevky SKP', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 73803, 34, NULL, N'Príspevky MV SR', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 73803, 35, NULL, N'Technický výsledok', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 73803, 36, NULL, N'Finančné výnosy spolu', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 73803, 37, NULL, N'Výnosy z finančného majetku a investičného majetku, ktoré kryjú technické rezervy', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 73803, 38, NULL, N'Kde riziko z investovaných prostriedkov nesie poisťovňa', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 73803, 39, NULL, N'Kde riziko z investovaných prostriedkov nesie klient', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 73803, 40, NULL, N'Výnosy z finančného majetku a investičného majetku, ktoré nekryjú technické rezervy', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 73803, 41, NULL, N'Ostatné finančné výnosy', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 73803, 42, NULL, N'Finančné náklady spolu', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 73803, 43, NULL, N'Náklady na finančný majetok a investičný majetok, ktorý kryje technické rezervy', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 73803, 44, NULL, N'Kde riziko z investovaných prostriedkov nesie poisťovňa', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 73803, 45, NULL, N'Kde riziko z ivestovaných prostriedkov nesie klient', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 73803, 46, NULL, N'Náklady na finančný majetok a investičný majetok, ktorý nekryje technické rezervy', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 73803, 47, NULL, N'Ostatné finančné náklady', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 73803, 48, NULL, N'Finančný výsledok', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 73803, 49, NULL, N'Ostatné výnosy', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 73803, 50, NULL, N'Ostatné náklady', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 73803, 51, NULL, N'Hospodársky výsledok pred zdanením', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 73803, 52, NULL, N'Splatná daň', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 73803, 53, NULL, N'Odložená daň', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 73803, 54, NULL, N'Hospodársky výsledok po zdanení', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 80101, 1, N'B.', N'Nehmotný majetok, z toho', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 80101, 2, N'I.', N'goodwill', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 80101, 3, N'II.', N'poskytnuté preddavky na obstaranie nehmotného majetku', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 80101, 4, N'C.', N'Finančné umiestnenie', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 80101, 5, N'I.', N'Pozemky a stavby, z toho', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 80101, 6, N'1.', N'pre prevádzkovú činnosť z toho', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 80101, 7, N'2.', N'budovy a stavby', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 80101, 8, N'II.', N'Finančné umiestnenie v obchodných spoločnostiach a ostatné dlhodobé pohľadávky z toho', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 80101, 9, N'1.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach s rozhodujúcim vplyvom', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 80101, 10, N'2.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach s podstatným vplyvom', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 80101, 11, N'3.', N'Dlhopisy vydané obchodnými spoločnosťami s rozhodujúcim vplyvom', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 80101, 12, N'4.', N'Dlhopisy vydané obchodnými spoločnosťami s podstatným vplyvom', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 80101, 13, N'5.', N'Ostatné dlhodobé pohľadávky', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 80101, 14, N'III.', N'Ostatné finančné umiestnenie', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 80101, 15, N'1.', N'Cenné papiere s premenlivým výnosom', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 80101, 16, N'2.', N'Cenné papiere s pevným výnosom', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 80101, 17, N'3.', N'Dlhové cenné papiere obstarané v primárnych emisiách neurčené na obchodovanie', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 80101, 18, N'4.', N'Ostatné pôžičky', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 80101, 19, N'5.', N'Vklady v bankách', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 80101, 20, N'6.', N'Iné finančné umiestnenie', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 80101, 21, N'E.', N'Pohľadávky, z toho', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 80101, 22, N'I.A.', N'z verejného zdravotného poistenia', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 80101, 23, N'1.', N'voči poisteným, z toho', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 80101, 24, N'1a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 80101, 25, N'1b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 80101, 26, N'2.', N'voči poskytovateľom zdravotnej starostlivosti, z toho', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 80101, 27, N'2a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 69903, 45, N'**.', N'Náklady na finančnú činnosť spolu r. 46 + r. 47 + r. 48 + r. 49 + r. 52 + r. 53 + r. 54', N'Expenses related to financial activities - total line 46 + line 47 + line 48 + line 49 + line 52 + line 53 + line 54', 1, NULL, NULL, 44
    UNION ALL SELECT 69903, 46, N'K.', N'Predané cenné papiere a podiely (561)', N'Securities and shares sold (561)', 0, NULL, NULL, 45
    UNION ALL SELECT 69903, 47, N'L.', N'Náklady na krátkodobý finančný majetok (566)', N'Expenses related to current financial assets (566)', 0, NULL, NULL, 46
    UNION ALL SELECT 69903, 48, N'M.', N'Opravné položky k finančnému majetku (+/-) (565)', N'Value adjustments to financial assets (+/-) (565)', 0, NULL, NULL, 47
    UNION ALL SELECT 69903, 49, N'N.', N'Nákladové úroky (r. 50 + r. 51)', N'Interest expense (line 50 + line 51)', 1, NULL, NULL, 48
    UNION ALL SELECT 69903, 50, N'N.1.', N'Nákladové úroky pre prepojené účtovné jednotky (562A)', N'Interest expenses related to affiliated accounting entities (562A)', 0, NULL, NULL, 49
    UNION ALL SELECT 69903, 51, N'2.', N'Ostatné nákladové úroky (562A)', N'Other interest expenses (562A)', 0, NULL, NULL, 50
    UNION ALL SELECT 69903, 52, N'O.', N'Kurzové straty (563)', N'Exchange rate losses (563)', 0, NULL, NULL, 51
    UNION ALL SELECT 69903, 53, N'P.', N'Náklady na precenenie cenných papierov a náklady na derivátové operácie (564, 567)', N'Loss on revaluation of securities and expenses related to derivative transactions (564, 567)', 0, NULL, NULL, 52
    UNION ALL SELECT 69903, 54, N'Q.', N'Ostatné náklady na finančnú činnosť (568, 569)', N'Other expenses related to financial activities (568, 569)', 0, NULL, NULL, 53
    UNION ALL SELECT 69903, 55, N'***', N'Výsledok hospodárenia z finančnej činnosti (+/-) (r. 29 - r. 45)', N'Profit/loss from financial activities (+/-) (line 29 - line 45)', 1, NULL, NULL, 54
    UNION ALL SELECT 69903, 56, N'****', N'Výsledok hospodárenia za účtovné obdobie pred zdanením (+/-) (r. 27 + r. 55)', N'Profit/loss for the accounting period before tax (+/-) (line 27 + line 55)', 1, NULL, NULL, 55
    UNION ALL SELECT 69903, 57, N'R.', N'Daň z príjmov (r. 58 + r. 59)', N'Income tax (line 58 + line 59)', 1, NULL, NULL, 56
    UNION ALL SELECT 69903, 58, N'R.1.', N'Daň z príjmov splatná (591, 595)', N'Income tax - current (591, 595)', 0, NULL, NULL, 57
    UNION ALL SELECT 69903, 59, N'2.', N'Daň z príjmov odložená (+/-) (592)', N'Income tax - deferred (+/-) (592)', 0, NULL, NULL, 58
    UNION ALL SELECT 69903, 60, N'S.', N'Prevod podielov na výsledku hospodárenia spoločníkom (+/- 596)', N'Transfer of net profit/net loss shares to partners (+/-596)', 0, NULL, NULL, 59
    UNION ALL SELECT 69903, 61, N'****', N'Výsledok hospodárenia za účtovné obdobie po zdanení (+/-) (r. 56 - r. 57 - r. 60)', N'Profit/loss for the accounting period after tax (+/-) (line 56 - line 57 - line 60)', 1, NULL, NULL, 60
    UNION ALL SELECT 80101, 46, N'1e.', N'poskytnuté preddavky na zásoby', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 80101, 47, N'II.', N'Pokladničné hodnoty a bankové účty z toho', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 80101, 48, N'1.', N'bankové účty', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 80101, 49, N'III.', N'Vlastné akcie', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 80101, 50, N'IV.', N'Iné aktíva', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 80101, 51, N'G.', N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 80101, 52, N'I.', N'Nájomné', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 80101, 53, N'II.', N'Obstarávacie náklady na poistné zmluvy', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 80101, 54, N'III.', N'Ostatné účty časového rozlíšenia', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 80101, 55, NULL, N'AKTÍVA spolu', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 66203, 46, NULL, N'Náklady na finančný majetok a investičný majetok, ktorý nekryje technické rezervy', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 66203, 47, NULL, N'Ostatné finančné náklady', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 69903, 39, N'XI.', N'Výnosové úroky (r. 40 + r. 41)', N'Interest income (line 40 + line 41)', 1, NULL, NULL, 38
    UNION ALL SELECT 69903, 40, N'XI.1.', N'Výnosové úroky od prepojených účtovných jednotiek (662A)', N'Interest income from affiliated accounting entities (662A)', 0, NULL, NULL, 39
    UNION ALL SELECT 69903, 41, N'2.', N'Ostatné výnosové úroky (662A)', N'Other interest income (662A)', 0, NULL, NULL, 40
    UNION ALL SELECT 69903, 42, N'XII.', N'Kurzové zisky (663)', N'Exchange rate gains (663)', 0, NULL, NULL, 41
    UNION ALL SELECT 69903, 43, N'XIII.', N'Výnosy z precenenia cenných papierov a výnosy z derivátových operácií (664, 667)', N'Gains on revaluation of securities and income from derivative transactions (664, 667)', 0, NULL, NULL, 42
    UNION ALL SELECT 69903, 44, N'XIV.', N'Ostatné výnosy z finančnej činnosti (668)', N'Other income from financial activities (668)', 0, NULL, NULL, 43
    UNION ALL SELECT 73803, 1, NULL, N'Technické výnosy spolu', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 73803, 2, NULL, N'Čisté zaslúžené poistné', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 114102, 63, N'2.', N'Účet tvorby fondov (921)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 114102, 64, N'2.', N'Osobitný fond (920)', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 114102, 65, N'2.', N'Základný fond nemocenského poistenia (+/- 922)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 114102, 66, N'2.', N'Základný fond starobného poistenia (+/- 923)', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 114102, 67, N'2.', N'Základný fond invalidného poistenia (+/- 924)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 114102, 68, N'2.', N'Základný fond garančného poistenia (+/- 925)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 114102, 69, N'2.', N'Základný fond poistenia v nezamestnanosti (+/- 926)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 114102, 70, N'2.', N'Základný fond úrazového poistenia (927)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 114102, 71, N'2.', N'Základný fond príspevkov na starobné dôchodkové sporenie (928)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 114102, 72, N'2.', N'Rezervný fond solidarity (929)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 114102, 73, N'3.', N'Výsledok hospodárenia súčet (r. 074 + r. 075)', NULL, 1, NULL, NULL, 17
    UNION ALL SELECT 114102, 74, N'3.', N'Výsledok hospodárenia za účtovné obdobie r. 055 (stĺpec 3 Netto) - (r. 057 + r. 062 + r. 075 + r. 076)', NULL, 1, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 18
    UNION ALL SELECT 114102, 75, N'3.', N'Nerozdelený zisk, neuhradená strata minulých rokov (+/- 932)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 114102, 76, NULL, N'B. Cudzie zdroje súčet (r.077 + r.078 + r.085 + r.099 + r.103)', NULL, 1, NULL, NULL, 20
    UNION ALL SELECT 114102, 77, N'1.', N'Rezervy (941)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 114102, 78, N'2.', N'Dlhodobé záväzky súčet (r.079 až r.084)', NULL, 1, NULL, NULL, 22
    UNION ALL SELECT 114102, 79, N'2.', N'Návratná finančná výpomoc (952AÚ)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 114102, 80, N'2.', N'Záväzky z nájmu (954AÚ)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 114102, 81, N'2.', N'Dlhodobé prijaté preddavky (955)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 114102, 82, N'2.', N'Sociálny fond (956)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 114102, 83, N'2.', N'Dlhodobé zmenky na úhradu (958)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 114102, 84, N'2.', N'Ostatné dlhodobé záväzky (959AÚ + 373AÚ)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 114102, 85, N'3.', N'Krátkodobé záväzky súčet (r.086 až r.098)', NULL, 1, NULL, NULL, 29
    UNION ALL SELECT 114102, 86, N'3.', N'Záväzky z obchodného styku (321 až 325 okrem r. 088)', NULL, 1, NULL, NULL, 30
    UNION ALL SELECT 114102, 87, N'3.', N'Nevyfakturované dodávky (329)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 114102, 88, N'3.', N'Krátkodobé rezervy (323)', NULL, 0, NULL, NULL, 32
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 114102 AS [TableErpId], 89 AS [RowNumber], N'3.' AS [Designation], N'Záväzky z poistných vzťahov (326)' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 33 AS [RowOrdinal]
    UNION ALL SELECT 114102, 90, N'3.', N'Záväzky voči dôchodkovej správcovskej spoločnosti (328)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 114102, 91, N'3.', N'Záväzky voči zamestnancom (331+ 333)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 114102, 92, N'3.', N'Zúčtovanie s inštitúciami sociálneho poistenia a zdravotného poistenia (336)', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 114102, 93, N'3.', N'Daňové záväzky (341+ 342 + 343 + 345)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 114102, 94, N'3.', N'Dotácie a ostatné zúčtovanie so štátnym rozpočtom (346)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 114102, 95, N'3.', N'Záväzky z upísaných nesplatených cenných papierov a vkladov (367)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 80101, 28, N'2b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 80101, 29, N'3.', N'voči inej zdravotnej poisťovni, z toho', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 80101, 30, N'3a.', N'z prerozdelenia poistného', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 80101, 31, N'4.', N'voči Úradu pre dohľad nad zdravotnou starostlivosťou', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 80101, 32, N'5.', N'voči Ministerstvu zdravotníctva Slovenskej republiky', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 80101, 33, N'II.', N'ostatné pohľadávky, z toho', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 80101, 34, N'1.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 80101, 35, N'2.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 80101, 36, N'3.', N'pohľadávky voči zamestnancom', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 80101, 37, N'4.', N'daňové pohľadávky', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 80101, 38, N'5.', N'dotácie zo štátneho rozpočtu a ostatné dotácie', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 80101, 39, N'III.', N'z upísaného základného imania', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 80101, 40, N'F.', N'Ostatné aktíva', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 80101, 41, N'I.', N'Hmotný hnuteľný majetok a zásoby z toho', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 80101, 42, N'1a.', N'stroje a zariadenia', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 80101, 43, N'1b.', N'zásoby', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 80101, 44, N'1c.', N'dopravné prostriedky', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 80101, 45, N'1d.', N'poskytnuté preddavky na hmotný majetok', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 38303, 1, NULL, N'Dlhodobý nehmotný majetok', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 38303, 2, NULL, N'Dlhodobý hmotný majetok', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 38303, 3, NULL, N'Dlhodobý finančný majetok', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 38303, 4, NULL, N'Zásoby', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 38303, 5, NULL, N'Pohľadávky', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 38303, 6, NULL, N'Peniaze', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 38303, 7, NULL, N'Ceniny', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 38303, 8, NULL, N'Priebežné položky (+/-)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 38303, 9, NULL, N'Bankové účty', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 38303, 10, NULL, N'Krátkodobé cenné papiere a ostatný krátkodobý finančný majetok', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 38303, 11, NULL, N'Majetok celkom (súčet r. 01 až r. 10)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 38304, 12, NULL, N'Záväzky', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 38304, 13, NULL, N'z toho: sociálny fond', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 38304, 14, NULL, N'fond prevádzky, údržby a opráv', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 38304, 15, NULL, N'Úvery a pôžičky', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 38304, 16, NULL, N'Záväzky celkom (súčet r. 12 a r.15)', NULL, 1, NULL, NULL, 4
    UNION ALL SELECT 38304, 17, NULL, N'Rozdiel majetku a záväzkov (r. 11 - r.16)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 94304, 1, NULL, N'Výnosy z prevádzkovej činnosti celkom (r. 02 + r. 06)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 94304, 2, NULL, N'Tržby celkom, z toho:', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 94304, 3, NULL, N'tržby z predaja tovaru', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94304, 4, NULL, N'tržby z predaja vlastných výrobkov', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 94304, 5, NULL, N'tržby z poskytnutých služieb a zákazkovej výroby', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 94304, 6, NULL, N'Ostatné prevádzkové výnosy celkom, z toho:', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 94304, 7, NULL, N'výnosy z predaja dlhodobého majetku', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 94304, 8, NULL, N'výnosy z predaja materiálu', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 94304, 9, NULL, N'Náklady na prevádzkovú činnosť celkom (r. 10 až r. 16)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 94304, 10, NULL, N'Náklady vynaložené na obstaranie predaného tovaru', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 94304, 11, NULL, N'Spotreba materiálu a energie', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 94304, 12, NULL, N'Osobné náklady', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 94304, 13, NULL, N'Náklady na službu', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 94304, 14, NULL, N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 94304, 15, NULL, N'Rezervy a straty zo znehodnotenia dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 94304, 16, NULL, N'Ostatné prevádzkové náklady, z toho:', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 94304, 17, NULL, N'náklady na predaj dlhodobého majetku', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 94304, 18, NULL, N'náklady na predaj materiálu', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 94304, 19, NULL, N'tvorba a zúčtovanie opravných položiek k pohľadávkam', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 94304, 20, NULL, N'Výsledok hospodárenia z prevádzkovej činnosti pred zdanením (+/-), (r. 01 - r. 09)', NULL, 1, NULL, NULL, 19
    UNION ALL SELECT 94304, 21, NULL, N'Finančné výnosy, z toho:', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 94304, 22, NULL, N'výnosové úroky', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 94304, 23, NULL, N'kurzové výnosy', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 94304, 24, NULL, N'Finančné náklady, z toho:', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 94304, 25, NULL, N'nákladové úroky', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 94304, 26, NULL, N'kurzové náklady', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 94304, 27, NULL, N'tvorba a zúčtovanie opravných položiek k pôžičkám', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 94304, 28, NULL, N'Výsledok hospodárenia z pokračujúcich činností pred zdanením (+/-), (r. 20 + r. 21 - r. 24)', NULL, 1, NULL, NULL, 27
    UNION ALL SELECT 94304, 29, NULL, N'Daň z príjmov', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 94304, 30, NULL, N'Výsledok hospodárenia z pokračujúcich činností po zdanení (+/-), (r. 28 - r. 29)', NULL, 1, NULL, NULL, 29
    UNION ALL SELECT 94304, 31, NULL, N'Výsledok hospodárenia z ukončených činností pred zdanením (+/-)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 94304, 32, NULL, N'Daň z príjmov', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 94304, 33, NULL, N'Výsledok hospodárenia z ukončených činností po zdanení (+/-), (r. 31 - r. 32)', NULL, 1, NULL, NULL, 32
    UNION ALL SELECT 94304, 34, NULL, N'Výsledok hospodárenia za účtovné obdobie po zdanení (+/-), (r. 30 + r. 33)', NULL, 1, NULL, NULL, 33
    UNION ALL SELECT 94304, 35, NULL, N'Ostatné súčasti komplexného výsledku', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 94304, 36, NULL, N'Daň z príjmov', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 94304, 37, NULL, N'Celkový komplexný výsledok za účtovné obdobie po zdanení (+/-), (r. 34 + r. 35 - r. 36)', NULL, 1, NULL, NULL, 36
    UNION ALL SELECT 94305, 1, NULL, N'Daň z príjmov celkom (r. 02 a r. 04)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 94305, 2, NULL, N'Daň z príjmov splatná celkom, z toho:', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 94305, 3, NULL, N'osobitný odvod z podnikania v regulovaných odvetviach', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94305, 4, NULL, N'Daň z príjmov odložená celkom', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 116401, 1, NULL, N'Z vkladu zriaďovateľa alebo zakladateľa', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 116401, 2, NULL, N'Z majetku', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 116401, 3, NULL, N'Z darov a príspevkov', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 116401, 4, NULL, N'Z členských príspevkov', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 116401, 5, NULL, N'Z podielu zaplatenej dane z príjmov', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 116401, 6, NULL, N'Z verejných zbierok', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 116401, 7, NULL, N'Z úverov a pôžičiek', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 116401, 8, NULL, N'Z dedičstva', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 116401, 9, NULL, N'Z organizovania podujatí', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 116401, 10, NULL, N'Z dotácií', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 116401, 11, NULL, N'Z likvidačného zostatku inej účtovnej jednotky', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 116401, 12, NULL, N'Z predaja majetku', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 116401, 13, NULL, N'Z poskytovania služieb a predaja vlastných výrobkov', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 116401, 14, NULL, N'Fond prevádzky, údržby a opráv', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 116401, 15, NULL, N'Ostatné', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 116401, 16, NULL, N'Príjmy celkom (súčet r. 01 až r. 15)', NULL, 1, NULL, NULL, 15
    UNION ALL SELECT 116402, 17, NULL, N'Zásoby', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 116402, 18, NULL, N'Služby', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 116402, 19, NULL, N'Mzdy, poistné a príspevky', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 116402, 20, NULL, N'Dary a príspevky iným subjektom', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 116402, 21, NULL, N'Prevádzková réžia', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 116402, 22, NULL, N'Splátky úverov a pôžičiek', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 116402, 23, NULL, N'Sociálny fond', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 116402, 24, NULL, N'Ostatné', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 116402, 25, NULL, N'Výdavky celkom (súčet r. 17 až r. 24)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 116402, 26, NULL, N'Rozdiel príjmov a výdavkov (r. 16 - r. 25)', NULL, 1, NULL, NULL, 9
    UNION ALL SELECT 116402, 27, NULL, N'Daň z príjmov', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 66203, 48, NULL, N'Finančný výsledok', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 66203, 49, NULL, N'Ostatné výnosy', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 66203, 50, NULL, N'Ostatné náklady', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 66203, 51, NULL, N'Hospodársky výsledok pred zdanením', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 66203, 52, NULL, N'Splatná daň', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 66203, 53, NULL, N'Odložená daň', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 66203, 54, NULL, N'Hospodársky výsledok po zdanení', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 101, 1, N'50', N'Spotrebované nákupy (r. 002 až r. 005)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 101, 2, N'501', N'Spotreba materiálu', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 101, 3, N'502', N'Spotreba energie', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 101, 4, N'503', N'Spotreba ostatných neskladovateľných dodávok', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 101, 5, N'504', N'Predaný tovar', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 101, 6, N'51', N'Služby (r. 007 až r. 010)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 101, 7, N'511', N'Opravy a udržiavanie', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 101, 8, N'512', N'Cestovné', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 101, 9, N'513', N'Náklady na reprezentáciu', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 101, 10, N'518', N'Ostatné služby', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 101, 11, N'52', N'Osobné náklady (r. 012 až r. 016)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 101, 12, N'521', N'Mzdové náklady', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 101, 13, N'524', N'Zákonné sociálne poistenie', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 101, 14, N'525', N'Ostatné sociálne poistenie', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 101, 15, N'527', N'Zákonné sociálne náklady', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 101, 16, N'528', N'Ostatné sociálne náklady', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 101, 17, N'53', N'Dane a poplatky (r. 018 až r. 020)', NULL, 1, NULL, NULL, 16
    UNION ALL SELECT 101, 18, N'531', N'Daň z motorových vozidiel', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 101, 19, N'532', N'Daň z nehnuteľnosti', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 101, 20, N'538', N'Ostatné dane a poplatky', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 101, 21, N'54', N'Ostatné náklady na prevádzkovú činnosť (r. 022 až r. 028)', NULL, 1, NULL, NULL, 20
    UNION ALL SELECT 101, 22, N'541', N'Zostatková cena predaného dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 101, 23, N'542', N'Predaný materiál', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 101, 24, N'544', N'Zmluvné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 101, 25, N'545', N'Ostatné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 101, 26, N'546', N'Odpis pohľadávky', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 101, 27, N'548', N'Ostatné náklady na prevádzkovú činnosť', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 101, 28, N'549', N'Manká a škody', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 101, 29, N'55', N'Odpisy, rezervy a opravné položky z prevádzkovej činnosti a finančnej činnosti a zúčtovanie časového rozlíšenia (r. 030 + r. 031 + r. 036 + r. 039)', NULL, 1, NULL, NULL, 28
    UNION ALL SELECT 101, 30, N'551', N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 101, 31, NULL, N'Rezervy a opravné položky z prevádzkovej činnosti (r. 032 až r. 035)', NULL, 1, NULL, NULL, 30
    UNION ALL SELECT 101, 32, N'552', N'Tvorba zákonných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 101, 33, N'553', N'Tvorba ostatných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 32
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 101 AS [TableErpId], 34 AS [RowNumber], N'557' AS [Designation], N'Tvorba zákonných opravných položiek z prevádzkovej činnosti' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 33 AS [RowOrdinal]
    UNION ALL SELECT 101, 35, N'558', N'Tvorba ostatných opravných položiek z prevádzkovej činnosti', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 101, 36, NULL, N'Rezervy a opravné položky z finančnej činnosti (r. 037+ r. 038)', NULL, 1, NULL, NULL, 35
    UNION ALL SELECT 101, 37, N'554', N'Tvorba rezerv z finančnej činnosti', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 101, 38, N'559', N'Tvorba opravných položiek z finančnej činnosti', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 101, 39, N'555', N'Zúčtovanie komplexných nákladov budúcich období', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 101, 40, N'56', N'Finančné náklady (r. 041 až r. 048)', NULL, 1, NULL, NULL, 39
    UNION ALL SELECT 101, 41, N'561', N'Predané cenné papiere a podiely', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 101, 42, N'562', N'Úroky', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 101, 43, N'563', N'Kurzové straty', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 101, 44, N'564', N'Náklady na precenenie cenných papierov', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 101, 45, N'565', N'Náklady na krátkodobý finančný majetok', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 101, 46, N'567', N'Náklady na derivátové operácie', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 101, 47, N'568', N'Ostatné finančné náklady', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 101, 48, N'569', N'Manká a škody na finančnom majetku', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 101, 49, N'57', N'Mimoriadne náklady (r. 050 až r. 053)', NULL, 1, NULL, NULL, 48
    UNION ALL SELECT 101, 50, N'572', N'Škody', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 101, 51, N'574', N'Tvorba rezerv', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 101, 52, N'578', N'Ostatné mimoriadne náklady', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 101, 53, N'579', N'Tvorba opravných položiek', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 101, 54, N'58', N'Náklady na transfery a náklady z odvodu príjmov (r. 055 až r. 063)', NULL, 1, NULL, NULL, 53
    UNION ALL SELECT 101, 55, N'581', N'Náklady na transfery zo štátneho rozpočtu do štátnych rozpočtových organizácií a príspevkových organizácií', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 101, 56, N'582', N'Náklady na transfery zo štátneho rozpočtu ostatným subjektom verejnej správy', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 101, 57, N'583', N'Náklady na transfery zo štátneho rozpočtu subjektom mimo verejnej správy', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 101, 58, N'584', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku do rozpočtových organizácií a príspevkových organizácií zriadených obcou alebo vyšším územným celkom', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 101, 59, N'585', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku ostatným subjektom verejnej správy', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 101, 60, N'586', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku subjektom mimo verejnej správy', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 101, 61, N'587', N'Náklady na ostatné transfery', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 101, 62, N'588', N'Náklady z odvodu príjmov', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 101, 63, N'589', N'Náklady z budúceho odvodu príjmov', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 101, 64, NULL, N'Účtové skupiny 50 - 58 celkom súčet (r.001 + r.006 + r.011 + r.017 + r.021 + r.029 + r.040 + r.049 + r.054)', NULL, 1, NULL, NULL, 63
    UNION ALL SELECT 101, 994, NULL, N'Kontrolné číslo súčet (r. 001 až r. 064)', NULL, 1, NULL, NULL, 64
    UNION ALL SELECT 201, 1, NULL, N'SPOLU MAJETOK r. 002 + r. 033 + r. 110 + r. 114', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 201, 2, N'A.', N'Neobežný majetok r. 003 + r. 011 + r. 024', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 201, 3, N'A.I.', N'Dlhodobý nehmotný majetok súčet (r. 004 až 010)', NULL, 1, NULL, NULL, 2
    UNION ALL SELECT 201, 4, N'A.I.1.', N'Aktivované náklady na vývoj (012) - (072+091AÚ)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 201, 5, N'2.', N'Softvér (013) - (073+091AÚ)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 201, 6, N'3.', N'Oceniteľné práva (014) - (074+091AÚ)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 201, 7, N'4.', N'Drobný dlhodobý nehmotný majetok (018) - (078+091AÚ)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 201, 8, N'5.', N'Ostatný dlhodobý nehmotný majetok (019) - (079+091AÚ)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 201, 9, N'6.', N'Obstaranie dlhodobého nehmotného majetku (041) - (093)', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 201, 10, N'7.', N'Poskytnuté preddavky na dlhodobý nehmotný majetok (051) - (095AÚ)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 201, 11, N'A.II.', N'Dlhodobý hmotný majetok súčet (r. 012 až 023)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 201, 12, N'A.II.1.', N'Pozemky (031) - (092AÚ)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 201, 13, N'2.', N'Umelecké diela a zbierky (032) - (092AÚ)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 201, 14, N'3.', N'Predmety z drahých kovov (033) - (092AÚ)/', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 201, 15, N'4.', N'Stavby (021) - (081+092AÚ)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 201, 16, N'5.', N'Samostatné hnuteľné veci a súbory hnuteľných vecí (022) - (082+092AÚ)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 201, 17, N'6.', N'Dopravné prostriedky (023) - (083+092AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 201, 18, N'7.', N'Pestovateľské celky trvalých porastov (025) - (085+092AÚ)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 201, 19, N'8.', N'Základné stádo a ťažné zvieratá (026) - (086+092AÚ)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 201, 20, N'9.', N'Drobný dlhodobý hmotný majetok (028) - (088+092AÚ)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 201, 21, N'10.', N'Ostatný dlhodobý hmotný majetok (029) - (089+092AÚ)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 201, 22, N'11.', N'Obstaranie dlhodobého hmotného majetku (042) - (094)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 201, 23, N'12.', N'Poskytnuté preddavky na dlhodobý hmotný majetok (052) - (095AÚ)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 201, 24, N'A.III.', N'Dlhodobý finančný majetok súčet (r. 025 až 032)', NULL, 1, NULL, NULL, 23
    UNION ALL SELECT 201, 25, N'A.III.1.', N'Podielové cenné papiere a podiely v dcérskej účtovnej jednotke (061) - (096AÚ)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 201, 26, N'2.', N'Podielové cenné papiere a podiely v spoločnosti s podstatným vplyvom (062) - (096AÚ)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 201, 27, N'3.', N'Realizovateľné cenné papiere a podiely (063) - (096AÚ)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 201, 28, N'4.', N'Dlhové cenné papiere držané do splatnosti (065) - (096AÚ)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 201, 29, N'5.', N'Pôžičky účtovnej jednotke v konsolidovanom celku (066) - (096AÚ)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 201, 30, N'6.', N'Ostatné pôžičky (067) - (096AÚ)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 201, 31, N'7.', N'Ostatný dlhodobý finančný majetok (069) - (096AÚ)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 201, 32, N'8.', N'Obstaranie dlhodobého finančného majetku (043) - (096AÚ)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 201, 33, N'B.', N'Obežný majetok r. 034 + r. 040 + r. 048+ r. 060 + r. 085+ r. 098 + r. 104', NULL, 1, NULL, NULL, 32
    UNION ALL SELECT 201, 34, N'B.I.', N'Zásoby súčet (r. 035 až 039)', NULL, 1, NULL, NULL, 33
    UNION ALL SELECT 201, 35, N'B.I.1.', N'Materiál (112 + 119) - (191)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 201, 36, N'2.', N'Nedokončená výroba a polotovary (121 + 122) - (192 + 193)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 201, 37, N'3.', N'Výrobky (123) - (194)', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 201, 38, N'4.', N'Zvieratá (124) - 195', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 201, 39, N'5.', N'Tovar (132 + 139) - (196)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 201, 40, N'B.II.', N'Zúčtovanie medzi subjektami verejnej správy súčet (r. 041 až r. 047)', NULL, 1, NULL, NULL, 39
    UNION ALL SELECT 201, 41, N'B.II.1.', N'Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa (351)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 201, 42, N'2.', N'Zúčtovanie transferov štátneho rozpočtu (353)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 201, 43, N'3.', N'Zúčtovanie transferov rozpočtu obce a vyššieho územného celku (355)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 201, 44, N'4.', N'Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku (356)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 201, 45, N'5.', N'Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku (357)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 201, 46, N'6.', N'Zúčtovanie transferov zo štátneho rozpočtu iným subjektom (358)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 201, 47, N'7.', N'Zúčtovanie transferov medzi subjektami verejnej správy (359)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 201, 48, N'B.III', N'Dlhodobé pohľadávky súčet (r. 049 až 059)', NULL, 1, NULL, NULL, 47
    UNION ALL SELECT 201, 49, N'B.III.1', N'Odberatelia (311AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 201, 50, N'2.', N'Zmenky na inkaso (312AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 201, 51, N'3.', N'Pohľadávky za eskontované cenné papiere (313AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 201, 52, N'4.', N'Ostatné pohľadávky (315AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 201, 53, N'5.', N'Pohľadávky voči zamestnancom (335AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 201, 54, N'6.', N'Pohľadávky voči združeniu (369AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 201, 55, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 201, 56, N'8.', N'Pohľadávky z nájmu (374AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 201, 57, N'9.', N'Pohľadávky z vydaných dlhopisov (375AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 201, 58, N'10', N'Nakúpené opcie (376AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 201, 59, N'11.', N'Iné pohľadávky (378AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 201, 60, N'B.IV.', N'Krátkodobé pohľadávky súčet (r. 061 až 084)', NULL, 1, NULL, NULL, 59
    UNION ALL SELECT 201, 61, N'B.IV.1', N'Odberatelia (311AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 201, 62, N'2.', N'Zmenky na inkaso (312AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 201, 63, N'3.', N'Pohľadávky za eskontované cenné papiere (313AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 201, 64, N'4.', N'Poskytnuté prevádzkové preddavky (314) - (391AÚ)', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 201, 65, N'5.', N'Ostatné pohľadávky (315AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 64
    UNION ALL SELECT 201, 66, N'6.', N'Pohľadávky z nedaňových rozpočtových príjmov (316) - (391AÚ)', NULL, 0, NULL, NULL, 65
    UNION ALL SELECT 201, 67, N'7.', N'Pohľadávky z daňových a colných rozpočtových príjmov (317) - (391AÚ)', NULL, 0, NULL, NULL, 66
    UNION ALL SELECT 201, 68, N'8.', N'Pohľadávky z nedaňových príjmov obcí a vyšších územných celkov a rozpočtových organizácií zriadených obcou a vyšším územným celkom (318) - (391AÚ)', NULL, 0, NULL, NULL, 67
    UNION ALL SELECT 201, 69, N'9.', N'Pohľadávky z daňových príjmov obcí a vyšších územných celkov (319) - (391AÚ)', NULL, 0, NULL, NULL, 68
    UNION ALL SELECT 201, 70, N'10.', N'Pohľadávky voči zamestnancom (335AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 69
    UNION ALL SELECT 201, 71, N'11.', N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia (336) - (391AÚ)', NULL, 0, NULL, NULL, 70
    UNION ALL SELECT 201, 72, N'12.', N'Daň z príjmov (341) - (391AÚ)', NULL, 0, NULL, NULL, 71
    UNION ALL SELECT 201, 73, N'13.', N'Ostatné priame dane (342) - (391AÚ)', NULL, 0, NULL, NULL, 72
    UNION ALL SELECT 201, 74, N'14.', N'Daň z pridanej hodnoty (343) - (391AÚ)', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 73
    UNION ALL SELECT 201, 75, N'15.', N'Ostatné dane a poplatky (345) - (391AÚ)', NULL, 0, NULL, NULL, 74
    UNION ALL SELECT 201, 76, N'16.', N'Pohľadávky voči združeniu (369AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 75
    UNION ALL SELECT 201, 77, N'17.', N'Pohľadávky a záväzky z pevných termínovaných operácií (373AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 76
    UNION ALL SELECT 201, 78, N'18.', N'Pohľadávky z nájmu (374AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 77
    UNION ALL SELECT 201, 79, N'19.', N'Pohľadávky z vydaných dlhopisov (375AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 78
    UNION ALL SELECT 201, 80, N'20.', N'Nakúpené opcie (376AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 79
    UNION ALL SELECT 201, 81, N'21.', N'Iné pohľadávky (378AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 80
    UNION ALL SELECT 201, 82, N'22.', N'Spojovací účet pri združení (396AÚ)', NULL, 0, NULL, NULL, 81
    UNION ALL SELECT 201, 83, N'23.', N'Zúčtovanie s Európskymi spoločenstvami (371AÚ)- (391AÚ)', NULL, 0, NULL, NULL, 82
    UNION ALL SELECT 201, 84, N'24.', N'Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy (372AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 83
    UNION ALL SELECT 201, 85, N'B.V.', N'Finančné účty súčet (r. 086 až 097)', NULL, 1, NULL, NULL, 84
    UNION ALL SELECT 701, 1, NULL, N'Predaj tovaru', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 701, 2, NULL, N'Predaj výrobkov a služieb', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 701, 3, NULL, N'Ostatné príjmy', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 701, 4, NULL, N'Príjmy celkom súčet (r. 01 až 03)', NULL, 1, NULL, NULL, 3
    UNION ALL SELECT 801, 1, NULL, N'Dlhodobý nehmotný majetok', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 801, 2, NULL, N'Dlhodobý hmotný majetok', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 801, 3, NULL, N'Dlhodobý finančný majetok', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 801, 4, NULL, N'Zásoby celkom súčet (r. 05 až 07)', NULL, 1, NULL, NULL, 3
    UNION ALL SELECT 801, 5, NULL, N'Materiál', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 801, 6, NULL, N'Tovar', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 801, 7, NULL, N'Nedokončená výroba, výrobky, zvieratá, ostatné', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 801, 8, NULL, N'Pohľadávky', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 801, 9, NULL, N'Krátkodobý finančný majetok súčet (r. 10 až 12)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 801, 10, NULL, N'Peniaze a ceniny', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 801, 11, NULL, N'Účty v bankách', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 801, 12, NULL, N'Ostatný krátkodobý finančný majetok', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 801, 13, NULL, N'Priebežné položky (+/-)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 801, 14, NULL, N'Opravná položka k odplatne nadobudnutému majetku (aktívna)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 801, 15, NULL, N'Majetok celkom r. 01 + r. 02 + r. 03 + r. 04 + r. 08 + r. 09 +/- r. 13 + r. 14', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 1902, 39, N'601', N'Tržby za vlastné výrobky', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 1902, 40, N'602', N'Tržby z predaja služieb', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 1902, 41, N'604', N'Tržby za predaný tovar', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1902, 42, N'611', N'Zmena stavu zásob nedokončenej výroby', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1902, 43, N'612', N'Zmena stavu zásob polotovarov', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1902, 44, N'613', N'Zmena stavu zásob výrobkov', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1902, 45, N'614', N'Zmena stavu zásob zvierat', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1902, 46, N'621', N'Aktivácia materiálu a tovaru', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1902, 47, N'622', N'Aktivácia vnútroorganizačných služieb', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 1902, 48, N'623', N'Aktivácia dlhodobého nehmotného majetku', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 1902, 49, N'624', N'Aktivácia dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 1902, 50, N'641', N'Zmluvné pokuty a penále', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 1902, 51, N'642', N'Ostatné pokuty a penále', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 1902, 52, N'643', N'Platby za odpísané pohľadávky', NULL, 0, NULL, NULL, 13
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 1902 AS [TableErpId], 53 AS [RowNumber], N'644' AS [Designation], N'Úroky' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 14 AS [RowOrdinal]
    UNION ALL SELECT 1902, 54, N'645', N'Kurzové zisky', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 1902, 55, N'646', N'Prijaté dary', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 1902, 56, N'647', N'Osobitné výnosy', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 1902, 57, N'648', N'Zákonné poplatky', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 1902, 58, N'649', N'Iné ostatné výnosy', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 1902, 59, N'651', N'Tržby z predaja dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 1902, 60, N'652', N'Výnosy z dlhodobého finančného majetku', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 1902, 61, N'653', N'Tržby z predaja cenných papierov a podielov', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 1902, 62, N'654', N'Tržby z predaja materiálu', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 1902, 63, N'655', N'Výnosy z krátkodobého finančného majetku', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 1902, 64, N'656', N'Výnosy z použitia fondu', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 1902, 65, N'657', N'Výnosy z precenenia cenných papierov', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 1902, 66, N'658', N'Výnosy z nájmu majetku', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 1902, 67, N'661', N'Prijaté príspevky od organizačných zložiek', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 1902, 68, N'662', N'Prijaté príspevky od iných organizácií', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 1902, 69, N'663', N'Prijaté príspevky od fyzických osôb', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 1902, 70, N'664', N'Prijaté členské príspevky', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 1902, 71, N'665', N'Príspevky z podielu zaplatenej dane', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 1902, 72, N'667', N'Prijaté príspevky z verejných zbierok', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 1902, 73, N'691', N'Dotácie', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 1902, 74, NULL, N'Účtová trieda 6 spolu r. 39 až r. 73', NULL, 1, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 35
    UNION ALL SELECT 1902, 75, NULL, N'Výsledok hospodárenia pred zdanením r. 74 - r. 38', NULL, 1, NULL, NULL, 36
    UNION ALL SELECT 1902, 76, N'591', N'Daň z príjmov', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 1902, 77, N'595', N'Dodatočné odvody dane z príjmov', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 1902, 78, NULL, N'Výsledok hospodárenia po zdanení (r. 75 - (r. 76 + r. 77) ) (+/-)', NULL, 1, NULL, NULL, 39
    UNION ALL SELECT 1902, 995, NULL, N'Kontrolné číslo r. 39 až r. 78', NULL, 1, NULL, NULL, 40
    UNION ALL SELECT 2002, 66, NULL, N'SPOLU VLASTNÉ IMANIE A ZÁVÄZKY r. 067 + r. 088 + r. 119', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 2002, 67, N'A.', N'Vlastné imanie r. 068 + r. 073 + r. 080 + r. 084 + r. 087', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 2002, 68, N'A.I.', N'Základné imanie súčet (r. 069 až 072)', NULL, 1, NULL, NULL, 2
    UNION ALL SELECT 2002, 69, N'A.I.1.', N'Základné imanie (411 alebo +/- 491)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 2002, 70, N'2.', N'Vlastné akcie a vlastné obchodné podiely (/-/252)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 2002, 71, N'3.', N'Zmena základného imania +/- 419', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 2002, 72, N'4.', N'Pohľadávky za upísané vlastné imanie (/-/353)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 2002, 73, N'A.II.', N'Kapitálové fondy súčet (r. 074 až 079)', NULL, 1, NULL, NULL, 7
    UNION ALL SELECT 2002, 74, N'A.II.1.', N'Emisné ážio (412)', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 8
    UNION ALL SELECT 2002, 75, N'2.', N'Ostatné kapitálové fondy (413)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 2002, 76, N'3.', N'Zákonný rezervný fond (Nedeliteľný fond) z kapitálových vkladov (417, 418)', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 2002, 77, N'4.', N'Oceňovacie rozdiely z precenenia majetku a záväzkov (+/- 414)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 2002, 78, N'5.', N'Oceňovacie rozdiely z kapitálových účastín (+/- 415)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 2002, 79, N'6.', N'Oceňovacie rozdiely z precenenia pri zlúčení, splynutí a rozdelení (+/- 416)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 2002, 80, N'A.III.', N'Fondy zo zisku súčet (r. 081 až r. 083)', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 2002, 81, N'A.III.1.', N'Zákonný rezervný fond (421)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 2002, 82, N'2.', N'Nedeliteľný fond (422)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 2002, 83, N'3.', N'Štatutárne fondy a ostatné fondy (423, 427, 42X)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 2002, 84, N'A.IV.', N'Výsledok hospodárenia minulých rokov r. 085 + r. 086', NULL, 1, NULL, NULL, 18
    UNION ALL SELECT 2002, 85, N'A.IV.1.', N'Nerozdelený zisk minulých rokov (428)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 2002, 86, N'2.', N'Neuhradená strata minulých rokov (/-/429)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 2002, 87, N'A.V.', N'Výsledok hospodárenia za účtovné obdobie po zdanení /+-/ r. 001 - (r. 068 + r. 073 + r. 080 + r. 084 + r. 088 + r. 119)', NULL, 1, NULL, NULL, 21
    UNION ALL SELECT 2002, 88, N'B.', N'Záväzky r. 89 + r. 94 + r. 105 + r. 115 + r. 116', NULL, 1, NULL, NULL, 22
    UNION ALL SELECT 2002, 89, N'B.I.', N'Rezervy súčet (r. 090 až r. 093)', NULL, 1, NULL, NULL, 23
    UNION ALL SELECT 2002, 90, N'B.I.1.', N'Rezervy zákonné dlhodobé (451A)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 2002, 91, N'2.', N'Rezervy zákonné krátkodobé (323A, 451A)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 2002, 92, N'3.', N'Ostatné dlhodobé rezervy (459A, 45XA)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 2002, 93, N'4.', N'Ostatné krátkodobé rezervy (323A, 32X, 459A, 45XA)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 2002, 94, N'B.II.', N'Dlhodobé záväzky súčet (r. 095 až r. 104)', NULL, 1, NULL, NULL, 28
    UNION ALL SELECT 2002, 95, N'B.II.1.', N'Dlhodobé záväzky z obchodného styku (479A)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 2002, 96, N'2.', N'Dlhodobé nevyfakturované dodávky (476A)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 2002, 97, N'3.', N'Dlhodobé záväzky voči dcérskej účtovnej jednotke a materskej účtovnej jednotke (471A)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 2002, 98, N'4.', N'Ostatné dlhodobé záväzky v rámci konsolidovaného celku (471A)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 2002, 99, N'5.', N'Dlhodobé prijaté preddavky (475A)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 2002, 100, N'6.', N'Dlhodobé zmenky na úhradu (478A)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 2002, 101, N'7.', N'Vydané dlhopisy (473A/-/255A)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 2002, 102, N'8.', N'Záväzky zo sociálneho fondu (472)', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 2002, 103, N'9.', N'Ostatné dlhodobé záväzky (474A, 479A, 47XA, 372A, 373A, 377A)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 2002, 104, N'10.', N'Odložený daňový záväzok (481A)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 2002, 105, N'B.III.', N'Krátkodobé záväzky súčet (r. 106 až r. 114)', NULL, 1, NULL, NULL, 39
    UNION ALL SELECT 2002, 106, N'B.III.1.', N'Záväzky z obchodného styku (321, 322, 324, 325, 32X, 475A, 478A, 479A, 47XA)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 2002, 107, N'2.', N'Nevyfakturované dodávky (326, 476A)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 2002, 108, N'3.', N'Záväzky voči dcérskej účtovnej jednotke a materskej účtovnej jednotke (361A, 471A)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 2002, 109, N'4.', N'Ostatné záväzky v rámci konsolidovaného celku (361A, 36XA, 471A, 47XA)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 2002, 110, N'5.', N'Záväzky voči spoločníkom a združeniu (364, 365, 366, 367, 368, 398A, 478A, 479A)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 2002, 111, N'6.', N'Záväzky voči zamestnancom (331, 333, 33X, 479A)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 2002, 112, N'7.', N'Záväzky zo sociálneho poistenia (336, 479A)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 2002, 113, N'8.', N'Daňové záväzky a dotácie (341, 342, 343, 345, 346, 347, 34X)', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 2002, 114, N'9.', N'Ostatné záväzky (372A, 373A, 377A, 379A, 474A, 479A, 47X)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 2002, 115, N'B.IV.', N'Krátkodobé finančné výpomoci (241, 249, 24X, 473A, /-/255A)', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 2002, 116, N'B.V.', N'Bankové úvery r. 117 + r. 118', NULL, 1, NULL, NULL, 50
    UNION ALL SELECT 2002, 117, N'B.V.1.', N'Bankové úvery dlhodobé (461A, 46XA)', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 2002, 118, N'2.', N'Bežné bankové úvery (221A, 231, 232, 23X, 461A, 46XA)', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 2002, 119, N'C.', N'Časové rozlíšenie súčet (r. 120 až r. 123)', NULL, 1, NULL, NULL, 53
    UNION ALL SELECT 2002, 120, N'C.1.', N'Výdavky budúcich období dlhodobé (383A)', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 2002, 121, N'2.', N'Výdavky budúcich období krátkodobé (383A)', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 2002, 122, N'3.', N'Výnosy budúcich období dlhodobé (384A)', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 2002, 123, N'4.', N'Výnosy budúcich období krátkodobé (384A)', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 2901, 26, N'1.', N'Zásoby súčet (r.027 až r. 029)', NULL, 1, NULL, NULL, 26
    UNION ALL SELECT 2901, 27, N'1.', N'Materiál (112 + 119) - (191)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 2901, 28, N'1.', N'Tovar (132 + 139) - (196)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 2901, 29, N'1.', N'Poskytnuté preddavky na zásoby (314AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 2901, 30, N'2.', N'Dlhodobé pohľadávky súčet (r.031 až r.034)', NULL, 1, NULL, NULL, 30
    UNION ALL SELECT 2901, 31, N'2.', N'Pohľadávky z obchodného styku (311AÚ až 315AÚ okrem r.030) - (391AÚ)', NULL, 1, NULL, NULL, 31
    UNION ALL SELECT 2901, 32, N'2.', N'Pohľadávky na poistnom (316AÚ - 391AÚ)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 2901, 33, N'2.', N'Pohľadávky voči zamestnancom (335AÚ - 391AÚ)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 2901, 34, N'2.', N'Ostatné dlhodobé pohľadávky (373 AÚ + 375AÚ + 378AÚ + 396AÚ ) - (391AÚ)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 2901, 35, N'3.', N'Krátkodobé pohľadávky súčet (r.036 až r.044)', NULL, 1, NULL, NULL, 35
    UNION ALL SELECT 2901, 36, N'3.', N'Pohľadávky z obchodného styku (311AÚ až 315AÚ okrem r.030) - (391AÚ)', NULL, 1, NULL, NULL, 36
    UNION ALL SELECT 2901, 37, N'3.', N'Pohľadávky na poistnom (316AÚ - 391AÚ)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 2901, 38, N'3.', N'Pohľadávky voči zamestnancom (335AÚ - 391AÚ)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 2901, 39, N'3.', N'Zúčtovanie s inštitúciami sociálneho poistenia a zdravotného poistenia (336 - 391AÚ)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 2901, 40, N'3.', N'Pohľadávky voči združeniu (358 - 391AÚ)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 2901, 41, N'3.', N'Daňové pohľadávky (341 + 342 + 343 + 345)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 2901, 42, N'3.', N'Dotácie a ostatné zúčtovanie so štátnym rozpočtom (346)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 2901, 43, N'3.', N'Iné pohľadávky (373 AÚ + 378AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 2901, 44, N'3.', N'Spojovací účet pri združení (396AÚ - 391AÚ)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 2901, 45, N'4.', N'Krátkodobý finančný majetok súčet (r.046 až r.051)', NULL, 1, NULL, NULL, 45
    UNION ALL SELECT 2901, 46, N'4.', N'Pokladnica (211)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 2901, 47, N'4.', N'Ceniny (213)', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 2901, 48, N'4.', N'Bankové účty (221 + 261)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 2901, 49, N'4.', N'Dlhové cenné papiere na obchodovanie (251)', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 2901, 50, N'4.', N'Dlhové cenné papiere na predaj (253)', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 2901, 51, N'4.', N'Obstaranie krátkodobého finančného amjetku', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 2901, 52, N'5.', N'Prechodné účty aktív súčet (r.053 + r.054)', NULL, 1, NULL, NULL, 52
    UNION ALL SELECT 2901, 53, N'5.', N'Náklady budúcich období (381)', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 2901, 54, N'5.', N'Príjmy budúcich období (385)', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 2901, 55, NULL, N'MAJETOK spolu súčet (r. 001 + r. 025)', NULL, 1, NULL, NULL, 55
    UNION ALL SELECT 2901, 992, NULL, N'Kontrolné číslo súčet (r.025 až r.055)', NULL, 1, NULL, NULL, 56
    UNION ALL SELECT 3001, 7, N'513', N'Náklady na reprezentáciu', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 3001, 8, N'514', N'Výkony pôšt a telekomunikácií', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 3001, 9, N'515', N'Poistné', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 3001, 10, N'516', N'Nájomné', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 3001, 11, N'518', N'Ostatné služby', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 3001, 12, N'519', N'Poplatky za poukazovanie dávok', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 3001, 13, N'521', N'Mzdové náklady', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 3001, 14, N'523', N'Odmeny členom dozornej rady', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 3001, 15, N'524', N'Zákonné sociálne poistenie', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 3001, 16, N'525', N'Ostatné sociálne poistenie', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 3001, 17, N'527', N'Zákonné sociálne náklady', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 3001, 18, N'528', N'Ostatné sociálne náklady', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 3001, 19, N'531', N'Daň z motorových vozidiel', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 3001, 20, N'532', N'Daň z nehnuteľností', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 3001, 21, N'538', N'Ostatné dane a poplatky', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 201, 86, N'B.V.1.', N'Pokladnica (211)', NULL, 0, NULL, NULL, 85
    UNION ALL SELECT 201, 87, N'2.', N'Ceniny (213)', NULL, 0, NULL, NULL, 86
    UNION ALL SELECT 201, 88, N'3.', N'Bankové účty (221AÚ +/- 261)', NULL, 0, NULL, NULL, 87
    UNION ALL SELECT 201, 89, N'4.', N'Účty v bankách s dobou viazanosti dlhšou ako jeden rok (221AÚ)', NULL, 0, NULL, NULL, 88
    UNION ALL SELECT 201, 90, N'5.', N'Výdavkový rozpočtový účet (222)', NULL, 0, NULL, NULL, 89
    UNION ALL SELECT 201, 91, N'6.', N'Príjmový rozpočtový účet (223)', NULL, 0, NULL, NULL, 90
    UNION ALL SELECT 201, 92, N'7.', N'Majetkové cenné papiere na obchodovanie (251) - (291AÚ)', NULL, 0, NULL, NULL, 91
    UNION ALL SELECT 201, 93, N'8.', N'Dlhové cenné papiere na obchodovanie (253) - (291AÚ)', NULL, 0, NULL, NULL, 92
    UNION ALL SELECT 201, 94, N'9.', N'Dlhové cenné papiere so splatnosťou do jedného roka držané do splatnosti (256) - (291AÚ)', NULL, 0, NULL, NULL, 93
    UNION ALL SELECT 201, 95, N'10.', N'Ostatné realizovateľné cenné papiere (257) - (291AÚ)', NULL, 0, NULL, NULL, 94
    UNION ALL SELECT 201, 96, N'11.', N'Obstaranie krátkodobého finančného majetku (259) - (291AÚ)', NULL, 0, NULL, NULL, 95
    UNION ALL SELECT 201, 97, N'12.', N'Účty štátnej pokladnice (účtová skupina 28)', NULL, 0, NULL, NULL, 96
    UNION ALL SELECT 201, 98, N'B.VI.', N'Poskytnuté návratné finančné výpomoci dlhodobé súčet (r. 099 až r. 103)', NULL, 1, NULL, NULL, 97
    UNION ALL SELECT 201, 99, N'B.VI.1.', N'Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku (271AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 98
    UNION ALL SELECT 201, 100, N'2.', N'Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy (272AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 99
    UNION ALL SELECT 201, 101, N'3.', N'Poskytnuté návratné finančné výpomoci podnikateľským subjektom (274AÚ )- (291AÚ)', NULL, 0, NULL, NULL, 100
    UNION ALL SELECT 201, 102, N'4.', N'Poskytnuté návratné finančné výpomoci ostatným organizáciám (275AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 101
    UNION ALL SELECT 201, 103, N'5.', N'Poskytnuté návratné finančné výpomoci fyzickým osobám (277AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 102
    UNION ALL SELECT 201, 104, N'B.VII.', N'Poskytnuté návratné finančné výpomoci krátkodobé súčet (r. 105 až r. 109)', NULL, 1, NULL, NULL, 103
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 201 AS [TableErpId], 105 AS [RowNumber], N'B.VII.1.' AS [Designation], N'Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku (271AÚ) - (291AÚ)' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 104 AS [RowOrdinal]
    UNION ALL SELECT 201, 106, N'2.', N'Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy (272AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 105
    UNION ALL SELECT 201, 107, N'3.', N'Poskytnuté návratné finančné výpomoci podnikateľským subjektom (274AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 106
    UNION ALL SELECT 201, 108, N'4.', N'Poskytnuté návratné finančné výpomoci ostatným organizáciám (275AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 107
    UNION ALL SELECT 201, 109, N'5.', N'Poskytnuté návratné finančné výpomoci fyzickým osobám (277AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 108
    UNION ALL SELECT 201, 110, N'C.', N'Časové rozlíšenie súčet (r. 111 až r. 113)', NULL, 1, NULL, NULL, 109
    UNION ALL SELECT 201, 111, N'C.1.', N'Náklady budúcich období (381)', NULL, 0, NULL, NULL, 110
    UNION ALL SELECT 201, 112, N'2.', N'Komplexné náklady budúcich období (382)', NULL, 0, NULL, NULL, 111
    UNION ALL SELECT 201, 113, N'3.', N'Príjmy budúcich období (385)', NULL, 0, NULL, NULL, 112
    UNION ALL SELECT 201, 114, N'D.', N'Vzťahy k účtom klientov štátnej pokladnice (účtová skupina 20)', NULL, 0, NULL, NULL, 113
    UNION ALL SELECT 201, 888, NULL, N'KONTROLNÉ ČÍSLO súčet (r. 001 až 114)', NULL, 1, NULL, NULL, 114
    UNION ALL SELECT 2901, 1, NULL, N'A. NEOBEŽNÝ MAJETOK súčet (r.002 + r.008 + r.018)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 2901, 2, N'1.', N'Dlhodobý nehmotný majetok súčet (r.003 až r.007)', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 2901, 3, N'1.', N'Softvér (013 - (073 + 091AÚ)', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 2901, 4, N'1.', N'Oceniteľné práva (014 - (074 + 091AÚ)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 2901, 5, N'1.', N'Iný dlhodobý nehmotný majetok (018 + 019) - (078 + 079 + 091AÚ)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 2901, 6, N'1.', N'Obstaranie dlhodobého nehmotného majetku (041 - 093)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 2901, 7, N'1.', N'Poskytnuté preddavky na dlhodobý nehmotný majetok (051 - 095AÚ)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 2901, 8, N'2.', N'Dlhodobý hmotný majetok súčet (r.009 až r.017)', NULL, 1, NULL, NULL, 7
    UNION ALL SELECT 2901, 9, N'2.', N'Pozemky (031)', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 2901, 10, N'2.', N'Umelecké diela a zbierky (032)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 2901, 11, N'2.', N'Stavby (021 - (081+ 092AÚ)', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 2901, 12, N'2.', N'Stroje, prístroje a zariadenia (022 - (082 + 092AÚ)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 2901, 13, N'2.', N'Dopravné prostriedky (023 - (083 + 092AÚ)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 2901, 14, N'2.', N'Dlhodobý drobný hmotný majetok (028 - (088 + 092AÚ)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 2901, 15, N'2.', N'Ostatný dlhodobý hmotný majetok (029 - (089 + 092 AÚ)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 2901, 16, N'2.', N'Obstaranie dlhodobého hmotného majetku (042 - 094)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 2901, 17, N'2.', N'Poskytnuté preddavky na dlhodobý hmotný majetok (052 - 095AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 2901, 18, N'3.', N'Dlhodobý finančný majetok súčet (r.019 až r.024)', NULL, 1, NULL, NULL, 17
    UNION ALL SELECT 2901, 19, N'3.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach v ovládanej osobe (061)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 2901, 20, N'3.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach s podstatným vplyvom (062)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 2901, 21, N'3.', N'Dlhové cenné papiere držané do splatnosti (063 - 096AÚ)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 2901, 22, N'3.', N'Ostatné pôžičky (067 - 096AÚ)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 2901, 23, N'3.', N'Ostatný dlhodobý finančný majetok (069 - 096AÚ)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 2901, 24, N'3.', N'Obstaranie dlhodobého finančného majetku (043 - 096AÚ)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 2901, 991, NULL, N'Kontrolné číslo súčet (r.001 až r.024)', NULL, 1, NULL, NULL, 24
    UNION ALL SELECT 2901, 25, NULL, N'B. OBEŽNÝ MAJETOK súčet (r.026 + r.030 + r.035 + r.045 + r. 052)', NULL, 1, NULL, NULL, 25
    UNION ALL SELECT 3001, 1, N'501', N'Spotreba materiálu', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 3001, 2, N'502', N'Spotreba energie', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 3001, 3, N'503', N'Spotreba ostatných neskladovateľných dodávok', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 3001, 4, N'504', N'Predaný tovar', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 3001, 5, N'511', N'Opravy a udržiavanie', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 3001, 6, N'512', N'Cestovné', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 54201, 46, N'F.', N'Ostatné aktíva', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 54201, 47, N'I.', N'Hmotný hnuteľný majetok a zásoby z toho', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 54201, 48, N'1a.', N'stroje a zariadenia', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 54201, 49, N'1b.', N'zásoby', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 54201, 50, N'1c.', N'dopravné prostriedky', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 54201, 51, N'1d.', N'poskytnuté preddavky na hmotný majetok', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 54201, 52, N'1e.', N'poskytnuté preddavky na zásoby', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 54201, 53, N'II.', N'Pokladničné hodnoty a bankové účty z toho', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 54201, 54, N'1.', N'bankové účty', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 54201, 55, N'III.', N'Iné aktíva', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 54201, 56, N'G.', N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 54201, 57, N'I.', N'Nájomné', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 54201, 58, N'II.', N'Obstarávacie náklady na poistné zmluvy', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 54201, 59, N'III.', N'Ostatné účty časového rozlíšenia', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 3001, 22, N'541', N'Zmluvné pokuty a úroky z omeškania', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 3001, 23, N'542', N'Ostatné pokuty a úroky z omeškania', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 3001, 24, N'543', N'Odpísanie nevymožiteľnej pohľadávky', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 3001, 25, N'544', N'Úroky', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 3001, 26, N'545', N'Kurzové straty', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 3001, 27, N'546', N'Dary', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 3001, 28, N'548', N'Manká a škody', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 3001, 29, N'549', N'Iné ostatné náklady', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 3001, 30, N'551', N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 3001, 31, N'552', N'Zostatková cena predaného dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 3001, 32, N'553', N'Predané cenné papiere', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 3001, 33, N'554', N'Predaný materiál', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 3001, 34, N'557', N'Náklady z precenenia cenných papierov', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 3001, 35, N'559', N'Tvorba a zúčtovanie opravných položiek', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 3001, 36, NULL, N'Účtová trieda 5 spolu súčet (r. 001 až r. 035)', NULL, 1, NULL, NULL, 35
    UNION ALL SELECT 3001, 994, NULL, N'Kontrolné číslo súčet (r.001 až r. 036)', NULL, 1, NULL, NULL, 36
    UNION ALL SELECT 3001, 37, N'601', N'Tržby za vlastné výkony', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 3001, 38, N'602', N'Tržby z predaja služieb', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 3001, 39, N'604', N'Tržby za predaný tovar', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 3001, 40, N'605', N'Iné ostatné tržby', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 3001, 41, N'621', N'Aktivácia materiálu', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 3001, 42, N'622', N'Aktivácia vnútroorganizačných služieb', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 3001, 43, N'623', N'Aktivácia dlhodobého nehmotného majetku', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 3001, 44, N'624', N'Aktivácia dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 3001, 45, N'641', N'Zmluvné pokuty a úroky z omeškania', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 3001, 46, N'642', N'Ostatné pokuty a úroky z omeškania', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 3001, 47, N'643', N'Platby za odpísané pohľadávky', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 3001, 48, N'644', N'Úroky', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 3001, 49, N'645', N'Kurzové zisky', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 3001, 50, N'649', N'Iné ostatné výnosy', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 3001, 51, N'651', N'Tržby z predaja dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 3001, 52, N'652', N'Výnosy z dlhodobého finančného majetku', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 3001, 53, N'653', N'Tržby z predaja cenných papierov', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 3001, 54, N'654', N'Tržby z predaja materiálu', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 3001, 55, N'655', N'Výnosy z krátkodobého finančného majetku', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 3001, 56, N'657', N'Výnosy z precenenia cenných papierov', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 3001, 57, N'658', N'Výnosy z nájmu majetku', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 3001, 58, N'691', N'Dotácie na prevádzku', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 3001, 59, NULL, N'Účtová trieda 6 spolu súčet (r.037 až r. 058)', NULL, 1, NULL, NULL, 59
    UNION ALL SELECT 3001, 60, NULL, N'Výsledok hospodárenia pred zdanením ( r.059 mínus r. 036)(+/-)', NULL, 1, NULL, NULL, 60
    UNION ALL SELECT 3001, 61, N'591', N'Daň z príjmov', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 3001, 62, N'595', N'Dodatočné odvody dane z príjmov', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 3001, 63, NULL, N'Výsledok hospodárenia po zdanení (r.060 mínus r. 061 a r. 062) (+/-)', NULL, 1, NULL, NULL, 63
    UNION ALL SELECT 3001, 995, NULL, N'Kontrolné číslo súčet ( r. 037 až r. 063)', NULL, 1, NULL, NULL, 64
    UNION ALL SELECT 54201, 1, N'B.', N'Nehmotný majetok, z toho', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 54201, 2, N'I.', N'goodwill', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 54201, 3, N'II.', N'poskytnuté preddavky na obstaranie nehmotného majetku', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 54201, 4, N'C.', N'Finančné umiestnenie', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 54201, 5, N'I.', N'Pozemky a stavby, z toho', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 54201, 6, N'1.', N'pre prevádzkovú činnosť z toho', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 54201, 7, N'2.', N'budovy a stavby', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 54201, 8, N'II.', N'Finančné umiestnenie v obchodných spoločnostiach a ostatné dlhodobé pohľadávky z toho', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 54201, 9, N'1.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach s rozhodujúcim vplyvom', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 54201, 10, N'2.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach s podstatným vplyvom', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 54201, 11, N'3.', N'Dlhopisy vydané obchodnými spoločnosťami s rozhodujúcim vplyvom', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 54201, 12, N'4.', N'Dlhopisy vydané obchodnými spoločnosťami s podstatným vplyvom', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 54201, 13, N'5.', N'Ostatné dlhodobé pohľadávky', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 54201, 14, N'III.', N'Ostatné finančné umiestnenie', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 54201, 15, N'1.', N'Cenné papiere s premenlivým výnosom', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 54201, 16, N'2.', N'Cenné papiere s pevným výnosom', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 54201, 17, N'3.', N'Dlhové cenné papiere obstarané v primárnych emisiách neurčené na obchodovanie', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 54201, 18, N'4.', N'Ostatné pôžičky', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 54201, 19, N'5.', N'Vklady v bankách', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 54201, 20, N'6.', N'Iné finančné umiestnenie', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 54201, 21, N'E.', N'Pohľadávky, z toho', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 54201, 22, N'I.A.', N'z verejného zdravotného poistenia', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 54201, 23, N'1.', N'voči poisteným, z toho', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 54201, 24, N'1a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 54201, 25, N'1b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 54201, 26, N'2.', N'voči poskytovateľom zdravotnej starostlivosti, z toho', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 54201, 27, N'2a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 54201, 28, N'2b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 54201, 29, N'3.', N'voči inej zdravotnej poisťovni, z toho', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 54201, 30, N'3a.', N'z prerozdelenia poistného', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 54201, 31, N'4.', N'voči Úradu pre dohľad nad zdravotnou starostlivosťou', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 54201, 32, N'5.', N'voči Ministerstvu zdravotníctva Slovenskej republiky', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 54201, 33, N'I.B.', N'z individuálneho zdravotného poistenia, z toho', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 54201, 34, N'1.', N'voči poisteným', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 54201, 35, N'2.', N'voči sprostredkovateľom', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 54201, 36, N'3.', N'voči poskytovateľom zdravotnej starostlivosti, z toho', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 54201, 37, N'3a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 54201, 38, N'II.', N'zo zaistenia', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 114101, 1, NULL, N'A. NEOBEŽNÝ MAJETOK súčet (r.002 + r.008 + r.018)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 114101, 2, N'1.', N'Dlhodobý nehmotný majetok súčet (r.003 až r.007)', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 114101, 3, N'1.', N'Softvér (013 - (073 + 091AÚ)', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 114101, 4, N'1.', N'Oceniteľné práva (014 - (074 + 091AÚ)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 114101, 5, N'1.', N'Iný dlhodobý nehmotný majetok (018 + 019) - (078 + 079 + 091AÚ)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 114101, 6, N'1.', N'Obstaranie dlhodobého nehmotného majetku (041 - 093)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 114101, 7, N'1.', N'Poskytnuté preddavky na dlhodobý nehmotný majetok (051 - 095AÚ)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 114101, 8, N'2.', N'Dlhodobý hmotný majetok súčet (r.009 až r.017)', NULL, 1, NULL, NULL, 7
    UNION ALL SELECT 114101, 9, N'2.', N'Pozemky (031)', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 114101, 10, N'2.', N'Umelecké diela a zbierky (032)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 114101, 11, N'2.', N'Stavby (021 - (081+ 092AÚ)', NULL, 0, NULL, NULL, 10
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 114101 AS [TableErpId], 12 AS [RowNumber], N'2.' AS [Designation], N'Stroje, prístroje a zariadenia (022 - (082 + 092AÚ)' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 11 AS [RowOrdinal]
    UNION ALL SELECT 114101, 13, N'2.', N'Dopravné prostriedky (023 - (083 + 092AÚ)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 114101, 14, N'2.', N'Dlhodobý drobný hmotný majetok (028 - (088 + 092AÚ)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 114101, 15, N'2.', N'Ostatný dlhodobý hmotný majetok (029 - (089 + 092 AÚ)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 114101, 16, N'2.', N'Obstaranie dlhodobého hmotného majetku (042 - 094)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 114101, 17, N'2.', N'Poskytnuté preddavky na dlhodobý hmotný majetok (052 - 095AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 114101, 18, N'3.', N'Dlhodobý finančný majetok súčet (r.019 až r.024)', NULL, 1, NULL, NULL, 17
    UNION ALL SELECT 114101, 19, N'3.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach v ovládanej osobe (061)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 114101, 20, N'3.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach s podstatným vplyvom (062)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 114101, 21, N'3.', N'Dlhové cenné papiere držané do splatnosti (063 - 096AÚ)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 114101, 22, N'3.', N'Ostatné pôžičky (067 - 096AÚ)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 114101, 23, N'3.', N'Ostatný dlhodobý finančný majetok (069 - 096AÚ)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 114101, 24, N'3.', N'Obstaranie dlhodobého finančného majetku (043 - 096AÚ)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 114101, 25, NULL, N'B. OBEŽNÝ MAJETOK súčet (r.026 + r.030 + r.035 + r.045 + r. 052)', NULL, 1, NULL, NULL, 24
    UNION ALL SELECT 114101, 26, N'1.', N'Zásoby súčet (r.027 až r. 029)', NULL, 1, NULL, NULL, 25
    UNION ALL SELECT 114101, 27, N'1.', N'Materiál (112 + 119) - (191)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 114101, 28, N'1.', N'Tovar (132 + 139) - (196)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 114101, 29, N'1.', N'Poskytnuté preddavky na zásoby (314AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 114101, 30, N'2.', N'Dlhodobé pohľadávky súčet (r.031 až r.034)', NULL, 1, NULL, NULL, 29
    UNION ALL SELECT 114101, 31, N'2.', N'Pohľadávky z obchodného styku (311AÚ až 315AÚ okrem r.029) - (391AÚ)', NULL, 1, NULL, NULL, 30
    UNION ALL SELECT 114101, 32, N'2.', N'Pohľadávky na poistnom (316AÚ - 391AÚ)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 114101, 33, N'2.', N'Pohľadávky voči zamestnancom (335AÚ - 391AÚ)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 114101, 34, N'2.', N'Ostatné dlhodobé pohľadávky (378AÚ + 396AÚ ) - (391AÚ)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 114101, 35, N'3.', N'Krátkodobé pohľadávky súčet (r.036 až r.044)', NULL, 1, NULL, NULL, 34
    UNION ALL SELECT 114101, 36, N'3.', N'Pohľadávky z obchodného styku (311AÚ až 315AÚ okrem r.029) - (391AÚ)', NULL, 1, NULL, NULL, 35
    UNION ALL SELECT 114101, 37, N'3.', N'Pohľadávky na poistnom (316AÚ - 391AÚ)', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 114101, 38, N'3.', N'Pohľadávky voči zamestnancom (335AÚ - 391AÚ)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 114101, 39, N'3.', N'Zúčtovanie so Sociálnou poisťovňou a zdravotnými poisťovňami (336 - 391AÚ)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 114101, 40, N'3.', N'Pohľadávky voči združeniu (358 - 391AÚ)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 114101, 41, N'3.', N'Daňové pohľadávky (341 + 342 + 343 + 345)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 114101, 42, N'3.', N'Dotácie a ostatné zúčtovanie so štátnym rozpočtom (346)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 114101, 43, N'3.', N'Iné pohľadávky (373 AÚ + 378AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 114101, 44, N'3.', N'Spojovací účet pri združení (396AÚ - 391AÚ)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 114101, 45, N'4.', N'Krátkodobý finančný majetok súčet (r.046 až r.051)', NULL, 1, NULL, NULL, 44
    UNION ALL SELECT 114101, 46, N'4.', N'Pokladnica (211)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 114101, 47, N'4.', N'Ceniny (213)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 114101, 48, N'4.', N'Bankové účty (221 + 261)', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 114101, 49, N'4.', N'Dlhové cenné papiere na obchodovanie (251)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 114101, 50, N'4.', N'Dlhové cenné papiere na predaj (253)', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 114101, 51, N'4.', N'Obstaranie krátkodobého finančného majetku (259)', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 114101, 52, N'5.', N'Prechodné účty aktív súčet (r.053 + r.054)', NULL, 1, NULL, NULL, 51
    UNION ALL SELECT 114101, 53, N'5.', N'Náklady budúcich období (381)', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 114101, 54, N'5.', N'Príjmy budúcich období (385)', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 114101, 55, NULL, N'MAJETOK spolu súčet (r. 001 + r. 025)', NULL, 1, NULL, NULL, 54
    UNION ALL SELECT 116404, 15, NULL, N'Úvery a pôžičky', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 116404, 16, NULL, N'Záväzky celkom (súčet r. 12 a r.15)', NULL, 1, NULL, NULL, 4
    UNION ALL SELECT 116404, 17, NULL, N'Rozdiel majetku a záväzkov (r. 11 - r.16)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 66101, 1, NULL, N'SPOLU MAJETOK (r. 02 + r. 08)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 66101, 2, N'A.', N'Neobežný majetok (r. 03 + r.04 + r. 05 + r. 07)', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 66101, 3, N'A. I.', N'Dlhodobý nehmotný majetok', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 66101, 4, N'A. II.', N'Dlhodobý hmotný majetok', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 66101, 5, N'A. III.', N'Dlhodobý finančný majetok, z toho:', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 66101, 6, N'A. III.1', N'pohľadávky z obchodného styku', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 66101, 7, N'A. IV.', N'Ostatný majetok', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 66101, 8, N'B.', N'Obežný majetok (r. 09 + r. 10 + r. 13)', NULL, 1, NULL, NULL, 7
    UNION ALL SELECT 66101, 9, N'B. I.', N'Zásoby', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 66101, 10, N'B. II.', N'Krátkodobý finančný majetok, z toho:', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 66101, 11, N'B. II. 1', N'pohľadávky z obchodného styku', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 66101, 12, N'B. II. 2', N'peniaze a peňažné ekvivalenty', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 66101, 13, N'B. III.', N'Ostatný majetok, z toho:', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 66101, 14, N'B. III. 1', N'majetok klasifikovaný ako držaný na predaj', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 66101, 15, NULL, N'SPOLU VLASTNÉ IMANIE A ZÁVÄZKY(r. 16 + r. 24)', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 66101, 16, N'C.', N'Vlastné imanie (r. 17 + r. 18 + r. 19 + r. 20 + r. 23)', NULL, 1, NULL, NULL, 15
    UNION ALL SELECT 66101, 17, N'C. I.', N'Základné imanie', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 66101, 18, N'C. II.', N'Kapitálové fondy', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 66101, 19, N'C. III.', N'Rezervné fondy a ostatné fondy tvorené zo zisku', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 66101, 20, N'C. IV.', N'Výsledok hospodárenia minulých rokov (r. 21 + r. 22)', NULL, 1, NULL, NULL, 19
    UNION ALL SELECT 66101, 21, N'C. IV. 1', N'Nerozdelený zisk minulých rokov', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 66101, 22, N'C. IV. 2', N'Neuhradená strata minulých rokov', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 66101, 23, N'C. V.', N'Výsledok hospodárenia za účtovné obdobie po zdanení(+/-)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 66101, 24, N'D.', N'Záväzky (r. 25 + r. 29)', NULL, 1, NULL, NULL, 23
    UNION ALL SELECT 66101, 25, N'D. I.', N'Dlhodobé záväzky, z toho:', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 66101, 26, N'D. I. 1', N'Záväzky z obchodného styku', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 66101, 27, N'D. I. 2', N'úvery a pôžičky', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 66101, 28, N'D. I. 3', N'rezervy', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 66101, 29, N'D. II.', N'Krátkodobé záväzky, z toho:', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 66101, 30, N'D. II. 1', N'záväzky z obchodného styku', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 66101, 31, N'D. II. 2', N'úvery a pôžičky', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 66101, 32, N'D. II. 3', N'rezervy', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 66101, 33, N'D. II. 4', N'záväzky spojené s majetkom klasifikovaným ako držaný na predaj', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 66201, 1, NULL, N'Majetkové podiely', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 66201, 2, NULL, N'Finančné zdroje poskytnuté pobočkám v zahraničí', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 66201, 3, NULL, N'Pozemky a stavby', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 66201, 4, NULL, N'Finančné nástroje v reálnej hodnote proti zisku a strate', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 66201, 5, NULL, N'Finančné nástroje na predaj', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 66201, 6, NULL, N'Finančné nástroje držané do splatnosti', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 66201, 7, NULL, N'Finančné umiestnenie v mene poistených', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 66201, 8, NULL, N'Kladná reálna hodnota derivátových operácií na zabezpečenie', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 66201, 9, NULL, N'Poskytnuté úvery, vklady a iné pohľadávky', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 66201, 10, NULL, N'Vklady pri aktívnom zaistení', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 66201, 11, NULL, N'Pohľadávky z poistenia a zaistenia', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 66201, 12, NULL, N'Podiely zaisťovateľov na technických rezervách', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 66201, 13, NULL, N'Pokladničné hodnoty a peňažné ekvivalenty', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 66201, 14, NULL, N'Hmotný hnuteľný majetok', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 66201, 15, NULL, N'Nehmotný majetok', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 66201, 16, NULL, N'Daňové pohľadávky', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 66201, 17, NULL, N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 66201, 18, NULL, N'Neobežné aktíva určené na predaj', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 66201, 19, NULL, N'Ostatné aktíva', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 66201, 20, NULL, N'AKTÍVA spolu', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 118002, 61, N'A.', N'VLASTNÉ IMANIE r. 062 + r. 067 + r. 071 + r. 072', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 118002, 62, N'A.I.', N'Imanie a fondy r. 063 až r. 066', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 118002, 63, N'A.I.1.', N'Základné imanie (411)', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 118002, 64, N'2.', N'Fondy tvorené podľa osobitných predpisov (412)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 118002, 65, N'3.', N'Fond reprodukcie (413)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 118002, 66, N'4.', N'Oceňovacie rozdiely z precenenia kapitálových účastín (415)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 118002, 67, N'A.II.', N'Fondy tvorené zo zisku r. 068 až r. 070', NULL, 1, NULL, NULL, 6
    UNION ALL SELECT 118002, 68, N'A.II.1.', N'Rezervný fond (421)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 118002, 69, N'2.', N'Fondy tvorené zo zisku (423)', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 118002, 70, N'3.', N'Ostatné fondy (427)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 118002, 71, N'A.III.', N'Nevysporiadaný výsledok hospodárenia minulých rokov (+; - 428)', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 118002, 72, N'A.IV.', N'Výsledok hospodárenia za účtovné obdobie r. 060 - (r. 062 + r. 067 + r. 071 + r. 073 + r. 100)', NULL, 1, NULL, NULL, 11
    UNION ALL SELECT 118002, 73, N'B.', N'ZÁVÄZKY r. 074 + r. 078 + r. 086 + r. 096', NULL, 1, NULL, NULL, 12
    UNION ALL SELECT 118002, 74, N'B.I.1.', N'Rezervy r. 075 až r. 077', NULL, 1, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 13
    UNION ALL SELECT 118002, 75, N'2.', N'Rezervy zákonné (451AÚ)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 118002, 76, N'3.', N'Ostatné rezervy (459AÚ)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 118002, 77, N'4.', N'Krátkodobé rezervy (323 + 451AÚ + 459AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 118002, 78, N'B.II.', N'Dlhodobé záväzky r. 079 až r. 085', NULL, 1, NULL, NULL, 17
    UNION ALL SELECT 118002, 79, N'B.II.1.', N'Záväzky zo sociálneho fondu (472)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 118002, 80, N'2.', N'Vydané dlhopisy (473 - 255AÚ)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 118002, 81, N'3.', N'Záväzky z nájmu (474 AÚ)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 118002, 82, N'4.', N'Dlhodobé prijaté preddavky (475)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 118002, 83, N'5.', N'Dlhodobé nevyfakturované dodávky (476 AÚ)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 118002, 84, N'6.', N'Dlhodobé zmenky na úhradu (478)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 118002, 85, N'7.', N'Ostatné dlhodobé záväzky (373 AÚ + 479 AÚ)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 118002, 86, N'B.III.', N'Krátkodobé záväzky r. 087 až r. 095', NULL, 1, NULL, NULL, 25
    UNION ALL SELECT 118002, 87, N'B.III.1.', N'Záväzky z obchodného styku (321 až 326) okrem 323', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 118002, 88, N'2.', N'Záväzky voči zamestnancom (331+ 333)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 118002, 89, N'3.', N'Zúčtovanie so Sociálnou poisťovňou a zdravotnými poisťovňami (336)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 118002, 90, N'4.', N'Daňové záväzky (341 až 345)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 118002, 91, N'5.', N'Záväzky z dôvodu finančných vzťahov k štátnemu rozpočtu a rozpočtom územnej samosprávy (346+348)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 54201, 39, N'III.', N'ostatné pohľadávky, z toho', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 54201, 40, N'1.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 54201, 41, N'2.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 54201, 42, N'3.', N'pohľadávky voči zamestnancom', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 54201, 43, N'4.', N'daňové pohľadávky', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 54201, 44, N'5.', N'dotácie zo štátneho rozpočtu a ostatné dotácie', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 54201, 45, N'IV.', N'z upísaného základného imania', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 116404, 12, NULL, N'Záväzky', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 116404, 13, NULL, N'z toho: sociálny fond', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 116404, 14, NULL, N'fond prevádzky, údržby a opráv', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 118002, 92, N'6.', N'Záväzky z upísaných nesplatených cenných papierov a vkladov (367)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 118002, 93, N'7.', N'Záväzky voči účastníkom združení (368)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 118002, 94, N'8.', N'Spojovací účet pri združení (396)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 118002, 95, N'9.', N'Ostatné záväzky (379 + 373 AÚ + 474 AÚ + 476AÚ + 479 AÚ)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 118002, 96, N'B.IV.', N'Bankové úvery a iné výpomoci a pôžičky r. 097 až r. 099', NULL, 1, NULL, NULL, 35
    UNION ALL SELECT 118002, 97, N'B.IV.1', N'Dlhodobé bankové úvery (461AÚ)', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 118002, 98, N'2.', N'Bežné bankové úvery (231 + 232 + 461AÚ)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 118002, 99, N'3.', N'Prijaté krátkodobé finančné výpomoci (241 + 249)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 118002, 100, N'C.', N'ČASOVÉ ROZLÍŠENIE SPOLU r. 101 až r. 103', NULL, 1, NULL, NULL, 39
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 118002 AS [TableErpId], 101 AS [RowNumber], N'C.I.1.' AS [Designation], N'Výdavky budúcich období (383)' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 40 AS [RowOrdinal]
    UNION ALL SELECT 118002, 102, N'2.', N'Výnosy budúcich období krátkodobé (384 AÚ)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 118002, 103, N'3.', N'Výnosy budúcich období dlhodobé (384 AÚ)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 118002, 104, NULL, N'SPOLU VLASTNÉ IMANIE, ZÁVÄZKY A ÚČTY ČASOVÉHO ROZLÍŠENIA r.061 + r.073 + r.100', NULL, 1, NULL, NULL, 43
    UNION ALL SELECT 68402, 118, NULL, N'VLASTNÉ IMANIE A ZÁVÄZKY r. 119 + r. 130 + r. 185 + r. 188', N'TOTAL EQUITY AND LIABILITIES line 119 + line 130 + line 185 + line 188', 1, NULL, NULL, 0
    UNION ALL SELECT 68402, 119, N'A.', N'Vlastné imanie r. 120 + r. 123 + r. 126 + r. 129', N'Equity line 120 + line 123 + line 126 + line 129', 1, NULL, NULL, 1
    UNION ALL SELECT 68402, 120, N'A.I.', N'Oceňovacie rozdiely súčet (r. 121 + r. 122)', N'Differences from revaluation - total (lines 121 to 122)', 1, NULL, NULL, 2
    UNION ALL SELECT 68402, 121, N'A.I.1.', N'Oceňovacie rozdiely z precenenia majetku a záväzkov (+/– 414)', N'Differences from revaluation of assets and liabilities (+/- 414)', 0, NULL, NULL, 3
    UNION ALL SELECT 68402, 122, N'2.', N'Oceňovacie rozdiely z kapitálových účastín (+/– 415)', N'Investment revaluation reserves (+/- 415)', 0, NULL, NULL, 4
    UNION ALL SELECT 68402, 123, N'A.II.', N'Fondy súčet (r. 124 + r. 125)', N'Funds - total (lines 124 to 125)', 1, NULL, NULL, 5
    UNION ALL SELECT 68402, 124, N'A.II.1.', N'Zákonný rezervný fond (421)', N'Statutory reserve fund (421)', 0, NULL, NULL, 6
    UNION ALL SELECT 68402, 125, N'2.', N'Ostatné fondy (427)', N'Other funds (427)', 0, NULL, NULL, 7
    UNION ALL SELECT 68402, 126, N'A.III.', N'Výsledok hospodárenia (+/-) súčet (r. 127 až 128)', N'Net profit or loss (+/-) total (lines 127 to 128)', 1, NULL, NULL, 8
    UNION ALL SELECT 68402, 127, N'A.III.1.', N'Nevysporiadaný výsledok hospodárenia minulých rokov (+/– 428)', N'Retained earnings or accumulated losses from previous years (+/- 428)', 0, NULL, NULL, 9
    UNION ALL SELECT 68402, 128, N'2.', N'Výsledok hospodárenia za účtovné obdobie (+/–) r. 001 - (r.120 + r. 123 + r.127 + r.129 + r. 130 + r. 185 + r. 188)', N'Net profit/loss for the accounting period (+/-) line 001 - (line 117 + line 120 + line 124 + line 126 + line 180 + line 183)', 1, NULL, NULL, 10
    UNION ALL SELECT 68402, 129, N'A.IV.', N'Podiely iných učtovných jednotiek', N'Shares of other entities', 0, NULL, NULL, 11
    UNION ALL SELECT 68402, 130, N'B.', N'Záväzky súčet r. 131 + r. 136 + r. 144 + r. 156 + r. 178', N'Liabilities - line 131 + line 136 + line 144 + line 156 + line 178', 1, NULL, NULL, 12
    UNION ALL SELECT 68402, 131, N'B.I.', N'Rezervy súčet (r. 132 až 135)', N'Provisions - total (lines 132 to 135)', 1, NULL, NULL, 13
    UNION ALL SELECT 68402, 132, N'B.I.1.', N'Rezervy zákonné dlhodobé (451AÚ)', N'Legal provisions - long-term (451A)', 0, NULL, NULL, 14
    UNION ALL SELECT 68402, 133, N'2.', N'Ostatné rezervy (459AÚ)', N'Other provisions (459A)', 0, NULL, NULL, 15
    UNION ALL SELECT 68402, 134, N'3.', N'Rezervy zákonné krátkodobé (323AÚ, 451AÚ)', N'Legal provisions - short-term (323A, 451A)', 0, NULL, NULL, 16
    UNION ALL SELECT 68402, 135, N'4.', N'Ostatné krátkodobé rezervy (323AÚ, 459AÚ)', N'Other short-term provisions (323A, 459A)', 0, NULL, NULL, 17
    UNION ALL SELECT 68402, 136, N'B.II.', N'Zúčtovanie medzi subjektami verejnej správy súčet (r. 137 až r. 143)', N'Clearance between the public administration entities - total (lines 137 to 143)', 1, NULL, NULL, 18
    UNION ALL SELECT 68402, 137, N'B.II.1.', N'Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa (351)', N'Clearing of state-funded organisation''s contributions to founder''s budget (351)', 0, NULL, NULL, 19
    UNION ALL SELECT 68402, 138, N'2.', N'Zúčtovanie transferov štátneho rozpočtu (353)', N'Clearing of state budget transfers (353)', 0, NULL, NULL, 20
    UNION ALL SELECT 68402, 139, N'3.', N'Zúčtovanie transferov rozpočtu obce a vyššieho územného celku (355)', N'Clearing of transfers of the budget of municipalities and higher territorial units (355)', 0, NULL, NULL, 21
    UNION ALL SELECT 68402, 140, N'4.', N'Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku (356)', N'Clearing of transfers from state budget within consolidated unit (356)', 0, NULL, NULL, 22
    UNION ALL SELECT 94201, 1, NULL, N'Majetkové podiely', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 94201, 2, NULL, N'Podiely v dcérskych spoločnostiach', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 94201, 3, NULL, N'Podiely v spoločných podnikoch', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94201, 4, NULL, N'Podiely v pridružených podnikoch', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 94201, 5, NULL, N'Finančné zdroje poskytnuté pobočkám v zahraničí', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 94201, 6, NULL, N'Pozemky a stavby', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 94201, 7, NULL, N'z toho: investície v nehnuteľnostiach', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 94201, 8, NULL, N'neprevádzkové', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 94201, 9, NULL, N'Finančné nástroje v reálnej hodnote proti zisku a strate', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 94201, 10, NULL, N'Nederivátové', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 94201, 11, NULL, N'z toho: akcie, podielové listy a iné majetkové účasti', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 94201, 12, NULL, N'Derivátové', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 94201, 13, NULL, N'Finančné nástroje na predaj', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 94201, 14, NULL, N'z toho: akcie, podielové listy a iné majetkové účasti', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 94201, 15, NULL, N'Finančné nástroje držané do splatnosti', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 94201, 16, NULL, N'Finančné umiestnenie v mene poistených', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 94201, 17, NULL, N'Kladná reálna hodnota derivátových operácií na zabezpečenie', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 94201, 18, NULL, N'Poskytnuté úvery, vklady a iné pohľadávky', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 94201, 19, NULL, N'z toho: termínované vklady v bankách', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 94201, 20, NULL, N'pôžičky poskytnuté poisteným', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 94201, 21, NULL, N'Vklady pri aktívnom zaistení', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 94201, 22, NULL, N'Pohľadávky z poistenia a zaistenia', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 94201, 23, NULL, N'Voči poisteným', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 94201, 24, NULL, N'Zo spolupoistenia', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 94201, 25, NULL, N'Voči sprostredkovateľom', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 94201, 26, NULL, N'Voči zaisťovateľom', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 94201, 27, NULL, N'Regresy', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 94201, 28, NULL, N'Ostatné pohľadávky z poistenia a zaistenia', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 94201, 29, NULL, N'Podiely zaisťovateľov na technických rezervách', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 94201, 30, NULL, N'Technická rezerva na poistné budúcich období', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 94201, 31, NULL, N'Technická rezerva na poisté plnenia', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 94201, 32, NULL, N'Technická rezerva na poistné prémie a zľavy', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 94201, 33, NULL, N'Technická rezerva na úhradu záväzkov voči Slovenskej kancelárii poisťovateľov vznikajúcich z činností podľa osobitného predpisu', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 54201, 60, NULL, N'AKTÍVA spolu', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 54201, 998, NULL, N'Kontrolné číslo', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 73301, 1, NULL, N'SPOLU MAJETOK (r. 02 + r. 08)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 73301, 2, N'A.', N'Neobežný majetok (r. 03 + r.04 + r. 05 + r. 07)', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 73301, 3, N'A. I.', N'Dlhodobý nehmotný majetok', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 73301, 4, N'A. II.', N'Dlhodobý hmotný majetok', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 73301, 5, N'A. III.', N'Dlhodobý finančný majetok, z toho:', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 73301, 6, N'A. III.1', N'pohľadávky z obchodného styku', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 73301, 7, N'A. IV.', N'Ostatný majetok', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 73301, 8, N'B.', N'Obežný majetok (r. 09 + r. 10 + r. 13)', NULL, 1, NULL, NULL, 7
    UNION ALL SELECT 73301, 9, N'B. I.', N'Zásoby', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 73301, 10, N'B. II.', N'Krátkodobý finančný majetok, z toho:', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 73301, 11, N'B. II. 1', N'pohľadávky z obchodného styku', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 73301, 12, N'B. II. 2', N'peniaze a peňažné ekvivalenty', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 73301, 13, N'B. III.', N'Ostatný majetok, z toho:', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 73301, 14, N'B. III. 1', N'majetok klasifikovaný ako držaný na predaj', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 73301, 15, NULL, N'SPOLU VLASTNÉ IMANIE A ZÁVÄZKY(r. 16 + r. 24)', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 73301, 16, N'C.', N'Vlastné imanie (r. 17 + r. 18 + r. 19 + r. 20 + r. 23)', NULL, 1, NULL, NULL, 15
    UNION ALL SELECT 73301, 17, N'C. I.', N'Základné imanie', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 73301, 18, N'C. II.', N'Kapitálové fondy', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 73301, 19, N'C. III.', N'Rezervné fondy a ostatné fondy tvorené zo zisku', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 73301, 20, N'C. IV.', N'Výsledok hospodárenia minulých rokov (r. 21 + r. 22)', NULL, 1, NULL, NULL, 19
    UNION ALL SELECT 73301, 21, N'C. IV. 1', N'Nerozdelený zisk minulých rokov', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 73301, 22, N'C. IV. 2', N'Neuhradená strata minulých rokov', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 73301, 23, N'C. V.', N'Výsledok hospodárenia za účtovné obdobie po zdanení(+/-)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 73301, 24, N'D.', N'Záväzky (r. 25 + r. 29)', NULL, 1, NULL, NULL, 23
    UNION ALL SELECT 73301, 25, N'D. I.', N'Dlhodobé záväzky, z toho:', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 73301, 26, N'D. I. 1', N'Záväzky z obchodného styku', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 73301, 27, N'D. I. 2', N'úvery a pôžičky', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 73301, 28, N'D. I. 3', N'rezervy', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 73301, 29, N'D. II.', N'Krátkodobé záväzky, z toho:', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 73301, 30, N'D. II. 1', N'záväzky z obchodného styku', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 73301, 31, N'D. II. 2', N'úvery a pôžičky', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 73301, 32, N'D. II. 3', N'rezervy', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 73301, 33, N'D. II. 4', N'záväzky spojené s majetkom klasifikovaným ako držaný na predaj', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 73801, 1, NULL, N'Majetkové podiely', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 73801, 2, NULL, N'Finančné zdroje poskytnuté pobočkám v zahraničí', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 73801, 3, NULL, N'Pozemky a stavby', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 73801, 4, NULL, N'Finančné nástroje v reálnej hodnote proti zisku a strate', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 73801, 5, NULL, N'Finančné nástroje na predaj', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 73801, 6, NULL, N'Finančné nástroje držané do splatnosti', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 73801, 7, NULL, N'Finančné umiestnenie v mene poistených', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 73801, 8, NULL, N'Kladná reálna hodnota derivátových operácií na zabezpečenie', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 73801, 9, NULL, N'Poskytnuté úvery, vklady a iné pohľadávky', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 73801, 10, NULL, N'Vklady pri aktívnom zaistení', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 73801, 11, NULL, N'Pohľadávky z poistenia a zaistenia', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 73801, 12, NULL, N'Podiely zaisťovateľov na technických rezervách', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 73801, 13, NULL, N'Pokladničné hodnoty a peňažné ekvivalenty', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 73801, 14, NULL, N'Hmotný hnuteľný majetok', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 73801, 15, NULL, N'Nehmotný majetok', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 73801, 16, NULL, N'Daňové pohľadávky', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 73801, 17, NULL, N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 73801, 18, NULL, N'Neobežné aktíva určené na predaj', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 73801, 19, NULL, N'Ostatné aktíva', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 73801, 20, NULL, N'AKTÍVA spolu', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 68402, 178, N'B.V.', N'Bankové úvery a výpomoci súčet (r. 179 až 184)', N'Bank loans and assistances - total (lines 179 to 184)', 1, NULL, NULL, 60
    UNION ALL SELECT 68402, 179, N'B.V.1.', N'Bankové úvery dlhodobé (461AÚ)', N'Long-term bank loans (461A)', 0, NULL, NULL, 61
    UNION ALL SELECT 68402, 180, N'2.', N'Bežné bankové úvery (461AÚ, 221AÚ, 231, 232)', N'Current bank loans (461A, 221A, 231, 232)', 0, NULL, NULL, 62
    UNION ALL SELECT 68402, 181, N'3.', N'Vydané dlhopisy krátkodobé (473AÚ, 241) - (255AÚ)', N'Issued short-term bonds (473A, 241 ) - (255A)', 0, NULL, NULL, 63
    UNION ALL SELECT 68402, 182, N'4.', N'Ostatné krátkodobé finančné výpomoci (249)', N'Other short-term financial assistance (249)', 0, NULL, NULL, 64
    UNION ALL SELECT 68402, 183, N'5.', N'Prijaté návratné finančné výpomoci od subjektov verejnej správy dlhodobé (273AÚ)', N'Repayable financial assistance accepted from entities of public administration - long-term (273A)', 0, NULL, NULL, 65
    UNION ALL SELECT 68402, 184, N'6.', N'Prijaté návratné finančné výpomoci od subjektov verejnej správy krátkodobé (273AÚ)', N'Repayable financial assistance accepted from entities of public administration - short-term (273A)', 0, NULL, NULL, 66
    UNION ALL SELECT 68402, 185, N'C.', N'Časové rozlíšenie súčet (r. 186 + r. 187)', N'Accruals and deferrals - total (lines 186 to 187)', 1, NULL, NULL, 67
    UNION ALL SELECT 68402, 186, N'C.1.', N'Výdavky budúcich období (383)', N'Accrued expenses (383)', 0, NULL, NULL, 68
    UNION ALL SELECT 68402, 187, N'2.', N'Výnosy budúcich období (384)', N'Deferred income (384)', 0, NULL, NULL, 69
    UNION ALL SELECT 68402, 188, N'D.', N'Vzťahy k účtom klientov Štátnej pokladnice (účtová skupina 20)', N'Relationships to the State Treasury client accounts (account group 20)', 0, NULL, NULL, 70
    UNION ALL SELECT 94201, 34, NULL, N'Technická rezerva na životné poistenie', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 94201, 35, NULL, N'Technická rezerva na vyrovnávanie mimoriadnych rizík', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 94201, 36, NULL, N'Ďalšie technické rezervy', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 94201, 37, NULL, N'Pokladničné hodnoty a peňažné ekvivalenty', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 94201, 38, NULL, N'Pokladničné hodnoty', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 94201, 39, NULL, N'Bežné účty v bankách', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 94201, 40, NULL, N'Termínované vklady v bankách', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 94201, 41, NULL, N'Ostatné', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 94201, 42, NULL, N'Hmotný hnuteľný majetok', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 94201, 43, NULL, N'Nehmotný majetok', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 94201, 44, NULL, N'Obstarávacie náklady na poistné zmluvy', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 94201, 45, NULL, N'Poistné zmluvy nadobudnuté v rámci portfóliového prevodu', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 94201, 46, NULL, N'Softvér', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 94201, 47, NULL, N'Goodwill', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 94201, 48, NULL, N'Ostatné', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 94201, 49, NULL, N'Daňové pohľadávky', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 94201, 50, NULL, N'z toho: bežná daňová pohľadávka', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 94201, 51, NULL, N'odložená daňová pohľadávka', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 94201, 52, NULL, N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 94201, 53, NULL, N'Neobežné aktíva určené na predaj', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 94201, 54, NULL, N'Ostatné aktíva', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 94201, 55, NULL, N'z toho: poskytnuté preddavky', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 94201, 56, NULL, N'Aktíva spolu', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 68402, 141, N'5.', N'Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku (357)', N'Other clearing of the budget of municipalities and higher territorial units (357)', 0, NULL, NULL, 23
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 68402 AS [TableErpId], 142 AS [RowNumber], N'6.' AS [Designation], N'Zúčtovanie transferov zo štátneho rozpočtu iným subjektom (358)' AS [Text_sk], N'Clearing of transfers from state budget to other entities (358)' AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 24 AS [RowOrdinal]
    UNION ALL SELECT 68402, 143, N'7.', N'Zúčtovanie transferov medzi subjektami verejnej správy a iné zúčtovania (359)', N'Clearance of transfers between the public administration entities and other clearance transactions (359)', 0, NULL, NULL, 25
    UNION ALL SELECT 68402, 144, N'B.III.', N'Dlhodobé záväzky súčet (r. 145 až 153 + r. 155)', N'Non-current liabilities - total (lines 145 to 153 + line 155)', 1, NULL, NULL, 26
    UNION ALL SELECT 68402, 145, N'B.III.1.', N'Ostatné dlhodobé záväzky (479AÚ)', N'Other non-current liabilities (479A)', 0, NULL, NULL, 27
    UNION ALL SELECT 68402, 146, N'2.', N'Dlhodobé prijaté preddavky (475AÚ)', N'Long-term advance payments received (475A)', 0, NULL, NULL, 28
    UNION ALL SELECT 68402, 147, N'3.', N'Dlhodobé zmenky na úhradu (478AÚ)', N'Long-term bills of exchange to be paid (478A)', 0, NULL, NULL, 29
    UNION ALL SELECT 68402, 148, N'4.', N'Záväzky zo sociálneho fondu (472)', N'Liabilities related to social fund (472)', 0, NULL, NULL, 30
    UNION ALL SELECT 68402, 149, N'5.', N'Záväzky z nájmu (474AÚ)', N'Liabilities under leasing contracts (474A)', 0, NULL, NULL, 31
    UNION ALL SELECT 68402, 150, N'6.', N'Dlhodobé nevyfakturované dodávky (476AÚ)', N'Unbilled long-term supplies (476A)', 0, NULL, NULL, 32
    UNION ALL SELECT 68402, 151, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ)', N'Receivables and liabilities from fixed term transactions (373A)', 0, NULL, NULL, 33
    UNION ALL SELECT 68402, 152, N'8.', N'Predané opcie (377AÚ)', N'Options sold (377A)', 0, NULL, NULL, 34
    UNION ALL SELECT 68402, 153, N'9.', N'Iné záväzky (379AÚ)', N'Other liabilities (379A)', 0, NULL, NULL, 35
    UNION ALL SELECT 68402, 154, NULL, N'z toho: odložený daňový záväzok', N'of that: deffered tax liability', 0, NULL, NULL, 36
    UNION ALL SELECT 68402, 155, N'10.', N'Vydané dlhopisy dlhodobé (473AÚ) - (255AÚ)', N'Bonds issued (473A ) - (255A)', 0, NULL, NULL, 37
    UNION ALL SELECT 68402, 156, N'B.IV.', N'Krátkodobé záväzky súčet (r. 157 až 177)', N'Current liabilities - total (lines 157 to 177)', 1, NULL, NULL, 38
    UNION ALL SELECT 68402, 157, N'B.IV.1.', N'Dodávatelia (321)', N'Suppliers (321)', 0, NULL, NULL, 39
    UNION ALL SELECT 68402, 158, N'2.', N'Zmenky na úhradu (322, 478AÚ)', N'Bills of exchange to be paid (322, 478A)', 0, NULL, NULL, 40
    UNION ALL SELECT 68402, 159, N'3.', N'Prijaté preddavky (324, 475AÚ)', N'Advance payments received (324, 475A)', 0, NULL, NULL, 41
    UNION ALL SELECT 68402, 160, N'4.', N'Ostatné záväzky (325, 479AÚ)', N'Other liabilities (325, 479A)', 0, NULL, NULL, 42
    UNION ALL SELECT 68402, 161, N'5.', N'Nevyfakturované dodávky (326, 476AÚ)', N'Unbilled supplies (326, 476A)', 0, NULL, NULL, 43
    UNION ALL SELECT 68402, 162, N'6.', N'Záväzky z nájmu (474AÚ)', N'Liabilities under leasing contracts (474A)', 0, NULL, NULL, 44
    UNION ALL SELECT 68402, 163, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ)', N'Receivables and liabilities from fixed term transactions (373A)', 0, NULL, NULL, 45
    UNION ALL SELECT 68402, 164, N'8.', N'Predané opcie (377AÚ)', N'Options sold (377A)', 0, NULL, NULL, 46
    UNION ALL SELECT 68402, 165, N'9.', N'Iné záväzky (379AÚ)', N'Other liabilities (379A)', 0, NULL, NULL, 47
    UNION ALL SELECT 68402, 166, N'10.', N'Záväzky z upísaných nesplatených cenných papierov a vkladov (367)', N'Liabilities out of subscribed unpaid securities and contributions (367)', 0, NULL, NULL, 48
    UNION ALL SELECT 68402, 167, N'11.', N'Záväzky voči združeniu (368)', N'Liabilities to participants in association (368)', 0, NULL, NULL, 49
    UNION ALL SELECT 68402, 168, N'12.', N'Zamestnanci (331)', N'Employees (331)', 0, N'Krátkodobé záväzky - Dan z pridanej hodnoty', NULL, 50
    UNION ALL SELECT 68402, 169, N'13.', N'Ostatné záväzky voči zamestnancom (333)', N'Other liabilities to employees (333)', 0, NULL, NULL, 51
    UNION ALL SELECT 68402, 170, N'14.', N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia (336)', N'Clearing with social and health insurance institutions (336)', 0, NULL, NULL, 52
    UNION ALL SELECT 68402, 171, N'15.', N'Daň z príjmov (341)', N'Income tax (341)', 0, NULL, NULL, 53
    UNION ALL SELECT 68402, 172, N'16.', N'Ostatné priame dane (342)', N'Other direct taxes (342)', 0, NULL, NULL, 54
    UNION ALL SELECT 68402, 173, N'17.', N'Daň z pridanej hodnoty (343)', N'Value added tax (343)', 0, NULL, NULL, 55
    UNION ALL SELECT 68402, 174, N'18.', N'Ostatné dane a poplatky (345)', N'Other taxes and fees (345)', 0, NULL, NULL, 56
    UNION ALL SELECT 68402, 175, N'19.', N'Spojovací účet pri združení (396AÚ)', N'Control account at association (396A)', 0, NULL, NULL, 57
    UNION ALL SELECT 68402, 176, N'20.', N'Zúčtovanie s Európskou úniou (371AÚ)', N'Clearing with the European Union (371A)', 0, NULL, NULL, 58
    UNION ALL SELECT 68402, 177, N'21.', N'Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy (372AÚ)', N'Transfers and other clearance with entities outside public administration (372A)', 0, NULL, NULL, 59
    UNION ALL SELECT 52102, 65, N'60', N'Tržby za vlastné výkony a tovar (r. 066 až r. 068)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 52102, 66, N'601', N'Tržby za vlastné výrobky', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 52102, 67, N'602', N'Tržby z predaja služieb', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 52102, 68, N'604, 607', N'Tržby za tovar, Výnosy z nehnuteľnosti na predaj', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 52102, 69, N'61', N'Zmena stavu vnútroorganizačných zásob (r. 070 až r. 073)', NULL, 1, NULL, NULL, 4
    UNION ALL SELECT 52102, 70, N'611', N'Zmena stavu nedokončenej výroby', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 52102, 71, N'612', N'Zmena stavu polotovarov', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 52102, 72, N'613', N'Zmena stavu výrobkov', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 52102, 73, N'614', N'Zmena stavu zvierat', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 52102, 74, N'62', N'Aktivácia (r. 075 až r. 078)', NULL, 1, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 9
    UNION ALL SELECT 52102, 75, N'621', N'Aktivácia materiálu a tovaru', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 52102, 76, N'622', N'Aktivácia vnútroorganizačných služieb', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 52102, 77, N'623', N'Aktivácia dlhodobého nehmotného majetku', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 52102, 78, N'624', N'Aktivácia dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 52102, 79, N'63', N'Daňové a colné výnosy a výnosy z poplatkov (r. 080 až r. 082)', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 52102, 80, N'631', N'Daňové a colné výnosy štátu', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 52102, 81, N'632', N'Daňové výnosy samosprávy', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 52102, 82, N'633', N'Výnosy z poplatkov', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 52102, 83, N'64', N'Ostatné výnosy z prevádzkovej činnosti (r. 084 až r. 089)', NULL, 1, NULL, NULL, 18
    UNION ALL SELECT 52102, 84, N'641', N'Tržby z predaja dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 52102, 85, N'642', N'Tržby z predaja materiálu', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 52102, 86, N'644', N'Zmluvné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 52102, 87, N'645', N'Ostatné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 52102, 88, N'646', N'Výnosy z odpísaných pohľadávok', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 52102, 89, N'648', N'Ostatné výnosy z prevádzkovej činnosti', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 52102, 90, N'65', N'Zúčtovanie rezerv a opravných položiek z prevádzkovej činnosti a finančnej činnosti a zúčtovanie časového rozlíšenia (r. 091 + r. 096 +r. 099)', NULL, 1, NULL, NULL, 25
    UNION ALL SELECT 52102, 91, NULL, N'Zúčtovanie rezerv a opravných položiek z prevádzkovej činnosti (r. 092 až r. 095)', NULL, 1, NULL, NULL, 26
    UNION ALL SELECT 52102, 92, N'652', N'Zúčtovanie zákonných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 52102, 93, N'653', N'Zúčtovanie ostatných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 52102, 94, N'657', N'Zúčtovanie zákonných opravných položiek z prevádzkovej činnosti', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 52102, 95, N'658', N'Zúčtovanie ostatných opravných položiek z prevádzkovej činnosti', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 52102, 96, NULL, N'Zúčtovanie rezerv a opravných položiek z finančnej činnosti (r. 097 + r. 098)', NULL, 1, NULL, NULL, 31
    UNION ALL SELECT 1002, 66, N'60', N'Tržby za vlastné výkony a tovar (r. 067 až r. 069)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 1002, 67, N'601', N'Tržby za vlastné výrobky', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 1002, 68, N'602', N'Tržby z predaja služieb', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1002, 69, N'604', N'Tržby za tovar', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1002, 70, N'61', N'Zmena stavu vnútroorganizačných zásob (r. 071 až r. 074)', NULL, 1, NULL, NULL, 4
    UNION ALL SELECT 1002, 71, N'611', N'Zmena stavu nedokončenej výroby', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1002, 72, N'612', N'Zmena stavu polotovarov', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1002, 73, N'613', N'Zmena stavu výrobkov', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1002, 74, N'614', N'Zmena stavu zvierat', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 8
    UNION ALL SELECT 1002, 75, N'62', N'Aktivácia (r. 076 až r. 079)', NULL, 1, NULL, NULL, 9
    UNION ALL SELECT 1002, 76, N'621', N'Aktivácia materiálu a tovaru', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 1002, 77, N'622', N'Aktivácia vnútroorganizačných služieb', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 1002, 78, N'623', N'Aktivácia dlhodobého nehmotného majetku', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 1002, 79, N'624', N'Aktivácia dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 1002, 80, N'63', N'Daňové a colné výnosy a výnosy z poplatkov (r. 081 až r. 083)', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 1002, 81, N'631', N'Daňové a colné výnosy štátu', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 1002, 82, N'632', N'Daňové výnosy samosprávy', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 1002, 83, N'633', N'Výnosy z poplatkov', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 1002, 84, N'64', N'Ostatné výnosy z prevádzkovej činnosti (r. 085 až r. 090)', NULL, 1, NULL, NULL, 18
    UNION ALL SELECT 1002, 85, N'641', N'Tržby z predaja dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 1002, 86, N'642', N'Tržby z predaja materiálu', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 1002, 87, N'644', N'Zmluvné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 1002, 88, N'645', N'Ostatné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 1002, 89, N'646', N'Výnosy z odpísaných pohľadávok', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 1002, 90, N'648', N'Ostatné výnosy z prevádzkovej činnosti', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 1002, 91, N'65', N'Zúčtovanie rezerv a opravných položiek z prevádzkovej činnosti a finančnej činnosti a zúčtovanie časového rozlíšenia (r. 092 + r. 097 + r. 100)', NULL, 1, NULL, NULL, 25
    UNION ALL SELECT 1002, 92, NULL, N'Zúčtovanie rezerv a opravných položiek z prevádzkovej činnosti (r. 093 až r. 096)', NULL, 1, NULL, NULL, 26
    UNION ALL SELECT 1002, 93, N'652', N'Zúčtovanie zákonných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 1002, 94, N'653', N'Zúčtovanie ostatných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 1002, 95, N'657', N'Zúčtovanie zákonných opravných položiek z prevádzkovej činnosti', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 1002, 96, N'658', N'Zúčtovanie ostatných opravných položiek z prevádzkovej činnosti', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 1002, 97, NULL, N'Zúčtovanie rezerv a opravných položiek z finančnej činnosti (r. 098 + r. 099)', NULL, 1, NULL, NULL, 31
    UNION ALL SELECT 1002, 98, N'654', N'Zúčtovanie rezerv z finančnej činnosti', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 1002, 99, N'659', N'Zúčtovanie opravných položiek z finančnej činnosti', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 1002, 100, N'655', N'Zúčtovanie komplexných nákladov budúcich období', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 1002, 101, N'66', N'Finančné výnosy (r. 102 až r. 110)', NULL, 1, NULL, NULL, 35
    UNION ALL SELECT 1002, 102, N'661', N'Tržby z predaja cenných papierov a podielov', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 1002, 103, N'662', N'Úroky', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 1002, 104, N'663', N'Kurzové zisky', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 1002, 105, N'664', N'Výnosy z precenenia cenných papierov', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 1002, 106, N'665', N'Výnosy z dlhodobého finančného majetku', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 1002, 107, N'666', N'Výnosy z krátkodobého finančného majetku', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 1002, 108, N'667', N'Výnosy z derivátových operácií', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 1002, 109, N'668', N'Ostatné finančné výnosy', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 1002, 110, NULL, N'Podiel konsolidujúcej účtovnej jednotky na výsledku hospodárenia pridružených účtovných jednotiek verejnej správy', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 52102, 97, N'654', N'Zúčtovanie rezerv z finančnej činnosti', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 52102, 98, N'659', N'Zúčtovanie opravných položiek z finančnej činnosti', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 52102, 99, N'655', N'Zúčtovanie komplexných nákladov budúcich období', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 52102, 100, N'66', N'Finančné výnosy (r. 101 až r. 108)', NULL, 1, NULL, NULL, 35
    UNION ALL SELECT 52102, 101, N'661', N'Tržby z predaja cenných papierov a podielov', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 52102, 102, N'662', N'Úroky', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 52102, 103, N'663', N'Kurzové zisky', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 52102, 104, N'664', N'Výnosy z precenenia cenných papierov', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 52102, 105, N'665', N'Výnosy z dlhodobého finančného majetku', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 52102, 106, N'666', N'Výnosy z krátkodobého finančného majetku', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 52102, 107, N'667', N'Výnosy z derivátových operácií', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 52102, 108, N'668', N'Ostatné finančné výnosy', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 52102, 109, N'67', N'Mimoriadne výnosy (r. 110 až r. 113)', NULL, 1, NULL, NULL, 44
    UNION ALL SELECT 52102, 110, N'672', N'Náhrady škôd', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 52102, 111, N'674', N'Zúčtovanie rezerv', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 52102, 112, N'678', N'Ostatné mimoriadne výnosy', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 52102, 113, N'679', N'Zúčtovanie opravných položiek', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 52102, 114, N'68', N'Výnosy z transferov a rozpočtových príjmov v štátnych rozpočtových organizáciách a príspevkových organizáciách (r. 115 až r. 123)', NULL, 1, NULL, NULL, 49
    UNION ALL SELECT 52102, 115, N'681', N'Výnosy z bežných transferov zo štátneho rozpočtu', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 52102, 116, N'682', N'Výnosy z kapitálových transferov zo štátneho rozpočtu', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 52102, 117, N'683', N'Výnosy z bežných transferov od ostatných subjektov verejnej správy', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 52102, 118, N'684', N'Výnosy z kapitálových transferov od ostatných subjektov verejnej správy', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 52102, 119, N'685', N'Výnosy z bežných transferov od Európskej únie', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 52102, 120, N'686', N'Výnosy z kapitálových transferov od Európskej únie', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 52102, 121, N'687', N'Výnosy z bežných transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 52102, 122, N'688', N'Výnosy z kapitálových transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 52102, 123, N'689', N'Výnosy z odvodu rozpočtových príjmov', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 52102, 124, N'69', N'Výnosy z transferov a rozpočtových príjmov v obciach, vyšších územných celkoch a v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom (r. 125 až r. 133)', NULL, 1, NULL, NULL, 59
    UNION ALL SELECT 52102, 125, N'691', N'Výnosy z bežných transferov z rozpočtu obce alebo z rozpočtu vyššieho územného celku v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 52102, 126, N'692', N'Výnosy z kapitálových transferov z rozpočtu obce alebo z rozpočtu vyššieho územného celku v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 52102, 127, N'693', N'Výnosy samosprávy z bežných transferov zo štátneho rozpočtu a od iných subjektov verejnej správy', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 52102, 128, N'694', N'Výnosy samosprávy z kapitálových transferov zo štátneho rozpočtu a od iných subjektov verejnej správy', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 52102, 129, N'695', N'Výnosy samosprávy z bežných transferov od Európskej únie', NULL, 0, NULL, NULL, 64
    UNION ALL SELECT 52102, 130, N'696', N'Výnosy samosprávy z kapitálových transferov od Európskych spoločenstiev', NULL, 0, NULL, NULL, 65
    UNION ALL SELECT 52102, 131, N'697', N'Výnosy samosprávy z bežných transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 66
    UNION ALL SELECT 52102, 132, N'698', N'Výnosy samosprávy z kapitálových transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 67
    UNION ALL SELECT 52102, 133, N'699', N'Výnosy samosprávy z odvodu rozpočtových príjmov', NULL, 0, NULL, NULL, 68
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 52102 AS [TableErpId], 134 AS [RowNumber], NULL AS [Designation], N'Účtová trieda 6 celkom súčet (r. 065 + r. 069 + r. 074 + r. 079 + r. 083 + r. 090 + r. 100 + r. 109 + r. 114 + r. 124)' AS [Text_sk], NULL AS [Text_en], 1 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 69 AS [RowOrdinal]
    UNION ALL SELECT 52102, 135, NULL, N'Výsledok hospodárenia pred zdanením (r. 134 mínus r. 064) (+/-)', NULL, 1, NULL, NULL, 70
    UNION ALL SELECT 52102, 136, N'591', N'Splatná daň z príjmov', NULL, 0, NULL, NULL, 71
    UNION ALL SELECT 52102, 137, N'595', N'Dodatočne platená daň z príjmov', NULL, 0, NULL, NULL, 72
    UNION ALL SELECT 52102, 138, NULL, N'Výsledok hospodárenia po zdanení r. 135 mínus (r. 136, r. 137) (+/-)', NULL, 1, NULL, NULL, 73
    UNION ALL SELECT 52102, 995, NULL, N'Kontrolné číslo súčet (r. 065 až r. 138)', NULL, 1, NULL, NULL, 74
    UNION ALL SELECT 1002, 129, N'693', N'Výnosy samosprávy z bežných transferov zo štátneho rozpočtu a od iných subjektov verejnej správy', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 1002, 130, N'694', N'Výnosy samosprávy z kapitálových transferov zo štátneho rozpočtu a od iných subjektov verejnej správy', NULL, 0, NULL, NULL, 64
    UNION ALL SELECT 1002, 131, N'695', N'Výnosy samosprávy z bežných transferov od Európskych spoločenstiev', NULL, 0, NULL, NULL, 65
    UNION ALL SELECT 1002, 132, N'696', N'Výnosy samosprávy z kapitálových transferov od Európskych spoločenstiev', NULL, 0, NULL, NULL, 66
    UNION ALL SELECT 1002, 133, N'697', N'Výnosy samosprávy z bežných transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 67
    UNION ALL SELECT 1002, 134, N'698', N'Výnosy samosprávy z kapitálových transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 68
    UNION ALL SELECT 1002, 135, N'699', N'Výnosy samosprávy z odvodu rozpočtových príjmov', NULL, 0, NULL, NULL, 69
    UNION ALL SELECT 1002, 136, NULL, N'Účtová trieda 6 celkom súčet (r. 066 + r. 070 + r. 075 + r. 080 + r. 084 + r. 091 + r. 101 + r. 111 + r. 116 + r. 126)', NULL, 1, NULL, NULL, 70
    UNION ALL SELECT 1002, 137, NULL, N'Výsledok hospodárenia pred zdanením (r. 136 mínus r. 065) (+/-)', NULL, 1, NULL, NULL, 71
    UNION ALL SELECT 1002, 138, N'591', N'Splatná daň z príjmov', NULL, 0, NULL, NULL, 72
    UNION ALL SELECT 1002, 139, N'595', N'Dodatočne platená daň z príjmov', NULL, 0, NULL, NULL, 73
    UNION ALL SELECT 1002, 140, NULL, N'Výsledok hospodárenia po zdanení r. 137 mínus (r. 138, r. 139) (+/-)', NULL, 1, NULL, NULL, 74
    UNION ALL SELECT 1002, 141, NULL, N'z toho: pripadajúci na podiely iných účtovných jednotiek', NULL, 0, NULL, NULL, 75
    UNION ALL SELECT 1002, 995, NULL, N'Kontrolné číslo súčet (r. 066 až r. 141)', NULL, 1, NULL, NULL, 76
    UNION ALL SELECT 1102, 143, N'7.', N'Zúčtovanie transferov medzi subjektami verejnej správy (359)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 1102, 144, N'B.III.', N'Dlhodobé záväzky súčet (r. 145 až 153 + r. 155)', NULL, 1, NULL, NULL, 26
    UNION ALL SELECT 1102, 145, N'B.III.1.', N'Ostatné dlhodobé záväzky (479AÚ)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 1102, 146, N'2.', N'Dlhodobé prijaté preddavky (475AÚ)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 1102, 147, N'3.', N'Dlhodobé zmenky na úhradu (478AÚ)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 1102, 148, N'4.', N'Záväzky zo sociálneho fondu (472)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 1102, 149, N'5.', N'Záväzky z nájmu (474AÚ)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 1102, 150, N'6.', N'Dlhodobé nevyfakturované dodávky (476AÚ)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 1102, 151, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 1102, 152, N'8.', N'Predané opcie (377AÚ)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 1002, 111, N'67', N'Mimoriadne výnosy (r. 112 až r. 115)', NULL, 1, NULL, NULL, 45
    UNION ALL SELECT 1002, 112, N'672', N'Náhrady škôd', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 1002, 113, N'674', N'Zúčtovanie rezerv', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 1002, 114, N'678', N'Ostatné mimoriadne výnosy', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 1002, 115, N'679', N'Zúčtovanie opravných položiek', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 1002, 116, N'68', N'Výnosy z transferov a rozpočtových príjmov v štátnych rozpočtových organizáciách a príspevkových organizáciách (r. 117 až r. 125)', NULL, 1, NULL, NULL, 50
    UNION ALL SELECT 1002, 117, N'681', N'Výnosy z bežných transferov zo štátneho rozpočtu', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 1002, 118, N'682', N'Výnosy z kapitálových transferov zo štátneho rozpočtu', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 1002, 119, N'683', N'Výnosy z bežných transferov od ostatných subjektov verejnej správy', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 1002, 120, N'684', N'Výnosy z kapitálových transferov od ostatných subjektov verejnej správy', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 1002, 121, N'685', N'Výnosy z bežných transferov od Európskych spoločenstiev', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 1002, 122, N'686', N'Výnosy z kapitálových transferov od Európskych spoločenstiev', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 1002, 123, N'687', N'Výnosy z bežných transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 1002, 124, N'688', N'Výnosy z kapitálových transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 1002, 125, N'689', N'Výnosy z odvodu rozpočtových príjmov', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 1002, 126, N'69', N'Výnosy z transferov a rozpočtových príjmov v obciach, vyšších územných celkoch a v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom (r. 127 až r. 135)', NULL, 1, NULL, NULL, 60
    UNION ALL SELECT 1002, 127, N'691', N'Výnosy z bežných transferov z rozpočtu obce alebo z rozpočtu vyššieho územného celku v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 1002, 128, N'692', N'Výnosy z kapitálových transferov z rozpočtu obce alebo z rozpočtu vyššieho územného celku v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 1102, 118, NULL, N'VLASTNÉ IMANIE A ZÁVÄZKY r. 119 + r. 130 + r. 185 + r. 188', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 1102, 119, N'A.', N'Vlastné imanie r. 120 + r. 123 + r. 126 + r. 129', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 1102, 120, N'A.I.', N'Oceňovacie rozdiely súčet (r. 121 + r. 122)', NULL, 1, NULL, NULL, 2
    UNION ALL SELECT 1102, 121, N'A.I.1.', N'Oceňovacie rozdiely z precenenia majetku a záväzkov (+/– 414)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1102, 122, N'2.', N'Oceňovacie rozdiely z kapitálových účastín (+/– 415)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1102, 123, N'A.II.', N'Fondy súčet (r. 124 + r. 125)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 1102, 124, N'A.II.1.', N'Zákonný rezervný fond (421)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1102, 125, N'2.', N'Ostatné fondy (427)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1102, 126, N'A.III.', N'Výsledok hospodárenia (+/-) súčet (r. 127 až 128)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 1102, 127, N'A.III.1.', N'Nevysporiadaný výsledok hospodárenia minulých rokov (+/– 428)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 1102, 128, N'2.', N'Výsledok hospodárenia za účtovné obdobie (+/–) r. 001 - (r.120 + r. 123 + r.127 + r.129 + r. 130 + r. 185 + r. 188)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 1102, 129, N'A.IV.', N'Podiely iných učtovných jednotiek', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 1102, 130, N'B.', N'Záväzky súčet r. 131 + r. 136 + r. 144 + r. 156 + r. 178', NULL, 1, NULL, NULL, 12
    UNION ALL SELECT 1102, 131, N'B.I.', N'Rezervy súčet (r. 132 až 135)', NULL, 1, NULL, NULL, 13
    UNION ALL SELECT 1102, 132, N'B.I.1.', N'Rezervy zákonné dlhodobé (451AÚ)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 1102, 133, N'2.', N'Ostatné rezervy (459AÚ)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 1102, 134, N'3.', N'Rezervy zákonné krátkodobé (323AÚ, 451AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 1102, 135, N'4.', N'Ostatné krátkodobé rezervy (323AÚ, 459AÚ)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 1102, 136, N'B.II.', N'Zúčtovanie medzi subjektami verejnej správy súčet (r. 137 až r. 143)', NULL, 1, NULL, NULL, 18
    UNION ALL SELECT 1102, 137, N'B.II.1.', N'Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa (351)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 1102, 138, N'2.', N'Zúčtovanie transferov štátneho rozpočtu (353)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 1102, 139, N'3.', N'Zúčtovanie transferov rozpočtu obce a vyššieho územného celku (355)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 1102, 140, N'4.', N'Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku (356)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 1102, 141, N'5.', N'Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku (357)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 1102, 142, N'6.', N'Zúčtovanie transferov zo štátneho rozpočtu iným subjektom (358)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 518201, 33, N'P.B.', N'Hodnota poistných zmlúv ako záväzok', N'Insurance contracts liabilities', 0, NULL, NULL, 32
    UNION ALL SELECT 518201, 34, N'P.B.1.', N'Hodnota poistných zmlúv na zostávajúce krytie', N'Liability for remaining coverage', 0, NULL, NULL, 33
    UNION ALL SELECT 518201, 35, N'P.B.1.1.', N'Súčasná hodnota budúcich peňažných tokov', N'PV FCF', 0, NULL, NULL, 34
    UNION ALL SELECT 518201, 36, N'P.B.1.2.', N'Servisná marža', N'CSM', 0, NULL, NULL, 35
    UNION ALL SELECT 518201, 37, N'P.B.1.3.', N'Riziková prirážka na nefinančné riziká', N'RA', 0, NULL, NULL, 36
    UNION ALL SELECT 518201, 38, N'P.B.1.4.', N'Hodnota poistných zmlúv ocenené podľa PAA modelu', N'Insurance contract liabilities measured under PAA', 0, NULL, NULL, 37
    UNION ALL SELECT 518201, 39, N'P.B.2.', N'Záväzky zo vzniknutých poistných udalostí', N'Liability for incurred claims', 0, NULL, NULL, 38
    UNION ALL SELECT 518201, 40, N'P.B.2.1.', N'Súčasná hodnota budúcich peňažných tokov', N'LIC FCF', 0, NULL, NULL, 39
    UNION ALL SELECT 518201, 41, N'P.B.2.2.', N'Riziková prirážka na nefinančné riziká', N'LIC RA', 0, NULL, NULL, 40
    UNION ALL SELECT 518201, 42, N'P.C.', N'Záväzky z pasívneho zaistenia', N'Reinsurance contracts that are liabilities', 0, NULL, NULL, 41
    UNION ALL SELECT 518201, 43, N'P.D.', N'Záväzky (iné ako z poistenia a zaistenia)', N'Liabilities (other than from insurance and reinsurance)', 0, NULL, NULL, 42
    UNION ALL SELECT 518201, 44, N'P.E.', N'Krátkodobé zamestnanecké požitky', N'Short - term employee benefits', 0, NULL, NULL, 43
    UNION ALL SELECT 518201, 45, N'P.F.', N'Rezervy (iné ako z poistenia a zaistenia)', N'Reserves (other than from insurance and reinsurance)', 0, NULL, NULL, 44
    UNION ALL SELECT 518201, 46, N'P.G', N'Podriadené záväzky', N'Subordinated liabilities', 0, NULL, NULL, 45
    UNION ALL SELECT 518201, 47, N'P.H', N'Daňové záväzky', N'Tax payables', 0, NULL, NULL, 46
    UNION ALL SELECT 518201, 48, N'P.H.1.', N'Splatná daň z príjmov - záväzok', N'Current tax liability', 0, NULL, NULL, 47
    UNION ALL SELECT 518201, 49, N'P.H.2.', N'Odložené daňové záväzky', N'Deferred income tax liabilities', 0, NULL, NULL, 48
    UNION ALL SELECT 518201, 50, N'P.I.', N'Účty časového rozlíšenia (pasívne)', N'Accrual accounts (passive)', 0, NULL, NULL, 49
    UNION ALL SELECT 518201, 51, N'P.J.', N'Ostatné pasíva, inde neuvedené', N'Other liabilities', 0, NULL, NULL, 50
    UNION ALL SELECT 518201, 52, NULL, N'ZÁVÄZKY SPOLU', N'TOTAL LIABILITIES', 0, NULL, NULL, 51
    UNION ALL SELECT 1102, 153, N'9.', N'Iné záväzky (379AÚ)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 1102, 154, NULL, N'z toho: odložený daňový záväzok', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 1102, 155, N'10.', N'Vydané dlhopisy dlhodobé (473AÚ ) - (255AÚ)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 1102, 156, N'B.IV.', N'Krátkodobé záväzky súčet (r. 157 až 177)', NULL, 1, NULL, NULL, 38
    UNION ALL SELECT 1102, 157, N'B.IV.1.', N'Dodávatelia (321)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 1102, 158, N'2.', N'Zmenky na úhradu (322, 478AÚ)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 1102, 159, N'3.', N'Prijaté preddavky (324, 475AÚ)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 1102, 160, N'4.', N'Ostatné záväzky (325, 479AÚ)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 1102, 161, N'5.', N'Nevyfakturované dodávky (326, 476AÚ)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 1102, 162, N'6.', N'Záväzky z nájmu (474AÚ)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 1102, 163, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 1102, 164, N'8.', N'Predané opcie (377AÚ)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 1102, 165, N'9.', N'Iné záväzky (379AÚ)', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 1102, 166, N'10.', N'Záväzky z upísaných nesplatených cenných papierov a vkladov (367)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 1102, 167, N'11.', N'Záväzky voči združeniu (368)', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 1102, 168, N'12.', N'Zamestnanci (331)', NULL, 0, N'Krátkodobé záväzky - Dan z pridanej hodnoty', NULL, 50
    UNION ALL SELECT 1102, 169, N'13.', N'Ostatné záväzky voči zamestnancom (333)', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 1102, 170, N'14.', N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia (336)', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 1102, 171, N'15.', N'Daň z príjmov (341)', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 1102, 172, N'16.', N'Ostatné priame dane (342)', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 1102, 173, N'17.', N'Daň z pridanej hodnoty (343)', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 1102, 174, N'18.', N'Ostatné dane a poplatky (345)', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 1102, 175, N'19.', N'Spojovací účet pri združení (396AÚ)', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 1102, 176, N'20.', N'Zúčtovanie s Európskymi spoločenstvami (371AÚ)', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 1102, 177, N'21.', N'Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy (372AÚ)', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 1102, 178, N'B.V.', N'Bankové úvery a výpomoci súčet (r. 179 až 184)', NULL, 1, NULL, NULL, 60
    UNION ALL SELECT 1102, 179, N'B.V.1.', N'Bankové úvery dlhodobé (461AÚ )', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 1102, 180, N'2.', N'Bežné bankové úvery (461AÚ, 221AÚ, 231, 232)', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 1102, 181, N'3.', N'Vydané dlhopisy krátkodobé (473AÚ, 241 ) - (255AÚ)', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 1102, 182, N'4.', N'Ostatné krátkodobé finančné výpomoci (249)', NULL, 0, NULL, NULL, 64
    UNION ALL SELECT 1102, 183, N'5.', N'Prijaté návratné finančné výpomoci od subjektov verejnej správy dlhodobé (273AÚ)', NULL, 0, NULL, NULL, 65
    UNION ALL SELECT 1102, 184, N'6.', N'Prijaté návratné finančné výpomoci od subjektov verejnej správy krátkodobé (273AÚ)', NULL, 0, NULL, NULL, 66
    UNION ALL SELECT 1102, 185, N'C.', N'Časové rozlíšenie súčet (r. 186 + r. 187)', NULL, 1, NULL, NULL, 67
    UNION ALL SELECT 1102, 186, N'C.1.', N'Výdavky budúcich období (383)', NULL, 0, NULL, NULL, 68
    UNION ALL SELECT 1102, 187, N'2.', N'Výnosy budúcich období (384)', NULL, 0, NULL, NULL, 69
    UNION ALL SELECT 1102, 188, N'D.', N'Vzťahy k účtom klientov štátnej pokladnice (účtová skupina 20)', NULL, 0, NULL, NULL, 70
    UNION ALL SELECT 1102, 999, NULL, N'KONTROLNÉ ČÍSLO súčet (r. 118 až 188)', NULL, 1, NULL, NULL, 71
    UNION ALL SELECT 1501, 1, NULL, N'Dlhodobý nehmotný majetok', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 1501, 2, NULL, N'Dlhodobý hmotný majetok', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 1501, 3, NULL, N'Dlhodobý finančný majetok', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1501, 4, NULL, N'Zásoby', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1501, 5, NULL, N'Pohľadávky', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1501, 6, NULL, N'Peniaze', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1501, 7, NULL, N'Ceniny', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1501, 8, NULL, N'Priebežné položky (+/-)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1501, 9, NULL, N'Bankové účty', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 1501, 10, NULL, N'Krátkodobé cenné papiere a ostatný krátkodobý finančný majetok', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 1501, 11, NULL, N'Majetok celkom (súčet r. 01 až r. 10)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 1601, 1, NULL, N'Dlhodobý nehmotný majetok', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 1601, 2, NULL, N'Dlhodobý hmotný majetok', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 1601, 3, NULL, N'Dlhodobý finančný majetok', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1601, 4, NULL, N'Zásoby', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1601, 5, NULL, N'Pohľadávky', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1601, 6, NULL, N'Peniaze', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1601, 7, NULL, N'Ceniny', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1601, 8, NULL, N'Priebežné položky (+/-)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1601, 9, NULL, N'Bankové účty', NULL, 0, NULL, NULL, 8
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 1601 AS [TableErpId], 10 AS [RowNumber], NULL AS [Designation], N'Krátkodobé cenné papiere a ostatný krátkodobý finančný majetok' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 9 AS [RowOrdinal]
    UNION ALL SELECT 1601, 11, NULL, N'Majetok celkom (súčet r. 01 až r. 10)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 518201, 1, N'A.A.', N'Peňažné prostriedky a peňažné ekvivalenty', N'Cash and cash equivalents', 0, NULL, NULL, 0
    UNION ALL SELECT 518201, 2, N'A.B.', N'Investície', N'Investments', 0, NULL, NULL, 1
    UNION ALL SELECT 518201, 3, N'A.B.1.', N'Finančné nástroje oceňované reálnou hodnotou cez výsledok hospodárenia (FVTPL)', N'Investments at FVTPL', 0, NULL, NULL, 2
    UNION ALL SELECT 518201, 4, N'A.B.2.', N'Finančné nástroje oceňované reálnou hodnotou cez ostatné súčasti komplexného výsledku (FVOCI)', N'Investments at FVOCI', 0, NULL, NULL, 3
    UNION ALL SELECT 518201, 5, N'A.B.3.', N'Finančné nástroje oceňované amortizovanou hodnotou (AC)', N'Investments at AC', 0, NULL, NULL, 4
    UNION ALL SELECT 518201, 6, N'A.B.4.', N'Podiely v prepojených podnikoch vrátane účastí', N'Investments in affiliated and associated enterprises and joint ventures', 0, NULL, NULL, 5
    UNION ALL SELECT 518201, 7, N'A.C.', N'Hodnota poistných zmlúv ako aktívum', N'Insurance contracts that are assets', 0, NULL, NULL, 6
    UNION ALL SELECT 518201, 8, N'A.C.1.', N'Hodnota poistných zmlúv na zostávajúce krytie', N'Assets for remaining coverage', 0, NULL, NULL, 7
    UNION ALL SELECT 518201, 9, N'A.C.1.1.', N'Súčasná hodnota budúcich peňažných tokov', N'PV FCF', 0, NULL, NULL, 8
    UNION ALL SELECT 518201, 10, N'A.C.1.2.', N'Servisná marža', N'CSM', 0, NULL, NULL, 9
    UNION ALL SELECT 518201, 11, N'A.C.1.3.', N'Riziková prirážka na nefinančné riziká', N'RA', 0, NULL, NULL, 10
    UNION ALL SELECT 518201, 12, N'A.C.1.4.', N'Hodnota poistných zmlúv ocenené podľa PAA modelu', N'Insurance contract assets measured under PAA', 0, NULL, NULL, 11
    UNION ALL SELECT 518201, 13, N'A.C.2.', N'Hodnota poistných zmlúv na vzniknuté poistné udalosti', N'Asset for Incurred Claims AIC', 0, NULL, NULL, 12
    UNION ALL SELECT 518201, 14, N'A.C.2.1.', N'Súčasná hodnota budúcich peňažných tokov', N'PV FCF', 0, NULL, NULL, 13
    UNION ALL SELECT 518201, 15, N'A.C.2.2.', N'Riziková prirážka na nefinančné riziká', N'RA', 0, NULL, NULL, 14
    UNION ALL SELECT 518201, 16, N'A.C.3.', N'Predplatené alebo nealokované obstarávacie náklady na poistné zmluvy', N'Prepaid or not allocated acqusition costs', 0, NULL, NULL, 15
    UNION ALL SELECT 518201, 17, N'A.D.', N'Pasívne zaistenie ako aktívum', N'Reinsurance contracts that are assets', 0, NULL, NULL, 16
    UNION ALL SELECT 518201, 18, N'A.E.', N'Pohľadávky (iné ako z poistenia a zaistenia)', N'Receivables (other than from insurance and reinsurance)', 0, NULL, NULL, 17
    UNION ALL SELECT 518201, 19, N'A.F.', N'Hmotný majetok', N'Property and equipment', 0, NULL, NULL, 18
    UNION ALL SELECT 518201, 20, N'A.G.', N'Nehmotné aktíva', N'Intangible assets', 0, NULL, NULL, 19
    UNION ALL SELECT 518201, 21, N'A.G.1.', N'Goodwill', N'Goodwill', 0, NULL, NULL, 20
    UNION ALL SELECT 518201, 22, N'A.G.2.', N'Softvér', N'Software', 0, NULL, NULL, 21
    UNION ALL SELECT 518201, 23, N'A.G.3.', N'Ostatné nehmotné aktíva', N'Other intangible assets', 0, NULL, NULL, 22
    UNION ALL SELECT 518201, 24, N'A.H.', N'Daňové pohľadávky', N'Tax receivables', 0, NULL, NULL, 23
    UNION ALL SELECT 518201, 25, N'A.H.1.', N'Splatná daň z príjmov - pohľadávka', N'Current tax asset', 0, NULL, NULL, 24
    UNION ALL SELECT 518201, 26, N'A.H.2.', N'Odložené daňové pohľadávky', N'Deferred tax assets', 0, NULL, NULL, 25
    UNION ALL SELECT 518201, 27, N'A.I.', N'Účty časového rozlíšenia (aktívne)', N'Accrual accounts (active)', 0, NULL, NULL, 26
    UNION ALL SELECT 518201, 28, N'A.J.', N'Ostatné aktíva, inde neuvedené', N'Other assets', 0, NULL, NULL, 27
    UNION ALL SELECT 518201, 29, NULL, N'AKTÍVA SPOLU', N'TOTAL ASSETS', 0, NULL, NULL, 28
    UNION ALL SELECT 518201, 30, N'P.A.', N'Finančné záväzky', N'Financial liabilities', 0, NULL, NULL, 29
    UNION ALL SELECT 518201, 31, N'P.A.1.', N'Záväzky z investičných zmlúv', N'Investment contract liabilities', 0, NULL, NULL, 30
    UNION ALL SELECT 518201, 32, N'P.A.2.', N'Prijaté úvery a pôžičky', N'Loans received', 0, NULL, NULL, 31
    UNION ALL SELECT 518302, 1, NULL, N'Pohľadávky po lehote splatnosti z pokračujúcej činnosti celkom, z toho:', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 518302, 2, NULL, N'- do 90 dní', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 518302, 3, NULL, N'- od 91 dní do 120 dní', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 518302, 4, NULL, N'- od 121 dní do 150 dní', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 518302, 5, NULL, N'- od 151 dní do 180 dní', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 518302, 6, NULL, N'- od 181 dní do 360 dní', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 518302, 7, NULL, N'- od 361 dní a viac', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 518304, 1, NULL, N'Výnosy z prevádzkovej činnosti celkom (r. 02 + r. 06)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 518304, 2, NULL, N'Tržby celkom, z toho:', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 518304, 3, NULL, N'tržby z predaja tovaru', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 518304, 4, NULL, N'tržby z predaja vlastných výrobkov', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 518304, 5, NULL, N'tržby z poskytnutých služieb a zákazkovej výroby', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 518304, 6, NULL, N'Ostatné prevádzkové výnosy celkom, z toho:', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 518304, 7, NULL, N'výnosy z predaja dlhodobého majetku', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 518304, 8, NULL, N'výnosy z predaja materiálu', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 518304, 9, NULL, N'Náklady na prevádzkovú činnosť celkom (r. 10 až r. 16)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 518304, 10, NULL, N'Náklady vynaložené na obstaranie predaného tovaru', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 518304, 11, NULL, N'Spotreba materiálu a energie', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 518304, 12, NULL, N'Osobné náklady', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 518304, 13, NULL, N'Náklady na službu', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 518304, 14, NULL, N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 518304, 15, NULL, N'Rezervy a straty zo znehodnotenia dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 518304, 16, NULL, N'Ostatné prevádzkové náklady, z toho:', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 518304, 17, NULL, N'náklady na predaj dlhodobého majetku', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 518304, 18, NULL, N'náklady na predaj materiálu', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 518304, 19, NULL, N'tvorba a zúčtovanie opravných položiek k pohľadávkam', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 518304, 20, NULL, N'Výsledok hospodárenia z prevádzkovej činnosti pred zdanením (+/-), (r. 01 - r. 09)', NULL, 1, NULL, NULL, 19
    UNION ALL SELECT 518304, 21, NULL, N'Finančné výnosy, z toho:', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 518304, 22, NULL, N'výnosové úroky', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 518304, 23, NULL, N'kurzové výnosy', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 518304, 24, NULL, N'Finančné náklady, z toho:', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 518304, 25, NULL, N'nákladové úroky', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 518304, 26, NULL, N'kurzové náklady', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 518304, 27, NULL, N'tvorba a zúčtovanie opravných položiek k pôžičkám', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 518304, 28, NULL, N'Výsledok hospodárenia z pokračujúcich činností pred zdanením (+/-), (r. 20 + r. 21 - r. 24)', NULL, 1, NULL, NULL, 27
    UNION ALL SELECT 518304, 29, NULL, N'Daň z príjmov', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 518304, 30, NULL, N'Výsledok hospodárenia z pokračujúcich činností po zdanení (+/-), (r. 28 - r. 29)', NULL, 1, NULL, NULL, 29
    UNION ALL SELECT 518304, 31, NULL, N'Výsledok hospodárenia z ukončených činností pred zdanením (+/-)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 518304, 32, NULL, N'Daň z príjmov', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 518304, 33, NULL, N'Výsledok hospodárenia z ukončených činností po zdanení (+/-), (r. 31 - r. 32)', NULL, 1, NULL, NULL, 32
    UNION ALL SELECT 69602, 87, N'644', N'Zmluvné pokuty, penále a úroky z omeškania', N'Contractual fines, penalties, and interest on late payment', 0, NULL, NULL, 21
    UNION ALL SELECT 69602, 88, N'645', N'Ostatné pokuty, penále a úroky z omeškania', N'Other fines, penalties, and interest on late payment', 0, NULL, NULL, 22
    UNION ALL SELECT 69602, 89, N'646', N'Výnosy z odpísaných pohľadávok', N'Revenues from written off receivables', 0, NULL, NULL, 23
    UNION ALL SELECT 69602, 90, N'648', N'Ostatné výnosy z prevádzkovej činnosti', N'Other operating revenues', 0, NULL, NULL, 24
    UNION ALL SELECT 69602, 91, N'65', N'Zúčtovanie rezerv a opravných položiek z prevádzkovej činnosti a finančnej činnosti a zúčtovanie časového rozlíšenia (r. 092 + r. 097 + r. 100)', N'Clearing of provisions and adjusting entries to operating and financial activities, and clearing of accruals and deferrals - total (line 092 + line 097 + line 100)', 1, NULL, NULL, 25
    UNION ALL SELECT 69602, 92, NULL, N'Zúčtovanie rezerv a opravných položiek z prevádzkovej činnosti (r. 093 až r. 096)', N'Clearing of provisions and adjusting entries to operating activities - total (lines 093 to 096)', 1, NULL, NULL, 26
    UNION ALL SELECT 69602, 93, N'652', N'Zúčtovanie zákonných rezerv z prevádzkovej činnosti', N'Clearing of legal provisions out of operations', 0, NULL, NULL, 27
    UNION ALL SELECT 69602, 94, N'653', N'Zúčtovanie ostatných rezerv z prevádzkovej činnosti', N'Clearing of other provisions out of operations', 0, NULL, NULL, 28
    UNION ALL SELECT 69602, 95, N'657', N'Zúčtovanie zákonných opravných položiek z prevádzkovej činnosti', N'Clearing of legal adjusting entries out of operations', 0, NULL, NULL, 29
    UNION ALL SELECT 69602, 96, N'658', N'Zúčtovanie ostatných opravných položiek z prevádzkovej činnosti', N'Clearing of other adjusting entries out of operations', 0, NULL, NULL, 30
    UNION ALL SELECT 69602, 97, NULL, N'Zúčtovanie rezerv a opravných položiek z finančnej činnosti (r. 098 + r. 099)', N'Clearing of provisions and adjusting entries to financial activities - total (lines 098 to 099)', 1, NULL, NULL, 31
    UNION ALL SELECT 69602, 98, N'654', N'Zúčtovanie rezerv z finančnej činnosti', N'Clearing of provisions out of financial activity', 0, NULL, NULL, 32
    UNION ALL SELECT 69602, 99, N'659', N'Zúčtovanie opravných položiek z finančnej činnosti', N'Clearing of adjusting entries out of financial activity', 0, NULL, NULL, 33
    UNION ALL SELECT 69602, 100, N'655', N'Zúčtovanie komplexných nákladov budúcich období', N'Clearing of complex deferred expenses', 0, NULL, NULL, 34
    UNION ALL SELECT 69602, 101, N'66', N'Finančné výnosy (r. 102 až r. 110)', N'Financial revenues - total (lines 102 to 110)', 1, NULL, NULL, 35
    UNION ALL SELECT 69602, 102, N'661', N'Tržby z predaja cenných papierov a podielov', N'Revenues from the sale of securities and shares', 0, NULL, NULL, 36
    UNION ALL SELECT 69602, 103, N'662', N'Úroky', N'Interest income', 0, NULL, NULL, 37
    UNION ALL SELECT 69602, 104, N'663', N'Kurzové zisky', N'Exchange rate gains', 0, NULL, NULL, 38
    UNION ALL SELECT 69602, 105, N'664', N'Výnosy z precenenia cenných papierov', N'Revenue from securities revaluation', 0, NULL, NULL, 39
    UNION ALL SELECT 69602, 106, N'665', N'Výnosy z dlhodobého finančného majetku', N'Revenues from non-current financial assets', 0, NULL, NULL, 40
    UNION ALL SELECT 69602, 107, N'666', N'Výnosy z krátkodobého finančného majetku', N'Revenues from current financial assets', 0, NULL, NULL, 41
    UNION ALL SELECT 69602, 108, N'667', N'Výnosy z derivátových operácií', N'Revenues from derivative transactions', 0, NULL, NULL, 42
    UNION ALL SELECT 69602, 109, N'668', N'Ostatné finančné výnosy', N'Other financial revenues', 0, NULL, NULL, 43
    UNION ALL SELECT 69602, 110, NULL, N'Podiel konsolidujúcej účtovnej jednotky na výsledku hospodárenia pridružených účtovných jednotiek verejnej správy', N'Share of controlling entity on profit/loss of associates', 0, NULL, NULL, 44
    UNION ALL SELECT 69602, 111, N'67', N'Mimoriadne výnosy (r. 112 až r. 115)', N'Extraordinary revenues - total (lines 112 to 115)', 1, NULL, NULL, 45
    UNION ALL SELECT 69602, 112, N'672', N'Náhrady škôd', N'Compensation of damages', 0, NULL, NULL, 46
    UNION ALL SELECT 69602, 113, N'674', N'Zúčtovanie rezerv', N'Accounting for provisions', 0, NULL, NULL, 47
    UNION ALL SELECT 69602, 114, N'678', N'Ostatné mimoriadne výnosy', N'Other extraordinary revenues', 0, NULL, NULL, 48
    UNION ALL SELECT 69602, 115, N'679', N'Zúčtovanie opravných položiek', N'Clearing of adjusting entries', 0, NULL, NULL, 49
    UNION ALL SELECT 69602, 116, N'68', N'Výnosy z transferov a rozpočtových príjmov v štátnych rozpočtových organizáciách a príspevkových organizáciách (r. 117 až r. 125)', N'Revenues from transfers and budgetary revenues in state-funded and state-subsidized organisations - total (lines 117 to 125)', 1, NULL, NULL, 50
    UNION ALL SELECT 69602, 117, N'681', N'Výnosy z bežných transferov zo štátneho rozpočtu', N'Revenues from current transfers from state budget', 0, NULL, NULL, 51
    UNION ALL SELECT 69602, 118, N'682', N'Výnosy z kapitálových transferov zo štátneho rozpočtu', N'Revenues from capital transfers from state budget', 0, NULL, NULL, 52
    UNION ALL SELECT 69602, 119, N'683', N'Výnosy z bežných transferov od ostatných subjektov verejnej správy', N'Revenues from current transfers from other entities of general government', 0, NULL, NULL, 53
    UNION ALL SELECT 69602, 120, N'684', N'Výnosy z kapitálových transferov od ostatných subjektov verejnej správy', N'Revenues from capital transfers from other entities of general government', 0, NULL, NULL, 54
    UNION ALL SELECT 69602, 121, N'685', N'Výnosy z bežných transferov od Európskej únie', N'Revenues from current transfers from the European Union', 0, NULL, NULL, 55
    UNION ALL SELECT 69602, 122, N'686', N'Výnosy z kapitálových transferov od Európskej únie', N'Revenues from capital transfers from the European Union', 0, NULL, NULL, 56
    UNION ALL SELECT 69602, 123, N'687', N'Výnosy z bežných transferov od ostatných subjektov mimo verejnej správy', N'Revenues from current transfers from other entities outside of general government', 0, NULL, NULL, 57
    UNION ALL SELECT 69602, 124, N'688', N'Výnosy z kapitálových transferov od ostatných subjektov mimo verejnej správy', N'Revenues from capital transfers from other entities outside of general government', 0, NULL, NULL, 58
    UNION ALL SELECT 69602, 125, N'689', N'Výnosy z odvodu rozpočtových príjmov', N'Revenues from budgetary contributions', 0, NULL, NULL, 59
    UNION ALL SELECT 69602, 126, N'69', N'Výnosy z transferov a rozpočtových príjmov v obciach, vyšších územných celkoch a v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom (r. 127 až r. 135)', N'Revenue from transfers and budgetary revenue in municipalities, higher regional units and state-subsidized organisations founded by municipality and higher regional unit - total (lines 127 to 135)', 1, NULL, NULL, 60
    UNION ALL SELECT 69602, 127, N'691', N'Výnosy z bežných transferov z rozpočtu obce alebo z rozpočtu vyššieho územného celku v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom', N'Revenues from current transfers from the budget of municipality or higher regional unit in state-funded and state-subsidized organisations founded by municipality or higher regional unit', 0, NULL, NULL, 61
    UNION ALL SELECT 8201, 1, NULL, N'Dlhodobý nehmotný majetok', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 8201, 2, NULL, N'Dlhodobý hmotný majetok', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 8201, 3, NULL, N'Dlhodobý finančný majetok', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 8201, 4, NULL, N'Zásoby celkom súčet (r. 05 až 07)', NULL, 1, NULL, NULL, 3
    UNION ALL SELECT 8201, 5, NULL, N'Materiál', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 8201, 6, NULL, N'Tovar', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 8201, 7, NULL, N'Nedokončená výroba, výrobky, zvieratá, ostatné', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 8201, 8, NULL, N'Pohľadávky', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 8201, 9, NULL, N'Krátkodobý finančný majetok súčet (r. 10 až 12)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 8201, 10, NULL, N'Peniaze a ceniny', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 8201, 11, NULL, N'Účty v bankách', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 8201, 12, NULL, N'Ostatný krátkodobý finančný majetok', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 8201, 13, NULL, N'Priebežné položky (+/-)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 8201, 14, NULL, N'Opravná položka k nadobudnutému majetku (aktívna)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 8201, 15, NULL, N'Majetok celkom r. 01 + r. 02 + r. 03 + r. 04 + r. 08 + r. 09 +/- r. 13 + r. 14', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 69602, 134, N'698', N'Výnosy samosprávy z kapitálových transferov od ostatných subjektov mimo verejnej správy', N'Revenues of the local government from capital transfers from other entities outside of general government', 0, NULL, NULL, 68
    UNION ALL SELECT 69602, 135, N'699', N'Výnosy samosprávy z odvodu rozpočtových príjmov', N'Revenues of the local government from budgetary contributions', 0, NULL, NULL, 69
    UNION ALL SELECT 69602, 136, NULL, N'Účtová trieda 6 súčet (r. 066 + r. 070 + r. 075 + r. 080 + r. 084 + r. 091 + r. 101 + r. 111 + r. 116 + r. 126)', N'Account class 6, line 066 + line 070 + line 075 + line 080 + line 084 + line 091 + line 101 + line 111 + line 116 + line 126', 1, NULL, NULL, 70
    UNION ALL SELECT 69602, 137, NULL, N'Výsledok hospodárenia pred zdanením (r. 136 mínus r. 065) (+/-)', N'Profit (loss) before tax (line 136 - line 065) (+/-)', 1, NULL, NULL, 71
    UNION ALL SELECT 69602, 138, N'591', N'Splatná daň z príjmov', N'Income tax - current', 0, NULL, NULL, 72
    UNION ALL SELECT 69602, 139, N'595', N'Dodatočne platená daň z príjmov', N'Supplementary income tax levies', 0, NULL, NULL, 73
    UNION ALL SELECT 69602, 140, NULL, N'Výsledok hospodárenia po zdanení r. 137 mínus (r. 138, r. 139) (+/-)', N'Profit/loss after tax (line 137 - line 138, line 139) (+/-)', 1, NULL, NULL, 74
    UNION ALL SELECT 69602, 141, NULL, N'z toho: pripadajúci na podiely iných účtovných jednotiek', N'of that: profit/loss on shares of other entities', 0, NULL, NULL, 75
    UNION ALL SELECT 69902, 92, N'2.', N'Ostatné fondy (427, 42X)', N'Other funds (427, 42X)', 0, NULL, NULL, 13
    UNION ALL SELECT 69902, 93, N'A.VI.', N'Oceňovacie rozdiely z precenenia súčet (r. 94 až r. 96)', N'Differences from revaluation - total (lines 94 to 96)', 1, NULL, NULL, 14
    UNION ALL SELECT 69902, 94, N'A.VI.1.', N'Oceňovacie rozdiely z precenenia majetku a záväzkov (+/- 414)', N'Differences from revaluation of assets and liabilities (+/- 414)', 0, NULL, NULL, 15
    UNION ALL SELECT 69902, 95, N'2.', N'Oceňovacie rozdiely z kapitálových účastín (+/- 415)', N'Investment revaluation reserves (+/- 415)', 0, NULL, NULL, 16
    UNION ALL SELECT 69902, 96, N'3.', N'Oceňovacie rozdiely z precenenia pri zlúčení, splynutí a rozdelení (+/- 416)', N'Differences from revaluation in the event of a merger, amalgamation into a separate accounting entity or demerger (+/- 416)', 0, NULL, NULL, 17
    UNION ALL SELECT 69902, 97, N'A.VII.', N'Výsledok hospodárenia minulých rokov r. 98 + r. 99', N'Net profit/loss of previous years line 98 + line 99', 1, NULL, NULL, 18
    UNION ALL SELECT 69902, 98, N'A.VII.1.', N'Nerozdelený zisk minulých rokov (428)', N'Retained earnings from previous years (428)', 0, NULL, NULL, 19
    UNION ALL SELECT 69902, 99, N'2.', N'Neuhradená strata minulých rokov (/-/429)', N'Accumulated losses from previous years (/-/429)', 0, NULL, NULL, 20
    UNION ALL SELECT 69902, 100, N'A.VIII.', N'Výsledok hospodárenia za účtovné obdobie po zdanení /+-/ r. 01 - (r. 81 + r. 85 + r. 86 + r. 87 + r. 90 + r. 93 + r. 97 + r. 101 + r. 141)', N'Net profit/loss for the accounting period after tax /+-/ line 01 - (line 81 + line 85 + line 86 + line 87 + line 90 + line 93 + line 97 + line 101 + line 141)', 1, NULL, NULL, 21
    UNION ALL SELECT 69902, 101, N'B.', N'Záväzky r. 102 + r. 118 + r. 121 + r. 122 + r. 136 + r. 139 + r. 140', N'Liabilities line 102 + line 118 + line 121 + line 122 + line 136 + line 139 + line 140', 1, NULL, NULL, 22
    UNION ALL SELECT 69902, 102, N'B.I.', N'Dlhodobé záväzky súčet (r. 103 + r. 107 až r. 117)', N'Non-current liabilities - total (line 103 + lines 107 to 117)', 1, NULL, NULL, 23
    UNION ALL SELECT 69902, 103, N'B.I.1.', N'Dlhodobé záväzky z obchodného styku súčet (r. 104 až r. 106)', N'Non-current trade liabilities - total (lines 104 to 106)', 1, NULL, NULL, 24
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 69902 AS [TableErpId], 104 AS [RowNumber], N'1.a.' AS [Designation], N'Záväzky z obchodného styku voči prepojeným účtovným jednotkám (321A, 475A, 476A)' AS [Text_sk], N'Trade liabilities to affiliated accounting entities (321A, 475A, 476A)' AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 25 AS [RowOrdinal]
    UNION ALL SELECT 69902, 105, N'1.b.', N'Záväzky z obchodného styku v rámci podielovej účasti okrem záväzkov voči prepojeným účtovným jednotkám (321A, 475A, 476A)', N'Trade liabilities within participating interest, except for liabilities to affiliated accounting entities (321A, 475A, 476A)', 0, NULL, NULL, 26
    UNION ALL SELECT 69902, 106, N'1.c.', N'Ostatné záväzky z obchodného styku (321A, 475A, 476A)', N'Other trade liabilities (321A, 475A, 476A)', 0, NULL, NULL, 27
    UNION ALL SELECT 69902, 107, N'2.', N'Čistá hodnota zákazky (316A)', N'Net value of contract (316A)', 0, NULL, NULL, 28
    UNION ALL SELECT 69902, 108, N'3.', N'Ostatné záväzky voči prepojeným účtovným jednotkám (471A, 47XA)', N'Other liabilities to affiliated accounting entities (471A, 47XA)', 0, NULL, NULL, 29
    UNION ALL SELECT 69902, 109, N'4.', N'Ostatné záväzky v rámci podielovej účasti okrem záväzkov voči prepojeným účtovným jednotkám (471A, 47XA)', N'Other liabilities within participating interest, except for liabilities to affiliated accounting entities (471A, 47XA)', 0, NULL, NULL, 30
    UNION ALL SELECT 69902, 110, N'5.', N'Ostatné dlhodobé záväzky (479A, 47XA)', N'Other non-current liabilities(479A, 47XA)', 0, NULL, NULL, 31
    UNION ALL SELECT 69902, 111, N'6.', N'Dlhodobé prijaté preddavky (475A)', N'Long-term advance payments received (475A)', 0, NULL, NULL, 32
    UNION ALL SELECT 69902, 112, N'7.', N'Dlhodobé zmenky na úhradu (478A)', N'Long-term bills of exchange to be paid (478A)', 0, NULL, NULL, 33
    UNION ALL SELECT 69902, 113, N'8.', N'Vydané dlhopisy (473A/-/255A)', N'Bonds issued (473A/-/255A)', 0, NULL, NULL, 34
    UNION ALL SELECT 69902, 114, N'9.', N'Záväzky zo sociálneho fondu (472)', N'Liabilities related to social fund (472)', 0, NULL, NULL, 35
    UNION ALL SELECT 69902, 115, N'10.', N'Iné dlhodobé záväzky (336A, 372A, 474A, 47XA)', N'Other non-current liabilities (336A, 372A, 474A, 47XA)', 0, N'VLASTNÉ IMANIE A ZÁVÄZKY', NULL, 36
    UNION ALL SELECT 69902, 116, N'11.', N'Dlhodobé záväzky z derivátových operácií (373A, 377A)', N'Non-current liabilities related to derivative transactions (373A, 377A)', 0, N'Vlastné imanie', NULL, 37
    UNION ALL SELECT 69602, 66, N'60', N'Tržby za vlastné výkony a tovar (r. 067 až r. 069)', N'Revenues from own activity and merchandise - total (lines 067 to 069)', 1, NULL, NULL, 0
    UNION ALL SELECT 69602, 67, N'601', N'Tržby za vlastné výrobky', N'Revenues from the sale of own products', 0, NULL, NULL, 1
    UNION ALL SELECT 69602, 68, N'602', N'Tržby z predaja služieb', N'Revenues from the sale of services provided', 0, NULL, NULL, 2
    UNION ALL SELECT 69602, 69, N'604', N'Tržby za tovar, Výnosy z nehnuteľnosti na predaj', N'Revenues from the sale of merchandise', 0, NULL, NULL, 3
    UNION ALL SELECT 69602, 70, N'61', N'Zmena stavu vnútroorganizačných zásob (r. 071 až r. 074)', N'Changes in internal inventory - total (lines 071 to 074) (+/-)', 1, NULL, NULL, 4
    UNION ALL SELECT 69602, 71, N'611', N'Zmena stavu nedokončenej výroby', N'Change in inventory of work in progress', 0, NULL, NULL, 5
    UNION ALL SELECT 69602, 72, N'612', N'Zmena stavu polotovarov', N'Change in inventory of semi-finished products', 0, NULL, NULL, 6
    UNION ALL SELECT 69602, 73, N'613', N'Zmena stavu výrobkov', N'Change in inventory of finished products', 0, NULL, NULL, 7
    UNION ALL SELECT 69602, 74, N'614', N'Zmena stavu zvierat', N'Change in animal inventory', 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 8
    UNION ALL SELECT 69602, 75, N'62', N'Aktivácia (r. 076 až r. 079)', N'Capitalization - total (lines 076 to 079)', 1, NULL, NULL, 9
    UNION ALL SELECT 69602, 76, N'621', N'Aktivácia materiálu a tovaru', N'Capitalization of material and merchandise', 0, NULL, NULL, 10
    UNION ALL SELECT 69602, 77, N'622', N'Aktivácia vnútroorganizačných služieb', N'Capitalization of internal services', 0, NULL, NULL, 11
    UNION ALL SELECT 69602, 78, N'623', N'Aktivácia dlhodobého nehmotného majetku', N'Capitalization of long-term intangible assets', 0, NULL, NULL, 12
    UNION ALL SELECT 69602, 79, N'624', N'Aktivácia dlhodobého hmotného majetku', N'Capitalization of long-term tangible assets', 0, NULL, NULL, 13
    UNION ALL SELECT 69602, 80, N'63', N'Daňové a colné výnosy a výnosy z poplatkov (r. 081 až r. 083)', N'Tax and customs revenues and revenues from fees - total (lines 081 to 083)', 1, NULL, NULL, 14
    UNION ALL SELECT 69602, 81, N'631', N'Daňové a colné výnosy štátu', N'Tax and customs revenues of the state', 0, NULL, NULL, 15
    UNION ALL SELECT 69602, 82, N'632', N'Daňové výnosy samosprávy', N'Tax revenues of the local government', 0, NULL, NULL, 16
    UNION ALL SELECT 69602, 83, N'633', N'Výnosy z poplatkov', N'Revenues from fees', 0, NULL, NULL, 17
    UNION ALL SELECT 69602, 84, N'64', N'Ostatné výnosy z prevádzkovej činnosti (r. 085 až r. 090)', N'Other operating revenues - total (lines 085 to 090)', 1, NULL, NULL, 18
    UNION ALL SELECT 69602, 85, N'641', N'Tržby z predaja dlhodobého nehmotného majetku a dlhodobého hmotného majetku', N'Revenues from the sale of non-current intangible assets and non-current tangible assets', 0, NULL, NULL, 19
    UNION ALL SELECT 69602, 86, N'642', N'Tržby z predaja materiálu', N'Revenues from material sold', 0, NULL, NULL, 20
    UNION ALL SELECT 69902, 79, NULL, N'SPOLU VLASTNÉ IMANIE A ZÁVÄZKY r. 80 + r. 101 + r. 141', N'TOTAL EQUITY AND LIABILITIES line 80 + line 101 + line 141', 1, NULL, NULL, 0
    UNION ALL SELECT 69902, 80, N'A.', N'Vlastné imanie r. 81 + r. 85 + r. 86 + r. 87 + r. 90 + r. 93 + r. 97 + r. 100', N'Equity line 81 + line 85 + line 86 + line 87 + line 90 + line 93 + line 97 + line 100', 1, NULL, NULL, 1
    UNION ALL SELECT 69902, 81, N'A.I.', N'Základné imanie súčet (r. 82 až r. 84)', N'Share capital - total (lines 82 to 84)', 1, NULL, NULL, 2
    UNION ALL SELECT 69902, 82, N'A.I.1.', N'Základné imanie (411 alebo +/- 491)', N'Share capital (411 or +/- 491)', 0, NULL, NULL, 3
    UNION ALL SELECT 69902, 83, N'2.', N'Zmena základného imania +/- 419', N'Change in share capital +/- 419', 0, NULL, NULL, 4
    UNION ALL SELECT 69902, 84, N'3.', N'Pohľadávky za upísané vlastné imanie (/-/353)', N'Unpaid share capital (/-/353)', 0, NULL, NULL, 5
    UNION ALL SELECT 69902, 85, N'A.II.', N'Emisné ážio (412)', N'Share premium (412)', 0, NULL, NULL, 6
    UNION ALL SELECT 69902, 86, N'A.III.', N'Ostatné kapitálové fondy (413)', N'Other capital funds (413)', 0, NULL, NULL, 7
    UNION ALL SELECT 69902, 87, N'A.IV.', N'Zákonné rezervné fondy r. 88 + r. 89', N'Legal reserve funds line 88 + line 89', 1, NULL, NULL, 8
    UNION ALL SELECT 69902, 88, N'A.IV.1.', N'Zákonný rezervný fond a nedeliteľný fond (417A, 418, 421A, 422)', N'Legal reserve fund and non-distributable fund (417A, 418, 421A, 422)', 0, NULL, NULL, 9
    UNION ALL SELECT 69902, 89, N'2.', N'Rezervný fond na vlastné akcie a vlastné podiely (417A, 421A)', N'Reserve fund for own shares and own ownership interests (417A, 421A)', 0, NULL, NULL, 10
    UNION ALL SELECT 69902, 90, N'A.V', N'Ostatné fondy zo zisku r. 91 + r. 92', N'Other funds created from profit line 91 + line 92', 1, NULL, NULL, 11
    UNION ALL SELECT 69902, 91, N'A.V.1.', N'Štatutárne fondy (423, 42X)', N'Statutory funds (423, 42X)', 0, NULL, NULL, 12
    UNION ALL SELECT 69902, 141, N'C.', N'Časové rozlíšenie súčet (r. 142 až r. 145)', N'Accruals/deferrals - total (lines 142 to 145)', 1, NULL, NULL, 62
    UNION ALL SELECT 69902, 142, N'C.1.', N'Výdavky budúcich období dlhodobé (383A)', N'Accrued expenses - long-term (383A)', 0, NULL, NULL, 63
    UNION ALL SELECT 69902, 143, N'2.', N'Výdavky budúcich období kratkodobé (383A)', N'Accrued expenses - short-term (383A)', 0, NULL, NULL, 64
    UNION ALL SELECT 69902, 144, N'3.', N'Výnosy budúcich období dlhodobé (384A)', N'Deferred income - long-term (384A)', 0, NULL, NULL, 65
    UNION ALL SELECT 69902, 145, N'4.', N'Výnosy budúcich období krátkodobé (384A)', N'Deferred income - short-term (384A)', 0, NULL, NULL, 66
    UNION ALL SELECT 71603, 15, NULL, N'Majetok celkom r. 01 + r. 02 + r. 03 + r. 04 + r. 08 + r. 09 +/- r. 13 + r. 14', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 38504, 39, N'601', N'Tržby za vlastné výrobky', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 38504, 40, N'602', N'Tržby z predaja služieb', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 38504, 41, N'604', N'Tržby za predaný tovar', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 38504, 42, N'611', N'Zmena stavu zásob nedokončenej výroby', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 38504, 43, N'612', N'Zmena stavu zásob polotovarov', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 38504, 44, N'613', N'Zmena stavu zásob výrobkov', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 38504, 45, N'614', N'Zmena stavu zásob zvierat', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 38504, 46, N'621', N'Aktivácia materiálu a tovaru', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 38504, 47, N'622', N'Aktivácia vnútroorganizačných služieb', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 38504, 48, N'623', N'Aktivácia dlhodobého nehmotného majetku', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 38504, 49, N'624', N'Aktivácia dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 38504, 50, N'641', N'Zmluvné pokuty a penále', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 38504, 51, N'642', N'Ostatné pokuty a penále', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 38504, 52, N'643', N'Platby za odpísané pohľadávky', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 38504, 53, N'644', N'Úroky', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 38504, 54, N'645', N'Kurzové zisky', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 38504, 55, N'646', N'Prijaté dary', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 38504, 56, N'647', N'Osobitné výnosy', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 38504, 57, N'648', N'Zákonné poplatky', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 38504, 58, N'649', N'Iné ostatné výnosy', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 38504, 59, N'651', N'Tržby z predaja dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 94206, 1, NULL, N'Finančné aktíva s obvyklým termínom dodania (spotové operácie)', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 94206, 2, NULL, N'Hodnoty dané ako záruky', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 94206, 3, NULL, N'Hodnoty odovzdané do úschovy', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94206, 4, NULL, N'Hodnoty odovzdané do správy', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 94206, 5, NULL, N'Hodnoty odovzdané na uloženie', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 94206, 6, NULL, N'Hodnoty prijaté ako záruky', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 94206, 7, NULL, N'Ostatné', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 100102, 56, N'A.', N'Vlastné imanie', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 100102, 57, N'I.', N'Základné imanie, z toho', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 100102, 58, N'1.', N'upísané základné imanie splatené', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 100102, 59, N'II.', N'Emisné ážio', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 100102, 60, N'III.', N'Oceňovacie rozdiely z ocenenia majetku a záväzkov', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 100102, 61, N'IV.', N'Rezervné fondy a ostatné fondy tvorené zo zisku', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 100102, 62, N'1.', N'Ostatné kapitálové fondy', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 100102, 63, N'2.', N'Rezervný fond na vlastné akcie', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 100102, 64, N'V.', N'Výsledok hospodárenia minulých rokov', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 100102, 65, N'VI.', N'Výsledok hospodárenia bežného účtovného obdobia', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 100102, 66, N'B.', N'Podriadené pasíva', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 100102, 67, N'C.', N'Technické rezervy', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 100102, 68, N'1.', N'Technická rezerva na poistné budúcich období', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 100102, 69, N'1a.', N'Hrubá výška', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 100102, 70, N'1b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 100102, 71, N'3.', N'Technická rezerva na poistné plnenie', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 100102, 72, N'3a.', N'Hrubá výška', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 100102, 73, N'3b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 100102, 74, N'4.', N'Technická rezerva na poistné prémie a zľavy', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 18
    UNION ALL SELECT 100102, 75, N'4a.', N'Hrubá výška', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 100102, 76, N'4b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 100102, 77, N'6.', N'Iné technické rezervy', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 100102, 78, N'6a.', N'Hrubá výška', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 100102, 79, N'6b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 100102, 80, N'E.', N'Ostatné rezervy', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 100102, 81, N'G.', N'Záväzky, z toho', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 100102, 82, N'I.', N'z verejného zdravotného poistenia, z toho', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 100102, 83, N'1.', N'voči poisteným, z toho', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 100102, 84, N'1a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 100102, 85, N'1b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 100102, 86, N'2.', N'voči poskytovateľom zdravotnej starostlivosti', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 100102, 87, N'2a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 100102, 88, N'2b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 100102, 89, N'3.', N'voči inej zdravotnej poisťovni, z toho', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 100102, 90, N'3a.', N'z prerozdelenia poistného', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 100102, 91, N'4.', N'voči Úradu pre dohľad nad zdravotnou starostlivosťou', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 100102, 92, N'5.', N'voči Ministerstvu zdravotníctva Slovenskej republiky', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 100102, 93, N'II.', N'pôžičky zaručené dlhopisom, z toho', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 100102, 94, N'1.', N'v konvertibilnej mene', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 100102, 95, N'2.', N'krátkodobé pôžičky', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 100102, 96, N'3.', N'dlhodobé pôžičky', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 100102, 97, N'III.', N'bankové úvery, z toho', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 100102, 98, N'1.', N'krátkodobé úvery', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 100102, 99, N'IV.', N'ostatné záväzky, z toho', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 100102, 100, N'1.', N'z daní', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 100102, 101, N'2.', N'záväzky voči zamestnancom celkom', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 100102, 102, N'2a.', N'z toho zo sociálneho poistenia a zdravotného poistenia', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 100102, 103, N'3.', N'z finančného prenájmu', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 100102, 104, N'4.', N'z dotácií zo štátneho rozpočtu a ostatné dotácie', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 100102, 105, N'H.', N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 100102, 106, NULL, N'PASÍVA spolu', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 100102, 999, NULL, N'Kontrolné číslo', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 69602, 128, N'692', N'Výnosy z kapitálových transferov z rozpočtu obce alebo z rozpočtu vyššieho územného celku v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom', N'Revenues from capital transfers from the budget of municipality or higher regional unit in state-funded and state-subsidized organisations founded by municipality or higher regional unit', 0, NULL, NULL, 62
    UNION ALL SELECT 69602, 129, N'693', N'Výnosy samosprávy z bežných transferov zo štátneho rozpočtu a od iných subjektov verejnej správy', N'Revenues of the local government from current transfers from the state budget and from other entities of general government', 0, NULL, NULL, 63
    UNION ALL SELECT 69602, 130, N'694', N'Výnosy samosprávy z kapitálových transferov zo štátneho rozpočtu a od iných subjektov verejnej správy', N'Revenues of the local government from capital transfers from the state budget and from other entities of general government', 0, NULL, NULL, 64
    UNION ALL SELECT 69602, 131, N'695', N'Výnosy samosprávy z bežných transferov od Európskej únie', N'Revenues of the local government from current transfers from the European Union', 0, NULL, NULL, 65
    UNION ALL SELECT 69602, 132, N'696', N'Výnosy samosprávy z kapitálových transferov od Európskej únie', N'Revenues of the local government from capital transfers from the European Union', 0, NULL, NULL, 66
    UNION ALL SELECT 69602, 133, N'697', N'Výnosy samosprávy z bežných transferov od ostatných subjektov mimo verejnej správy', N'Revenues of the local government from current transfers from other entities outside of general government', 0, NULL, NULL, 67
    UNION ALL SELECT 94208, 1, NULL, N'Technické výnosy spolu', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 94208, 2, NULL, N'Čisté zaslúžené poistné', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 94208, 3, NULL, N'Predpísané poistné v hrubej výške', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94208, 4, NULL, N'Podiel zaisťovateľa na predpísanom poistnom', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 94208, 5, NULL, N'Zmena stavu rezervy na poistné budúcich období v hrubej výške', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 94208, 6, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na poistné budúcich období', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 94208, 7, NULL, N'Ostatné technické výnosy', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 94208, 8, NULL, N'z toho: provízie od zaisťovateľov', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 94208, 9, NULL, N'provízie zo spolupoistenia', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 94208, 10, NULL, N'poplatky', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 94208, 11, NULL, N'Technické náklady spolu', NULL, 0, NULL, NULL, 10
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 94208 AS [TableErpId], 12 AS [RowNumber], NULL AS [Designation], N'Náklady na poistné plnenia' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 11 AS [RowOrdinal]
    UNION ALL SELECT 94208, 13, NULL, N'Náklady na poistné plnenia v hrubej výške', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 94208, 14, NULL, N'Podiel zaisťovateľa na nákladoch na poistné plnenia', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 94208, 15, NULL, N'Zmena stavu rezervy na poistné plnenie v hrubej výške', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 94208, 16, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na poistné plnenie', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 94208, 17, NULL, N'Zmena stavu ostatných rezerv', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 94208, 18, NULL, N'Zmena stavu rezervy na životné poistenie v hrubej výške', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 94208, 19, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na životné poistenie v hrubej výške', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 94208, 20, NULL, N'Zmena stavu rezervy na poistné prémie a zľavy v hrubej výške', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 94208, 21, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na poistné prémie a zľavy v hrubej výške', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 94208, 22, NULL, N'Zmena stavu rezervy na úhradu záväzkov voči SKP vznikajúcich z činností podľa osobitného predpisu', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 94208, 23, NULL, N'Zmena stavu ďalších rezerv v hrubej výške', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 94208, 24, NULL, N'Podiel zaisťovateľa na zmene stavu ďalších rezerv', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 94208, 25, NULL, N'Zmena stavu rezervy na krytie rizika z investovania finančných prostriedkov v mene poistených', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 94208, 26, NULL, N'Prevádzkové náklady', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 94208, 27, NULL, N'Obstarávacie náklady na poistné zmluvy', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 94208, 28, NULL, N'z toho: provízie', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 94208, 29, NULL, N'marketing', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 94208, 30, NULL, N'Správna réžia', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 94208, 31, NULL, N'z toho: provízie', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 94208, 32, NULL, N'odpisy', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 94208, 33, NULL, N'Ostatné technické náklady', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 94208, 34, NULL, N'z toho: príspevky SKP', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 94208, 35, NULL, N'príspevky MV SR', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 94208, 36, NULL, N'Technický výsledok', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 94208, 37, NULL, N'Finančné výnosy spolu', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 94208, 38, NULL, N'Výnosy z finančného majetku a investičného majetku, ktoré kryjú technické rezervy', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 94208, 39, NULL, N'Kde riziko z investovaných prostriedkov nesie poisťovňa', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 94208, 40, NULL, N'Kde riziko z investovaných prostriedkov nesie klient', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 94208, 41, NULL, N'Výnosy z finančného majetku a investičného majetku, ktoré nekryjú technické rezervy', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 94208, 42, NULL, N'Ostatné finančné výnosy', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 94208, 43, NULL, N'Finančné náklady spolu', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 94208, 44, NULL, N'Náklady na finančný majetok a investičný majetok, ktorý kryje technické rezervy', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 94208, 45, NULL, N'Kde riziko z investovaných prostriedkov nesie poisťovňa', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 94208, 46, NULL, N'Kde riziko z investovaných prostriedkov nesie klient', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 518201, 53, N'P.K.', N'Základné imanie', N'Issued capital', 0, NULL, NULL, 52
    UNION ALL SELECT 518201, 54, N'P.K.1.', N'z toho: upísané základné imanie splatené', N'of which: paid-up subscribed capital', 0, NULL, NULL, 53
    UNION ALL SELECT 518201, 55, N'P.L.', N'Vlastné akcie', N'Own shares', 0, NULL, NULL, 54
    UNION ALL SELECT 518201, 56, N'P.M.', N'Emisné ážio', N'Share premium', 0, NULL, NULL, 55
    UNION ALL SELECT 518201, 57, N'P.N.', N'Rezervné fondy a fondy tvorené zo zisku', N'Capital reserve', 0, NULL, NULL, 56
    UNION ALL SELECT 518201, 58, N'P.O.', N'Ostatné kapitálové fondy', N'Other capital funds', 0, NULL, NULL, 57
    UNION ALL SELECT 518201, 59, N'P.P.', N'Oceňovacie rozdiely, z toho:', N'Valuation differences, of which:', 0, NULL, NULL, 58
    UNION ALL SELECT 518201, 60, N'P.P.1.', N'Úpravy vyplývajúce z prepočtu cudzích mien', N'Foreign currency translation adjustments', 0, NULL, NULL, 59
    UNION ALL SELECT 518201, 61, N'P.P.2.', N'Oceňovacie rozdiely z ocenenia finančných nástrojov cez OCI', N'Unrealized gains and losses (OCI)', 0, NULL, NULL, 60
    UNION ALL SELECT 518201, 62, N'P.P.3.', N'Oceňovacie rozdiely z poistných zmlúv a zaistných zmlúv cez OCI', N'(Re)insurance assets and liabilities / (Re)insurance finance reserve', 0, NULL, NULL, 61
    UNION ALL SELECT 518201, 63, N'P.P.4.', N'Očakávané kreditné straty', N'Expected credit loss (ECL)', 0, NULL, NULL, 62
    UNION ALL SELECT 518201, 64, N'P.Q.', N'Výsledok hospodárenia minulých rokov', N'Profit or loss from previous periods', 0, NULL, NULL, 63
    UNION ALL SELECT 518201, 65, N'P.R.', N'Výsledok hospodárenia bežného obdobia', N'Profit or loss for the current period', 0, NULL, NULL, 64
    UNION ALL SELECT 518201, 66, NULL, N'VLASTNÉ IMANIE SPOLU', N'TOTAL EQUITY', 0, NULL, NULL, 65
    UNION ALL SELECT 518201, 67, NULL, N'PASÍVA SPOLU', N'TOTAL EQUITY AND LIABILITIES', 0, NULL, NULL, 66
    UNION ALL SELECT 69902, 117, N'12.', N'Odložený daňový záväzok (481A)', N'Deferred tax liability (481A)', 0, N'Oceňovacie rozdiely', NULL, 38
    UNION ALL SELECT 69902, 118, N'B.II.', N'Dlhodobé rezervy r. 119 + r. 120', N'Long-term provisions line 119 + line 120', 1, NULL, NULL, 39
    UNION ALL SELECT 69902, 119, N'B.II.1.', N'Zákonné rezervy (451A)', N'Legal provisions (451A)', 0, NULL, NULL, 40
    UNION ALL SELECT 69902, 120, N'2.', N'Ostatné rezervy (459A, 45X)', N'Other provisions (459A, 45XA)', 0, N'Fondy', NULL, 41
    UNION ALL SELECT 69902, 121, N'B.III', N'Dlhodobé bankové úvery (461A, 46XA)', N'Long-term bank loans (461A, 46XA)', 0, NULL, NULL, 42
    UNION ALL SELECT 69902, 122, N'B.IV.', N'Krátkodobé záväzky súčet (r. 123 + r. 127 až r. 135)', N'Current liabilities - total (line 123 + lines 127 to 135)', 1, NULL, NULL, 43
    UNION ALL SELECT 69902, 123, N'B.IV.1.', N'Záväzky z obchodného styku súčet (r. 124 až r. 126)', N'Trade liabilities - total (lines 124 to 126)', 1, N'Výsledok hospodárenia', NULL, 44
    UNION ALL SELECT 69902, 124, N'1.a.', N'Záväzky z obchodného styku voči prepojeným účtovným jednotkám (321A, 322A, 324A, 325A, 326A, 32XA, 475A, 476A, 478A, 47XA)', N'Trade liabilities to affiliated accounting entities (321A, 322A, 324A, 325A, 326A, 32XA, 475A, 476A, 478A, 47XA)', 0, NULL, NULL, 45
    UNION ALL SELECT 69902, 125, N'1.b.', N'Záväzky z obchodného styku v rámci podielovej účasti okrem záväzkov voči prepojeným účtovným jednotkám (321A, 322A, 324A, 325A, 326A, 32XA, 475A, 476A, 478A, 47XA)', N'Trade liabilities within participating interest, except for liabilities to affiliated accounting entities (321A, 322A, 324A, 325A, 326A, 32XA, 475A, 476A, 478A, 47XA)', 0, NULL, NULL, 46
    UNION ALL SELECT 69902, 126, N'1.c.', N'Ostatné záväzky z obchodného styku (321A, 322A, 324A, 325A, 326A, 32XA, 475A, 476A, 478A, 47XA)', N'Other trade liabilities (321A, 322A, 324A, 325A, 326A, 32XA, 475A, 476A, 478A, 47XA)', 0, N'Záväzky', NULL, 47
    UNION ALL SELECT 69902, 127, N'2.', N'Čistá hodnota zákazky (316A)', N'Net value of contract (316A)', 0, N'Rezervy', NULL, 48
    UNION ALL SELECT 69902, 128, N'3.', N'Ostatné záväzky voči prepojeným účtovným jednotkám (361A, 36XA, 471A, 47XA)', N'Other liabilities to affiliated accounting entities (361A, 36XA, 471A, 47XA)', 0, NULL, NULL, 49
    UNION ALL SELECT 69902, 129, N'4.', N'Ostatné záväzky v rámci podielovej účasti okrem záväzkov voči prepojeným účtovným jednotkám (361A, 36XA, 471A, 47XA)', N'Other liabilities within participating interest, except for liabilities to affiliated accounting entities (361A, 36XA, 471A, 47XA)', 0, NULL, NULL, 50
    UNION ALL SELECT 69902, 130, N'5.', N'Záväzky voči spoločníkom a združeniu (364, 365, 366, 367, 368, 398A, 478A, 479A)', N'Liabilities to partners and association (364, 365, 366, 367, 368, 398A, 478A, 479A)', 0, NULL, NULL, 51
    UNION ALL SELECT 69902, 131, N'6.', N'Záväzky voči zamestnancom (331, 333, 33X, 479A)', N'Liabilities to employees (331, 333, 33X, 479A)', 0, NULL, NULL, 52
    UNION ALL SELECT 69902, 132, N'7.', N'Záväzky zo sociálneho poistenia (336A)', N'Liabilities related to social security (336A)', 0, N'Zúčtovanie medzi subjektami verejnej správy', NULL, 53
    UNION ALL SELECT 69902, 133, N'8.', N'Daňové záväzky a dotácie (341, 342, 343, 345, 346, 347, 34X)', N'Tax liabilities and subsidies (341, 342, 343, 345, 346, 347, 34X)', 0, NULL, NULL, 54
    UNION ALL SELECT 69902, 134, N'9.', N'Záväzky z derivátových operácií (373A, 377A)', N'Liabilities related to derivative transactions (373A, 377A)', 0, NULL, NULL, 55
    UNION ALL SELECT 69902, 135, N'10.', N'Iné záväzky (372A, 379A, 474A, 475A, 479A, 47XA)', N'Other liabilities (372A, 379A, 474A, 475A, 479A, 47XA)', 0, NULL, NULL, 56
    UNION ALL SELECT 69902, 136, N'B.V.', N'Krátkodobé rezervy r. 137 + r. 138', N'Short-term provisions line 137 + line 138', 1, NULL, NULL, 57
    UNION ALL SELECT 69902, 137, N'B.V.1.', N'Zákonné rezervy (323A, 451A)', N'Legal provisions (323A, 451A)', 0, NULL, NULL, 58
    UNION ALL SELECT 69902, 138, N'2.', N'Ostatné rezervy (323A, 32X, 459A, 45XA)', N'Other provisions (323A, 32X, 459A, 45XA)', 0, NULL, NULL, 59
    UNION ALL SELECT 69902, 139, N'B.VI.', N'Bežné bankové úvery (221A, 231, 232, 23X, 461A, 46XA)', N'Current bank loans (221A, 231, 232, 23X, 461A, 46XA)', 0, NULL, NULL, 60
    UNION ALL SELECT 69902, 140, N'B.VII.', N'Krátkodobé finančné výpomoci (241, 249, 24X, 473A, /-/255A)', N'Short-term financial assistance (241, 249, 24X, 473A /-/255A)', 0, N'Dlhodobé záväzky', NULL, 61
    UNION ALL SELECT 71601, 1, NULL, N'Predaj tovaru', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 71601, 2, NULL, N'Predaj výrobkov a služieb', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 71601, 3, NULL, N'Ostatné príjmy', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 71601, 4, NULL, N'Príjmy celkom súčet (r. 01 až 03)', NULL, 1, NULL, NULL, 3
    UNION ALL SELECT 71603, 1, NULL, N'Dlhodobý nehmotný majetok', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 71603, 2, NULL, N'Dlhodobý hmotný majetok', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 71603, 3, NULL, N'Dlhodobý finančný majetok', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 71603, 4, NULL, N'Zásoby celkom súčet (r. 05 až 07)', NULL, 1, NULL, NULL, 3
    UNION ALL SELECT 71603, 5, NULL, N'Materiál', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 71603, 6, NULL, N'Tovar', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 71603, 7, NULL, N'Nedokončená výroba, výrobky, zvieratá, ostatné', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 71603, 8, NULL, N'Pohľadávky', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 71603, 9, NULL, N'Krátkodobý finančný majetok súčet (r. 10 až 12)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 71603, 10, NULL, N'Peniaze a ceniny', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 71603, 11, NULL, N'Účty v bankách', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 71603, 12, NULL, N'Ostatný krátkodobý finančný majetok', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 71603, 13, NULL, N'Priebežné položky (+/-)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 71603, 14, NULL, N'Opravná položka k nadobudnutému majetku (aktívna)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 38301, 1, NULL, N'Z vkladu zriaďovateľa alebo zakladateľa', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 38301, 2, NULL, N'Z majetku', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 38301, 3, NULL, N'Z darov a príspevkov', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 38301, 4, NULL, N'Z členských príspevkov', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 38301, 5, NULL, N'Z podielu zaplatenej dane z príjmov', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 38301, 6, NULL, N'Z verejných zbierok', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 38301, 7, NULL, N'Z úverov a pôžičiek', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 38301, 8, NULL, N'Z dedičstva', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 38301, 9, NULL, N'Z organizovania podujatí', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 38301, 10, NULL, N'Z dotácií', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 38301, 11, NULL, N'Z likvidačného zostatku inej účtovnej jednotky', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 38301, 12, NULL, N'Z predaja majetku', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 38301, 13, NULL, N'Z poskytovania služieb a predaja vlastných výrobkov', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 38301, 14, NULL, N'Fond prevádzky, údržby a opráv', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 38301, 15, NULL, N'Ostatné', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 38301, 16, NULL, N'Príjmy celkom (súčet r. 01 až r. 15)', NULL, 1, NULL, NULL, 15
    UNION ALL SELECT 38504, 60, N'652', N'Výnosy z dlhodobého finančného majetku', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 38504, 61, N'653', N'Tržby z predaja cenných papierov a podielov', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 38504, 62, N'654', N'Tržby z predaja materiálu', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 38504, 63, N'655', N'Výnosy z krátkodobého finančného majetku', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 38504, 64, N'656', N'Výnosy z použitia fondu', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 38504, 65, N'657', N'Výnosy z precenenia cenných papierov', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 38504, 66, N'658', N'Výnosy z nájmu majetku', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 38504, 67, N'661', N'Prijaté príspevky od organizačných zložiek', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 38504, 68, N'662', N'Prijaté príspevky od iných organizácií', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 38504, 69, N'663', N'Prijaté príspevky od fyzických osôb', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 38504, 70, N'664', N'Prijaté členské príspevky', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 38504, 71, N'665', N'Príspevky z podielu zaplatenej dane', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 38504, 72, N'667', N'Prijaté príspevky z verejných zbierok', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 38504, 73, N'691', N'Dotácie', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 38504, 74, NULL, N'Účtová trieda 6 spolu r. 39 až r. 73', NULL, 1, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 35
    UNION ALL SELECT 38504, 75, NULL, N'Výsledok hospodárenia pred zdanením r. 74 - r. 38', NULL, 1, NULL, NULL, 36
    UNION ALL SELECT 38504, 76, N'591', N'Daň z príjmov', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 38504, 77, N'595', N'Dodatočné odvody dane z príjmov', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 38504, 78, NULL, N'Výsledok hospodárenia po zdanení (r. 75 - (r. 76 + r. 77)) (+/-)', NULL, 1, NULL, NULL, 39
    UNION ALL SELECT 94208, 47, NULL, N'Náklady na finančný majetok a investičný majetok, ktorý nekryje technické rezervy', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 94208, 48, NULL, N'Ostatné finančné náklady', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 94208, 49, NULL, N'Finančný výsledok', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 901, 19, N'7.', N'Pestovateľské celky trvalých porastov (025) - (085+092AÚ)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 901, 20, N'8.', N'Základné stádo a ťažné zvieratá (026) - (086+092AÚ)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 901, 21, N'9.', N'Drobný dlhodobý hmotný majetok (028) - (088+092AÚ)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 901, 22, N'10.', N'Ostatný dlhodobý hmotný majetok (029) - (089+092AÚ)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 901, 23, N'11.', N'Obstaranie dlhodobého hmotného majetku (042) - (094)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 901, 24, N'12.', N'Poskytnuté preddavky na dlhodobý hmotný majetok (052) - (095AÚ)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 901, 25, N'A.III.', N'Dlhodobý finančný majetok súčet (r. 026 + r. 27 + r. 29 až 034)', NULL, 1, NULL, NULL, 24
    UNION ALL SELECT 901, 26, N'A.III.1.', N'Podielové cenné papiere a podiely v dcérskej účtovnej jednotke (061) - (096AÚ)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 901, 27, N'2.', N'Podielové cenné papiere a podiely v spoločnosti s podstatným vplyvom (062) - (096AÚ)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 901, 28, NULL, N'z toho: goodwill', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 901, 29, N'3.', N'Realizovateľné cenné papiere a podiely (063) - (096AÚ)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 901, 30, N'4.', N'Dlhové cenné papiere držané do splatnosti (065) - (096AÚ)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 901, 31, N'5.', N'Pôžičky účtovnej jednotke v konsolidovanom celku (066) - (096AÚ)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 901, 32, N'6.', N'Ostatné pôžičky (067) - (096AÚ)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 901, 33, N'7.', N'Ostatný dlhodobý finančný majetok (069) - (096AÚ)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 901, 34, N'8.', N'Obstaranie dlhodobého finančného majetku (043) - (096AÚ)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 901, 35, N'B.', N'Obežný majetok r. 036 + r. 042 + r. 050 + r. 063 + r. 088+ r. 101 + r. 107', NULL, 1, NULL, NULL, 34
    UNION ALL SELECT 901, 36, N'B.I.', N'Zásoby súčet (r. 037 až 041)', NULL, 1, NULL, NULL, 35
    UNION ALL SELECT 901, 37, N'B.I.1.', N'Materiál (112 + 119) - (191)', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 901, 38, N'2.', N'Nedokončená výroba a polotovary (121 + 122) - (192 + 193)', NULL, 0, NULL, NULL, 37
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 901 AS [TableErpId], 39 AS [RowNumber], N'3.' AS [Designation], N'Výrobky (123) - (194)' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 38 AS [RowOrdinal]
    UNION ALL SELECT 901, 40, N'4.', N'Zvieratá (124) - (195)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 901, 41, N'5.', N'Tovar (132 + 139) - (196)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 901, 42, N'B.II.', N'Zúčtovanie medzi subjektami verejnej správy súčet (r. 043 až r. 049)', NULL, 1, NULL, NULL, 41
    UNION ALL SELECT 901, 43, N'B.II.1.', N'Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa (351)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 901, 44, N'2.', N'Zúčtovanie transferov štátneho rozpočtu (353)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 901, 45, N'3.', N'Zúčtovanie transferov rozpočtu obce a vyššieho územného celku (355)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 901, 46, N'4.', N'Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku (356)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 901, 47, N'5.', N'Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku (357)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 901, 48, N'6.', N'Zúčtovanie transferov zo štátneho rozpočtu iným subjektom (358)', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 901, 49, N'7.', N'Zúčtovanie transferov medzi subjektami verejnej správy (359)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 901, 50, N'B.', N'III Dlhodobé pohľadávky súčet (r. 051 až 061)', NULL, 1, NULL, NULL, 49
    UNION ALL SELECT 901, 51, N'B.III.1.', N'Odberatelia (311AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 901, 52, N'2.', N'Zmenky na inkaso (312AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 901, 53, N'3.', N'Pohľadávky za eskontované cenné papiere (313AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 901, 54, N'4.', N'Ostatné pohľadávky (315AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 901, 55, N'5.', N'Pohľadávky voči zamestnancom (335AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 901, 56, N'6.', N'Pohľadávky voči združeniu (369AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 901, 57, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 901, 58, N'8.', N'Pohľadávky z nájmu (374AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 901, 59, N'9.', N'Pohľadávky z vydaných dlhopisov (375AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 901, 60, N'10.', N'Nakúpené opcie (376AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 901, 61, N'11.', N'Iné pohľadávky (378AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 901, 62, NULL, N'z toho: odložená daňová pohľadávka', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 901, 63, N'B.IV.', N'Krátkodobé pohľadávky súčet (r. 064 až 087)', NULL, 1, NULL, NULL, 62
    UNION ALL SELECT 901, 64, N'B.IV.1.', N'Odberatelia (311AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 901, 65, N'2.', N'Zmenky na inkaso (312AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 64
    UNION ALL SELECT 901, 66, N'3.', N'Pohľadávky za eskontované cenné papiere (313AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 65
    UNION ALL SELECT 901, 67, N'4.', N'Poskytnuté prevádzkové preddavky (314) - (391AÚ)', NULL, 0, NULL, NULL, 66
    UNION ALL SELECT 901, 68, N'5.', N'Ostatné pohľadávky (315AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 67
    UNION ALL SELECT 901, 69, N'6.', N'Pohľadávky z nedaňových rozpočtových príjmov (316) - (391AÚ)', NULL, 0, NULL, NULL, 68
    UNION ALL SELECT 901, 70, N'7.', N'Pohľadávky z daňových a colných rozpočtových príjmov (317) - (391AÚ)', NULL, 0, NULL, NULL, 69
    UNION ALL SELECT 901, 71, N'8.', N'Pohľadávky z nedaňových príjmov obcí a vyšších územných celkov a rozpočtových organizácií zriadených obcou a vyšším územným celkom (318) - (391AÚ)', NULL, 0, NULL, NULL, 70
    UNION ALL SELECT 901, 72, N'9.', N'Pohľadávky z daňových príjmov obcí a vyšších územných celkov (319) - (391AÚ)', NULL, 0, NULL, NULL, 71
    UNION ALL SELECT 901, 73, N'10.', N'Pohľadávky voči zamestnancom (335AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 72
    UNION ALL SELECT 901, 74, N'11.', N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia (336) - (391AÚ)', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 73
    UNION ALL SELECT 901, 75, N'12.', N'Daň z príjmov (341) - (391AÚ)', NULL, 0, NULL, NULL, 74
    UNION ALL SELECT 901, 76, N'13.', N'Ostatné priame dane (342) - (391AÚ)', NULL, 0, NULL, NULL, 75
    UNION ALL SELECT 901, 77, N'14.', N'Daň z pridanej hodnoty (343) - (391AÚ)', NULL, 0, NULL, NULL, 76
    UNION ALL SELECT 901, 78, N'15.', N'Ostatné dane a poplatky (345) - (391AÚ)', NULL, 0, NULL, NULL, 77
    UNION ALL SELECT 901, 79, N'16.', N'Pohľadávky voči združeniu (369AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 78
    UNION ALL SELECT 901, 80, N'17.', N'Pohľadávky a záväzky z pevných termínovaných operácií (373AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 79
    UNION ALL SELECT 901, 81, N'18.', N'Pohľadávky z nájmu (374AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 80
    UNION ALL SELECT 901, 82, N'19.', N'Pohľadávky z vydaných dlhopisov (375AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 81
    UNION ALL SELECT 901, 83, N'20.', N'Nakúpené opcie (376AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 82
    UNION ALL SELECT 901, 84, N'21.', N'Iné pohľadávky (378AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 83
    UNION ALL SELECT 901, 85, N'22.', N'Spojovací účet pri združení (396AÚ)', NULL, 0, NULL, NULL, 84
    UNION ALL SELECT 901, 86, N'23.', N'Zúčtovanie s Európskymi spoločenstvami (371AÚ)- (391AÚ)', NULL, 0, NULL, NULL, 85
    UNION ALL SELECT 901, 87, N'24.', N'Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy (372AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 86
    UNION ALL SELECT 901, 88, N'B.V.', N'Finančné účty súčet (r. 089 až 100)', NULL, 1, NULL, NULL, 87
    UNION ALL SELECT 901, 89, N'B.V.1.', N'Pokladnica (211)', NULL, 0, NULL, NULL, 88
    UNION ALL SELECT 901, 90, N'2.', N'Ceniny (213)', NULL, 0, NULL, NULL, 89
    UNION ALL SELECT 901, 91, N'3.', N'Bankové účty (221AÚ +/- 261)', NULL, 0, NULL, NULL, 90
    UNION ALL SELECT 901, 1, NULL, N'SPOLU MAJETOK r. 002 + r. 035 + r. 113 + r. 117', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 901, 2, N'A.', N'Neobežný majetok r. 003 + r. 012 + r. 025', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 901, 3, N'A.I.', N'Dlhodobý nehmotný majetok súčet (r. 004 až 011)', NULL, 1, NULL, NULL, 2
    UNION ALL SELECT 901, 4, N'A.I.1.', N'Aktivované náklady na vývoj (012) - (072+091AÚ)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 901, 5, N'2.', N'Softvér (013) - (073+091AÚ)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 901, 6, N'3.', N'Oceniteľné práva (014) - (074+091AÚ)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 901, 7, N'4.', N'Goodwill z konsolidácie kapitálu alebo negatívny goodwill z konsolidácie kapitálu (+/-)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 901, 8, N'5.', N'Drobný dlhodobý nehmotný majetok (018) - (078+091AÚ)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 901, 9, N'6.', N'Ostatný dlhodobý nehmotný majetok (019) - (079+091AÚ)', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 901, 10, N'7.', N'Obstaranie dlhodobého nehmotného majetku (041) - (093)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 901, 11, N'8.', N'Poskytnuté preddavky na dlhodobý nehmotný majetok (051) - (095AÚ)', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 901, 12, N'A.II.', N'Dlhodobý hmotný majetok súčet (r. 013 až 024)', NULL, 1, NULL, NULL, 11
    UNION ALL SELECT 901, 13, N'A.II.1.', N'Pozemky (031) - (092AÚ)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 901, 14, N'2.', N'Umelecké diela a zbierky (032) - (092AÚ)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 901, 15, N'3.', N'Predmety z drahých kovov (033) - (092AÚ)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 901, 16, N'4.', N'Stavby (021) - (081+092AÚ)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 901, 17, N'5.', N'Samostatné hnuteľné veci a súbory hnuteľných vecí (022) - (082+092AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 901, 18, N'6.', N'Dopravné prostriedky (023) - (083+092AÚ)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 901, 105, N'4.', N'Poskytnuté návratné finančné výpomoci ostatným organizáciám (275AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 104
    UNION ALL SELECT 901, 106, N'5.', N'Poskytnuté návratné finančné výpomoci fyzickým osobám (277AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 105
    UNION ALL SELECT 901, 107, N'B.VII.', N'Poskytnuté návratné finančné výpomoci krátkodobé súčet (r. 108 až r. 112)', NULL, 1, NULL, NULL, 106
    UNION ALL SELECT 901, 108, N'B.VII.1.', N'Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku (271AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 107
    UNION ALL SELECT 901, 109, N'2.', N'Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy (272AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 108
    UNION ALL SELECT 901, 110, N'3.', N'Poskytnuté návratné finančné výpomoci podnikateľským subjektom (274AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 109
    UNION ALL SELECT 901, 111, N'4.', N'Poskytnuté návratné finančné výpomoci ostatným organizáciám (275AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 110
    UNION ALL SELECT 901, 112, N'5.', N'Poskytnuté návratné finančné výpomoci fyzickým osobám (277AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 111
    UNION ALL SELECT 901, 113, N'C.', N'Časové rozlíšenie súčet (r. 114 až r. 116)', NULL, 1, NULL, NULL, 112
    UNION ALL SELECT 901, 114, N'C.', N'1. Náklady budúcich období (381)', NULL, 0, NULL, NULL, 113
    UNION ALL SELECT 901, 115, N'2.', N'Komplexné náklady budúcich období (382)', NULL, 0, NULL, NULL, 114
    UNION ALL SELECT 901, 116, N'3.', N'Príjmy budúcich období (385)', NULL, 0, NULL, NULL, 115
    UNION ALL SELECT 901, 117, N'D.', N'Vzťahy k účtom klientov štátnej pokladnice (účtová skupina 20)', NULL, 0, NULL, NULL, 116
    UNION ALL SELECT 901, 888, NULL, N'KONTROLNÉ ČÍSLO súčet (r. 001 až r. 117)', NULL, 1, NULL, NULL, 117
    UNION ALL SELECT 1001, 1, N'50', N'Spotrebované nákupy (r. 002 až r. 005)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 1001, 2, N'501', N'Spotreba materiálu', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 1001, 3, N'502', N'Spotreba energie', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1001, 4, N'503', N'Spotreba ostatných neskladovateľných dodávok', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1001, 5, N'504', N'Predaný tovar', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1001, 6, N'51', N'Služby (r. 007 až r. 010)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 1001, 7, N'511', N'Opravy a udržiavanie', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1001, 8, N'512', N'Cestovné', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1001, 9, N'513', N'Náklady na reprezentáciu', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 1001, 10, N'518', N'Ostatné služby', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 1001, 11, N'52', N'Osobné náklady (r. 012 až r. 016)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 1001, 12, N'521', N'Mzdové náklady', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 1001, 13, N'524', N'Zákonné sociálne poistenie', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 1001, 14, N'525', N'Ostatné sociálne poistenie', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 1001, 15, N'527', N'Zákonné sociálne náklady', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 1001, 16, N'528', N'Ostatné sociálne náklady', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 1001, 17, N'53', N'Dane a poplatky (r. 018 až r. 020)', NULL, 1, NULL, NULL, 16
    UNION ALL SELECT 1001, 18, N'531', N'Daň z motorových vozidiel', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 1001, 19, N'532', N'Daň z nehnuteľnosti', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 1001, 20, N'538', N'Ostatné dane a poplatky', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 1001, 21, N'54', N'Ostatné náklady na prevádzkovú činnosť (r. 022 až r. 028)', NULL, 1, NULL, NULL, 20
    UNION ALL SELECT 1001, 22, N'541', N'Zostatková cena predaného dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 1001, 23, N'542', N'Predaný materiál', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 1001, 24, N'544', N'Zmluvné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 1001, 25, N'545', N'Ostatné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 1001, 26, N'546', N'Odpis pohľadávky', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 1001, 27, N'548', N'Ostatné náklady na prevádzkovú činnosť', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 1001, 28, N'549', N'Manká a škody', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 1001, 29, N'55', N'Odpisy, rezervy a opravné položky z prevádzkovej činnosti a finančnej činnosti a zúčtovanie časového rozlíšenia (r. 030 + r. 031 + r. 036 + r. 039)', NULL, 1, NULL, NULL, 28
    UNION ALL SELECT 1001, 30, N'551', N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 1001, 31, NULL, N'Rezervy a opravné položky z prevádzkovej činnosti (r. 032 až r. 035)', NULL, 1, NULL, NULL, 30
    UNION ALL SELECT 1001, 32, N'552', N'Tvorba zákonných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 1001, 33, N'553', N'Tvorba ostatných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 1001, 34, N'557', N'Tvorba zákonných opravných položiek z prevádzkovej činnosti', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 1001, 35, N'558', N'Tvorba ostatných opravných položiek z prevádzkovej činnosti', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 1001, 36, NULL, N'Rezervy a opravné položky z finančnej činnosti (r. 037+ r. 038)', NULL, 1, NULL, NULL, 35
    UNION ALL SELECT 1001, 37, N'554', N'Tvorba rezerv z finančnej činnosti', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 1001, 38, N'559', N'Tvorba opravných položiek z finančnej činnosti', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 1001, 39, N'555', N'Zúčtovanie komplexných nákladov budúcich období', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 1001, 40, N'56', N'Finančné náklady (r. 041 až r. 049)', NULL, 1, NULL, NULL, 39
    UNION ALL SELECT 1001, 41, N'561', N'Predané cenné papiere a podiely', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 1001, 42, N'562', N'Úroky', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 1001, 43, N'563', N'Kurzové straty', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 1001, 44, N'564', N'Náklady na precenenie cenných papierov', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 1001, 45, N'566', N'Náklady na krátkodobý finančný majetok', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 901, 92, N'4.', N'Účty v bankách s dobou viazanosti dlhšou ako jeden rok (221AÚ)', NULL, 0, NULL, NULL, 91
    UNION ALL SELECT 901, 93, N'5.', N'Výdavkový rozpočtový účet (222)', NULL, 0, NULL, NULL, 92
    UNION ALL SELECT 901, 94, N'6.', N'Príjmový rozpočtový účet (223)', NULL, 0, NULL, NULL, 93
    UNION ALL SELECT 901, 95, N'7.', N'Majetkové cenné papiere na obchodovanie (251) - (291AÚ)', NULL, 0, NULL, NULL, 94
    UNION ALL SELECT 901, 96, N'8.', N'Dlhové cenné papiere na obchodovanie (253) - (291AÚ)', NULL, 0, NULL, NULL, 95
    UNION ALL SELECT 901, 97, N'9.', N'Dlhové cenné papiere so splatnosťou do jedného roka držané do splatnosti (256) - (291AÚ)', NULL, 0, NULL, NULL, 96
    UNION ALL SELECT 901, 98, N'10', N'Ostatné realizovateľné cenné papiere (257) - (291AÚ)', NULL, 0, NULL, NULL, 97
    UNION ALL SELECT 901, 99, N'11.', N'Obstaranie krátkodobého finančného majetku (259) - (291AÚ)', NULL, 0, NULL, NULL, 98
    UNION ALL SELECT 901, 100, N'12.', N'Účty štátnej pokladnice (účtová skupina 28)', NULL, 0, NULL, NULL, 99
    UNION ALL SELECT 901, 101, N'B.VI.', N'Poskytnuté návratné finančné výpomoci dlhodobé súčet (r. 102 až r. 106)', NULL, 1, NULL, NULL, 100
    UNION ALL SELECT 901, 102, N'B.VI.', N'1 Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku (271AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 101
    UNION ALL SELECT 901, 103, N'2.', N'Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy (272AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 102
    UNION ALL SELECT 901, 104, N'3.', N'Poskytnuté návratné finančné výpomoci podnikateľským subjektom (274AÚ )- (291AÚ)', NULL, 0, NULL, NULL, 103
    UNION ALL SELECT 1001, 46, N'567', N'Náklady na derivátové operácie', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 1001, 47, N'568', N'Ostatné finančné náklady', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 1001, 48, N'569', N'Manká a škody na finančnom majetku', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 1001, 49, NULL, N'Podiel konsolidujúcej účtovnej jednotky na výsledku hospodárenia pridružených účtovných jednotiek verejnej správy', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 1001, 50, N'57', N'Mimoriadne náklady (r. 051 až r. 054)', NULL, 1, NULL, NULL, 49
    UNION ALL SELECT 1001, 51, N'572', N'Škody', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 1001, 52, N'574', N'Tvorba rezerv', NULL, 0, NULL, NULL, 51
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 1001 AS [TableErpId], 53 AS [RowNumber], N'578' AS [Designation], N'Ostatné mimoriadne náklady' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 52 AS [RowOrdinal]
    UNION ALL SELECT 1001, 54, N'579', N'Tvorba opravných položiek', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 1001, 55, N'58', N'Náklady na transfery a náklady z odvodu príjmov (r. 056 až r. 064)', NULL, 1, NULL, NULL, 54
    UNION ALL SELECT 1001, 56, N'581', N'Náklady na transfery zo štátneho rozpočtu do štátnych rozpočtových organizácií a príspevkových organizácií', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 1001, 57, N'582', N'Náklady na transfery zo štátneho rozpočtu ostatným subjektom verejnej správy', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 1001, 58, N'583', N'Náklady na transfery zo štátneho rozpočtu subjektom mimo verejnej správy', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 1001, 59, N'584', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku do rozpočtových organizácií a príspevkových organizácií zriadených obcou alebo vyšším územným celkom', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 1001, 60, N'585', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku ostatným subjektom verejnej správy', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 1001, 61, N'586', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku subjektom mimo verejnej správy', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 1001, 62, N'587', N'Náklady na ostatné transfery', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 1001, 63, N'588', N'Náklady z odvodu príjmov', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 1001, 64, N'589', N'Náklady z budúceho odvodu príjmov', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 1001, 65, NULL, N'Účtové skupiny 50 - 58 celkom súčet (r. 001 + r. 006 + r. 011 + r. 017 + r. 021 + r. 029 + r. 040 + r. 050 + r. 055)', NULL, 1, NULL, NULL, 64
    UNION ALL SELECT 1001, 994, NULL, N'Kontrolné číslo súčet (r. 001 až r. 065)', NULL, 1, NULL, NULL, 65
    UNION ALL SELECT 52201, 69, N'9.', N'Pohľadávky z daňových príjmov obcí a vyšších územných celkov (319) - (391AÚ)', NULL, 0, NULL, NULL, 68
    UNION ALL SELECT 52201, 70, N'10.', N'Pohľadávky voči zamestnancom (335AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 69
    UNION ALL SELECT 52201, 71, N'11.', N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia (336) - (391AÚ)', NULL, 0, NULL, NULL, 70
    UNION ALL SELECT 52201, 72, N'12.', N'Daň z príjmov (341) - (391AÚ)', NULL, 0, NULL, NULL, 71
    UNION ALL SELECT 52201, 73, N'13.', N'Ostatné priame dane (342) - (391AÚ)', NULL, 0, NULL, NULL, 72
    UNION ALL SELECT 52201, 74, N'14.', N'Daň z pridanej hodnoty (343) - (391AÚ)', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 73
    UNION ALL SELECT 52201, 75, N'15.', N'Ostatné dane a poplatky (345) - (391AÚ)', NULL, 0, NULL, NULL, 74
    UNION ALL SELECT 52201, 76, N'16.', N'Pohľadávky voči združeniu (369AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 75
    UNION ALL SELECT 52201, 77, N'17.', N'Pohľadávky a záväzky z pevných termínovaných operácií (373AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 76
    UNION ALL SELECT 52201, 78, N'18.', N'Pohľadávky z nájmu (374AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 77
    UNION ALL SELECT 52201, 79, N'19.', N'Pohľadávky z vydaných dlhopisov (375AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 78
    UNION ALL SELECT 52201, 80, N'20.', N'Nakúpené opcie (376AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 79
    UNION ALL SELECT 52201, 81, N'21.', N'Iné pohľadávky (378AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 80
    UNION ALL SELECT 52201, 82, N'22.', N'Spojovací účet pri združení (396AÚ)', NULL, 0, NULL, NULL, 81
    UNION ALL SELECT 52201, 83, N'23.', N'Zúčtovanie s Európskou úniou (371AÚ)- (391AÚ)', NULL, 0, NULL, NULL, 82
    UNION ALL SELECT 52201, 84, N'24.', N'Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy (372AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 83
    UNION ALL SELECT 52201, 85, N'B.V.', N'Finančné účty súčet (r. 086 až 097)', NULL, 1, NULL, NULL, 84
    UNION ALL SELECT 52201, 86, N'B.V.1.', N'Pokladnica (211)', NULL, 0, NULL, NULL, 85
    UNION ALL SELECT 52201, 87, N'2.', N'Ceniny (213)', NULL, 0, NULL, NULL, 86
    UNION ALL SELECT 52201, 88, N'3.', N'Bankové účty (221AÚ +/- 261)', NULL, 0, NULL, NULL, 87
    UNION ALL SELECT 52201, 89, N'4.', N'Účty v bankách s dobou viazanosti dlhšou ako jeden rok (221AÚ)', NULL, 0, NULL, NULL, 88
    UNION ALL SELECT 52201, 90, N'5.', N'Výdavkový rozpočtový účet (222)', NULL, 0, NULL, NULL, 89
    UNION ALL SELECT 52201, 91, N'6.', N'Príjmový rozpočtový účet (223)', NULL, 0, NULL, NULL, 90
    UNION ALL SELECT 52201, 92, N'7.', N'Majetkové cenné papiere na obchodovanie (251) - (291AÚ)', NULL, 0, NULL, NULL, 91
    UNION ALL SELECT 52201, 93, N'8.', N'Dlhové cenné papiere na obchodovanie (253) - (291AÚ)', NULL, 0, NULL, NULL, 92
    UNION ALL SELECT 52201, 94, N'9.', N'Dlhové cenné papiere so splatnosťou do jedného roka držané do splatnosti (256) - (291AÚ)', NULL, 0, NULL, NULL, 93
    UNION ALL SELECT 52201, 95, N'10.', N'Ostatné realizovateľné cenné papiere (257) - (291AÚ)', NULL, 0, NULL, NULL, 94
    UNION ALL SELECT 52201, 96, N'11.', N'Obstaranie krátkodobého finančného majetku (259) - (291AÚ)', NULL, 0, NULL, NULL, 95
    UNION ALL SELECT 52201, 97, N'12.', N'Účty štátnej pokladnice (účtová skupina 28)', NULL, 0, NULL, NULL, 96
    UNION ALL SELECT 52201, 98, N'B.VI.', N'Poskytnuté návratné finančné výpomoci dlhodobé súčet (r. 099 až r. 103)', NULL, 1, NULL, NULL, 97
    UNION ALL SELECT 52201, 99, N'B.VI.1.', N'Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku (271AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 98
    UNION ALL SELECT 52201, 100, N'2.', N'Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy (272AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 99
    UNION ALL SELECT 52201, 101, N'3.', N'Poskytnuté návratné finančné výpomoci podnikateľským subjektom (274AÚ)- (291AÚ)', NULL, 0, NULL, NULL, 100
    UNION ALL SELECT 52201, 102, N'4.', N'Poskytnuté návratné finančné výpomoci ostatným organizáciám (275AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 101
    UNION ALL SELECT 52201, 103, N'5.', N'Poskytnuté návratné finančné výpomoci fyzickým osobám (277AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 102
    UNION ALL SELECT 52201, 104, N'B.VII.', N'Poskytnuté návratné finančné výpomoci krátkodobé súčet (r. 105 až r. 109)', NULL, 1, NULL, NULL, 103
    UNION ALL SELECT 52201, 105, N'B.VII.1.', N'Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku (271AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 104
    UNION ALL SELECT 52201, 106, N'2.', N'Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy (272AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 105
    UNION ALL SELECT 52201, 107, N'3.', N'Poskytnuté návratné finančné výpomoci podnikateľským subjektom (274AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 106
    UNION ALL SELECT 52201, 108, N'4.', N'Poskytnuté návratné finančné výpomoci ostatným organizáciám (275AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 107
    UNION ALL SELECT 52201, 109, N'5.', N'Poskytnuté návratné finančné výpomoci fyzickým osobám (277AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 108
    UNION ALL SELECT 52201, 110, N'C.', N'Časové rozlíšenie súčet (r. 111 až r. 113)', NULL, 1, NULL, NULL, 109
    UNION ALL SELECT 52201, 111, N'C.1.', N'Náklady budúcich období (381)', NULL, 0, NULL, NULL, 110
    UNION ALL SELECT 52201, 112, N'2.', N'Komplexné náklady budúcich období (382)', NULL, 0, NULL, NULL, 111
    UNION ALL SELECT 52201, 113, N'3.', N'Príjmy budúcich období (385)', NULL, 0, NULL, NULL, 112
    UNION ALL SELECT 52201, 114, N'D.', N'Vzťahy k účtom klientov štátnej pokladnice (účtová skupina 20)', NULL, 0, NULL, NULL, 113
    UNION ALL SELECT 52201, 888, NULL, N'KONTROLNÉ ČÍSLO súčet (r. 001 až 114)', NULL, 1, NULL, NULL, 114
    UNION ALL SELECT 1702, 61, N'A.', N'VLASTNÉ ZDROJE KRYTIA MAJETKU SPOLU r. 062+ r. 068 + r. 072 + r. 073', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 1702, 62, N'1.', N'Imanie a peňažné fondy r. 063 až r. 067', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 1702, 63, NULL, N'Základné imanie (411)', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1702, 64, NULL, N'Peňažné fondy tvorené podľa osobitného predpisu (412)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1702, 65, NULL, N'Fond reprodukcie (413)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1702, 66, NULL, N'Oceňovacie rozdiely z precenenia majetku a záväzkov (414)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1702, 67, NULL, N'Oceňovacie rozdiely z precenenia kapitálových účastín (415)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1702, 68, N'2.', N'Fondy tvorené zo zisku r. 069 až r. 071', NULL, 1, NULL, NULL, 7
    UNION ALL SELECT 1702, 69, NULL, N'Rezervný fond (421)', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 1702, 70, NULL, N'Fondy tvorené zo zisku (423)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 1702, 71, NULL, N'Ostatné fondy (427)', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 1702, 72, N'3.', N'Nevysporiadaný výsledok hospodárenia minulých rokov (+; - 428)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 1702, 73, N'4.', N'Výsledok hospodárenia za účtovné obdobie r. 060 - (r. 062 + r. 068 + r. 072 + r. 074 + r. 101)', NULL, 1, NULL, NULL, 12
    UNION ALL SELECT 1702, 74, N'B.', N'CUDZIE ZDROJE SPOLU r. 075 + r. 079 + r. 087 + r. 097', NULL, 1, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 13
    UNION ALL SELECT 1702, 75, N'1.', N'Rezervy r. 076 až r. 078', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 1702, 76, NULL, N'Rezervy zákonné (451AÚ)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 1702, 77, NULL, N'Ostatné rezervy (459AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 1702, 78, NULL, N'Krátkodobé rezervy (323 + 451AÚ + 459AÚ)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 1702, 79, N'2.', N'Dlhodobé záväzky r. 080 až r. 086', NULL, 1, NULL, NULL, 18
    UNION ALL SELECT 1702, 80, NULL, N'Záväzky zo sociálneho fondu (472)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 1702, 81, NULL, N'Vydané dlhopisy (473)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 1702, 82, NULL, N'Záväzky z nájmu (474 AÚ)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 1702, 83, NULL, N'Dlhodobé prijaté preddavky (475)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 1702, 84, NULL, N'Dlhodobé nevyfakturované dodávky (476)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 1702, 85, NULL, N'Dlhodobé zmenky na úhradu (478)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 1702, 86, NULL, N'Ostatné dlhodobé záväzky (373 AÚ + 479 AÚ)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 1702, 87, N'3.', N'Krátkodobé záväzky r. 088 až r. 096', NULL, 1, NULL, NULL, 26
    UNION ALL SELECT 1702, 88, NULL, N'Záväzky z obchodného styku (321 až 326) okrem 323', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 1702, 89, NULL, N'Záväzky voči zamestnancom (331+ 333)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 1702, 90, NULL, N'Zúčtovanie so Sociálnou poisťovňou a zdravotnými poisťovňami (336)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 1702, 91, NULL, N'Daňové záväzky (341 až 345)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 1702, 92, NULL, N'Záväzky z dôvodu finančných vzťahov k štátnemu rozpočtu a rozpočtom územnej samosprávy (346+348)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 1702, 93, NULL, N'Záväzky z upísaných nesplatených cenných papierov a vkladov (367)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 1702, 94, NULL, N'Záväzky voči účastníkom združení (368)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 1702, 95, NULL, N'Spojovací účet pri združení (396)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 1702, 96, NULL, N'Ostatné záväzky (379 + 373 AÚ + 474 AÚ + 479 AÚ)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 1702, 97, N'4.', N'Bankové výpomoci a pôžičky r. 098 až r. 100', NULL, 1, NULL, NULL, 36
    UNION ALL SELECT 1702, 98, NULL, N'Dlhodobé bankové úvery (461AÚ)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 1702, 99, NULL, N'Bežné bankové úvery ( 231+ 232 + 461AÚ)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 1702, 100, NULL, N'Prijaté krátkodobé finančné výpomoci (241+ 249)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 1702, 101, N'C.', N'ČASOVÉ ROZLÍŠENIE SPOLU r. 102 a r. 103', NULL, 1, NULL, NULL, 40
    UNION ALL SELECT 1702, 102, N'1.', N'Výdavky budúcich období (383)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 1702, 103, NULL, N'Výnosy budúcich období (384)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 1702, 104, NULL, N'VLASTNÉ ZDROJE A CUDZIE ZDROJE SPOLU r.061+ r.074 + r.101', NULL, 1, NULL, NULL, 43
    UNION ALL SELECT 1702, 993, NULL, N'Kontrolné číslo r. 061 až r. 104', NULL, 1, NULL, NULL, 44
    UNION ALL SELECT 1802, 39, N'601', N'Tržby za vlastné výrobky', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 1802, 40, N'602', N'Tržby z predaja služieb', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 1802, 41, N'604', N'Tržby za predaný tovar', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1802, 42, N'611', N'Zmena stavu zásob nedokončenej výroby', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1802, 43, N'612', N'Zmena stavu zásob polotovarov', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1802, 44, N'613', N'Zmena stavu zásob výrobkov', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1802, 45, N'614', N'Zmena stavu zásob zvierat', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1802, 46, N'621', N'Aktivácia materiálu a tovaru', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1802, 47, N'622', N'Aktivácia vnútroorganizačných služieb', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 1802, 48, N'623', N'Aktivácia dlhodobého nehmotného majetku', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 1802, 49, N'624', N'Aktivácia dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 1802, 50, N'641', N'Zmluvné pokuty a penále', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 1802, 51, N'642', N'Ostatné pokuty a penále', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 1802, 52, N'643', N'Platby za odpísané pohľadávky', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 1802, 53, N'644', N'Úroky', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 1802, 54, N'645', N'Kurzové zisky', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 1802, 55, N'646', N'Prijaté dary', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 1802, 56, N'647', N'Osobitné výnosy', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 1802, 57, N'648', N'Zákonné poplatky', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 1802, 58, N'649', N'Iné ostatné výnosy', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 1802, 59, N'651', N'Tržby z predaja dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 1802, 60, N'652', N'Výnosy z dlhodobého finančného majetku', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 1802, 61, N'653', N'Tržby z predaja cenných papierov a podielov', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 52201, 1, NULL, N'SPOLU MAJETOK r. 002 + r. 033 + r. 110 + r. 114', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 52201, 2, N'A.', N'Neobežný majetok r. 003 + r. 011 + r. 024', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 52201, 3, N'A.I.', N'Dlhodobý nehmotný majetok súčet (r. 004 až 010)', NULL, 1, NULL, NULL, 2
    UNION ALL SELECT 52201, 4, N'A.I.1.', N'Aktivované náklady na vývoj (012) - (072+091AÚ)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 52201, 5, N'2.', N'Softvér (013) - (073+091AÚ)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 52201, 6, N'3.', N'Oceniteľné práva (014) - (074+091AÚ)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 52201, 7, N'4.', N'Drobný dlhodobý nehmotný majetok (018) - (078+091AÚ)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 52201, 8, N'5.', N'Ostatný dlhodobý nehmotný majetok (019) - (079+091AÚ)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 52201, 9, N'6.', N'Obstaranie dlhodobého nehmotného majetku (041) - (093)', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 52201, 10, N'7.', N'Poskytnuté preddavky na dlhodobý nehmotný majetok (051) - (095AÚ)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 52201, 11, N'A.II.', N'Dlhodobý hmotný majetok súčet (r. 012 až 023)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 52201, 12, N'A.II.1.', N'Pozemky (031) - (092AÚ)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 52201, 13, N'2.', N'Umelecké diela a zbierky (032) - (092AÚ)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 52201, 14, N'3.', N'Predmety z drahých kovov (033) - (092AÚ)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 52201, 15, N'4.', N'Stavby (021) - (081+092AÚ)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 52201, 16, N'5.', N'Samostatné hnuteľné veci a súbory hnuteľných vecí (022) - (082+092AÚ)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 52201, 17, N'6.', N'Dopravné prostriedky (023) - (083+092AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 52201, 18, N'7.', N'Pestovateľské celky trvalých porastov (025) - (085+092AÚ)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 52201, 19, N'8.', N'Základné stádo a ťažné zvieratá (026) - (086+092AÚ)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 52201, 20, N'9.', N'Drobný dlhodobý hmotný majetok (028) - (088+092AÚ)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 52201, 21, N'10.', N'Ostatný dlhodobý hmotný majetok (029) - (089+092AÚ)', NULL, 0, NULL, NULL, 20
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 52201 AS [TableErpId], 22 AS [RowNumber], N'11.' AS [Designation], N'Obstaranie dlhodobého hmotného majetku (042) - (094)' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 21 AS [RowOrdinal]
    UNION ALL SELECT 52201, 23, N'12.', N'Poskytnuté preddavky na dlhodobý hmotný majetok (052) - (095AÚ)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 52201, 24, N'A.III.', N'Dlhodobý finančný majetok súčet (r. 025 až 032)', NULL, 1, NULL, NULL, 23
    UNION ALL SELECT 52201, 25, N'A.III.1.', N'Podielové cenné papiere a podiely v dcérskej účtovnej jednotke (061) - (096AÚ)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 52201, 26, N'2.', N'Podielové cenné papiere a podiely v spoločnosti s podstatným vplyvom (062) - (096AÚ)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 52201, 27, N'3.', N'Realizovateľné cenné papiere a podiely (063) - (096AÚ)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 52201, 28, N'4.', N'Dlhové cenné papiere držané do splatnosti (065) - (096AÚ)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 52201, 29, N'5.', N'Pôžičky účtovnej jednotke v konsolidovanom celku (066) - (096AÚ)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 52201, 30, N'6.', N'Ostatné pôžičky (067) - (096AÚ)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 52201, 31, N'7.', N'Ostatný dlhodobý finančný majetok (069) - (096AÚ)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 52201, 32, N'8.', N'Obstaranie dlhodobého finančného majetku (043) - (096AÚ)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 52201, 33, N'B.', N'Obežný majetok r. 034 + r. 040 + r. 048+ r. 060 + r. 085+ r. 098 + r. 104', NULL, 1, NULL, NULL, 32
    UNION ALL SELECT 52201, 34, N'B.I.', N'Zásoby súčet (r. 035 až 039)', NULL, 1, NULL, NULL, 33
    UNION ALL SELECT 52201, 35, N'B.I.1.', N'Materiál (112 + 119) - (191)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 52201, 36, N'2.', N'Nedokončená výroba a polotovary (121 + 122) - (192 + 193)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 52201, 37, N'3.', N'Výrobky (123) - (194)', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 52201, 38, N'4.', N'Zvieratá (124) - (195)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 52201, 39, N'5.', N'Tovar (132 + 133 + 139) - (196)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 52201, 40, N'B.II.', N'Zúčtovanie medzi subjektami verejnej správy súčet (r. 041 až r. 047)', NULL, 1, NULL, NULL, 39
    UNION ALL SELECT 52201, 41, N'B.II.1.', N'Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa (351)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 52201, 42, N'2.', N'Zúčtovanie transferov štátneho rozpočtu (353)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 52201, 43, N'3.', N'Zúčtovanie transferov rozpočtu obce a vyššieho územného celku (355)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 52201, 44, N'4.', N'Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku (356)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 52201, 45, N'5.', N'Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku (357)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 52201, 46, N'6.', N'Zúčtovanie transferov zo štátneho rozpočtu iným subjektom (358)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 52201, 47, N'7.', N'Zúčtovanie transferov medzi subjektami verejnej správy (359)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 52201, 48, N'B.III', N'Dlhodobé pohľadávky súčet (r. 049 až 059)', NULL, 1, NULL, NULL, 47
    UNION ALL SELECT 52201, 49, N'B.III.1', N'Odberatelia (311AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 52201, 50, N'2.', N'Zmenky na inkaso (312AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 52201, 51, N'3.', N'Pohľadávky za eskontované cenné papiere (313AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 52201, 52, N'4.', N'Ostatné pohľadávky (315AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 52201, 53, N'5.', N'Pohľadávky voči zamestnancom (335AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 52201, 54, N'6.', N'Pohľadávky voči združeniu (369AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 52201, 55, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 52201, 56, N'8.', N'Pohľadávky z nájmu (374AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 52201, 57, N'9.', N'Pohľadávky z vydaných dlhopisov (375AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 52201, 58, N'10', N'Nakúpené opcie (376AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 52201, 59, N'11.', N'Iné pohľadávky (378AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 52201, 60, N'B.IV.', N'Krátkodobé pohľadávky súčet (r. 061 až 084)', NULL, 1, NULL, NULL, 59
    UNION ALL SELECT 52201, 61, N'B.IV.1', N'Odberatelia (311AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 52201, 62, N'2.', N'Zmenky na inkaso (312AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 52201, 63, N'3.', N'Pohľadávky za eskontované cenné papiere (313AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 52201, 64, N'4.', N'Poskytnuté prevádzkové preddavky (314) - (391AÚ)', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 52201, 65, N'5.', N'Ostatné pohľadávky (315AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 64
    UNION ALL SELECT 52201, 66, N'6.', N'Pohľadávky z nedaňových rozpočtových príjmov (316) - (391AÚ)', NULL, 0, NULL, NULL, 65
    UNION ALL SELECT 52201, 67, N'7.', N'Pohľadávky z daňových a colných rozpočtových príjmov (317) - (391AÚ)', NULL, 0, NULL, NULL, 66
    UNION ALL SELECT 52201, 68, N'8.', N'Pohľadávky z nedaňových príjmov obcí a vyšších územných celkov a rozpočtových organizácií zriadených obcou a vyšším územným celkom (318) - (391AÚ)', NULL, 0, NULL, NULL, 67
    UNION ALL SELECT 54101, 1, N'B.', N'Nehmotný majetok, z toho', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 54101, 2, N'I.', N'zriaďovacie náklady', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 54101, 3, N'II.', N'goodwill', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 54101, 4, N'III.', N'poskytnuté preddavky na obstaranie nehmotného majetku', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 54101, 5, N'C.', N'Finančné umiestnenie', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 54101, 6, N'I.', N'Pozemky a stavby, z toho', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1802, 62, N'654', N'Tržby z predaja materiálu', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 1802, 63, N'655', N'Výnosy z krátkodobého finančného majetku', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 1802, 64, N'656', N'Výnosy z použitia fondu', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 1802, 65, N'657', N'Výnosy z precenenia cenných papierov', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 1802, 66, N'658', N'Výnosy z nájmu majetku', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 1802, 67, N'661', N'Prijaté príspevky od organizačných zložiek', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 1802, 68, N'662', N'Prijaté príspevky od iných organizácií', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 1802, 69, N'663', N'Prijaté príspevky od fyzických osôb', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 1802, 70, N'664', N'Prijaté členské príspevky', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 1802, 71, N'665', N'Príspevky z podielu zaplatenej dane', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 1802, 72, N'667', N'Prijaté príspevky z verejných zbierok', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 1802, 73, N'691', N'Dotácie', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 1802, 74, NULL, N'Účtová trieda 6 spolu r. 39 až r. 73', NULL, 1, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 35
    UNION ALL SELECT 1802, 75, NULL, N'Výsledok hospodárenia pred zdanením r. 74 - r. 38', NULL, 1, NULL, NULL, 36
    UNION ALL SELECT 1802, 76, N'591', N'Daň z príjmov', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 1802, 77, N'595', N'Dodatočné odvody dane z príjmov', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 1802, 78, NULL, N'Výsledok hospodárenia po zdanení (r. 75 - (r. 76 + r. 77) ) (+/-)', NULL, 1, NULL, NULL, 39
    UNION ALL SELECT 1802, 995, NULL, N'Kontrolné číslo r. 39 až r. 78', NULL, 1, NULL, NULL, 40
    UNION ALL SELECT 54101, 13, N'4.', N'Dlhopisy vydané obchodnými spoločnosťami s podstatným vplyvom', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 54101, 14, N'5.', N'Ostatné dlhodobé pohľadávky', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 54101, 15, N'III.', N'Ostatné finančné umiestnenie', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 54101, 16, N'1.', N'Cenné papiere s premenlivým výnosom', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 54101, 17, N'2.', N'Cenné papiere s pevným výnosom', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 54101, 18, N'3.', N'Dlhové cenné papiere obstarané v primárnych emisiách neurčené na obchodovanie', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 54101, 19, N'4.', N'Ostatné pôžičky', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 54101, 20, N'5.', N'Vklady v bankách', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 54101, 21, N'6.', N'Iné finančné umiestnenie', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 54101, 22, N'E.', N'Pohľadávky, z toho', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 54101, 23, N'I.A.', N'z verejného zdravotného poistenia', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 54101, 24, N'1.', N'voči poisteným, z toho', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 54101, 25, N'1a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 54101, 26, N'1b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 54101, 27, N'2.', N'voči poskytovateľom zdravotnej starostlivosti, z toho', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 54101, 28, N'2a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 54101, 29, N'2b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 54101, 30, N'3.', N'voči inej zdravotnej poisťovni, z toho', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 54101, 31, N'3a.', N'z prerozdelenia poistného', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 54101, 32, N'4.', N'voči Úradu pre dohľad nad zdravotnou starostlivosťou', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 54101, 33, N'5.', N'voči Ministerstvu zdravotníctva Slovenskej republiky', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 54101, 34, N'I.B.', N'z individuálneho zdravotného poistenia, z toho', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 54101, 35, N'1.', N'voči poisteným', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 54101, 36, N'2.', N'voči sprostredkovateľom', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 54101, 37, N'3.', N'voči poskytovateľom zdravotnej starostlivosti, z toho', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 54101, 38, N'3a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 54101, 39, N'II.', N'zo zaistenia', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 54101, 40, N'III.', N'ostatné pohľadávky, z toho', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 54101, 41, N'1.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 54101, 42, N'2.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 54101, 43, N'IV', N'z upísaného základného imania', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 54101, 44, N'F.', N'Ostatné aktíva', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 54101, 45, N'I.', N'Hmotný hnuteľný majetok a zásoby z toho', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 54101, 46, N'1a.', N'stroje a zariadenia', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 54101, 47, N'1b.', N'zásoby', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 54101, 48, N'1c.', N'poskytnuté preddavky na hmotný majetok', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 54101, 49, N'1d.', N'poskytnuté preddavky na zásoby', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 54101, 50, N'II.', N'Pokladničné hodnoty a bankové účty z toho', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 54101, 51, N'1.', N'bankové účty', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 54101, 52, N'III.', N'Iné aktíva', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 54101, 53, N'G.', N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 54101, 54, N'I.', N'Nájomné', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 54101, 55, N'II.', N'Obstarávacie náklady na poistné zmluvy', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 54101, 56, N'III.', N'Ostatné účty časového rozlíšenia', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 54101, 57, NULL, N'AKTÍVA spolu', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 54101, 998, NULL, N'Kontrolné číslo', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 6102, 5, NULL, N'Nákup materiálu', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 6102, 6, NULL, N'Nákup tovaru', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 6102, 7, NULL, N'Mzdy', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 6102, 8, NULL, N'Platby poistného a príspevkov', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 6102, 9, NULL, N'Prevádzková réžia', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 6102, 10, NULL, N'Výdavky celkom súčet (r. 05 až 09)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 6102, 11, NULL, N'Rozdiel príjmov a výdavkov (r. 04 - r. 10)', NULL, 1, NULL, NULL, 6
    UNION ALL SELECT 518403, 1, N'I.', N'TECHNICKÝ ÚČET K NEŽIVOTNÉMU POISTENIU - VEREJNÉ ZDRAVOTNÉ POISTENIE', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 518403, 2, N'1.', N'Čisté zaslúžené poistné, z toho', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 518403, 3, N'1a.', N'Predpísané poistné v hrubej výške', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 518403, 4, N'1b.', N'Zmena stavu technickej rezervy na poistné budúcich období v hrubej výške', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 518403, 5, N'1c.', N'Vplyv prerozdeľovania poistného', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 518403, 6, N'2.', N'Prevedený výsledok z finančného umiestnenia z netechnického účtu', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 518403, 7, N'3.', N'Ostatné technické výnosy', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 518403, 8, N'4.', N'Náklady na poistné plnenia, z toho', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 518403, 9, N'4a.', N'Náklady na poistné plnenia v hrubej výške', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 518403, 10, N'4b.', N'Zmena stavu technickej rezervy na poistné plnenia v hrubej výške', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 518403, 11, N'5.', N'Zmena stavu iných technických rezerv', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 54101, 7, N'1.', N'pre prevádzkovú činnosť z toho', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 54101, 8, N'2.', N'budovy a stavby', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 54101, 9, N'II.', N'Finančné umiestnenie v obchodných spoločnostiach a ostatné dlhodobé pohľadávky', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 54101, 10, N'1.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach s rozhodujúcim vplyvom', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 54101, 11, N'2.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach s podstatným vplyvom', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 54101, 12, N'3.', N'Dlhopisy vydané obchodnými spoločnosťami s rozhodujúcim vplyvom', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 6202, 5, NULL, N'Zásoby', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 6202, 6, NULL, N'Služby', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 6202, 7, NULL, N'Mzdy', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 6202, 8, NULL, N'Platby poistného a príspevkov', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 6202, 9, NULL, N'Tvorba sociálneho fondu', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 6202, 10, NULL, N'Ostatné výdavky', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 6202, 11, NULL, N'Výdavky celkom súčet (r. 05 až 10)', NULL, 1, NULL, NULL, 6
    UNION ALL SELECT 6202, 12, NULL, N'Rozdiel príjmov a výdavkov (r. 04 - r. 11)', NULL, 1, NULL, NULL, 7
    UNION ALL SELECT 518305, 1, NULL, N'Daň z príjmov celkom (r. 02 a r. 04)', NULL, 1, NULL, NULL, 0
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 518305 AS [TableErpId], 2 AS [RowNumber], NULL AS [Designation], N'Daň z príjmov splatná celkom, z toho:' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 1 AS [RowOrdinal]
    UNION ALL SELECT 518305, 3, NULL, N'osobitný odvod z podnikania v regulovaných odvetviach', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 518305, 4, NULL, N'Daň z príjmov odložená celkom', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 110103, 1, N'I. A.', N'TECHNICKÝ ÚČET K NEŽIVOTNÉMU POISTENIU - VEREJNÉ ZDRAVOTNÉ POISTENIE', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 110103, 2, N'1.', N'Poistné v hrubej výške', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 110103, 3, N'2.', N'Prevedený výsledok z finančného umiestnenia z netechnického účtu', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 110103, 4, N'3.', N'Ostatné technické výnosy', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 110103, 5, N'4.', N'Náklady na poistné plnenia', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 110103, 6, N'4a.', N'Náklady na poistné plnenia v hrubej výške v tom', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 110103, 7, N'4aa.', N'Náklady na ambulantnú zdravotnú starostlivosť', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 110103, 8, N'4ab.', N'Náklady na ústavnú zdravotnú starostlivosť', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 110103, 9, N'4ac.', N'Náklady na lieky', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 110103, 10, N'4ad.', N'Náklady na zdravotnícke pomôcky', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 110103, 11, N'4ae.', N'Náklady na ostatné poistné plnenia', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 110103, 12, N'4b.', N'Nárok na úhradu nákladov od iných subjektov', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 110103, 13, N'4c.', N'Zmena stavu technickej rezervy na poistné plnenia v hrubej výške', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 110103, 14, N'5.', N'Zmena stavu iných technických rezerv', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 110103, 15, N'7.', N'Čistá výška prevádzkových nákladov', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 110103, 16, N'7a.', N'Obstarávacie náklady na poistné zmluvy', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 110103, 17, N'7b.', N'Správna réžia', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 110103, 18, N'8.', N'Ostatné technické náklady', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 110103, 19, N'10.', N'Výsledok technického účtu k neživotnému poisteniu A', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 110103, 20, N'III.', N'NETECHNICKÝ ÚČET', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 110103, 21, N'1.', N'Výsledok technického účtu k neživotnému poisteniu', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 110103, 22, N'3.', N'Výnosy z finančného umiestnenia', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 110103, 23, N'3a.', N'Výnosy z podielových cenných papierov a vkladov a v tom rozhodujúci vplyv', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 110103, 24, N'3b.', N'Výnosy z ostatného finančného umiestnenia a v tom rozhodujúci vplyv', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 110103, 25, N'3ba.', N'Výnosy z pozemkov a stavieb', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 110103, 26, N'3bb.', N'Výnosy z ostatných zložiek finančného umiestnenia', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 110103, 27, N'3c.', N'Použitie opravných položiek k finančnému umiestneniu', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 110103, 28, N'3d.', N'Výnosy z realizácie finančného umiestnenia', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 110103, 29, N'3e.', N'Prírastky hodnoty finančného umiestnenia', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 110103, 30, N'5.', N'Náklady na finančné umiestnenie', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 110103, 31, N'5a.', N'Náklady na finančné umiestnenie', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 110103, 32, N'5b.', N'Tvorba opravných položiek k finančnému umiestneniu', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 110103, 33, N'5c.', N'Náklady na realizáciu finančného umiestnenia', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 110103, 34, N'5d.', N'Úbytky hodnoty finančného umiestnenia', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 110103, 35, N'6.', N'Prevedené výnosy z finančného umiestnenia na technický účet', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 110103, 36, N'7.', N'Ostatné výnosy', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 110103, 37, N'8.', N'Ostatné náklady', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 110103, 38, N'8a.', N'Ostatné dane a poplatky', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 110103, 39, N'9.', N'Daň z príjmov z bežnej činnosti', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 110103, 40, N'10.', N'Výsledok hospodárenia z bežnej činnosti po zdanení', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 110103, 41, N'11.', N'Mimoriadne výnosy', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 110103, 42, N'12.', N'Mimoriadne náklady', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 110103, 43, N'13.', N'Mimoriadny výsledok hospodárenia', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 110103, 44, N'14.', N'Daň z príjmov z mimoriadnej činnosti', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 518401, 1, N'B.', N'Nehmotný majetok, z toho', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 518401, 2, N'I.', N'goodwill', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 518401, 3, N'II.', N'poskytnuté preddavky na obstaranie nehmotného majetku', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 518401, 4, N'C.', N'Finančné umiestnenie', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 518401, 5, N'I.', N'Pozemky a stavby, z toho', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 518401, 6, N'1.', N'pre prevádzkovú činnosť, z toho', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 518401, 7, N'2.', N'budovy a stavby', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 518401, 8, N'II.', N'Finančné umiestnenie v obchodných spoločnostiach a ostatné dlhodobé pohľadávky, z toho', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 518401, 9, N'1.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach s rozhodujúcim vplyvom', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 518401, 10, N'2.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach s podstatným vplyvom', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 518401, 11, N'3.', N'Dlhopisy vydané obchodnými spoločnosťami s rozhodujúcim vplyvom', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 518401, 12, N'4.', N'Dlhopisy vydané obchodnými spoločnosťami s podstatným vplyvom', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 518401, 13, N'5.', N'Ostatné dlhodobé pohľadávky', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 518401, 14, N'III.', N'Ostatné finančné umiestnenie', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 518401, 15, N'1.', N'Cenné papiere s premenlivým výnosom', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 518401, 16, N'2.', N'Cenné papiere s pevným výnosom', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 518401, 17, N'3.', N'Dlhové cenné papiere obstarané v primárnych emisiách neurčené na obchodovanie', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 518401, 18, N'4.', N'Ostatné pôžičky', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 518401, 19, N'5.', N'Vklady v bankách', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 518401, 20, N'6.', N'Iné finančné umiestnenie', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 518401, 21, N'IV.', N'Vlastné akcie', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 518401, 22, N'E.', N'Pohľadávky, z toho', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 518401, 23, N'I.', N'z verejného zdravotného poistenia', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 518401, 24, N'1.', N'voči poisteným, z toho', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 518401, 25, N'1a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 518401, 26, N'1b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 518401, 27, N'2.', N'voči poskytovateľom zdravotnej starostlivosti, z toho', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 518401, 28, N'2a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 518401, 29, N'2b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 518401, 30, N'3.', N'voči inej zdravotnej poisťovni, z toho', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 518401, 31, N'3a.', N'z prerozdelenia poistného', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 518401, 32, N'4.', N'voči Úradu pre dohľad nad zdravotnou starostlivosťou', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 518401, 33, N'5.', N'voči Ministerstvu zdravotníctva Slovenskej republiky', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 518401, 34, N'II.', N'ostatné pohľadávky, z toho', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 518401, 35, N'1.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 518401, 36, N'2.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 518401, 37, N'3.', N'pohľadávky voči zamestnancom', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 518401, 38, N'4.', N'daňové pohľadávky', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 518401, 39, N'5.', N'dotácie zo štátneho rozpočtu a ostatné dotácie', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 518401, 40, N'III.', N'z upísaného základného imania', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 518401, 41, N'F.', N'Ostatné aktíva', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 518401, 42, N'I.', N'Hmotný hnuteľný majetok a zásoby, z toho', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 518401, 43, N'1a.', N'stroje a zariadenia', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 518401, 44, N'1b.', N'zásoby', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 518401, 45, N'1c.', N'dopravné prostriedky', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 518401, 46, N'1d.', N'poskytnuté preddavky na hmotný majetok', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 518401, 47, N'1e.', N'poskytnuté preddavky na zásoby', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 518401, 48, N'II.', N'Pokladničné hodnoty a bankové účty, z toho', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 518401, 49, NULL, N'bankové účty', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 518401, 50, N'III.', N'Iné aktíva', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 518401, 51, N'G.', N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 518401, 52, N'I.', N'Nájomné', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 518401, 53, N'II.', N'Obstarávacie náklady na poistné zmluvy', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 518401, 54, N'III.', N'Ostatné účty časového rozlíšenia', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 518401, 55, NULL, N'AKTÍVA spolu', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 518401, 998, NULL, N'Kontrolné číslo', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 8102, 16, NULL, N'Rezervy', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 8102, 17, NULL, N'Záväzky', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 8102, 18, NULL, N'Úvery', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 8102, 19, NULL, N'Opravná položka k nadobudnutému majetku (pasívna)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 8102, 20, NULL, N'Záväzky celkom súčet (r. 16 až 19)', NULL, 1, NULL, NULL, 4
    UNION ALL SELECT 8102, 21, NULL, N'Rozdiel majetku a záväzkov (r. 15 - r. 20)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 68702, 24, NULL, N'SPOLU VLASTNÉ IMANIE A ZÁVÄZKY r. 25 + r. 34', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 68702, 25, N'A.', N'Vlastné imanie r. 26 + r. 29 + r. 30 + r. 31 + r. 32 + r. 33', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 68702, 26, N'A.I.', N'Základné imanie r. 27 + r. 28', NULL, 1, NULL, NULL, 2
    UNION ALL SELECT 68702, 27, N'A.I.1.', N'Základné imanie a zmeny základného imania (411, +/- 419) alebo (+/- 491)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 68702, 28, N'2.', N'Pohľadávky za upísané vlastné imanie (/-/353)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 68702, 29, N'A.II.', N'Kapitálové fondy (412, 413, 417, 418)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 68702, 30, N'A.III.', N'Fondy zo zisku (421, 422, 423, 427, 42X)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 68702, 31, N'A.IV.', N'Oceňovacie rozdiely (+/- 415, 416)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 68702, 32, N'A.V.', N'Nerozdelený zisk alebo neuhradená strata minulých rokov (428, /-/429)', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 68702, 33, N'A.VI.', N'Výsledok hospodárenia za účtovné obdobie po zdanení (+/-) r. 01 - (r. 26 + r. 29 + r. 30 + r. 31 + r. 32 + r. 34)', NULL, 1, NULL, NULL, 9
    UNION ALL SELECT 68702, 34, N'B.', N'Záväzky r. 35 + r. 36 + r. 37 + r. 38 + r. 43 + r. 44 + r. 45', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 68702, 35, N'B.I.', N'Dlhodobé záväzky okrem rezerv a úverov (316A, 321A, 32XA, 372A, 471A, 472A, 473A, 474A, 475A, 476A, 478A, 479A, 47XA, /-/255A, 383A, 384A)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 68702, 36, N'B.II.', N'Dlhodobé rezervy (451A, 459A, 45XA)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 68702, 37, N'B.III.', N'Dlhodobé bankové úvery (461A, 46XA)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 68702, 38, N'B.IV.', N'Krátkodobé záväzky okrem rezerv, úverov a výpomoci súčet (r. 39 až r. 42)', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 68702, 39, N'B.IV.1.', N'Krátkodobé záväzky z obchodného styku (316A, 321A, 32XA, 322, 324, 325, 326, 32X, 475A, 476A, 478A, 479A, 47XA)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 68702, 40, N'2.', N'Záväzky voči zamestnancom a zo sociálneho poistenia (331, 333, 336A, 33X, 479A)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 69001, 41, N'B.II.1.', N'Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa (351)', N'Clearing of state-funded organisation''s contributions to founder''s budget (351)', 0, N'Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa', N'r.41 - Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa (351)', 40
    UNION ALL SELECT 69001, 42, N'2.', N'Zúčtovanie transferov štátneho rozpočtu (353)', N'Clearing of state budget transfers (353)', 0, N'Zúčtovanie transferov štátneho rozpočtu', N'r.42 - Zúčtovanie transferov štátneho rozpočtu (353)', 41
    UNION ALL SELECT 69001, 43, N'3.', N'Zúčtovanie transferov rozpočtu obce a vyššieho územného celku (355)', N'Clearing of transfers of the budget of municipalities and higher territorial units (355)', 0, N'Zúčtovanie transferov rozpočtu obce a vyššieho územného celku', N'r.43 - Zúčtovanie transferov rozpočtu obce a vyššieho územného celku (355)', 42
    UNION ALL SELECT 69001, 44, N'4.', N'Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku (356)', N'Clearing of transfers from state budget within consolidated unit (356)', 0, N'Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku', N'r.44 - Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku (356)', 43
    UNION ALL SELECT 69001, 45, N'5.', N'Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku (357)', N'Other clearing of the budget of municipalities and higher territorial units (357)', 0, N'Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku', N'r.45 - Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku (357)', 44
    UNION ALL SELECT 69001, 46, N'6.', N'Zúčtovanie transferov zo štátneho rozpočtu iným subjektom (358)', N'Clearing of transfers from state budget to other entities (358)', 0, N'Zúčtovanie transferov zo štátneho rozpočtu iným subjektom', N'r.46 - Zúčtovanie transferov zo štátneho rozpočtu iným subjektom (358)', 45
    UNION ALL SELECT 69001, 47, N'7.', N'Zúčtovanie transferov medzi subjektami verejnej správy a iné zúčtovania (359)', N'Clearance of transfers between the public administration entities and other clearance transactions (359)', 0, N'Zúčtovanie transferov medzi subjektami verejnej správy a iné zúčtovania', N'r.47 - Zúčtovanie transferov medzi subjektami verejnej správy a iné zúčtovania (359)', 46
    UNION ALL SELECT 69001, 48, N'B.III', N'Dlhodobé pohľadávky súčet (r. 049 až 059)', N'Non-current receivables - total (lines 049 to 059)', 1, N'Iné pohladávky', N'r.48 - Dlhodobé pohľadávky súčet (r. 049 až 059)', 47
    UNION ALL SELECT 69001, 49, N'B.III.1', N'Odberatelia (311AÚ) - (391AÚ)', N'Customers (311A) - (391A)', 0, N'Odberatelia', N'r.49 - Dlhodobé pohľadávky / Odberatelia (311AÚ) - (391AÚ)', 48
    UNION ALL SELECT 69001, 50, N'2.', N'Zmenky na inkaso (312AÚ) - (391AÚ)', N'Bills of exchange to be collected (312A) - (391A)', 0, N'Zmenky na inkaso', N'r.50 - Dlhodobé pohľadávky / Zmenky na inkaso (312AÚ) - (391AÚ)', 49
    UNION ALL SELECT 69001, 51, N'3.', N'Pohľadávky za eskontované cenné papiere (313AÚ) - (391AÚ)', N'Receivables for discounted securities (313A) - (391A)', 0, N'Pohľadávky za eskontované cenné papiere', N'r.51 - Dlhodobé pohľadávky / Pohľadávky za eskontované cenné papiere (313AÚ) - (391AÚ)', 50
    UNION ALL SELECT 69001, 52, N'4.', N'Ostatné pohľadávky (315AÚ) - (391AÚ)', N'Other receivables (315A) - (391A)', 0, N'Ostatné pohľadávky', N'r.52 - Dlhodobé pohľadávky / Ostatné pohľadávky (315AÚ) - (391AÚ)', 51
    UNION ALL SELECT 69001, 53, N'5.', N'Pohľadávky voči zamestnancom (335AÚ) - (391AÚ)', N'Receivables to employees (335A) - (391A)', 0, N'Pohľadávky voči zamestnancom', N'r.53 - Dlhodobé pohľadávky / Pohľadávky voči zamestnancom (335AÚ) - (391AÚ)', 52
    UNION ALL SELECT 69001, 54, N'6.', N'Pohľadávky voči združeniu (369AÚ) - (391AÚ)', N'Receivables from participants in association (369A) - (391A)', 0, N'Pohľadávky voči združeniu', N'r.54 - Dlhodobé pohľadávky / Pohľadávky voči združeniu (369AÚ) - (391AÚ)', 53
    UNION ALL SELECT 69001, 55, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ) - (391AÚ)', N'Receivables and liabilities from fixed term transactions (373A) - (391A)', 0, N'Pohľadávky a záväzky z pevných termínových operácií', N'r.55 - Pohľadávky a záväzky z pevných termínových operácií (373AÚ) - (391AÚ)', 54
    UNION ALL SELECT 69001, 56, N'8.', N'Pohľadávky z nájmu (374AÚ) - (391AÚ)', N'Receivables from leasing (374A) - (391A)', 0, N'Pohľadávky z nájmu', N'r.56 - Dlhodobé pohľadávky / Pohľadávky z nájmu (374AÚ) - (391AÚ)', 55
    UNION ALL SELECT 69001, 57, N'9.', N'Pohľadávky z vydaných dlhopisov (375AÚ) - (391AÚ)', N'Receivables from issued bonds (375A) - (391A)', 0, N'Pohľadávky z vydaných dlhopisov', N'r.57 - Dlhodobé pohľadávky / Pohľadávky z vydaných dlhopisov (375AÚ) - (391AÚ)', 56
    UNION ALL SELECT 69001, 58, N'10', N'Nakúpené opcie (376AÚ) - (391AÚ)', N'Options purchased (376A) - (391A)', 0, N'Nakúpené opcie', N'r.58 - Dlhodobé pohľadávky / Nakúpené opcie (376AÚ) - (391AÚ)', 57
    UNION ALL SELECT 69001, 59, N'11.', N'Iné pohľadávky (378AÚ) - (391AÚ)', N'Other receivables (378A) - (391A)', 0, N'Iné pohľadávky', N'r.59 - Dlhodobé pohľadávky / Iné pohľadávky (378AÚ) - (391AÚ)', 58
    UNION ALL SELECT 69001, 60, N'B.IV.', N'Krátkodobé pohľadávky súčet (r. 061 až 084)', N'Current receivables - total (lines 061 to 084)', 1, N'Iné pohladávky', N'r.60 - Krátkodobé pohľadávky súčet (r. 061 až 084)', 59
    UNION ALL SELECT 69001, 61, N'B.IV.1', N'Odberatelia (311AÚ) - (391AÚ)', N'Customers (311A) - (391A)', 0, N'Odberatelia', N'r.61 - Krátkodobé pohľadávky / Odberatelia (311AÚ) - (391AÚ)', 60
    UNION ALL SELECT 69001, 62, N'2.', N'Zmenky na inkaso (312AÚ) - (391AÚ)', N'Bills of exchange to be collected (312AA) - (391A)', 0, N'Zmenky na inkaso', N'r.62 - Krátkodobé pohľadávky / Zmenky na inkaso (312AÚ) - (391AÚ)', 61
    UNION ALL SELECT 69001, 63, N'3.', N'Pohľadávky za eskontované cenné papiere (313AÚ) - (391AÚ)', N'Receivables for discounted securities (313A) - (391A)', 0, N'Pohľadávky za eskontované cenné papiere', N'r.63 - Krátkodobé pohľadávky / Pohľadávky za eskontované cenné papiere (313AÚ) - (391AÚ)', 62
    UNION ALL SELECT 69001, 64, N'4.', N'Poskytnuté prevádzkové preddavky (314) - (391AÚ)', N'Provided advance payments (314) - (391A)', 0, N'Poskytnuté prevádzkové preddavky', N'r.64 - Poskytnuté prevádzkové preddavky (314) - (391AÚ)', 63
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 69001 AS [TableErpId], 65 AS [RowNumber], N'5.' AS [Designation], N'Ostatné pohľadávky (315AÚ) - (391AÚ)' AS [Text_sk], N'Other receivables (315A) - (391A)' AS [Text_en], 0 AS [IsSumRow], N'Ostatné pohľadávky' AS [Category_sk], N'r.65 - Krátkodobé pohľadávky / Ostatné pohľadávky (315AÚ) - (391AÚ)' AS [MappingCaption_sk], 64 AS [RowOrdinal]
    UNION ALL SELECT 69001, 66, N'6.', N'Pohľadávky z nedaňových rozpočtových príjmov (316) - (391AÚ)', N'Receivables from non-tax revenue (316) - (391A)', 0, N'Pohľadávky z nedaňových rozpočtových príjmov', N'r.66 - Pohľadávky z nedaňových rozpočtových príjmov (316) - (391AÚ)', 65
    UNION ALL SELECT 69001, 67, N'7.', N'Pohľadávky z daňových a colných rozpočtových príjmov (317) - (391AÚ)', N'Receivables from tax and customs revenue (317) - (391A)', 0, N'Pohľadávky z daňových a colných rozpočtových príjmov', N'r.67 - Pohľadávky z daňových a colných rozpočtových príjmov (317) - (391AÚ)', 66
    UNION ALL SELECT 69001, 68, N'8.', N'Pohľadávky z nedaňových príjmov obcí a vyšších územných celkov a rozpočtových organizácií zriadených obcou a vyšším územným celkom (318) - (391AÚ)', N'Receivables from non-tax revenue of municipalities and higher territorial units and state-funded organisations founded by municipality and higher territorial unit (318) - (391A)', 0, N'Pohľadávky z nedaňových príjmov obcí a vyšších územných celkov a rozpočtových organizácií zriadených obcou a vyšším územným celkom', N'r.68 - Pohľadávky z nedaňových príjmov obcí a vyšších územných celkov a rozpočtových organizácií zriadených obcou a vyšším územným celkom (318) - (391AÚ)', 67
    UNION ALL SELECT 69001, 69, N'9.', N'Pohľadávky z daňových príjmov obcí a vyšších územných celkov (319) - (391AÚ)', N'Receivables from tax revenue of municipalities and higher territorial units (319) - (391A)', 0, N'Pohľadávky z daňových príjmov obcí a vyšších územných celkov', N'r.69 - Pohľadávky z daňových príjmov obcí a vyšších územných celkov (319) - (391AÚ)', 68
    UNION ALL SELECT 69001, 70, N'10.', N'Pohľadávky voči zamestnancom (335AÚ) - (391AÚ)', N'Receivables to employees (335A) - (391A)', 0, N'Pohľadávky voči zamestnancom', N'r.70 - Krátkodobé pohľadávky / Pohľadávky voči zamestnancom (335AÚ) - (391AÚ)', 69
    UNION ALL SELECT 69001, 71, N'11.', N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia (336) - (391AÚ)', N'Clearing with social and health insurance institutions (336) - (391A)', 0, N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia', N'r.71 - Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia (336) - (391AÚ)', 70
    UNION ALL SELECT 69001, 72, N'12.', N'Daň z príjmov (341) - (391AÚ)', N'Income tax (341) - (391A)', 0, N'Dan z príjmov', N'r.72 - Daň z príjmov (341) - (391AÚ)', 71
    UNION ALL SELECT 69001, 73, N'13.', N'Ostatné priame dane (342) - (391AÚ)', N'Other direct taxes (342) - (391A)', 0, N'Ostatné priame dane', N'r.73 - Ostatné priame dane (342) - (391AÚ)', 72
    UNION ALL SELECT 69001, 74, N'14.', N'Daň z pridanej hodnoty (343) - (391AÚ)', N'Value added tax (343) - (391A)', 0, N'Dan z pridanej hodnoty', N'r.74 - Daň z pridanej hodnoty (343) - (391AÚ)', 73
    UNION ALL SELECT 69001, 75, N'15.', N'Ostatné dane a poplatky (345) - (391AÚ)', N'Other taxes and fees (345) - (391A)', 0, N'Ostatné dane a poplatky', N'r.75 - Ostatné dane a poplatky (345) - (391AÚ)', 74
    UNION ALL SELECT 69001, 76, N'16.', N'Pohľadávky voči združeniu (369AÚ) - (391AÚ)', N'Receivables from participants in association (369A) - (391A)', 0, N'Pohľadávky voči združeniu', N'r.76 - Krátkodobé pohľadávky / Pohľadávky voči združeniu (369AÚ) - (391AÚ)', 75
    UNION ALL SELECT 69001, 77, N'17.', N'Pohľadávky a záväzky z pevných termínovaných operácií (373AÚ) - (391AÚ)', N'Receivables and liabilities from fixed term transactions (373A) - (391A)', 0, N'Pohľadávky a záväzky z pevných termínovaných operácií', N'r.77 - Pohľadávky a záväzky z pevných termínovaných operácií (373AÚ) - (391AÚ)', 76
    UNION ALL SELECT 69001, 78, N'18.', N'Pohľadávky z nájmu (374AÚ) - (391AÚ)', N'Receivables from leasing (374A) - (391A)', 0, N'Pohľadávky z nájmu', N'r.78 - Krátkodobé pohľadávky / Pohľadávky z nájmu (374AÚ) - (391AÚ)', 77
    UNION ALL SELECT 69001, 79, N'19.', N'Pohľadávky z vydaných dlhopisov (375AÚ) - (391AÚ)', N'Receivables from issued bonds (375A) - (391A)', 0, N'Pohľadávky z vydaných dlhopisov', N'r.79 - Krátkodobé pohľadávky / Pohľadávky z vydaných dlhopisov (375AÚ) - (391AÚ)', 78
    UNION ALL SELECT 69001, 80, N'20.', N'Nakúpené opcie (376AÚ) - (391AÚ)', N'Options purchased (376A) - (391A)', 0, N'Nakúpené opcie', N'r.80 - Krátkodobé pohľadávky / Nakúpené opcie (376AÚ) - (391AÚ)', 79
    UNION ALL SELECT 69001, 81, N'21.', N'Iné pohľadávky (378AÚ) - (391AÚ)', N'Other receivables (378A) - (391A)', 0, N'Iné pohľadávky', N'r.81 - Krátkodobé pohľadávky / Iné pohľadávky (378AÚ) - (391AÚ)', 80
    UNION ALL SELECT 69001, 82, N'22.', N'Spojovací účet pri združení (396AÚ)', N'Control account at association (396A)', 0, N'Spojovací účet pri združení', N'r.82 - Krátkodobé pohľadávky / Spojovací účet pri združení (396AÚ)', 81
    UNION ALL SELECT 69001, 83, N'23.', N'Zúčtovanie s Európskou úniou (371AÚ)- (391AÚ)', N'Clearing with the European Union (371A)- (391A)', 0, N'Zúčtovanie s Európskou úniou', N'r.83 - Zúčtovanie s Európskou úniou (371AÚ)- (391AÚ)', 82
    UNION ALL SELECT 518403, 12, N'7.', N'Čistá výška prevádzkových nákladov', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 518403, 13, N'8.', N'Ostatné technické náklady, z toho', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 518403, 14, N'8a.', N'Zmena stavu opravných položiek k pohľadávkam z poistenia a zaistenia', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 518403, 15, N'8b.', N'Odpis pohľadávok z poistenia a zaistenia', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 518403, 16, N'8c.', N'Náklady na zákonom ustanovené príspevky', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 518403, 17, N'10.', N'Výsledok technického účtu k neživotnému poisteniu', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 518403, 18, N'III.', N'NETECHNICKÝ ÚČET', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 518403, 19, N'1.', N'Výsledok technického účtu k neživotnému poisteniu', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 518403, 20, N'3.', N'Výnosy z finančného umiestnenia', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 518403, 21, N'5.', N'Náklady na finančné umiestnenie', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 518403, 22, N'6.', N'Prevedené výnosy z finančného umiestnenia na technický účet', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 518403, 23, N'7.', N'Ostatné výnosy', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 518403, 24, N'8.', N'Ostatné náklady, z toho', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 518403, 25, N'8a.', N'Ostatné dane a poplatky', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 518403, 26, N'9.', N'Daň z príjmov z bežnej činnosti', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 518403, 27, N'10.', N'Výsledok hospodárenia z bežnej činnosti po zdanení', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 518403, 28, N'11.', N'Mimoriadne výnosy', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 518403, 29, N'12.', N'Mimoriadne náklady', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 518403, 30, N'13.', N'Mimoriadny výsledok hospodárenia', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 518403, 31, N'14.', N'Daň z príjmov z mimoriadnej činnosti', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 518403, 32, N'16.', N'Výsledok hospodárenia za účtovné obdobie', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 518403, 999, NULL, N'Kontrolné číslo', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 110101, 1, N'B.', N'Nehmotný majetok, z toho', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 110101, 2, N'I.', N'goodwill', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 110101, 3, N'II.', N'poskytnuté preddavky na obstaranie nehmotného majetku', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 110101, 4, N'C.', N'Finančné umiestnenie', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 110101, 5, N'I.', N'Pozemky a stavby, z toho', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 110101, 6, N'1.', N'pre prevádzkovú činnosť z toho', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 110101, 7, N'2.', N'budovy a stavby', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 110101, 8, N'II.', N'Finančné umiestnenie v obchodných spoločnostiach a ostatné dlhodobé pohľadávky z toho', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 110101, 9, N'1.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach s rozhodujúcim vplyvom', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 110101, 10, N'2.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach s podstatným vplyvom', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 110101, 11, N'3.', N'Dlhopisy vydané obchodnými spoločnosťami s rozhodujúcim vplyvom', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 110101, 12, N'4.', N'Dlhopisy vydané obchodnými spoločnosťami s podstatným vplyvom', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 110101, 13, N'5.', N'Ostatné dlhodobé pohľadávky', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 110101, 14, N'III.', N'Ostatné finančné umiestnenie', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 110101, 15, N'1.', N'Cenné papiere s premenlivým výnosom', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 110101, 16, N'2.', N'Cenné papiere s pevným výnosom', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 110101, 17, N'3.', N'Dlhové cenné papiere obstarané v primárnych emisiách neurčené na obchodovanie', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 110101, 18, N'4.', N'Ostatné pôžičky', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 110101, 19, N'5.', N'Vklady v bankách', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 110101, 20, N'6.', N'Iné finančné umiestnenie', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 110101, 21, N'IV.', N'Vlastné akcie', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 110101, 22, N'E.', N'Pohľadávky, z toho', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 110101, 23, N'I.A.', N'z verejného zdravotného poistenia', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 110101, 24, N'1.', N'voči poisteným, z toho', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 110101, 25, N'1a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 110101, 26, N'1b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 110101, 27, N'2.', N'voči poskytovateľom zdravotnej starostlivosti, z toho', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 110101, 28, N'2a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 110101, 29, N'2b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 110101, 30, N'3.', N'voči inej zdravotnej poisťovni, z toho', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 110101, 31, N'3a.', N'z prerozdelenia poistného', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 110101, 32, N'4.', N'voči Úradu pre dohľad nad zdravotnou starostlivosťou', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 110101, 33, N'5.', N'voči Ministerstvu zdravotníctva Slovenskej republiky', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 110101, 34, N'II.', N'ostatné pohľadávky, z toho', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 110101, 35, N'1.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 110101, 36, N'2.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 110101, 37, N'3.', N'pohľadávky voči zamestnancom', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 110101, 38, N'4.', N'daňové pohľadávky', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 110101, 39, N'5.', N'dotácie zo štátneho rozpočtu a ostatné dotácie', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 110101, 40, N'III.', N'z upísaného základného imania', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 110101, 41, N'F.', N'Ostatné aktíva', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 110101, 42, N'I.', N'Hmotný hnuteľný majetok a zásoby z toho', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 110101, 43, N'1a.', N'stroje a zariadenia', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 110101, 44, N'1b.', N'zásoby', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 110101, 45, N'1c.', N'dopravné prostriedky', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 110101, 46, N'1d.', N'poskytnuté preddavky na hmotný majetok', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 110101, 47, N'1e.', N'poskytnuté preddavky na zásoby', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 110101, 48, N'II.', N'Pokladničné hodnoty a bankové účty z toho', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 110101, 49, N'1.', N'bankové účty', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 110101, 50, N'III.', N'Iné aktíva', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 110101, 51, N'G.', N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 110101, 52, N'I.', N'Nájomné', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 110101, 53, N'II.', N'Obstarávacie náklady na poistné zmluvy', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 110101, 54, N'III.', N'Ostatné účty časového rozlíšenia', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 110101, 55, NULL, N'AKTÍVA spolu', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 110101, 998, NULL, N'Kontrolné číslo', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 69001, 1, NULL, N'SPOLU MAJETOK r. 002 + r. 033 + r. 110 + r. 114', N'TOTAL ASSETS line 002 + line 033 + line 110 + line 114', 1, N'SPOLU MAJETOK', N'r.1 - SPOLU MAJETOK r. 002 + r. 033 + r. 110 + r. 114', 0
    UNION ALL SELECT 69001, 2, N'A.', N'Neobežný majetok r. 003 + r. 011 + r. 024', N'Non-current assets line 003 + line 011 + line 024', 1, N'Neobežný majetok', N'r.2 - Neobežný majetok r. 003 + r. 011 + r. 024', 1
    UNION ALL SELECT 69001, 3, N'A.I.', N'Dlhodobý nehmotný majetok súčet (r. 004 až 010)', N'Non-current intangible assets - total (lines 004 to 010)', 1, N'Dlhodobý nehmotný majetok', N'r.3 - Dlhodobý nehmotný majetok súčet (r. 004 až 010)', 2
    UNION ALL SELECT 68702, 41, N'3.', N'Daňové záväzky a dotácie (341A, 342A, 343A, 345A, 346A, 347A, 34XA)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 68702, 42, N'4.', N'Ostatné krátkodobé záväzky (364, 365, 366, 367, 368A, 36X, 372A, 379, 383A, 384A, 398A, 471A, 472A, 474A, 478A, 479A, 47XA)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 68702, 43, N'B.V.', N'Krátkodobé rezervy (323, 32XA, 451A, 459A, 45XA)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 68702, 44, N'B.VI.', N'Bežné bankové úvery (221A, 231, 232, 23X, 461A, 46XA)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 68702, 45, N'B.VII.', N'Krátkodobé finančné výpomoci (241, 249, 24X, 473A, /-/255A)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 69001, 96, N'11.', N'Obstaranie krátkodobého finančného majetku (259) - (291AÚ)', N'Acquisition of current financial assets (259) - (291A)', 0, N'Obstaranie krátkodobého finančného majetku', N'r.96 - Obstaranie krátkodobého finančného majetku (259) - (291AÚ)', 95
    UNION ALL SELECT 69001, 97, N'12.', N'Účty Štátnej pokladnice (účtová skupina 28)', N'State Treasury accounts (account group 28)', 0, N'Účty Štátnej pokladnice', N'r.97 - Účty Štátnej pokladnice (účtová skupina 28)', 96
    UNION ALL SELECT 69001, 98, N'B.VI.', N'Poskytnuté návratné finančné výpomoci dlhodobé súčet (r. 099 až r. 103)', N'Provided non-current repayable financial assistance - total (lines 099 to 103)', 1, N'Poskytnuté návratné finančné výpomoci dlhodobé', N'r.98 - Poskytnuté návratné finančné výpomoci dlhodobé súčet (r. 099 až r. 103)', 97
    UNION ALL SELECT 69001, 99, N'B.VI.1.', N'Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku (271AÚ) - (291AÚ)', N'Repayable financial assistance provided to entities within consolidated unit (271A) - (291A)', 0, N'Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku', N'r.99 - Poskytnuté návratné finančné výpomoci dlhodobé / Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku (271AÚ) - (291AÚ)', 98
    UNION ALL SELECT 69001, 100, N'2.', N'Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy (272AÚ) - (291AÚ)', N'Repayable financial assistance provided to other entities of general government (272A) - (291A)', 0, N'Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy', N'r.100 - Poskytnuté návratné finančné výpomoci dlhodobé / Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy (272AÚ) - (291AÚ)', 99
    UNION ALL SELECT 69001, 101, N'3.', N'Poskytnuté návratné finančné výpomoci podnikateľským subjektom (274AÚ)- (291AÚ)', N'Repayable financial assistance provided to business entities (274A) - (291A)', 0, N'Poskytnuté návratné finančné výpomoci podnikateľským subjektom', N'r.101 - Poskytnuté návratné finančné výpomoci podnikateľským subjektom (274AÚ)- (291AÚ)', 100
    UNION ALL SELECT 69001, 102, N'4.', N'Poskytnuté návratné finančné výpomoci ostatným organizáciám (275AÚ) - (291AÚ)', N'Repayable financial assistance provided to other organisations (275A) - (291A)', 0, N'Poskytnuté návratné finančné výpomoci ostatným organizáciám', N'r.102 - Poskytnuté návratné finančné výpomoci dlhodobé / Poskytnuté návratné finančné výpomoci ostatným organizáciám (275AÚ) - (291AÚ)', 101
    UNION ALL SELECT 69001, 103, N'5.', N'Poskytnuté návratné finančné výpomoci fyzickým osobám (277AÚ) - (291AÚ)', N'Repayable financial assistance provided to natural persons (277A) - (291A)', 0, N'Poskytnuté návratné finančné výpomoci fyzickým osobám', N'r.103 - Poskytnuté návratné finančné výpomoci dlhodobé / Poskytnuté návratné finančné výpomoci fyzickým osobám (277AÚ) - (291AÚ)', 102
    UNION ALL SELECT 69001, 104, N'B.VII.', N'Poskytnuté návratné finančné výpomoci krátkodobé súčet (r. 105 až r. 109)', N'Provided current repayable financial assistance - total (lines 105 to 109)', 1, N'Poskytnuté návratné finančné výpomoci krátkodobé', N'r.104 - Poskytnuté návratné finančné výpomoci krátkodobé súčet (r. 105 až r. 109)', 103
    UNION ALL SELECT 69001, 105, N'B.VII.1.', N'Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku (271AÚ) - (291AÚ)', N'Repayable financial assistance provided to entities within consolidated unit (271A) - (291A)', 0, N'Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku', N'r.105 - Poskytnuté návratné finančné výpomoci krátkodobé / Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku (271AÚ) - (291AÚ)', 104
    UNION ALL SELECT 69001, 106, N'2.', N'Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy (272AÚ) - (291AÚ)', N'Repayable financial assistance provided to other entities of general government (272A) - (291A)', 0, N'Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy', N'r.106 - Poskytnuté návratné finančné výpomoci krátkodobé / Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy (272AÚ) - (291AÚ)', 105
    UNION ALL SELECT 69001, 107, N'3.', N'Poskytnuté návratné finančné výpomoci podnikateľským subjektom (274AÚ) - (291AÚ)', N'Repayable financial assistance provided to business entities (274A) - (291A)', 0, N'Poskytnuté návratné finančné výpomoci podnikateľským subjektom', N'r.107 - Poskytnuté návratné finančné výpomoci podnikateľským subjektom (274AÚ) - (291AÚ)', 106
    UNION ALL SELECT 69001, 108, N'4.', N'Poskytnuté návratné finančné výpomoci ostatným organizáciám (275AÚ) - (291AÚ)', N'Repayable financial assistance provided to other organisations (275A) - (291A)', 0, N'Poskytnuté návratné finančné výpomoci ostatným organizáciám', N'r.108 - Poskytnuté návratné finančné výpomoci krátkodobé / Poskytnuté návratné finančné výpomoci ostatným organizáciám (275AÚ) - (291AÚ)', 107
    UNION ALL SELECT 69001, 109, N'5.', N'Poskytnuté návratné finančné výpomoci fyzickým osobám (277AÚ) - (291AÚ)', N'Repayable financial assistance provided to natural persons (277A) - (291A)', 0, N'Poskytnuté návratné finančné výpomoci fyzickým osobám', N'r.109 - Poskytnuté návratné finančné výpomoci krátkodobé / Poskytnuté návratné finančné výpomoci fyzickým osobám (277AÚ) - (291AÚ)', 108
    UNION ALL SELECT 69001, 110, N'C.', N'Časové rozlíšenie súčet (r. 111 až r. 113)', N'Accruals and deferrals - total (lines 111 to 113)', 1, N'Časové rozlíšenie', N'r.110 - Časové rozlíšenie súčet (r. 111 až r. 113)', 109
    UNION ALL SELECT 69001, 111, N'C.1.', N'Náklady budúcich období (381)', N'Prepaid expenses (381)', 0, N'Náklady budúcich období', N'r.111 - Náklady budúcich období (381)', 110
    UNION ALL SELECT 69001, 112, N'2.', N'Komplexné náklady budúcich období (382)', N'Complex prepaid expenses (382)', 0, N'Komplexné náklady budúcich období', N'r.112 - Komplexné náklady budúcich období (382)', 111
    UNION ALL SELECT 69001, 113, N'3.', N'Príjmy budúcich období (385)', N'Accrued income (385)', 0, N'Príjmy budúcich období', N'r.113 - Príjmy budúcich období (385)', 112
    UNION ALL SELECT 69001, 114, N'D.', N'Vzťahy k účtom klientov Štátnej pokladnice (účtová skupina 20)', N'Relationships to the State Treasury client accounts (account group 20)', 0, N'Vzťahy k účtom klientov Štátnej pokladnice', N'r.114 - Vzťahy k účtom klientov Štátnej pokladnice (účtová skupina 20)', 113
    UNION ALL SELECT 72701, 2, N'501', N'Spotreba materiálu', N'Raw material consumption', 0, NULL, NULL, 1
    UNION ALL SELECT 72701, 3, N'502', N'Spotreba energie', N'Energy consumption', 0, NULL, NULL, 2
    UNION ALL SELECT 72701, 4, N'503', N'Spotreba ostatných neskladovateľných dodávok', N'Consumption of other non-inventory supplies', 0, NULL, NULL, 3
    UNION ALL SELECT 72701, 5, N'504, 507', N'Predaný tovar, Predaná nehnuteľnosť', N'Cost on merchandise sold', 0, NULL, NULL, 4
    UNION ALL SELECT 72701, 6, N'51', N'Služby (r. 007 až r. 010)', N'Services - total (lines 007 to 010)', 1, NULL, NULL, 5
    UNION ALL SELECT 72701, 7, N'511', N'Opravy a udržiavanie', N'Repairs and maintenance', 0, NULL, NULL, 6
    UNION ALL SELECT 72701, 8, N'512', N'Cestovné', N'Travel expenses', 0, NULL, NULL, 7
    UNION ALL SELECT 72701, 9, N'513', N'Náklady na reprezentáciu', N'Representation costs', 0, NULL, NULL, 8
    UNION ALL SELECT 72701, 10, N'518', N'Ostatné služby', N'Other services', 0, NULL, NULL, 9
    UNION ALL SELECT 72701, 11, N'52', N'Osobné náklady (r. 012 až r. 016)', N'Personnel expenses - total (lines 012 to 016)', 1, NULL, NULL, 10
    UNION ALL SELECT 72701, 12, N'521', N'Mzdové náklady', N'Wages and salaries', 0, NULL, NULL, 11
    UNION ALL SELECT 72701, 13, N'524', N'Zákonné sociálne poistenie', N'Legal social insurance', 0, NULL, NULL, 12
    UNION ALL SELECT 72701, 14, N'525', N'Ostatné sociálne poistenie', N'Other social insurance', 0, NULL, NULL, 13
    UNION ALL SELECT 72701, 15, N'527', N'Zákonné sociálne náklady', N'Legal social expenses', 0, NULL, NULL, 14
    UNION ALL SELECT 72701, 16, N'528', N'Ostatné sociálne náklady', N'Other social costs', 0, NULL, NULL, 15
    UNION ALL SELECT 72701, 17, N'53', N'Dane a poplatky (r. 018 až r. 020)', N'Taxes and fees - total (lines 018 to 020)', 1, NULL, NULL, 16
    UNION ALL SELECT 72701, 18, N'531', N'Daň z motorových vozidiel', N'Road tax', 0, NULL, NULL, 17
    UNION ALL SELECT 72701, 19, N'532', N'Daň z nehnuteľnosti', N'Real estate tax', 0, NULL, NULL, 18
    UNION ALL SELECT 72701, 20, N'538', N'Ostatné dane a poplatky', N'Other taxes and fees', 0, NULL, NULL, 19
    UNION ALL SELECT 72701, 21, N'54', N'Ostatné náklady na prevádzkovú činnosť (r. 022 až r. 028)', N'Other operating expenses - total (lines 022 to 028)', 1, NULL, NULL, 20
    UNION ALL SELECT 72701, 22, N'541', N'Zostatková cena predaného dlhodobého nehmotného majetku a dlhodobého hmotného majetku', N'Carrying value of non-current intangible and tangible assets sold', 0, NULL, NULL, 21
    UNION ALL SELECT 72701, 23, N'542', N'Predaný materiál', N'Material sold', 0, NULL, NULL, 22
    UNION ALL SELECT 72701, 24, N'544', N'Zmluvné pokuty, penále a úroky z omeškania', N'Contractual fines, penalties, and interest on late payment', 0, NULL, NULL, 23
    UNION ALL SELECT 94203, 26, NULL, N'Voči zaisťovateľom', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 94203, 27, NULL, N'Regresy', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 94203, 28, NULL, N'Ostatné pohľadávky z poistenia a zaistenia', NULL, 0, NULL, NULL, 27
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 94203 AS [TableErpId], 29 AS [RowNumber], NULL AS [Designation], N'Podiely zaisťovateľov na technických rezervách' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 28 AS [RowOrdinal]
    UNION ALL SELECT 94203, 30, NULL, N'Technická rezerva na poistné budúcich období', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 94203, 31, NULL, N'Technická rezerva na poisté plnenia', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 94203, 32, NULL, N'Technická rezerva na poistné prémie a zľavy', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 94203, 33, NULL, N'Technická rezerva na úhradu záväzkov voči Slovenskej kancelárii poisťovateľov vznikajúcich z činností podľa osobitného predpisu', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 94203, 34, NULL, N'Technická rezerva na životné poistenie', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 94203, 35, NULL, N'Technická rezerva na vyrovnávanie mimoriadnych rizík', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 94203, 36, NULL, N'Ďalšie technické rezervy', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 94203, 37, NULL, N'Pokladničné hodnoty a peňažné ekvivalenty', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 94203, 38, NULL, N'Pokladničné hodnoty', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 94203, 39, NULL, N'Bežné účty v bankách', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 94203, 40, NULL, N'Termínované vklady v bankách', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 94203, 41, NULL, N'Ostatné', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 94203, 42, NULL, N'Hmotný hnuteľný majetok', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 94203, 43, NULL, N'Nehmotný majetok', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 94203, 44, NULL, N'Obstarávacie náklady na poistné zmluvy', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 94203, 45, NULL, N'Poistné zmluvy nadobudnuté v rámci portfóliového prevodu', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 94203, 46, NULL, N'Softvér', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 94203, 47, NULL, N'Goodwill', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 94203, 48, NULL, N'Ostatné', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 94203, 49, NULL, N'Daňové pohľadávky', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 94203, 50, NULL, N'z toho: bežná daňová pohľadávka', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 94203, 51, NULL, N'odložená daňová pohľadávka', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 94203, 52, NULL, N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 94203, 53, NULL, N'Neobežné aktíva určené na predaj', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 94203, 54, NULL, N'Ostatné aktíva', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 94203, 55, NULL, N'z toho: poskytnuté preddavky', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 94203, 56, NULL, N'Aktíva spolu', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 94205, 1, NULL, N'Pohľadávky celkom po lehote splatnosti v členení', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 94205, 2, NULL, N'od 91 do 120 dní', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 94205, 3, NULL, N'od 121 do 150 dní', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94205, 4, NULL, N'od 151 do 180 dní', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 94205, 5, NULL, N'od 181 do 360 dní', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 94205, 6, NULL, N'od 361 a viac dní', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 72701, 37, N'554', N'Tvorba rezerv z finančnej činnosti', N'Additions to provisions out of financial activity', 0, NULL, NULL, 36
    UNION ALL SELECT 1202, 66, N'60', N'Tržby za vlastné výkony a tovar (r. 067 až r. 069)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 1202, 67, N'601', N'Tržby za vlastné výrobky', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 1202, 68, N'602', N'Tržby z predaja služieb', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1202, 69, N'604', N'Tržby za tovar', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1202, 70, N'61', N'Zmena stavu vnútroorganizačných zásob (r. 071 až r. 074)', NULL, 1, NULL, NULL, 4
    UNION ALL SELECT 1202, 71, N'611', N'Zmena stavu nedokončenej výroby', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1202, 72, N'612', N'Zmena stavu polotovarov', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1202, 73, N'613', N'Zmena stavu výrobkov', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1202, 74, N'614', N'Zmena stavu zvierat', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 8
    UNION ALL SELECT 1202, 75, N'62', N'Aktivácia (r. 076 až r. 079)', NULL, 1, NULL, NULL, 9
    UNION ALL SELECT 1202, 76, N'621', N'Aktivácia materiálu a tovaru', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 1202, 77, N'622', N'Aktivácia vnútroorganizačných služieb', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 1202, 78, N'623', N'Aktivácia dlhodobého nehmotného majetku', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 1202, 79, N'624', N'Aktivácia dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 1202, 80, N'63', N'Daňové a colné výnosy a výnosy z poplatkov (r. 081 až r. 083)', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 1202, 81, N'631', N'Daňové a colné výnosy štátu', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 1202, 82, N'632', N'Daňové výnosy samosprávy', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 1202, 83, N'633', N'Výnosy z poplatkov', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 1202, 84, N'64', N'Ostatné výnosy z prevádzkovej činnosti (r. 085 až r. 090)', NULL, 1, NULL, NULL, 18
    UNION ALL SELECT 1202, 85, N'641', N'Tržby z predaja dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 1202, 86, N'642', N'Tržby z predaja materiálu', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 1202, 87, N'644', N'Zmluvné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 1202, 88, N'645', N'Ostatné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 1202, 89, N'646', N'Výnosy z odpísaných pohľadávok', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 1202, 90, N'648', N'Ostatné výnosy z prevádzkovej činnosti', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 1202, 91, N'65', N'Zúčtovanie rezerv a opravných položiek z prevádzkovej činnosti a finančnej činnosti a zúčtovanie časového rozlíšenia (r. 092 + r. 097 + r. 100)', NULL, 1, NULL, NULL, 25
    UNION ALL SELECT 1202, 92, NULL, N'Zúčtovanie rezerv a opravných položiek z prevádzkovej činnosti (r. 093 až r. 096)', NULL, 1, NULL, NULL, 26
    UNION ALL SELECT 1202, 93, N'652', N'Zúčtovanie zákonných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 1202, 94, N'653', N'Zúčtovanie ostatných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 1202, 95, N'657', N'Zúčtovanie zákonných opravných položiek z prevádzkovej činnosti', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 1202, 96, N'658', N'Zúčtovanie ostatných opravných položiek z prevádzkovej činnosti', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 1202, 97, NULL, N'Zúčtovanie rezerv a opravných položiek z finančnej činnosti (r. 098 + r. 099)', NULL, 1, NULL, NULL, 31
    UNION ALL SELECT 1202, 98, N'654', N'Zúčtovanie rezerv z finančnej činnosti', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 1202, 99, N'659', N'Zúčtovanie opravných položiek z finančnej činnosti', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 1202, 100, N'655', N'Zúčtovanie komplexných nákladov budúcich období', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 1202, 101, N'66', N'Finančné výnosy (r. 102 až r. 110)', NULL, 1, NULL, NULL, 35
    UNION ALL SELECT 1202, 102, N'661', N'Tržby z predaja cenných papierov a podielov', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 1202, 103, N'662', N'Úroky', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 1202, 104, N'663', N'Kurzové zisky', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 1202, 105, N'664', N'Výnosy z precenenia cenných papierov', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 1202, 106, N'665', N'Výnosy z dlhodobého finančného majetku', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 1202, 107, N'666', N'Výnosy z krátkodobého finančného majetku', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 1202, 108, N'667', N'Výnosy z derivátových operácií', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 1202, 109, N'668', N'Ostatné finančné výnosy', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 1202, 110, NULL, N'Podiel konsolidujúcej účtovnej jednotky na výsledku hospodárenia pridružených účtovných jednotiek verejnej správy', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 1202, 111, N'67', N'Mimoriadne výnosy (r. 112 až r. 115)', NULL, 1, NULL, NULL, 45
    UNION ALL SELECT 1202, 112, N'672', N'Náhrady škôd', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 1202, 113, N'674', N'Zúčtovanie rezerv', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 1202, 114, N'678', N'Ostatné mimoriadne výnosy', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 1202, 115, N'679', N'Zúčtovanie opravných položiek', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 1202, 133, N'697', N'Výnosy samosprávy z bežných transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 67
    UNION ALL SELECT 1202, 134, N'698', N'Výnosy samosprávy z kapitálových transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 68
    UNION ALL SELECT 1202, 135, N'699', N'Výnosy samosprávy z odvodu rozpočtových príjmov', NULL, 0, NULL, NULL, 69
    UNION ALL SELECT 1202, 136, NULL, N'Účtová trieda 6 celkom súčet (r. 066 + r. 070 + r. 075 + r. 080 + r. 084 + r. 091 + r. 101 + r. 111 + r. 116 + r. 126)', NULL, 1, NULL, NULL, 70
    UNION ALL SELECT 1202, 137, NULL, N'Výsledok hospodárenia pred zdanením ( r. 136 mínus r. 065) (+/-)', NULL, 1, NULL, NULL, 71
    UNION ALL SELECT 1202, 138, N'591', N'Splatná daň z príjmov', NULL, 0, NULL, NULL, 72
    UNION ALL SELECT 1202, 139, N'595', N'Dodatočne platená daň z príjmov', NULL, 0, NULL, NULL, 73
    UNION ALL SELECT 1202, 140, NULL, N'Výsledok hospodárenia po zdanení r. 137 mínus (r. 138, r. 139) (+/-)', NULL, 1, NULL, NULL, 74
    UNION ALL SELECT 1202, 141, NULL, N'z toho: pripadajúci na podiely iných účtovných jednotiek', NULL, 0, NULL, NULL, 75
    UNION ALL SELECT 1202, 995, NULL, N'Kontrolné číslo súčet (r. 066 až r. 141)', NULL, 1, NULL, NULL, 76
    UNION ALL SELECT 1301, 1, NULL, N'Z vkladu zriaďovateľa alebo zakladateľa', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 1301, 2, NULL, N'Z majetku', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 1301, 3, NULL, N'Z darov a príspevkov', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1301, 4, NULL, N'Z členských príspevkov', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1301, 5, NULL, N'Z podielu zaplatenej dane z príjmov', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1301, 6, NULL, N'Z verejných zbierok', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1301, 7, NULL, N'Z hazardných hier', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1301, 8, NULL, N'Z dedičstva', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1301, 9, NULL, N'Z organizovania podujatí', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 1301, 10, NULL, N'Z dotácií', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 1301, 11, NULL, N'Z likvidačného zostatku inej účtovnej jednotky', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 1301, 12, NULL, N'Z predaja majetku', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 1301, 13, NULL, N'Z poskytovania služieb', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 1301, 14, NULL, N'Fond prevádzky, údržby a opráv', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 1301, 15, NULL, N'Ostatné', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 1301, 16, NULL, N'Príjmy celkom (súčet r. 01 až r. 15)', NULL, 1, NULL, NULL, 15
    UNION ALL SELECT 1402, 17, NULL, N'Zásoby', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 1402, 18, NULL, N'Služby', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 1402, 19, NULL, N'Mzdy, poistné a príspevky', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1402, 20, NULL, N'Dary a príspevky iným subjektom', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1402, 21, NULL, N'Prevádzková réžia', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1402, 22, NULL, N'Splátky úverov a pôžičiek', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1402, 23, NULL, N'Sociálny fond', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1402, 24, NULL, N'Ostatné', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1402, 25, NULL, N'Výdavky celkom (súčet r. 17 až r. 24)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 1402, 26, NULL, N'Rozdiel príjmov a výdavkov (r. 16 - r. 25)', NULL, 1, NULL, NULL, 9
    UNION ALL SELECT 1402, 27, NULL, N'Daň z príjmov', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 2102, 102, N'8.', N'Vydané dlhopisy (473A/-/255A)', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 2102, 103, N'9.', N'Záväzky zo sociálneho fondu (472)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 2102, 104, N'10.', N'Ostatné dlhodobé záväzky (474A, 479A, 47XA, 372A, 373A, 377A)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 2102, 105, N'11.', N'Odložený daňový záväzok (481A)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 2102, 106, N'B.III.', N'Krátkodobé záväzky súčet (r. 107 až r. 116)', NULL, 1, NULL, NULL, 40
    UNION ALL SELECT 2102, 107, N'B.III.1.', N'Záväzky z obchodného styku (321, 322, 324, 325, 32X, 475A, 478A, 479A, 47XA)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 2102, 108, N'2.', N'Čistá hodnota zákazky (316A)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 2102, 109, N'3.', N'Nevyfakturované dodávky (326, 476A)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 2102, 110, N'4.', N'Záväzky voči dcérskej účtovnej jednotke a materskej účtovnej jednotke (361A, 471A)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 2102, 111, N'5.', N'Ostatné záväzky v rámci konsolidovaného celku (361A, 36XA, 471A, 47XA)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 2102, 112, N'6.', N'Záväzky voči spoločníkom a združeniu (364, 365, 366, 367, 368, 398A, 478A, 479A)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 2102, 113, N'7.', N'Záväzky voči zamestnancom (331, 333, 33X, 479A)', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 2102, 114, N'8.', N'Záväzky zo sociálneho poistenia (336, 479A)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 2102, 115, N'9.', N'Daňové záväzky a dotácie (341, 342, 343, 345, 346, 347, 34X)', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 2102, 116, N'10.', N'Ostatné záväzky (372A, 373A, 377A, 379A, 474A, 479A, 47X)', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 2102, 117, N'B.IV.', N'Krátkodobé finančné výpomoci (241, 249, 24X, 473A, /-/255A)', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 2102, 118, N'B.V.', N'Bankové úvery r. 119 + r. 120', NULL, 1, NULL, NULL, 52
    UNION ALL SELECT 2102, 119, N'B.V.1.', N'Bankové úvery dlhodobé (461A, 46XA)', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 2102, 120, N'2.', N'Bežné bankové úvery (221A, 231, 232, 23X, 461A, 46XA)', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 2102, 121, N'C.', N'Časové rozlíšenie súčet (r. 122 až r. 125)', NULL, 1, NULL, NULL, 55
    UNION ALL SELECT 2102, 122, N'C.1.', N'Výdavky budúcich období dlhodobé (383A)', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 2102, 123, N'2.', N'Výdavky budúcich období krátkodobé (383A)', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 2102, 124, N'3.', N'Výnosy budúcich období dlhodobé (384A)', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 2102, 125, N'4.', N'Výnosy budúcich období krátkodobé (384A)', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 2201, 1, N'I.', N'Tržby z predaja tovaru (604)', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 2201, 2, N'A.', N'Náklady vynaložené na obstaranie predaného tovaru (504, 505A)', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 2201, 3, N'+', N'Obchodná marža r. 01 - r. 02', NULL, 1, NULL, NULL, 2
    UNION ALL SELECT 2201, 4, N'II.', N'Výroba r. 05 + r. 06 + r. 07', NULL, 1, NULL, NULL, 3
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 2201 AS [TableErpId], 5 AS [RowNumber], N'II.1.' AS [Designation], N'Tržby z predaja vlastných výrobkov a služieb (601, 602)' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 4 AS [RowOrdinal]
    UNION ALL SELECT 2201, 6, N'2.', N'Zmeny stavu vnútroorganizačných zásob (+/- účtová skupina 61)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 2201, 7, N'3.', N'Aktivácia (účtová skupina 62)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 2201, 8, N'B.', N'Výrobná spotreba r. 09 + r. 10', NULL, 1, NULL, NULL, 7
    UNION ALL SELECT 2201, 9, N'B.1.', N'Spotreba materiálu, energie a ostatných neskladovateľných dodávok (501, 502, 503, 505A)', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 2201, 10, N'2.', N'Služby (účtová skupina 51)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 2201, 11, N'+', N'Pridaná hodnota r. 03 + r. 04 - r. 08', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 2201, 12, N'C.', N'Osobné náklady súčet (r. 13 až 16)', NULL, 1, NULL, NULL, 11
    UNION ALL SELECT 2201, 13, N'C.1.', N'Mzdové náklady (521, 522)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 2201, 14, N'2.', N'Odmeny členom orgánov spoločnosti a družstva (523)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 2201, 15, N'3.', N'Náklady na sociálne poistenie (524, 525, 526)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 2201, 16, N'4.', N'Sociálne náklady (527, 528)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 2201, 17, N'D.', N'Dane a poplatky (účtová skupina 53)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 2201, 18, N'E.', N'Odpisy a opravné položky k dlhodobému nehmotnému majetku a dlhodobému hmotnému majetku (551, 553)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 2201, 19, N'III.', N'Tržby z predaja dlhodobého majetku a materiálu (641, 642)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 2201, 20, N'F.', N'Zostatková cena predaného dlhodobého majetku a predaného materiálu (541, 542)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 2201, 21, N'G.', N'Tvorba a zúčtovanie opravných položiek k pohľadávkam (+/-547)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 2201, 22, N'IV.', N'Ostatné výnosy z hospodárskej činnosti (644, 645, 646, 648, 655, 657)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 2201, 23, N'H.', N'Ostatné náklady na hospodársku činnosť (543, 544, 545, 546, 548, 549, 555, 557)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 2201, 24, N'V.', N'Prevod výnosov z hospodárskej činnosti (-)(697)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 2201, 25, N'I.', N'Prevod nákladov na hospodársku činnosť (-)(597)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 2201, 26, N'*', N'Výsledok hospodárenia z hospodárskej činnosti r.11 - r. 12 - r. 17 - r. 18 + r. 19 - r. 20 - r. 21 + r. 22 -r. 23 + (-r. 24) - (-r.25)', NULL, 1, NULL, NULL, 25
    UNION ALL SELECT 2201, 27, N'VI.', N'Tržby z predaja cenných papierov a podielov (661)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 2201, 28, N'J.', N'Predané cenné papiere a podiely (561)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 2201, 29, N'VII.', N'Výnosy z dlhodobého finančného majetku r. 30 + r. 31 + r. 32', NULL, 1, NULL, NULL, 28
    UNION ALL SELECT 2201, 30, N'VII.1.', N'Výnosy z cenných papierov a podielov v dcérskej účtovnej jednotke a v spoločnosti s podstatným vplyvom (665A)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 2201, 31, N'2.', N'Výnosy z ostatných dlhodobých cenných papierov a podielov (665A)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 2201, 32, N'3.', N'Výnosy z ostatného dlhodobého finančného majetku (665A)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 2201, 33, N'VIII.', N'Výnosy z krátkodobého finančného majetku (666)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 2201, 34, N'K.', N'Náklady na krátkodobý finančný majetok (566)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 2201, 35, N'IX.', N'Výnosy z precenenia cenných papierov a výnosy z derivátových operácií (664, 667)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 2201, 36, N'L.', N'Náklady na precenenie cenných papierov a náklady na derivátové operácie (564, 567)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 2201, 37, N'M.', N'Tvorba a zúčtovanie opravných položiek k finančnému majetku +/- 565', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 2201, 38, N'X.', N'Výnosové úroky (662)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 2201, 39, N'N.', N'Nákladové úroky (562)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 2201, 40, N'XI.', N'Kurzové zisky (663)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 2201, 41, N'O.', N'Kurzové straty (563)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 2201, 42, N'XII.', N'Ostatné výnosy z finančnej činnosti (668)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 2201, 43, N'P.', N'Ostatné náklady na finančnú činnosť (568, 569)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 2201, 44, N'XIII.', N'Prevod finančných výnosov (-) (698)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 2201, 45, N'R.', N'Prevod finančných nákladov (-) (598)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 2201, 46, N'*', N'Výsledok hospodárenia z finančnej činnosti r. 27 - r. 28 + r. 29 + r. 33 - r. 34 + r. 35 - r. 36 - r. 37 + r. 38 - r. 39 + r. 40 - r. 41 + r. 42 - r. 43 + (-r. 44) - (-r.45)', NULL, 1, NULL, NULL, 45
    UNION ALL SELECT 2201, 47, N'**', N'Výsledok hospodárenia z bežnej činnosti pred zdanením r. 26 + r. 46', NULL, 1, NULL, NULL, 46
    UNION ALL SELECT 2201, 48, N'S.', N'Daň z príjmov z bežnej činnosti r. 49 + r. 50', NULL, 1, NULL, NULL, 47
    UNION ALL SELECT 2201, 49, N'S.1.', N'- splatná (591, 595)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 2201, 50, N'2.', N'- odložená (+/- 592)', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 2201, 51, N'**', N'Výsledok hospodárenia z bežnej činnosti po zdanení r. 47 - r. 48', NULL, 1, NULL, NULL, 50
    UNION ALL SELECT 2201, 52, N'XIV.', N'Mimoriadne výnosy (účtová skupina 68)', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 2201, 53, N'T.', N'Mimoriadne náklady (účtová skupina 58)', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 2201, 54, N'*', N'Výsledok hospodárenia z mimoriadnej činnosti pred zdanením r. 52 - r. 53', NULL, 1, NULL, NULL, 53
    UNION ALL SELECT 2201, 55, N'U.', N'Daň z príjmov z mimoriadnej činnosti r. 56 + r. 57', NULL, 1, NULL, NULL, 54
    UNION ALL SELECT 2201, 56, N'U.1.', N'- splatná (593)', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 2201, 57, N'2.', N'- odložená (+/- 594)', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 2201, 58, N'*', N'Výsledok hospodárenia z mimoriadnej činnosti po zdanení r. 54 - r. 55', NULL, 1, NULL, NULL, 57
    UNION ALL SELECT 2201, 59, N'***', N'Výsledok hospodárenia za účtovné obdobie pred zdanením (+/-) [r. 47 + r. 54]', NULL, 1, NULL, NULL, 58
    UNION ALL SELECT 2201, 60, N'V.', N'Prevod podielov na výsledku hospodárenia spoločníkom (+/- 596)', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 2201, 61, N'***', N'Výsledok hospodárenia za účtovné obdobie po zdanení (+/-) [r. 51 + r. 58 - r. 60]', NULL, 1, NULL, NULL, 60
    UNION ALL SELECT 518205, 1, N'1.', N'Priemerný prepočítaný stav zamestnancov', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 518205, 2, N'2.', N'Evidenčný počet zamestnancov', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 518206, 1, N'1.', N'Splatná daň z príjmov', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 518206, 2, N'2.', N'Odložená daň z príjmov', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 518206, 3, N'3.', N'Osobitný odvod z podnikania v regulovaných odvetviach', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 518206, 4, N'4.', N'Odvod časti poistného pri PZP', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 518206, 5, N'5.', N'Daň z poistenia', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 518206, 6, N'6.', N'Miestne dane a miestne poplatky za komunálne odpady a drobné stavebné odpady', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 518206, 7, N'7.', N'Daň z motorových vozidiel', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 114201, 1, N'501', N'Spotreba materiálu', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 114201, 2, N'502', N'Spotreba energie', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 114201, 3, N'503', N'Spotreba ostatných neskladovateľných dodávok', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 114201, 4, N'504', N'Predaný tovar', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 114201, 5, N'511', N'Opravy a udržiavanie', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 114201, 6, N'512', N'Cestovné', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 114201, 7, N'513', N'Náklady na reprezentáciu', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 114201, 8, N'514', N'Výkony pôšt a telekomunikácií', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 114201, 9, N'515', N'Poistné', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 114201, 10, N'516', N'Nájomné', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 114201, 11, N'518', N'Ostatné služby', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 114201, 12, N'519', N'Poplatky za poukazovanie dávok', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 80102, 56, N'A.', N'Vlastné imanie', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 80102, 57, N'I.', N'Základné imanie, z toho', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 80102, 58, N'1.', N'upísané základné imanie splatené', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 80102, 59, N'II.', N'Emisné ážio', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 80102, 60, N'III.', N'Oceňovacie rozdiely z ocenenia majetku a záväzkov', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 80102, 61, N'IV.', N'Rezervné fondy a ostatné fondy tvorené zo zisku', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 80102, 62, N'1.', N'Ostatné kapitálové fondy', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 80102, 63, N'2.', N'Rezervný fond na vlastné akcie', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 80102, 64, N'V.', N'Výsledok hospodárenia minulých rokov', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 80102, 65, N'VI.', N'Výsledok hospodárenia bežného účtovného obdobia', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 80102, 66, N'B.', N'Podriadené pasíva', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 80102, 67, N'C.', N'Technické rezervy', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 80102, 68, N'1.', N'Technická rezerva na poistné budúcich období', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 80102, 69, N'1a.', N'Hrubá výška', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 80102, 70, N'1b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 80102, 71, N'3.', N'Technická rezerva na poistné plnenie', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 80102, 72, N'3a.', N'Hrubá výška', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 80102, 73, N'3b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 80102, 74, N'4.', N'Technická rezerva na poistné prémie a zľavy', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 18
    UNION ALL SELECT 80102, 75, N'4a.', N'Hrubá výška', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 80102, 76, N'4b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 80102, 77, N'6.', N'Iné technické rezervy', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 80102, 78, N'6a.', N'Hrubá výška', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 80102, 79, N'6b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 80102, 80, N'E.', N'Ostatné rezervy', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 80102, 81, N'G.', N'Záväzky, z toho', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 80102, 82, N'I.', N'z verejného zdravotného poistenia, z toho', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 80102, 83, N'1.', N'voči poisteným, z toho', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 80102, 84, N'1a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 80102, 85, N'1b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 80102, 86, N'2.', N'voči poskytovateľom zdravotnej starostlivosti', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 80102, 87, N'2a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 80102, 88, N'2b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 80102, 89, N'3.', N'voči inej zdravotnej poisťovni, z toho', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 80102, 90, N'3a.', N'z prerozdelenia poistného', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 80102, 91, N'4.', N'voči Úradu pre dohľad nad zdravotnou starostlivosťou', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 80102, 92, N'5.', N'voči Ministerstvu zdravotníctva Slovenskej republiky', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 80102, 93, N'II.', N'pôžičky zaručené dlhopisom, z toho', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 80102, 94, N'1.', N'v konvertibilnej mene', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 80102, 95, N'2.', N'krátkodobé pôžičky', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 80102, 96, N'3.', N'dlhodobé pôžičky', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 80102, 97, N'III.', N'bankové úvery, z toho', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 80102, 98, N'1.', N'krátkodobé úvery', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 80102, 99, N'IV.', N'ostatné záväzky, z toho', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 80102, 100, N'1.', N'z daní', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 80102, 101, N'2.', N'záväzky voči zamestnancom celkom', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 80102, 102, N'2a.', N'z toho zo sociálneho poistenia a zdravotného poistenia', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 80102, 103, N'3.', N'z finančného prenájmu', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 80102, 104, N'4.', N'z dotácií zo štátneho rozpočtu a ostatné dotácie', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 80102, 105, N'H.', N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 80102, 106, NULL, N'PASÍVA spolu', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 80103, 1, N'I. A.', N'TECHNICKÝ ÚČET K NEŽIVOTNÉMU POISTENIU - VEREJNÉ ZDRAVOTNÉ POISTENIE', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 80103, 2, N'1.', N'Poistné v hrubej výške', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 80103, 3, N'2.', N'Prevedený výsledok z finančného umiestnenia z netechnického účtu', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 80103, 4, N'3.', N'Ostatné technické výnosy', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 80103, 5, N'4.', N'Náklady na poistné plnenia', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 80103, 6, N'4a.', N'Náklady na poistné plnenia v hrubej výške v tom', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 80103, 7, N'4aa.', N'Náklady na ambulantnú zdravotnú starostlivosť', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 80103, 8, N'4ab.', N'Náklady na ústavnú zdravotnú starostlivosť', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 80103, 9, N'4ac.', N'Náklady na lieky', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 80103, 10, N'4ad.', N'Náklady na zdravotnícke pomôcky', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 80103, 11, N'4ae.', N'Náklady na ostatné poistné plnenia', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 80103, 12, N'4b.', N'Nárok na úhradu nákladov od iných subjektov', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 80103, 13, N'4c.', N'Zmena stavu technickej rezervy na poistné plnenia v hrubej výške', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 38501, 58, N'1.', N'Náklady budúcich období (381)', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 38501, 59, NULL, N'Príjmy budúcich období (385)', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 38501, 60, NULL, N'MAJETOK SPOLU r. 001 + r. 029 + r. 057', NULL, 1, NULL, NULL, 59
    UNION ALL SELECT 38502, 61, N'A.', N'VLASTNÉ ZDROJE KRYTIA MAJETKU SPOLU r. 062+ r. 068 + r. 072 + r. 073', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 38502, 62, N'1.', N'Imanie a peňažné fondy r. 063 až r. 067', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 38502, 63, NULL, N'Základné imanie (411)', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 38502, 64, NULL, N'Peňažné fondy tvorené podľa osobitného predpisu (412)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 38502, 65, NULL, N'Fond reprodukcie (413)', NULL, 0, NULL, NULL, 4
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 38502 AS [TableErpId], 66 AS [RowNumber], NULL AS [Designation], N'Oceňovacie rozdiely z precenenia majetku a záväzkov (414)' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 5 AS [RowOrdinal]
    UNION ALL SELECT 38502, 67, NULL, N'Oceňovacie rozdiely z precenenia kapitálových účastín (415)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 38502, 68, N'2.', N'Fondy tvorené zo zisku r. 069 až r. 071', NULL, 1, NULL, NULL, 7
    UNION ALL SELECT 38502, 69, NULL, N'Rezervný fond (421)', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 38502, 70, NULL, N'Fondy tvorené zo zisku (423)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 38502, 71, NULL, N'Ostatné fondy (427)', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 38502, 72, N'3.', N'Nevysporiadaný výsledok hospodárenia minulých rokov (+; - 428)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 38502, 73, N'4.', N'Výsledok hospodárenia za účtovné obdobie r. 060 - (r. 062 + r. 068 + r. 072 + r. 074 + r. 101)', NULL, 1, NULL, NULL, 12
    UNION ALL SELECT 38502, 74, N'B.', N'CUDZIE ZDROJE SPOLU r. 075 + r. 079 + r. 087 + r. 097', NULL, 1, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 13
    UNION ALL SELECT 38502, 75, N'1.', N'Rezervy r. 076 až r. 078', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 38502, 76, NULL, N'Rezervy zákonné (451AÚ)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 38502, 77, NULL, N'Ostatné rezervy (459AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 38502, 78, NULL, N'Krátkodobé rezervy (323 + 451AÚ + 459AÚ)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 38502, 79, N'2.', N'Dlhodobé záväzky r. 080 až r. 086', NULL, 1, NULL, NULL, 18
    UNION ALL SELECT 38502, 80, NULL, N'Záväzky zo sociálneho fondu (472)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 38502, 81, NULL, N'Vydané dlhopisy (473)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 38502, 82, NULL, N'Záväzky z nájmu (474 AÚ)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 38502, 83, NULL, N'Dlhodobé prijaté preddavky (475)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 38502, 84, NULL, N'Dlhodobé nevyfakturované dodávky (476)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 38502, 85, NULL, N'Dlhodobé zmenky na úhradu (478)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 38502, 86, NULL, N'Ostatné dlhodobé záväzky (373 AÚ + 479 AÚ)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 38502, 87, N'3.', N'Krátkodobé záväzky r. 088 až r. 096', NULL, 1, NULL, NULL, 26
    UNION ALL SELECT 38502, 88, NULL, N'Záväzky z obchodného styku (321 až 326) okrem 323', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 38502, 89, NULL, N'Záväzky voči zamestnancom (331+ 333)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 38502, 90, NULL, N'Zúčtovanie so Sociálnou poisťovňou a zdravotnými poisťovňami (336)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 38502, 91, NULL, N'Daňové záväzky (341 až 345)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 38502, 92, NULL, N'Záväzky z dôvodu finančných vzťahov k štátnemu rozpočtu a rozpočtom územnej samosprávy (346+348)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 38502, 93, NULL, N'Záväzky z upísaných nesplatených cenných papierov a vkladov (367)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 38502, 94, NULL, N'Záväzky voči účastníkom združení (368)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 38502, 95, NULL, N'Spojovací účet pri združení (396)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 38502, 96, NULL, N'Ostatné záväzky (379 + 373 AÚ + 474 AÚ + 479 AÚ)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 38502, 97, N'4.', N'Bankové výpomoci a pôžičky r. 098 až r. 100', NULL, 1, NULL, NULL, 36
    UNION ALL SELECT 38502, 98, NULL, N'Dlhodobé bankové úvery (461AÚ)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 38502, 99, NULL, N'Bežné bankové úvery (231+ 232 + 461AÚ)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 38502, 100, NULL, N'Prijaté krátkodobé finančné výpomoci (241+ 249)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 38502, 101, N'C.', N'ČASOVÉ ROZLÍŠENIE SPOLU r. 102 a r. 103', NULL, 1, NULL, NULL, 40
    UNION ALL SELECT 38502, 102, N'1.', N'Výdavky budúcich období (383)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 38502, 103, NULL, N'Výnosy budúcich období (384)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 38502, 104, NULL, N'VLASTNÉ ZDROJE A CUDZIE ZDROJE SPOLU r.061+ r.074 + r.101', NULL, 1, NULL, NULL, 43
    UNION ALL SELECT 100101, 5, N'I.', N'Pozemky a stavby, z toho', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 100101, 6, N'1.', N'pre prevádzkovú činnosť z toho', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 100101, 7, N'2.', N'budovy a stavby', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 100101, 8, N'II.', N'Finančné umiestnenie v obchodných spoločnostiach a ostatné dlhodobé pohľadávky z toho', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 100101, 9, N'1.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach s rozhodujúcim vplyvom', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 100101, 10, N'2.', N'Podielové cenné papiere a vklady v obchodných spoločnostiach s podstatným vplyvom', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 100101, 11, N'3.', N'Dlhopisy vydané obchodnými spoločnosťami s rozhodujúcim vplyvom', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 100101, 12, N'4.', N'Dlhopisy vydané obchodnými spoločnosťami s podstatným vplyvom', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 100101, 13, N'5.', N'Ostatné dlhodobé pohľadávky', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 100101, 14, N'III.', N'Ostatné finančné umiestnenie', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 100101, 15, N'1.', N'Cenné papiere s premenlivým výnosom', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 100101, 16, N'2.', N'Cenné papiere s pevným výnosom', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 100101, 17, N'3.', N'Dlhové cenné papiere obstarané v primárnych emisiách neurčené na obchodovanie', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 100101, 18, N'4.', N'Ostatné pôžičky', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 100101, 19, N'5.', N'Vklady v bankách', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 100101, 20, N'6.', N'Iné finančné umiestnenie', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 100101, 21, N'E.', N'Pohľadávky, z toho', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 100101, 22, N'I.A.', N'z verejného zdravotného poistenia', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 100101, 23, N'1.', N'voči poisteným, z toho', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 100101, 24, N'1a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 100101, 25, N'1b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 100101, 26, N'2.', N'voči poskytovateľom zdravotnej starostlivosti, z toho', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 100101, 27, N'2a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 100101, 28, N'2b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 100101, 29, N'3.', N'voči inej zdravotnej poisťovni, z toho', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 100101, 30, N'3a.', N'z prerozdelenia poistného', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 100101, 31, N'4.', N'voči Úradu pre dohľad nad zdravotnou starostlivosťou', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 100101, 32, N'5.', N'voči Ministerstvu zdravotníctva Slovenskej republiky', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 100101, 33, N'II.', N'ostatné pohľadávky, z toho', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 100101, 34, N'1.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 100101, 35, N'2.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 100101, 36, N'3.', N'pohľadávky voči zamestnancom', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 100101, 37, N'4.', N'daňové pohľadávky', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 100101, 38, N'5.', N'dotácie zo štátneho rozpočtu a ostatné dotácie', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 100101, 39, N'III.', N'z upísaného základného imania', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 100101, 40, N'F.', N'Ostatné aktíva', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 100101, 41, N'I.', N'Hmotný hnuteľný majetok a zásoby z toho', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 100101, 42, N'1a.', N'stroje a zariadenia', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 100101, 43, N'1b.', N'zásoby', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 100101, 44, N'1c.', N'dopravné prostriedky', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 100101, 45, N'1d.', N'poskytnuté preddavky na hmotný majetok', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 100101, 46, N'1e.', N'poskytnuté preddavky na zásoby', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 100101, 47, N'II.', N'Pokladničné hodnoty a bankové účty z toho', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 100101, 48, N'1.', N'bankové účty', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 100101, 49, N'III.', N'Vlastné akcie', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 100101, 50, N'IV.', N'Iné aktíva', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 100101, 51, N'G.', N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 100101, 52, N'I.', N'Nájomné', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 100101, 53, N'II.', N'Obstarávacie náklady na poistné zmluvy', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 100101, 54, N'III.', N'Ostatné účty časového rozlíšenia', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 100101, 55, NULL, N'AKTÍVA spolu', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 100101, 998, NULL, N'Kontrolné číslo', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 80103, 14, N'5.', N'Zmena stavu iných technických rezerv', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 80103, 15, N'7.', N'Čistá výška prevádzkových nákladov', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 80103, 16, N'7a.', N'Obstarávacie náklady na poistné zmluvy', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 80103, 17, N'7b.', N'Správna réžia', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 80103, 18, N'8.', N'Ostatné technické náklady', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 80103, 19, N'10.', N'Výsledok technického účtu k neživotnému poisteniu A', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 80103, 20, N'III.', N'NETECHNICKÝ ÚČET', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 80103, 21, N'1.', N'Výsledok technického účtu k neživotnému poisteniu', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 80103, 22, N'3.', N'Výnosy z finančného umiestnenia', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 80103, 23, N'3a.', N'Výnosy z podielových cenných papierov a vkladov a v tom rozhodujúci vplyv', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 80103, 24, N'3b.', N'Výnosy z ostatného finančného umiestnenia a v tom rozhodujúci vplyv', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 80103, 25, N'3ba.', N'Výnosy z pozemkov a stavieb', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 80103, 26, N'3bb.', N'Výnosy z ostatných zložiek finančného umiestnenia', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 80103, 27, N'3c.', N'Použitie opravných položiek k finančnému umiestneniu', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 80103, 28, N'3d.', N'Výnosy z realizácie finančného umiestnenia', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 80103, 29, N'3e.', N'Prírastky hodnoty finančného umiestnenia', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 80103, 30, N'5.', N'Náklady na finančné umiestnenie', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 80103, 31, N'5a.', N'Náklady na finančné umiestnenie', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 80103, 32, N'5b.', N'Tvorba opravných položiek k finančnému umiestneniu', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 80103, 33, N'5c.', N'Náklady na realizáciu finančného umiestnenia', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 80103, 34, N'5d.', N'Úbytky hodnoty finančného umiestnenia', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 80103, 35, N'6.', N'Prevedené výnosy z finančného umiestnenia na technický účet', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 80103, 36, N'7.', N'Ostatné výnosy', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 80103, 37, N'8.', N'Ostatné náklady', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 80103, 38, N'8a.', N'Ostatné dane a poplatky', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 80103, 39, N'9.', N'Daň z príjmov z bežnej činnosti', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 80103, 40, N'10.', N'Výsledok hospodárenia z bežnej činnosti po zdanení', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 80103, 41, N'11.', N'Mimoriadne výnosy', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 80103, 42, N'12.', N'Mimoriadne náklady', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 80103, 43, N'13.', N'Mimoriadny výsledok hospodárenia', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 80103, 44, N'14.', N'Daň z príjmov z mimoriadnej činnosti', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 80103, 45, N'16.', N'Výsledok hospodárenia za účtovné obdobie', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 1202, 116, N'68', N'Výnosy z transferov a rozpočtových príjmov v štátnych rozpočtových organizáciách a príspevkových organizáciách (r. 117 až r. 125)', NULL, 1, NULL, NULL, 50
    UNION ALL SELECT 1202, 117, N'681', N'Výnosy z bežných transferov zo štátneho rozpočtu', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 1202, 118, N'682', N'Výnosy z kapitálových transferov zo štátneho rozpočtu', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 1202, 119, N'683', N'Výnosy z bežných transferov od ostatných subjektov verejnej správy', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 1202, 120, N'684', N'Výnosy z kapitálových transferov od ostatných subjektov verejnej správy', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 1202, 121, N'685', N'Výnosy z bežných transferov od Európskych spoločenstiev', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 1202, 122, N'686', N'Výnosy z kapitálových transferov od Európskych spoločenstiev', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 1202, 123, N'687', N'Výnosy z bežných transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 1202, 124, N'688', N'Výnosy z kapitálových transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 1202, 125, N'689', N'Výnosy z odvodu rozpočtových príjmov', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 1202, 126, N'69', N'Výnosy z transferov a rozpočtových príjmov v obciach, vyšších územných celkoch a v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom (r. 127 až r. 135)', NULL, 1, NULL, NULL, 60
    UNION ALL SELECT 1202, 127, N'691', N'Výnosy z bežných transferov z rozpočtu obce alebo z rozpočtu vyššieho územného celku v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 1202, 128, N'692', N'Výnosy z kapitálových transferov z rozpočtu obce alebo z rozpočtu vyššieho územného celku v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 1202, 129, N'693', N'Výnosy samosprávy z bežných transferov zo štátneho rozpočtu a od iných subjektov verejnej správy', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 1202, 130, N'694', N'Výnosy samosprávy z kapitálových transferov zo štátneho rozpočtu a od iných subjektov verejnej správy', NULL, 0, NULL, NULL, 64
    UNION ALL SELECT 1202, 131, N'695', N'Výnosy samosprávy z bežných transferov od Európskych spoločenstiev', NULL, 0, NULL, NULL, 65
    UNION ALL SELECT 1202, 132, N'696', N'Výnosy samosprávy z kapitálových transferov od Európskych spoločenstiev', NULL, 0, NULL, NULL, 66
    UNION ALL SELECT 2102, 66, NULL, N'SPOLU VLASTNÉ IMANIE A ZÁVÄZKY r. 067 + r. 088 + r. 121', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 2102, 67, N'A.', N'Vlastné imanie r. 068 + r. 073 + r. 080 + r. 084 + r. 087', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 2102, 68, N'A.I.', N'Základné imanie súčet (r. 069 až 072)', NULL, 1, NULL, NULL, 2
    UNION ALL SELECT 2102, 69, N'A.I.1.', N'Základné imanie (411 alebo +/- 491)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 2102, 70, N'2.', N'Vlastné akcie a vlastné obchodné podiely (/-/252)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 2102, 71, N'3.', N'Zmena základného imania +/- 419', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 2102, 72, N'4.', N'Pohľadávky za upísané vlastné imanie (/-/353)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 2102, 73, N'A.II.', N'Kapitálové fondy súčet (r. 074 až 079)', NULL, 1, NULL, NULL, 7
    UNION ALL SELECT 2102, 74, N'A.II.1.', N'Emisné ážio (412)', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 8
    UNION ALL SELECT 2102, 75, N'2.', N'Ostatné kapitálové fondy (413)', NULL, 0, NULL, NULL, 9
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 2102 AS [TableErpId], 76 AS [RowNumber], N'3.' AS [Designation], N'Zákonný rezervný fond (Nedeliteľný fond) z kapitálových vkladov (417, 418)' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 10 AS [RowOrdinal]
    UNION ALL SELECT 2102, 77, N'4.', N'Oceňovacie rozdiely z precenenia majetku a záväzkov (+/- 414)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 2102, 78, N'5.', N'Oceňovacie rozdiely z kapitálových účastín (+/- 415)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 2102, 79, N'6.', N'Oceňovacie rozdiely z precenenia pri zlúčení, splynutí a rozdelení (+/- 416)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 2102, 80, N'A.III.', N'Fondy zo zisku súčet (r. 081 až r. 083)', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 2102, 81, N'A.III.1.', N'Zákonný rezervný fond (421)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 2102, 82, N'2.', N'Nedeliteľný fond (422)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 2102, 83, N'3.', N'Štatutárne fondy a ostatné fondy (423, 427, 42X)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 2102, 84, N'A.IV.', N'Výsledok hospodárenia minulých rokov r. 085 + r. 086', NULL, 1, NULL, NULL, 18
    UNION ALL SELECT 2102, 85, N'A.IV.1.', N'Nerozdelený zisk minulých rokov (428)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 2102, 86, N'2.', N'Neuhradená strata minulých rokov (/-/429)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 2102, 87, N'A.V.', N'Výsledok hospodárenia za účtovné obdobie po zdanení /+-/ r. 001 - (r. 068 + r. 073 + r. 080 + r. 084 + r. 088 + r. 121)', NULL, 1, NULL, NULL, 21
    UNION ALL SELECT 2102, 88, N'B.', N'Záväzky r. 89 + r. 94 + r. 106 + r. 117 + r. 118', NULL, 1, NULL, NULL, 22
    UNION ALL SELECT 2102, 89, N'B.I.', N'Rezervy súčet (r. 090 až r. 093)', NULL, 1, NULL, NULL, 23
    UNION ALL SELECT 2102, 90, N'B.I.1.', N'Rezervy zákonné dlhodobé (451A)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 2102, 91, N'2.', N'Rezervy zákonné krátkodobé (323A, 451A)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 2102, 92, N'3.', N'Ostatné dlhodobé rezervy (459A, 45XA)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 2102, 93, N'4.', N'Ostatné krátkodobé rezervy (323A, 32X, 459A, 45XA)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 2102, 94, N'B.II.', N'Dlhodobé záväzky súčet (r. 095 až r. 105)', NULL, 1, NULL, NULL, 28
    UNION ALL SELECT 2102, 95, N'B.II.1.', N'Dlhodobé záväzky z obchodného styku (321A,479A)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 2102, 96, N'2.', N'Čistá hodnota zákazky (316A)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 2102, 97, N'3.', N'Dlhodobé nevyfakturované dodávky (476A)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 2102, 98, N'4.', N'Dlhodobé záväzky voči dcérskej účtovnej jednotke a materskej účtovnej jednotke (471A)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 2102, 99, N'5.', N'Ostatné dlhodobé záväzky v rámci konsolidovaného celku (471A)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 2102, 100, N'6.', N'Dlhodobé prijaté preddavky (475A)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 2102, 101, N'7.', N'Dlhodobé zmenky na úhradu (478A)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 66202, 21, NULL, N'Vlastné imanie z toho:', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 66202, 22, NULL, N'Základné imanie', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 66202, 23, NULL, N'Vlastné akcie', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 66202, 24, NULL, N'Emisné ážio', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 66202, 25, NULL, N'Finančné zdroje poskytnuté pobočke zahraničnej poisťovne', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 66202, 26, NULL, N'Rezervné fondy a ostatné fondy tvorené zo zisku', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 66202, 27, NULL, N'Fond vyrovnávacej rezervy', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 66202, 28, NULL, N'Ostatné kapitálové fondy', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 66202, 29, NULL, N'Oceňovacie rozdiely', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 66202, 30, NULL, N'Vlastnosti ľubovoľnej účasti', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 66202, 31, NULL, N'Hospodársky výsledok minulých rokov', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 66202, 32, NULL, N'Hospodársky výsledok vo schvaľovacom období', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 66202, 33, NULL, N'Hospodársky výsledok bežného obdobia', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 66202, 34, NULL, N'Záväzky z toho:', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 66202, 35, NULL, N'Podriadené záväzky', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 66202, 36, NULL, N'Prijaté úvery a pôžičky', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 66202, 37, NULL, N'Vklady pri pasívnom zaistení', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 66202, 38, NULL, N'Záporná reálna hodnota derivátových operácií na obchodovanie', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 66202, 39, NULL, N'Záporná reálna hodnota derivátových operácií na zabezpečenie', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 66202, 40, NULL, N'Rezervy na poistné zmluvy', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 66202, 41, NULL, N'Rezerva na poistné budúcich období', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 66202, 42, NULL, N'Rezerva na poistné plnenia', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 66202, 43, NULL, N'Rezerva na poistné prémie a zľavy', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 66202, 44, NULL, N'Rezerva na úhradu záväzkov voči SKP vznikajúcich z činnosti podľa osobitného predpisu', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 66202, 45, NULL, N'Rezerva na životné poistenie', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 66202, 46, NULL, N'Ďalšie rezervy', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 66202, 47, NULL, N'Rezerva na krytie rizika z investovania finančných prostriedkov v mene poistených', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 66202, 48, NULL, N'Finančné záväzky z investičných zmlúv', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 66202, 49, NULL, N'Netechnické rezervy', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 66202, 50, NULL, N'Záväzky z poistenia a zaistenia', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 66202, 51, NULL, N'Krátkodobé zamestnanecké pôžičky', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 66202, 52, NULL, N'Daňové záväzky', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 66202, 53, NULL, N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 66202, 54, NULL, N'Ostatné záväzky', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 66202, 55, NULL, N'PASÍVA spolu', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 38501, 1, N'A.', N'NEOBEŽNÝ MAJETOK SPOLU r. 002 + r. 009 + r. 021', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 38501, 2, N'1.', N'Dlhodobý nehmotný majetok r. 003 až r. 008', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 38501, 3, NULL, N'Nehmotné výsledky z vývojovej a obdobnej činnosti 012-(072+091AÚ)', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 38501, 4, NULL, N'Softvér 013 - (073+091AÚ)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 38501, 5, NULL, N'Oceniteľné práva 014 - (074 + 091AÚ)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 38501, 6, NULL, N'Ostatný dlhodobý nehmotný majetok (018+ 019)-(078 + 079 + 091 AÚ)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 38501, 7, NULL, N'Obstaranie dlhodobého nehmotného majetku (041-093)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 38501, 8, NULL, N'Poskytnuté preddavky na dlhodobý nehmotný majetok (051- 095AÚ)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 38501, 9, N'2.', N'Dlhodobý hmotný majetok r. 010 až r. 020', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 38501, 10, NULL, N'Pozemky (031)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 38501, 11, NULL, N'Umelecké diela a zbierky (032)', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 38501, 12, NULL, N'Stavby 021 - (081 + 092AÚ)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 38501, 13, NULL, N'Samostatné hnuteľné veci a súbory hnuteľných vecí 022 - (082 + 092AÚ)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 38501, 14, NULL, N'Dopravné prostriedky 023 - (083 + 092AÚ)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 38501, 15, NULL, N'Pestovateľské celky trvalých porastov 025 - (085 + 092AÚ)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 38501, 16, NULL, N'Základné stádo a ťažné zvieratá 026 - (086 + 092AÚ)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 38501, 17, NULL, N'Drobný dlhodobý hmotný majetok 028 - (088 + 092AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 38501, 18, NULL, N'Ostatný dlhodobý hmotný majetok 029 - (089 +092AÚ)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 38501, 19, NULL, N'Obstaranie dlhodobého hmotného majetku (042 - 094)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 38501, 20, NULL, N'Poskytnuté preddavky na dlhodobý hmotný majetok (052 - 095AÚ)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 38501, 21, N'3.', N'Dlhodobý finančný majetok r. 022 až r. 028', NULL, 1, NULL, NULL, 20
    UNION ALL SELECT 38501, 22, NULL, N'Podielové cenné papiere a podiely v obchodných spoločnostiach v ovládanej osobe (061- 096 AÚ)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 38501, 23, NULL, N'Podielové cenné papiere a podiely v obchodných spoločnostiach s podstatným vplyvom (062 - 096 AÚ)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 38501, 24, NULL, N'Dlhové cenné papiere držané do splatnosti (065 - 096 AÚ)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 38501, 25, NULL, N'Pôžičky podnikom v skupine a ostatné pôžičky (066 + 067) - 096 AÚ', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 38501, 26, NULL, N'Ostatný dlhodobý finančný majetok (069 - 096 AÚ)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 38501, 27, NULL, N'Obstaranie dlhodobého finančného majetku (043 - 096 AÚ)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 38501, 28, NULL, N'Poskytnuté preddavky na dlhodobý finančný majetok (053 - 096 AÚ)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 38501, 29, N'B.', N'OBEŽNÝ MAJETOK SPOLU r. 030+ r. 037+ r. 042 + r. 051', NULL, 1, NULL, NULL, 28
    UNION ALL SELECT 38501, 30, N'1.', N'Zásoby r. 031 až r. 036', NULL, 1, NULL, NULL, 29
    UNION ALL SELECT 38501, 31, NULL, N'Materiál (112 + 119) - 191', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 38501, 32, NULL, N'Nedokončená výroba a polotovary vlastnej výroby (121+122)-(192+193)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 38501, 33, NULL, N'Výrobky (123 - 194)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 38501, 34, NULL, N'Zvieratá (124 - 195)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 38501, 35, NULL, N'Tovar (132 + 139) - 196', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 38501, 36, NULL, N'Poskytnuté prevádzkové preddavky na zásoby (314 AÚ - 391 AÚ)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 38501, 37, N'2.', N'Dlhodobé pohľadávky r. 038 až r. 041', NULL, 1, NULL, NULL, 36
    UNION ALL SELECT 38501, 38, NULL, N'Pohľadávky z obchodného styku (311 AÚ až 314 AÚ) - 391 AÚ', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 38501, 39, NULL, N'Ostatné pohľadávky (315 AÚ - 391AÚ)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 38501, 40, NULL, N'Pohľadávky voči účastníkom združení (358AÚ - 391AÚ)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 38501, 41, NULL, N'Iné pohľadávky (335 AÚ + 373 AÚ + 375 AÚ + 378AÚ) - 391AÚ', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 38501, 42, N'3.', N'Krátkodobé pohľadávky r. 043 až r. 050', NULL, 1, NULL, NULL, 41
    UNION ALL SELECT 38501, 43, NULL, N'Pohľadávky z obchodného styku (311AÚ až 314 AÚ) - 391AÚ', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 38501, 44, NULL, N'Ostatné pohľadávky (315 AÚ - 391 AÚ)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 38501, 45, NULL, N'Zúčtovanie so Sociálnou poisťovňou a zdravotnými poisťovňami (336)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 38501, 46, NULL, N'Daňové pohľadávky (341 až 345)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 38501, 47, NULL, N'Pohľadávky z dôvodu finančných vzťahov k štátnemu rozpočtu a rozpočtom územnej samosprávy (346+ 348)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 38501, 48, NULL, N'Pohľadávky voči účastníkom združení (358 AÚ - 391AÚ)', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 38501, 49, NULL, N'Spojovací účet pri združení (396 - 391AÚ)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 38501, 50, NULL, N'Iné pohľadávky (335AÚ + 373AÚ + 375AÚ + 378AÚ) - 391AÚ', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 38501, 51, N'4.', N'Finančné účty r. 052 až r. 056', NULL, 1, NULL, NULL, 50
    UNION ALL SELECT 38501, 52, NULL, N'Pokladnica (211 + 213)', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 38501, 53, NULL, N'Bankové účty (221 AÚ + 261)', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 38501, 54, NULL, N'Bankové účty s dobou viazanosti dlhšou ako jeden rok (221 AÚ)', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 38501, 55, NULL, N'Krátkodobý finančný majetok (251+ 253 + 255 + 256 + 257) - 291AÚ', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 38501, 56, NULL, N'Obstaranie krátkodobého finančného majetku (259 - 291AÚ)', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 38501, 57, N'C.', N'ČASOVÉ ROZLÍŠENIE SPOLU r. 058 a r. 059', NULL, 1, NULL, NULL, 56
    UNION ALL SELECT 94303, 1, NULL, N'Záväzky po lehote splatnosti z pokračujúcej činnosti celkom, z toho:', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 94303, 2, NULL, N'- do 90 dní', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 94303, 3, NULL, N'- od 91 dní do 120 dní', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94303, 4, NULL, N'- od 121 dní do 150 dní', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 94303, 5, NULL, N'- od 151 dní do 180 dní', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 94303, 6, NULL, N'- od 181 dní do 360 dní', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 94303, 7, NULL, N'- od 361 dní a viac', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 100101, 1, N'B.', N'Nehmotný majetok, z toho', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 100101, 2, N'I.', N'goodwill', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 100101, 3, N'II.', N'poskytnuté preddavky na obstaranie nehmotného majetku', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 100101, 4, N'C.', N'Finančné umiestnenie', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 114201, 13, N'521', N'Mzdové náklady', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 114201, 14, N'523', N'Odmeny členom dozornej rady', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 114201, 15, N'524', N'Zákonné sociálne poistenie', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 114201, 16, N'525', N'Ostatné sociálne poistenie', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 114201, 17, N'527', N'Zákonné sociálne náklady', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 114201, 18, N'528', N'Ostatné sociálne náklady', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 114201, 19, N'531', N'Daň z motorových vozidiel', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 114201, 20, N'532', N'Daň z nehnuteľností', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 114201, 21, N'538', N'Ostatné dane a poplatky', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 114201, 22, N'541', N'Zmluvné pokuty a úroky z omeškania', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 114201, 23, N'542', N'Ostatné pokuty a úroky z omeškania', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 114201, 24, N'543', N'Odpísanie nevymožiteľnej pohľadávky', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 114201, 25, N'544', N'Úroky', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 114201, 26, N'545', N'Kurzové straty', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 114201, 27, N'546', N'Dary', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 114201, 28, N'548', N'Manká a škody', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 114201, 29, N'549', N'Iné ostatné náklady', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 114201, 30, N'551', N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 114201, 31, N'552', N'Zostatková cena predaného dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 114201, 32, N'553', N'Predané cenné papiere', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 114201, 33, N'554', N'Predaný materiál', NULL, 0, NULL, NULL, 32
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 114201 AS [TableErpId], 34 AS [RowNumber], N'557' AS [Designation], N'Náklady z precenenia cenných papierov' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 33 AS [RowOrdinal]
    UNION ALL SELECT 114201, 35, N'559', N'Tvorba a zúčtovanie opravných položiek', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 114201, 36, NULL, N'Účtová trieda 5 spolu súčet (r. 001 až r. 035)', NULL, 1, NULL, NULL, 35
    UNION ALL SELECT 114202, 37, N'601', N'Tržby za vlastné výkony', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 114202, 38, N'602', N'Tržby z predaja služieb', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 114202, 39, N'604', N'Tržby za predaný tovar', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 114202, 40, N'605', N'Iné ostatné tržby', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 114202, 41, N'621', N'Aktivácia materiálu', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 114202, 42, N'622', N'Aktivácia vnútroorganizačných služieb', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 114202, 43, N'623', N'Aktivácia dlhodobého nehmotného majetku', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 114202, 44, N'624', N'Aktivácia dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 114202, 45, N'641', N'Zmluvné pokuty a úroky z omeškania', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 114202, 46, N'642', N'Ostatné pokuty a úroky z omeškania', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 114202, 47, N'643', N'Platby za odpísané pohľadávky', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 114202, 48, N'644', N'Úroky', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 114202, 49, N'645', N'Kurzové zisky', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 114202, 50, N'649', N'Iné ostatné výnosy', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 114202, 51, N'651', N'Tržby z predaja dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 114202, 52, N'652', N'Výnosy z dlhodobého finančného majetku', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 114202, 53, N'653', N'Tržby z predaja cenných papierov', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 114202, 54, N'654', N'Tržby z predaja materiálu', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 114202, 55, N'655', N'Výnosy z krátkodobého finančného majetku', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 114202, 56, N'657', N'Výnosy z precenenia cenných papierov', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 114202, 57, N'658', N'Výnosy z nájmu majetku', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 114202, 58, N'691', N'Dotácie na prevádzku', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 114202, 59, NULL, N'Účtová trieda 6 spolu súčet (r.037 až r. 058)', NULL, 1, NULL, NULL, 22
    UNION ALL SELECT 114202, 60, NULL, N'Výsledok hospodárenia pred zdanením (r.059 mínus r. 036)(+/-)', NULL, 1, NULL, NULL, 23
    UNION ALL SELECT 114202, 61, N'591', N'Daň z príjmov', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 114202, 62, N'595', N'Dodatočné odvody dane z príjmov', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 114202, 63, NULL, N'Výsledok hospodárenia po zdanení (r.060 mínus r. 061 a r. 062) (+/-)', NULL, 1, NULL, NULL, 26
    UNION ALL SELECT 94208, 50, NULL, N'Ostatné výnosy', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 94208, 51, NULL, N'Ostatné náklady', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 94208, 52, NULL, N'Hospodársky výsledok pred zdanením', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 94208, 53, NULL, N'Splatná daň', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 94208, 54, NULL, N'Odložená daň', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 94208, 55, NULL, N'Osobitný odvod', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 94208, 56, NULL, N'Hospodársky výsledok po zdanení', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 94302, 1, NULL, N'Pohľadávky po lehote splatnosti z pokračujúcej činnosti celkom, z toho:', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 94302, 2, NULL, N'- do 90 dní', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 94302, 3, NULL, N'- od 91 dní do 120 dní', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94302, 4, NULL, N'- od 121 dní do 150 dní', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 94302, 5, NULL, N'- od 151 dní do 180 dní', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 94302, 6, NULL, N'- od 181 dní do 360 dní', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 94302, 7, NULL, N'- od 361 dní a viac', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 518304, 34, NULL, N'Výsledok hospodárenia za účtovné obdobie po zdanení (+/-), (r. 30 + r. 33)', NULL, 1, NULL, NULL, 33
    UNION ALL SELECT 518304, 35, NULL, N'Ostatné súčasti komplexného výsledku', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 518304, 36, NULL, N'Daň z príjmov', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 518304, 37, NULL, N'Celkový komplexný výsledok za účtovné obdobie po zdanení (+/-), (r. 34 + r. 35 - r. 36)', NULL, 1, NULL, NULL, 36
    UNION ALL SELECT 72701, 38, N'559', N'Tvorba opravných položiek z finančnej činnosti', N'Additions to adjusting entries out of financial activity', 0, NULL, NULL, 37
    UNION ALL SELECT 72701, 39, N'555', N'Zúčtovanie komplexných nákladov budúcich období', N'Clearing of complex deferred expenses', 0, NULL, NULL, 38
    UNION ALL SELECT 72701, 40, N'56', N'Finančné náklady (r. 041 až r. 048)', N'Financial expenses - total (lines 041 to 048)', 1, NULL, NULL, 39
    UNION ALL SELECT 72701, 41, N'561', N'Predané cenné papiere a podiely', N'Securities and shares sold', 0, NULL, NULL, 40
    UNION ALL SELECT 72701, 42, N'562', N'Úroky', N'Interest expense', 0, NULL, NULL, 41
    UNION ALL SELECT 72701, 43, N'563', N'Kurzové straty', N'Exchange rate losses', 0, NULL, NULL, 42
    UNION ALL SELECT 72701, 44, N'564', N'Náklady na precenenie cenných papierov', N'Costs of securities revaluation', 0, NULL, NULL, 43
    UNION ALL SELECT 72701, 45, N'566', N'Náklady na krátkodobý finančný majetok', N'Costs of current financial assets', 0, NULL, NULL, 44
    UNION ALL SELECT 72701, 46, N'567', N'Náklady na derivátové operácie', N'Costs of derivative operations', 0, NULL, NULL, 45
    UNION ALL SELECT 72701, 47, N'568', N'Ostatné finančné náklady', N'Other financial expenses', 0, NULL, NULL, 46
    UNION ALL SELECT 72701, 48, N'569', N'Manká a škody na finančnom majetku', N'Deficits and damages to financial assets', 0, NULL, NULL, 47
    UNION ALL SELECT 72701, 49, N'57', N'Mimoriadne náklady (r. 050 až r. 053)', N'Extraordinary expenses - total (lines 050 to 053)', 1, NULL, NULL, 48
    UNION ALL SELECT 72701, 50, N'572', N'Škody', N'Damages', 0, NULL, NULL, 49
    UNION ALL SELECT 72701, 51, N'574', N'Tvorba rezerv', N'Additions to provisions', 0, NULL, NULL, 50
    UNION ALL SELECT 72701, 52, N'578', N'Ostatné mimoriadne náklady', N'Other extraordinary expenses', 0, NULL, NULL, 51
    UNION ALL SELECT 72701, 53, N'579', N'Tvorba opravných položiek', N'Additions to adjusting entries', 0, NULL, NULL, 52
    UNION ALL SELECT 72701, 54, N'58', N'Náklady na transfery a náklady z odvodu príjmov (r. 055 až r. 063)', N'Costs of transfers and costs of revenue transfer - total (lines 055 to 063)', 1, NULL, NULL, 53
    UNION ALL SELECT 72701, 55, N'581', N'Náklady na transfery zo štátneho rozpočtu do štátnych rozpočtových organizácií a príspevkových organizácií', N'Cost of transfers from state budget to state-funded and state-subsidized organizations', 0, NULL, NULL, 54
    UNION ALL SELECT 72701, 56, N'582', N'Náklady na transfery zo štátneho rozpočtu ostatným subjektom verejnej správy', N'Cost of transfers from state budget to other entities of general government', 0, NULL, NULL, 55
    UNION ALL SELECT 72701, 57, N'583', N'Náklady na transfery zo štátneho rozpočtu subjektom mimo verejnej správy', N'Cost of transfers from state budget to the entities outside of general government', 0, NULL, NULL, 56
    UNION ALL SELECT 72701, 58, N'584', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku do rozpočtových organizácií a príspevkových organizácií zriadených obcou alebo vyšším územným celkom', N'Cost of transfers from the budget of municipality or higher regional unit to state-funded and state-subsidized organisations founded by the municipality or higher regional unit', 0, NULL, NULL, 57
    UNION ALL SELECT 72701, 59, N'585', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku ostatným subjektom verejnej správy', N'Cost of transfers from the budget of municipality or higher regional unit to other entities of general government', 0, NULL, NULL, 58
    UNION ALL SELECT 72701, 60, N'586', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku subjektom mimo verejnej správy', N'Cost of transfers from the budget of municipality or higher regional unit to the entities outside of general government', 0, NULL, NULL, 59
    UNION ALL SELECT 72701, 61, N'587', N'Náklady na ostatné transfery', N'Costs of other transfers', 0, NULL, NULL, 60
    UNION ALL SELECT 72701, 62, N'588', N'Náklady z odvodu príjmov', N'Cost of budgetary contributions', 0, NULL, NULL, 61
    UNION ALL SELECT 72701, 63, N'589', N'Náklady z budúceho odvodu príjmov', N'Cost of future budgetary contributions', 0, NULL, NULL, 62
    UNION ALL SELECT 72701, 64, NULL, N'Účtové skupiny 50 - 58 súčet (r.001 + r.006 + r.011 + r.017 + r.021 + r.029 + r.040 + r.049 + r.054)', N'Account groups 50 - 58, line 001 + line 006 + line 011 + line 017 + line 021 + line 029 + line 040 + line 049 + line 054', 1, NULL, NULL, 63
    UNION ALL SELECT 69001, 4, N'A.I.1.', N'Aktivované náklady na vývoj (012) - (072+091AÚ)', N'Capitalized development costs (012) - (072+091A)', 0, N'Aktivované náklady na vývoj', N'r.4 - Aktivované náklady na vývoj (012) - (072+091AÚ)', 3
    UNION ALL SELECT 69001, 5, N'2.', N'Softvér (013) - (073+091AÚ)', N'Software (013) - (073+091A)', 0, N'Softvér', N'r.5 - Softvér (013) - (073+091AÚ)', 4
    UNION ALL SELECT 69001, 6, N'3.', N'Oceniteľné práva (014) - (074+091AÚ)', N'Valuable rights (014) - (074+091A)', 0, N'Oceniteľné práva', N'r.6 - Oceniteľné práva (014) - (074+091AÚ)', 5
    UNION ALL SELECT 69001, 7, N'4.', N'Drobný dlhodobý nehmotný majetok (018) - (078+091AÚ)', N'Small non-current intangible assets (018) - (078+091A)', 0, N'Drobný dlhodobý nehmotný majetok', N'r.7 - Drobný dlhodobý nehmotný majetok (018) - (078+091AÚ)', 6
    UNION ALL SELECT 69001, 8, N'5.', N'Ostatný dlhodobý nehmotný majetok (019) - (079+091AÚ)', N'Other non-current intangible assets (019) - (079+091A)', 0, N'Ostatný dlhodobý nehmotný majetok', N'r.8 - Ostatný dlhodobý nehmotný majetok (019) - (079+091AÚ)', 7
    UNION ALL SELECT 69001, 9, N'6.', N'Obstaranie dlhodobého nehmotného majetku (041) - (093)', N'Acquisition of non-current intangible assets (041) - (093)', 0, N'Obstaranie dlhodobého nehmotného majetku', N'r.9 - Obstaranie dlhodobého nehmotného majetku (041) - (093)', 8
    UNION ALL SELECT 69001, 10, N'7.', N'Poskytnuté preddavky na dlhodobý nehmotný majetok (051) - (095AÚ)', N'Advance payments made for non-current intangible assets (051) - (095A)', 0, N'Poskytnuté preddavky na dlhodobý nehmotný majetok', N'r.10 - Poskytnuté preddavky na dlhodobý nehmotný majetok (051) - (095AÚ)', 9
    UNION ALL SELECT 69001, 11, N'A.II.', N'Dlhodobý hmotný majetok súčet (r. 012 až 023)', N'Property, plant and equipment - total (lines 012 to 023)', 1, N'Dlhodobý hmotný majetok', N'r.11 - Dlhodobý hmotný majetok súčet (r. 012 až 023)', 10
    UNION ALL SELECT 69001, 12, N'A.II.1.', N'Pozemky (031) - (092AÚ)', N'Land (031) - (092AA)', 0, N'Pozemky', N'r.12 - Pozemky (031) - (092AÚ)', 11
    UNION ALL SELECT 69001, 13, N'2.', N'Umelecké diela a zbierky (032) - (092AÚ)', N'Works of art and collections (032) - (092A)', 0, N'Umelecké diela a zbierky', N'r.13 - Umelecké diela a zbierky (032) - (092AÚ)', 12
    UNION ALL SELECT 69001, 14, N'3.', N'Predmety z drahých kovov (033) - (092AÚ)', N'Objects of precious metals (033) - (092A)', 0, N'Predmety z drahých kovov', N'r.14 - Predmety z drahých kovov (033) - (092AÚ)', 13
    UNION ALL SELECT 69001, 15, N'4.', N'Stavby (021) - (081+092AÚ)', N'Structures (021) - (081+092A)', 0, N'Stavby', N'r.15 - Stavby (021) - (081+092AÚ)', 14
    UNION ALL SELECT 69001, 16, N'5.', N'Samostatné hnuteľné veci a súbory hnuteľných vecí (022) - (082+092AÚ)', N'Individual movable assets and sets of movable assets (022) - (082+092A)', 0, N'Samostatné hnuteľné veci a súbory hnuteľných vecí', N'r.16 - Samostatné hnuteľné veci a súbory hnuteľných vecí (022) - (082+092AÚ)', 15
    UNION ALL SELECT 69001, 17, N'6.', N'Dopravné prostriedky (023) - (083+092AÚ)', N'Means of transport (023) - (083+092A)', 0, N'Dopravné prostriedky', N'r.17 - Dopravné prostriedky (023) - (083+092AÚ)', 16
    UNION ALL SELECT 69001, 18, N'7.', N'Pestovateľské celky trvalých porastov (025) - (085+092AÚ)', N'Perennial crops (025) - (085+092A)', 0, N'Pestovateľské celky trvalých porastov', N'r.18 - Pestovateľské celky trvalých porastov (025) - (085+092AÚ)', 17
    UNION ALL SELECT 69001, 19, N'8.', N'Základné stádo a ťažné zvieratá (026) - (086+092AÚ)', N'Livestoc (026) - (086+092A)', 0, N'Základné stádo a ťažné zvieratá', N'r.19 - Základné stádo a ťažné zvieratá (026) - (086+092AÚ)', 18
    UNION ALL SELECT 69001, 20, N'9.', N'Drobný dlhodobý hmotný majetok (028) - (088+092AÚ)', N'Small non-current tangible assets (028) - (088+092A)', 0, N'Drobný dlhodobý hmotný majetok', N'r.20 - Drobný dlhodobý hmotný majetok (028) - (088+092AÚ)', 19
    UNION ALL SELECT 69001, 21, N'10.', N'Ostatný dlhodobý hmotný majetok (029) - (089+092AÚ)', N'Other property, plant and equipment (029) - (089+092A)', 0, N'Ostatný dlhodobý hmotný majetok', N'r.21 - Ostatný dlhodobý hmotný majetok (029) - (089+092AÚ)', 20
    UNION ALL SELECT 69001, 22, N'11.', N'Obstaranie dlhodobého hmotného majetku (042) - (094)', N'Acquisition of property, plant and equipment (042) - (094)', 0, N'Obstaranie dlhodobého hmotného majetku', N'r.22 - Obstaranie dlhodobého hmotného majetku (042) - (094)', 21
    UNION ALL SELECT 69001, 23, N'12.', N'Poskytnuté preddavky na dlhodobý hmotný majetok (052) - (095AÚ)', N'Advance payments made for property, plant and equipment (052) - (095A)', 0, N'Poskytnuté preddavky na dlhodobý hmotný majetok', N'r.23 - Poskytnuté preddavky na dlhodobý hmotný majetok (052) - (095AÚ)', 22
    UNION ALL SELECT 69001, 24, N'A.III.', N'Dlhodobý finančný majetok súčet (r. 025 až 032)', N'Non-current financial assets - total (lines 025 to 032)', 1, N'Dlhodobý finančný majetok', N'r.24 - Dlhodobý finančný majetok súčet (r. 025 až 032)', 23
    UNION ALL SELECT 69001, 25, N'A.III.1.', N'Podielové cenné papiere a podiely v dcérskej účtovnej jednotke (061) - (096AÚ)', N'Shares and ownership interests in subsidiary (061) - (096A)', 0, N'Podielové cenné papiere a podiely v dcérskej účtovnej jednotke', N'r.25 - Podielové cenné papiere a podiely v dcérskej účtovnej jednotke (061) - (096AÚ)', 24
    UNION ALL SELECT 69001, 26, N'2.', N'Podielové cenné papiere a podiely v spoločnosti s podstatným vplyvom (062) - (096AÚ)', N'Shares and ownership interests with significant influence over enterprises (062) - (096A)', 0, N'Podielové cenné papiere a podiely v spoločnosti s podstatným vplyvom', N'r.26 - Podielové cenné papiere a podiely v spoločnosti s podstatným vplyvom (062) - (096AÚ)', 25
    UNION ALL SELECT 69001, 27, N'3.', N'Realizovateľné cenné papiere a podiely (063) - (096AÚ)', N'Realisable securities and shares (063) - (096A)', 0, N'Realizovateľné cenné papiere a podiely', N'r.27 - Realizovateľné cenné papiere a podiely (063) - (096AÚ)', 26
    UNION ALL SELECT 69001, 28, N'4.', N'Dlhové cenné papiere držané do splatnosti (065) - (096AÚ)', N'Debt securities held up to their maturity (065) - (096A)', 0, N'Dlhové cenné papiere držané do splatnosti', N'r.28 - Dlhové cenné papiere držané do splatnosti (065) - (096AÚ)', 27
    UNION ALL SELECT 69001, 29, N'5.', N'Pôžičky účtovnej jednotke v konsolidovanom celku (066) - (096AÚ)', N'Loans to accounting entity within consolidated unit (066) - (096A)', 0, N'Pôžičky účtovnej jednotke v konsolidovanom celku', N'r.29 - Pôžičky účtovnej jednotke v konsolidovanom celku (066) - (096AÚ)', 28
    UNION ALL SELECT 69001, 30, N'6.', N'Ostatné pôžičky (067) - (096AÚ)', N'Other loans (067) - (096AA)', 0, N'Ostatné pôžičky', N'r.30 - Ostatné pôžičky (067) - (096AÚ)', 29
    UNION ALL SELECT 69001, 31, N'7.', N'Ostatný dlhodobý finančný majetok (069) - (096AÚ)', N'Other non-current financial assets (069) - (089+092A)', 0, N'Ostatný dlhodobý finančný majetok', N'r.31 - Ostatný dlhodobý finančný majetok (069) - (096AÚ)', 30
    UNION ALL SELECT 69001, 32, N'8.', N'Obstaranie dlhodobého finančného majetku (043) - (096AÚ)', N'Acquisition of non-current financial assets (043) - (096A)', 0, N'Obstaranie dlhodobého finančného majetku', N'r.32 - Obstaranie dlhodobého finančného majetku (043) - (096AÚ)', 31
    UNION ALL SELECT 69001, 33, N'B.', N'Obežný majetok r. 034 + r. 040 + r. 048+ r. 060 + r. 085+ r. 098 + r. 104', N'Current assets line 034 + line 040 + line 048 + line 060 + line 085 + line 098 + line 104', 1, N'Obežný majetok', N'r.33 - Obežný majetok r. 034 + r. 040 + r. 048+ r. 060 + r. 085+ r. 098 + r. 104', 32
    UNION ALL SELECT 69001, 34, N'B.I.', N'Zásoby súčet (r. 035 až 039)', N'Inventory - total (lines 035 to 039)', 1, N'Zásoby', N'r.34 - Zásoby súčet (r. 035 až 039)', 33
    UNION ALL SELECT 69001, 35, N'B.I.1.', N'Materiál (112 + 119) - (191)', N'Raw material (112 + 119) - (191)', 0, N'Materiál', N'r.35 - Materiál (112 + 119) - (191)', 34
    UNION ALL SELECT 69001, 36, N'2.', N'Nedokončená výroba a polotovary (121 + 122) - (192 + 193)', N'Work in progress and semi-finished products (121 + 122) - (192 + 193)', 0, N'Nedokončená výroba a polotovary', N'r.36 - Nedokončená výroba a polotovary (121 + 122) - (192 + 193)', 35
    UNION ALL SELECT 69001, 37, N'3.', N'Výrobky (123) - (194)', N'Finished goods (123) - (194)', 0, N'Výrobky', N'r.37 - Výrobky (123) - (194)', 36
    UNION ALL SELECT 69001, 38, N'4.', N'Zvieratá (124) - (195)', N'Animals (124) - (195)', 0, N'Zvieratá', N'r.38 - Zvieratá (124) - (195)', 37
    UNION ALL SELECT 69001, 39, N'5.', N'Tovar (132 + 133 + 139) - (196)', N'Merchandise (132 + 133 + 139) - (196)', 0, N'Tovar', N'r.39 - Tovar (132 + 133 + 139) - (196)', 38
    UNION ALL SELECT 69001, 40, N'B.II.', N'Zúčtovanie medzi subjektami verejnej správy súčet (r. 041 až r. 047)', N'Clearance between the public administration entities - total (lines 041 to 047)', 1, N'Zúčtovanie medzi subjektami verejnej správy', N'r.40 - Zúčtovanie medzi subjektami verejnej správy súčet (r. 041 až r. 047)', 39
    UNION ALL SELECT 72701, 1, N'50', N'Spotrebované nákupy (r. 002 až r. 005)', N'Consumed purchases - total (lines 002 to 005)', 1, NULL, NULL, 0
    UNION ALL SELECT 72701, 25, N'545', N'Ostatné pokuty, penále a úroky z omeškania', N'Other fines, penalties, and interest on late payment', 0, NULL, NULL, 24
    UNION ALL SELECT 72701, 26, N'546', N'Odpis pohľadávky', N'Receivable write off', 0, NULL, NULL, 25
    UNION ALL SELECT 72701, 27, N'548', N'Ostatné náklady na prevádzkovú činnosť', N'Other operating expenses', 0, NULL, NULL, 26
    UNION ALL SELECT 72701, 28, N'549', N'Manká a škody', N'Deficits and damages', 0, NULL, NULL, 27
    UNION ALL SELECT 72701, 29, N'55', N'Odpisy, rezervy a opravné položky z prevádzkovej činnosti a finančnej činnosti a zúčtovanie časového rozlíšenia (r. 030 + r. 031 + r. 036 + r. 039)', N'Depreciation, provisions and adjusting entries to operating and financial expenses, and accrual-based accounting - total (line 030 + line 031 + line 036 + line 039)', 1, NULL, NULL, 28
    UNION ALL SELECT 72701, 30, N'551', N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', N'Depreciation of non-current intangible assets and non-current tangible assets', 0, NULL, NULL, 29
    UNION ALL SELECT 72701, 31, NULL, N'Rezervy a opravné položky z prevádzkovej činnosti (r. 032 až r. 035)', N'Provisions and adjusting entries to operating expenses - total (lines 032 to 035)', 1, NULL, NULL, 30
    UNION ALL SELECT 72701, 32, N'552', N'Tvorba zákonných rezerv z prevádzkovej činnosti', N'Additions to legal provisions out of operations', 0, NULL, NULL, 31
    UNION ALL SELECT 72701, 33, N'553', N'Tvorba ostatných rezerv z prevádzkovej činnosti', N'Additions to other provisions out of operations', 0, NULL, NULL, 32
    UNION ALL SELECT 72701, 34, N'557', N'Tvorba zákonných opravných položiek z prevádzkovej činnosti', N'Additions to legal adjusting entries out of operations', 0, NULL, NULL, 33
    UNION ALL SELECT 72701, 35, N'558', N'Tvorba ostatných opravných položiek z prevádzkovej činnosti', N'Additions to other adjusting entries out of operations', 0, NULL, NULL, 34
    UNION ALL SELECT 72701, 36, NULL, N'Rezervy a opravné položky z finančnej činnosti (r. 037+ r. 038)', N'Provisions and adjusting entries to financial expenses - total (lines 037 to 038)', 1, NULL, NULL, 35
    UNION ALL SELECT 69001, 84, N'24.', N'Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy (372AÚ) - (391AÚ)', N'Transfers and other clearance with entities outside public administration (372A) - (391A)', 0, N'Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy', N'r.84 - Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy (372AÚ) - (391AÚ)', 83
    UNION ALL SELECT 69001, 85, N'B.V.', N'Finančné účty súčet (r. 086 až 097)', N'Financial accounts - total (lines 086 to 097)', 1, N'Finančné účty', N'r.85 - Finančné účty súčet (r. 086 až 097)', 84
    UNION ALL SELECT 69001, 86, N'B.V.1.', N'Pokladnica (211)', N'Cash (211)', 0, N'Pokladnica', N'r.86 - Pokladnica (211)', 85
    UNION ALL SELECT 69001, 87, N'2.', N'Ceniny (213)', N'Stamps and vouchers (213)', 0, N'Ceniny', N'r.87 - Ceniny (213)', 86
    UNION ALL SELECT 69001, 88, N'3.', N'Bankové účty (221AÚ +/- 261)', N'Bank accounts (221AA +/- 261)', 0, N'Bankové účty', N'r.88 - Bankové účty (221AÚ +/- 261)', 87
    UNION ALL SELECT 69001, 89, N'4.', N'Účty v bankách s dobou viazanosti dlhšou ako jeden rok (221AÚ)', N'Accounts in banks with term longer than one year (221A)', 0, N'Účty v bankách s dobou viazanosti dlhšou ako jeden rok', N'r.89 - Účty v bankách s dobou viazanosti dlhšou ako jeden rok (221AÚ)', 88
    UNION ALL SELECT 69001, 90, N'5.', N'Výdavkový rozpočtový účet (222)', N'Account of budgetary expenditures (222)', 0, N'Výdavkový rozpočtový účet', N'r.90 - Výdavkový rozpočtový účet (222)', 89
    UNION ALL SELECT 69001, 91, N'6.', N'Príjmový rozpočtový účet (223)', N'Account of budgetary revenue (223)', 0, N'Príjmový rozpočtový účet', N'r.91 - Príjmový rozpočtový účet (223)', 90
    UNION ALL SELECT 69001, 92, N'7.', N'Majetkové cenné papiere na obchodovanie (251) - (291AÚ)', N'Equity securities held for trading (251) - (291A)', 0, N'Majetkové cenné papiere na obchodovanie', N'r.92 - Majetkové cenné papiere na obchodovanie (251) - (291AÚ)', 91
    UNION ALL SELECT 69001, 93, N'8.', N'Dlhové cenné papiere na obchodovanie (253) - (291AÚ)', N'Debt securities held for trading (253) - (291A)', 0, N'Dlhové cenné papiere na obchodovanie', N'r.93 - Dlhové cenné papiere na obchodovanie (253) - (291AÚ)', 92
    UNION ALL SELECT 69001, 94, N'9.', N'Dlhové cenné papiere so splatnosťou do jedného roka držané do splatnosti (256) - (291AÚ)', N'Debt securities payable to one year held to maturity (256) - (291A)', 0, N'Dlhové cenné papiere so splatnosťou do jedného roka držané do splatnosti', N'r.94 - Dlhové cenné papiere so splatnosťou do jedného roka držané do splatnosti (256) - (291AÚ)', 93
    UNION ALL SELECT 69001, 95, N'10.', N'Ostatné realizovateľné cenné papiere (257) - (291AÚ)', N'Other realisable securities (257) - (291A)', 0, N'Ostatné realizovateľné cenné papiere', N'r.95 - Ostatné realizovateľné cenné papiere (257) - (291AÚ)', 94
    UNION ALL SELECT 94203, 1, NULL, N'Majetkové podiely', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 94203, 2, NULL, N'Podiely v dcérskych spoločnostiach', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 94203, 3, NULL, N'Podiely v spoločných podnikoch', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94203, 4, NULL, N'Podiely v pridružených podnikoch', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 94203, 5, NULL, N'Finančné zdroje poskytnuté pobočkám v zahraničí', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 94203, 6, NULL, N'Pozemky a stavby', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 94203, 7, NULL, N'z toho: investície v nehnuteľnostiach', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 94203, 8, NULL, N'neprevádzkové', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 94203, 9, NULL, N'Finančné nástroje v reálnej hodnote proti zisku a strate', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 94203, 10, NULL, N'Nederivátové', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 94203, 11, NULL, N'z toho: akcie, podielové listy a iné majetkové účasti', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 94203, 12, NULL, N'Derivátové', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 94203, 13, NULL, N'Finančné nástroje na predaj', NULL, 0, NULL, NULL, 12
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 94203 AS [TableErpId], 14 AS [RowNumber], NULL AS [Designation], N'z toho: akcie, podielové listy a iné majetkové účasti' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 13 AS [RowOrdinal]
    UNION ALL SELECT 94203, 15, NULL, N'Finančné nástroje držané do splatnosti', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 94203, 16, NULL, N'Finančné umiestnenie v mene poistených', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 94203, 17, NULL, N'Kladná reálna hodnota derivátových operácií na zabezpečenie', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 94203, 18, NULL, N'Poskytnuté úvery, vklady a iné pohľadávky', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 94203, 19, NULL, N'z toho: termínované vklady v bankách', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 94203, 20, NULL, N'pôžičky poskytnuté poisteným', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 94203, 21, NULL, N'Vklady pri aktívnom zaistení', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 94203, 22, NULL, N'Pohľadávky z poistenia a zaistenia', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 94203, 23, NULL, N'Voči poisteným', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 94203, 24, NULL, N'Zo spolupoistenia', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 94203, 25, NULL, N'Voči sprostredkovateľom', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 110103, 45, N'16.', N'Výsledok hospodárenia za účtovné obdobie', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 110103, 999, NULL, N'Kontrolné číslo', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 118004, 39, N'601', N'Tržby za vlastné výrobky', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 118004, 40, N'602', N'Tržby z predaja služieb', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 118004, 41, N'604', N'Tržby za predaný tovar', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 118004, 42, N'611', N'Zmena stavu zásob nedokončenej výroby', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 118004, 43, N'612', N'Zmena stavu zásob polotovarov', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 118004, 44, N'613', N'Zmena stavu zásob výrobkov', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 118004, 45, N'614', N'Zmena stavu zásob zvierat', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 118004, 46, N'621', N'Aktivácia materiálu a tovaru', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 118004, 47, N'622', N'Aktivácia vnútroorganizačných služieb', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 118004, 48, N'623', N'Aktivácia dlhodobého nehmotného majetku', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 118004, 49, N'624', N'Aktivácia dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 118004, 50, N'641', N'Zmluvné pokuty a penále', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 118004, 51, N'642', N'Ostatné pokuty a penále', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 118004, 52, N'643', N'Platby za odpísané pohľadávky', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 118004, 53, N'644', N'Úroky', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 118004, 54, N'645', N'Kurzové zisky', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 118004, 55, N'646', N'Prijaté dary', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 118004, 56, N'647', N'Osobitné výnosy', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 118004, 57, N'648', N'Zákonné poplatky', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 118004, 58, N'649', N'Iné ostatné výnosy', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 118004, 59, N'651', N'Tržby z predaja dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 118004, 60, N'652', N'Výnosy z dlhodobého finančného majetku', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 118004, 61, N'653', N'Tržby z predaja cenných papierov a podielov', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 118004, 62, N'654', N'Tržby z predaja materiálu', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 118004, 63, N'655', N'Výnosy z krátkodobého finančného majetku', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 118004, 64, N'656', N'Výnosy z použitia fondu', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 118004, 65, N'657', N'Výnosy z precenenia cenných papierov', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 118004, 66, N'658', N'Výnosy z nájmu majetku', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 118004, 67, N'661', N'Prijaté príspevky od organizačných zložiek', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 118004, 68, N'662', N'Prijaté príspevky od právnických osôb', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 118004, 69, N'663', N'Prijaté príspevky od fyzických osôb', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 118004, 70, N'664', N'Prijaté členské príspevky', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 118004, 71, N'665', N'Príspevky z podielu zaplatenej dane', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 118004, 72, N'667', N'Prijaté príspevky z verejných zbierok', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 118004, 73, N'691', N'Dotácie', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 118004, 74, NULL, N'Účtová trieda 6 spolu r. 39 až r. 73', NULL, 1, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 35
    UNION ALL SELECT 118004, 75, NULL, N'Výsledok hospodárenia pred zdanením r. 74 - r. 38', NULL, 1, NULL, NULL, 36
    UNION ALL SELECT 118004, 76, N'591', N'Daň z príjmov', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 118004, 77, N'595', N'Dodatočné odvody dane z príjmov', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 118004, 78, NULL, N'Výsledok hospodárenia po zdanení (r. 75 - (r. 76 + r. 77)) (+/-)', NULL, 1, NULL, NULL, 39
    UNION ALL SELECT 102, 65, N'60', N'Tržby za vlastné výkony a tovar (r. 066 až r. 068)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 102, 66, N'601', N'Tržby za vlastné výrobky', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 102, 67, N'602', N'Tržby z predaja služieb', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 102, 68, N'604', N'Tržby za tovar', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 102, 69, N'61', N'Zmena stavu vnútroorganizačných zásob (r. 070 až r. 073)', NULL, 1, NULL, NULL, 4
    UNION ALL SELECT 102, 70, N'611', N'Zmena stavu nedokončenej výroby', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 102, 71, N'612', N'Zmena stavu polotovarov', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 102, 72, N'613', N'Zmena stavu výrobkov', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 102, 73, N'614', N'Zmena stavu zvierat', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 102, 74, N'62', N'Aktivácia (r. 075 až r. 078)', NULL, 1, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 9
    UNION ALL SELECT 102, 75, N'621', N'Aktivácia materiálu a tovaru', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 102, 76, N'622', N'Aktivácia vnútroorganizačných služieb', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 102, 77, N'623', N'Aktivácia dlhodobého nehmotného majetku', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 102, 78, N'624', N'Aktivácia dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 102, 79, N'63', N'Daňové a colné výnosy a výnosy z poplatkov (r. 080 až r. 082)', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 102, 80, N'631', N'Daňové a colné výnosy štátu', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 102, 81, N'632', N'Daňové výnosy samosprávy', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 102, 82, N'633', N'Výnosy z poplatkov', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 102, 83, N'64', N'Ostatné výnosy z prevádzkovej činnosti (r. 084 až r. 089)', NULL, 1, NULL, NULL, 18
    UNION ALL SELECT 102, 84, N'641', N'Tržby z predaja dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 102, 85, N'642', N'Tržby z predaja materiálu', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 102, 86, N'644', N'Zmluvné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 102, 87, N'645', N'Ostatné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 102, 88, N'646', N'Výnosy z odpísaných pohľadávok', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 102, 89, N'648', N'Ostatné výnosy z prevádzkovej činnosti', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 102, 90, N'65', N'Zúčtovanie rezerv a opravných položiek z prevádzkovej činnosti a finančnej činnosti a zúčtovanie časového rozlíšenia (r. 091 + r. 096 +r. 099)', NULL, 1, NULL, NULL, 25
    UNION ALL SELECT 102, 91, NULL, N'Zúčtovanie rezerv a opravných položiek z prevádzkovej činnosti (r. 092 až r. 095)', NULL, 1, NULL, NULL, 26
    UNION ALL SELECT 102, 92, N'652', N'Zúčtovanie zákonných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 102, 93, N'653', N'Zúčtovanie ostatných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 102, 94, N'657', N'Zúčtovanie zákonných opravných položiek z prevádzkovej činnosti', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 102, 95, N'658', N'Zúčtovanie ostatných opravných položiek z prevádzkovej činnosti', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 102, 96, NULL, N'Zúčtovanie rezerv a opravných položiek z finančnej činnosti (r. 097 + r. 098)', NULL, 1, NULL, NULL, 31
    UNION ALL SELECT 102, 97, N'654', N'Zúčtovanie rezerv z finančnej činnosti', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 102, 98, N'659', N'Zúčtovanie opravných položiek z finančnej činnosti', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 102, 99, N'655', N'Zúčtovanie komplexných nákladov budúcich období', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 102, 100, N'66', N'Finančné výnosy (r. 101 až r. 108)', NULL, 1, NULL, NULL, 35
    UNION ALL SELECT 102, 101, N'661', N'Tržby z predaja cenných papierov a podielov', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 102, 102, N'662', N'Úroky', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 102, 103, N'663', N'Kurzové zisky', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 102, 104, N'664', N'Výnosy z precenenia cenných papierov', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 102, 105, N'665', N'Výnosy z dlhodobého finančného majetku', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 102, 106, N'666', N'Výnosy z krátkodobého finančného majetku', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 102, 107, N'667', N'Výnosy z derivátových operácií', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 102, 108, N'668', N'Ostatné finančné výnosy', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 102, 109, N'67', N'Mimoriadne výnosy (r. 110 až r. 113)', NULL, 1, NULL, NULL, 44
    UNION ALL SELECT 102, 110, N'672', N'Náhrady škôd', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 102, 111, N'674', N'Zúčtovanie rezerv', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 102, 112, N'678', N'Ostatné mimoriadne výnosy', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 102, 113, N'679', N'Zúčtovanie opravných položiek', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 102, 114, N'68', N'Výnosy z transferov a rozpočtových príjmov v štátnych rozpočtových organizáciách a príspevkových organizáciách (r. 115 až r. 123)', NULL, 1, NULL, NULL, 49
    UNION ALL SELECT 102, 115, N'681', N'Výnosy z bežných transferov zo štátneho rozpočtu', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 102, 116, N'682', N'Výnosy z kapitálových transferov zo štátneho rozpočtu', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 102, 117, N'683', N'Výnosy z bežných transferov od ostatných subjektov verejnej správy', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 102, 118, N'684', N'Výnosy z kapitálových transferov od ostatných subjektov verejnej správy', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 102, 119, N'685', N'Výnosy z bežných transferov od Európskych spoločenstiev', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 102, 120, N'686', N'Výnosy z kapitálových transferov od Európskych spoločenstiev', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 102, 121, N'687', N'Výnosy z bežných transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 102, 122, N'688', N'Výnosy z kapitálových transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 102, 123, N'689', N'Výnosy z odvodu rozpočtových príjmov', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 102, 124, N'69', N'Výnosy z transferov a rozpočtových príjmov v obciach, vyšších územných celkoch a v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom (r. 125 až r. 133)', NULL, 1, NULL, NULL, 59
    UNION ALL SELECT 102, 125, N'691', N'Výnosy z bežných transferov z rozpočtu obce alebo z rozpočtu vyššieho územného celku v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 102, 126, N'692', N'Výnosy z kapitálových transferov z rozpočtu obce alebo z rozpočtu vyššieho územného celku v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 102, 127, N'693', N'Výnosy samosprávy z bežných transferov zo štátneho rozpočtu a od iných subjektov verejnej správy', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 102, 128, N'694', N'Výnosy samosprávy z kapitálových transferov zo štátneho rozpočtu a od iných subjektov verejnej správy', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 102, 129, N'695', N'Výnosy samosprávy z bežných transferov od Európskych spoločenstiev', NULL, 0, NULL, NULL, 64
    UNION ALL SELECT 102, 130, N'696', N'Výnosy samosprávy z kapitálových transferov od Európskych spoločenstiev', NULL, 0, NULL, NULL, 65
    UNION ALL SELECT 102, 131, N'697', N'Výnosy samosprávy z bežných transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 66
    UNION ALL SELECT 102, 132, N'698', N'Výnosy samosprávy z kapitálových transferov od ostatných subjektov mimo verejnej správy', NULL, 0, NULL, NULL, 67
    UNION ALL SELECT 102, 133, N'699', N'Výnosy samosprávy z odvodu rozpočtových príjmov', NULL, 0, NULL, NULL, 68
    UNION ALL SELECT 102, 134, NULL, N'Účtová trieda 6 celkom súčet (r. 065 + r. 069 + r. 074 + r. 079 + r. 083 + r. 090 + r. 100 + r. 109 + r. 114 + r. 124)', NULL, 1, NULL, NULL, 69
    UNION ALL SELECT 1201, 1, N'50', N'Spotrebované nákupy (r. 002 až r. 005)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 1201, 2, N'501', N'Spotreba materiálu', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 1201, 3, N'502', N'Spotreba energie', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1201, 4, N'503', N'Spotreba ostatných neskladovateľných dodávok', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1201, 5, N'504', N'Predaný tovar', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1201, 6, N'51', N'Služby (r. 007 až r. 010)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 1201, 7, N'511', N'Opravy a udržiavanie', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1201, 8, N'512', N'Cestovné', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1201, 9, N'513', N'Náklady na reprezentáciu', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 1201, 10, N'518', N'Ostatné služby', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 1201, 11, N'52', N'Osobné náklady (r. 012 až r. 016)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 1201, 12, N'521', N'Mzdové náklady', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 1201, 13, N'524', N'Zákonné sociálne poistenie', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 1201, 14, N'525', N'Ostatné sociálne poistenie', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 1201, 15, N'527', N'Zákonné sociálne náklady', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 1201, 16, N'528', N'Ostatné sociálne náklady', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 1201, 17, N'53', N'Dane a poplatky (r. 018 až r. 020)', NULL, 1, NULL, NULL, 16
    UNION ALL SELECT 1201, 18, N'531', N'Daň z motorových vozidiel', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 1201, 19, N'532', N'Daň z nehnuteľnosti', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 1201, 20, N'538', N'Ostatné dane a poplatky', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 1201, 21, N'54', N'Ostatné náklady na prevádzkovú činnosť (r. 022 až r. 028)', NULL, 1, NULL, NULL, 20
    UNION ALL SELECT 1201, 22, N'541', N'Zostatková cena predaného dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 1201, 23, N'542', N'Predaný materiál', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 1201, 24, N'544', N'Zmluvné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 1201, 25, N'545', N'Ostatné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 1201, 26, N'546', N'Odpis pohľadávky', NULL, 0, NULL, NULL, 25
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 1201 AS [TableErpId], 27 AS [RowNumber], N'548' AS [Designation], N'Ostatné náklady na prevádzkovú činnosť' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 26 AS [RowOrdinal]
    UNION ALL SELECT 1201, 28, N'549', N'Manká a škody', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 1201, 29, N'55', N'Odpisy, rezervy a opravné položky z prevádzkovej činnosti a finančnej činnosti a zúčtovanie časového rozlíšenia (r. 030 + r. 031 + r. 036 + r. 039)', NULL, 1, NULL, NULL, 28
    UNION ALL SELECT 1201, 30, N'551', N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 1201, 31, NULL, N'Rezervy a opravné položky z prevádzkovej činnosti (r. 032 až r. 035)', NULL, 1, NULL, NULL, 30
    UNION ALL SELECT 1201, 32, N'552', N'Tvorba zákonných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 1201, 33, N'553', N'Tvorba ostatných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 1201, 34, N'557', N'Tvorba zákonných opravných položiek z prevádzkovej činnosti', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 8202, 16, NULL, N'Rezervy', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 8202, 17, NULL, N'Záväzky', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 8202, 18, NULL, N'Úvery', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 8202, 19, NULL, N'Opravná položka k nadobudnutému majetku (pasívna)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 8202, 20, NULL, N'Záväzky celkom súčet (r. 16 až 19)', NULL, 1, NULL, NULL, 4
    UNION ALL SELECT 8202, 21, NULL, N'Rozdiel majetku a záväzkov (r. 15 - r. 20)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 66102, 1, NULL, N'Výnosy z prevádzkovej činnosti, z toho:', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 66102, 2, NULL, N'tržby', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 66102, 3, NULL, N'Náklady na prevádzkovú činnosť', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 66102, 4, NULL, N'Výsledok hospodárenia z prevádzkovej činnosti(+/-), (r.01 - r. 03)', NULL, 1, NULL, NULL, 3
    UNION ALL SELECT 66102, 5, NULL, N'Finančné výnosy', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 66102, 6, NULL, N'Finančné náklady', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 66102, 7, NULL, N'Výsledok hospodárenia z pokračujúcich činností pred zdanením (+/-), (r. 04 + r. 05 - r. 06)', NULL, 1, NULL, NULL, 6
    UNION ALL SELECT 66102, 8, NULL, N'Daň z príjmu', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 66102, 9, NULL, N'Výsledok hospodárenia z pokračujúcich činností po zdanení (+/-), (r. 07 - r. 08)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 66102, 10, NULL, N'Výsledok hospodárenia z ukončených činností pred zdanením (+/-)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 66102, 11, NULL, N'Daň z príjmu', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 66102, 12, NULL, N'Výsledok hospodárenia z ukončených činností po zdanení (+/-), (r. 10 - r. 11)', NULL, 1, NULL, NULL, 11
    UNION ALL SELECT 66102, 13, NULL, N'Výsledok hospodárenia za účtovné obdobie po zdanení (+/-), (r. 09 + r. 12)', NULL, 1, NULL, NULL, 12
    UNION ALL SELECT 66102, 14, NULL, N'Ostatné súčasti komplexného výsledku', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 66102, 15, NULL, N'Celkový komplexný výsledok za účtovné obdobie po zdanení (+/-), (r. 13 + r. 14)', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 118001, 1, N'A.', N'NEOBEŽNÝ MAJETOK SPOLU r. 002 + r. 009 + r. 021', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 118001, 2, N'A.I.', N'Dlhodobý nehmotný majetok r. 003 až r. 008', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 118001, 3, N'A.I.1.', N'Nehmotné výsledky z vývojovej a obdobnej činnosti 012-(072+091AÚ)', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 118001, 4, N'2.', N'Softvér 013 - (073 + 091AÚ)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 118001, 5, N'3.', N'Oceniteľné práva 014 - (074 + 091AÚ)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 118001, 6, N'4.', N'Ostatný dlhodobý nehmotný majetok (018 + 019)-(078 + 079 + 091 AÚ)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 118001, 7, N'5.', N'Obstaranie dlhodobého nehmotného majetku (041 - 093)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 118001, 8, N'6.', N'Poskytnuté preddavky na dlhodobý nehmotný majetok (051- 095AÚ)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 118001, 9, N'A.II.', N'Dlhodobý hmotný majetok r. 010 až r. 020', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 118001, 10, N'A.II.1', N'Pozemky (031)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 118001, 11, N'2.', N'Umelecké diela a zbierky (032)', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 118001, 12, N'3.', N'Stavby 021 - (081 + 092AÚ)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 118001, 13, N'4.', N'Samostatné hnuteľné veci a súbory hnuteľných vecí 022 - (082 + 092AÚ)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 118001, 14, N'5.', N'Dopravné prostriedky 023 - (083 + 092AÚ)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 118001, 15, N'6.', N'Pestovateľské celky trvalých porastov 025 - (085 + 092AÚ)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 118001, 16, N'7.', N'Základné stádo a ťažné zvieratá 026 - (086 + 092AÚ)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 118001, 17, N'8.', N'Drobný dlhodobý hmotný majetok 028 - (088 + 092AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 118001, 18, N'9.', N'Ostatný dlhodobý hmotný majetok 029 - (089 +092AÚ)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 118001, 19, N'10.', N'Obstaranie dlhodobého hmotného majetku (042 - 094)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 118001, 20, N'11.', N'Poskytnuté preddavky na dlhodobý hmotný majetok (052 - 095AÚ)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 118001, 21, N'A.III.', N'Dlhodobý finančný majetok r. 022 až r. 028', NULL, 1, NULL, NULL, 20
    UNION ALL SELECT 118001, 22, N'A.III.1', N'Podielové cenné papiere a podiely v obchodných spoločnostiach v ovládanej osobe (061- 096 AÚ)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 118001, 23, N'2.', N'Podielové cenné papiere a podiely v obchodných spoločnostiach s podstatným vplyvom (062 - 096 AÚ)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 118001, 24, N'3.', N'Dlhové cenné papiere držané do splatnosti (065 - 096 AÚ)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 118001, 25, N'4.', N'Pôžičky podnikom v skupine a ostatné pôžičky (066 + 067) - 096 AÚ', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 102, 135, NULL, N'Výsledok hospodárenia pred zdanením ( r. 134 mínus r. 064) (+/-)', NULL, 1, NULL, NULL, 70
    UNION ALL SELECT 102, 136, N'591', N'Splatná daň z príjmov', NULL, 0, NULL, NULL, 71
    UNION ALL SELECT 102, 137, N'595', N'Dodatočne platená daň z príjmov', NULL, 0, NULL, NULL, 72
    UNION ALL SELECT 102, 138, NULL, N'Výsledok hospodárenia po zdanení r. 135 mínus (r. 136, r. 137) (+/-)', NULL, 1, NULL, NULL, 73
    UNION ALL SELECT 102, 995, NULL, N'Kontrolné číslo súčet (r. 065 až r. 138)', NULL, 1, NULL, NULL, 74
    UNION ALL SELECT 1201, 54, N'1579', N'Tvorba opravných položiek', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 1201, 55, N'58', N'Náklady na transfery a náklady z odvodu príjmov (r. 056 až r. 064)', NULL, 1, NULL, NULL, 54
    UNION ALL SELECT 1201, 56, N'581', N'Náklady na transfery zo štátneho rozpočtu do štátnych rozpočtových organizácií a príspevkových organizácií', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 1201, 57, N'582', N'Náklady na transfery zo štátneho rozpočtu ostatným subjektom verejnej správy', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 1201, 58, N'583', N'Náklady na transfery zo štátneho rozpočtu subjektom mimo verejnej správy', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 1201, 59, N'584', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku do rozpočtových organizácií a príspevkových organizácií zriadených obcou alebo vyšším územným celkom', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 1201, 60, N'585', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku ostatným subjektom verejnej správy', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 1201, 61, N'586', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku subjektom mimo verejnej správy', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 1201, 62, N'587', N'Náklady na ostatné transfery', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 1201, 63, N'588', N'Náklady z odvodu príjmov', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 1201, 64, N'589', N'Náklady z budúceho odvodu príjmov', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 1201, 65, NULL, N'Účtové skupiny 50 - 58 celkom súčet (r. 001 + r. 006 + r. 011 + r. 017 + r. 021 + r. 029 + r. 040 + r. 050 + r. 055)', NULL, 1, NULL, NULL, 64
    UNION ALL SELECT 1201, 994, NULL, N'Kontrolné číslo súčet (r. 001 až r. 065)', NULL, 1, NULL, NULL, 65
    UNION ALL SELECT 1901, 1, N'501', N'Spotreba materiálu', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 1901, 2, N'502', N'Spotreba energie', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 1901, 3, N'504', N'Predaný tovar', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1901, 4, N'511', N'Opravy a udržiavanie', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1901, 5, N'512', N'Cestovné', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1901, 6, N'513', N'Náklady na reprezentáciu', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1901, 7, N'518', N'Ostatné služby', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1901, 8, N'521', N'Mzdové náklady', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1901, 9, N'524', N'Zákonné sociálne poistenie a zdravotné poistenie', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 1901, 10, N'525', N'Ostatné sociálne poistenie', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 1901, 11, N'527', N'Zákonné sociálne náklady', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 1901, 12, N'528', N'Ostatné sociálne náklady', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 1901, 13, N'531', N'Daň z motorových vozidiel', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 1901, 14, N'532', N'Daň z nehnuteľností', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 1901, 15, N'538', N'Ostatné dane a poplatky', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 1901, 16, N'541', N'Zmluvné pokuty a penále', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 1901, 17, N'542', N'Ostatné pokuty a penále', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 1901, 18, N'543', N'Odpísanie pohľadávky', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 1901, 19, N'544', N'Úroky', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 1901, 20, N'545', N'Kurzové straty', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 1901, 21, N'546', N'Dary', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 1901, 22, N'547', N'Osobitné náklady', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 1901, 23, N'548', N'Manká a škody', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 1901, 24, N'549', N'Iné ostatné náklady', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 1901, 25, N'551', N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 1901, 26, N'552', N'Zostatková cena predaného dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 1901, 27, N'553', N'Predané cenné papiere', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 1901, 28, N'554', N'Predaný materiál', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 1901, 29, N'555', N'Náklady na krátkodobý finančný majetok', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 1901, 30, N'556', N'Tvorba fondov', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 1901, 31, N'557', N'Náklady na precenenie cenných papierov', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 1901, 32, N'558', N'Tvorba a zúčtovanie opravných položiek', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 1901, 33, N'561', N'Poskytnuté príspevky organizačným zložkám', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 1901, 34, N'562', N'Poskytnuté príspevky iným účtovným jednotkám', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 6101, 1, NULL, N'Predaj tovaru', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 6101, 2, NULL, N'Predaj výrobkov a služieb', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 6101, 3, NULL, N'Ostatné príjmy', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 6101, 4, NULL, N'Príjmy celkom súčet (r. 01 až 03)', NULL, 1, NULL, NULL, 3
    UNION ALL SELECT 68401, 11, N'8.', N'Poskytnuté preddavky na dlhodobý nehmotný majetok (051) - (095AÚ)', N'Advance payments made for non-current intangible assets (051) - (095A)', 0, NULL, NULL, 10
    UNION ALL SELECT 68401, 12, N'A.II.', N'Dlhodobý hmotný majetok súčet (r. 013 až 024)', N'Property, plant and equipment - total (lines 013 to 024)', 1, NULL, NULL, 11
    UNION ALL SELECT 68401, 13, N'A.II.1.', N'Pozemky (031) - (092AÚ)', N'Land (031) - (092AA)', 0, NULL, NULL, 12
    UNION ALL SELECT 68401, 14, N'2.', N'Umelecké diela a zbierky (032) - (092AÚ)', N'Works of art and collections (032) - (092A)', 0, NULL, NULL, 13
    UNION ALL SELECT 68401, 15, N'3.', N'Predmety z drahých kovov (033) - (092AÚ)', N'Objects of precious metals (033) - (092A)', 0, NULL, NULL, 14
    UNION ALL SELECT 68401, 16, N'4.', N'Stavby (021) - (081+092AÚ)', N'Structures (021) - (081+092A)', 0, NULL, NULL, 15
    UNION ALL SELECT 68401, 17, N'5.', N'Samostatné hnuteľné veci a súbory hnuteľných vecí (022)-(082+092AÚ)', N'Individual movable assets and sets of movable assets (022) - (082+092A)', 0, NULL, NULL, 16
    UNION ALL SELECT 68401, 18, N'6.', N'Dopravné prostriedky (023) - (083+092AÚ)', N'Means of transport (023) - (083+092A)', 0, NULL, NULL, 17
    UNION ALL SELECT 68401, 19, N'7.', N'Pestovateľské celky trvalých porastov (025) - (085+092AÚ)', N'Perennial crops (025) - (085+092A)', 0, NULL, NULL, 18
    UNION ALL SELECT 68401, 20, N'8.', N'Základné stádo a ťažné zvieratá (026) - (086+092AÚ)', N'Livestoc (026) - (086+092A)', 0, NULL, NULL, 19
    UNION ALL SELECT 68401, 21, N'9.', N'Drobný dlhodobý hmotný majetok (028) - (088+092AÚ)', N'Small non-current tangible assets (028) - (088+092A)', 0, NULL, NULL, 20
    UNION ALL SELECT 68401, 22, N'10.', N'Ostatný dlhodobý hmotný majetok (029) - (089+092AÚ)', N'Other property, plant and equipment (029) - (089+092A)', 0, NULL, NULL, 21
    UNION ALL SELECT 68401, 23, N'11.', N'Obstaranie dlhodobého hmotného majetku (042) - (094)', N'Acquisition of property, plant and equipment (042) - (094)', 0, NULL, NULL, 22
    UNION ALL SELECT 68401, 24, N'12.', N'Poskytnuté preddavky na dlhodobý hmotný majetok (052) - (095AÚ)', N'Advance payments made for property, plant and equipment (052) - (095A)', 0, NULL, NULL, 23
    UNION ALL SELECT 118001, 26, N'5.', N'Ostatný dlhodobý finančný majetok (069 - 096 AÚ)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 118001, 27, N'6.', N'Obstaranie dlhodobého finančného majetku (043 - 096 AÚ)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 118001, 28, N'7.', N'Poskytnuté preddavky na dlhodobý finančný majetok (053 - 096 AÚ)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 118001, 29, N'B.', N'OBEŽNÝ MAJETOK SPOLU r. 030+ r. 037+ r. 042 + r. 051', NULL, 1, NULL, NULL, 28
    UNION ALL SELECT 118001, 30, N'B.I.', N'Zásoby r. 031 až r. 036', NULL, 1, NULL, NULL, 29
    UNION ALL SELECT 118001, 31, N'B.I.1.', N'Materiál (112 + 119) - 191', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 118001, 32, N'2.', N'Nedokončená výroba a polotovary vlastnej výroby (121+122)-(192+193)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 118001, 33, N'3.', N'Výrobky (123 - 194)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 118001, 34, N'4.', N'Zvieratá (124 - 195)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 118001, 35, N'5.', N'Tovar (132 + 139) - 196', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 118001, 36, N'6.', N'Poskytnuté prevádzkové preddavky na zásoby (314 AÚ - 391 AÚ)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 118001, 37, N'B.II.', N'Dlhodobé pohľadávky r. 038 až r. 041', NULL, 1, NULL, NULL, 36
    UNION ALL SELECT 118001, 38, N'1.', N'Pohľadávky z obchodného styku (311 AÚ až 314 AÚ) - 391 AÚ', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 118001, 39, N'2.', N'Ostatné pohľadávky (315 AÚ - 391AÚ)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 118001, 40, N'3.', N'Pohľadávky voči účastníkom združení (358AÚ - 391AÚ)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 118001, 41, N'4.', N'Iné pohľadávky (335 AÚ + 373 AÚ + 375 AÚ + 378AÚ) - 391AÚ', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 118001, 42, N'B.III.', N'Krátkodobé pohľadávky r. 043 až r. 050', NULL, 1, NULL, NULL, 41
    UNION ALL SELECT 118001, 43, N'B.III.1.', N'Pohľadávky z obchodného styku (311AÚ až 314 AÚ) - 391AÚ', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 118001, 44, N'2.', N'Ostatné pohľadávky (315 AÚ - 391 AÚ)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 118001, 45, N'3.', N'Zúčtovanie so Sociálnou poisťovňou a zdravotnými poisťovňami (336)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 118001, 46, N'4.', N'Daňové pohľadávky (341 až 345)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 118001, 47, N'5.', N'Pohľadávky z dôvodu finančných vzťahov k štátnemu rozpočtu a rozpočtom územnej samosprávy (346+ 348)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 118001, 48, N'6.', N'Pohľadávky voči účastníkom združení (358 AÚ - 391AÚ)', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 118001, 49, N'7.', N'Spojovací účet pri združení (396 - 391AÚ)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 118001, 50, N'8.', N'Iné pohľadávky (335AÚ + 373AÚ + 375AÚ + 378AÚ) - 391AÚ', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 118001, 51, N'B.IV.', N'Finančné účty r. 052 až r. 056', NULL, 1, NULL, NULL, 50
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 118001 AS [TableErpId], 52 AS [RowNumber], N'B.IV.1.' AS [Designation], N'Pokladnica (211 + 213)' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 51 AS [RowOrdinal]
    UNION ALL SELECT 118001, 53, N'2.', N'Bankové účty (221 AÚ + 261)', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 118001, 54, N'3.', N'Bankové účty s dobou viazanosti dlhšou ako jeden rok (221 AÚ)', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 118001, 55, N'4.', N'Krátkodobý finančný majetok (251 + 253 + 255AÚ + 256 + 257) - 291AÚ', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 118001, 56, N'5.', N'Obstaranie krátkodobého finančného majetku (259 - 291AÚ)', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 118001, 57, N'C.', N'ČASOVÉ ROZLÍŠENIE SPOLU r. 058 a r. 059', NULL, 1, NULL, NULL, 56
    UNION ALL SELECT 118001, 58, N'C.1.', N'Náklady budúcich období (381)', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 118001, 59, N'2.', N'Príjmy budúcich období (385)', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 118001, 60, NULL, N'MAJETOK SPOLU r. 001 + r. 029 + r. 057', NULL, 1, NULL, NULL, 59
    UNION ALL SELECT 68401, 66, N'3.', N'Pohľadávky za eskontované cenné papiere (313AÚ) - (391AÚ)', N'Receivables for discounted securities (313A) - (391A)', 0, NULL, NULL, 65
    UNION ALL SELECT 68401, 67, N'4.', N'Poskytnuté prevádzkové preddavky (314) - (391AÚ)', N'Provided advance payments (314) - (391A)', 0, NULL, NULL, 66
    UNION ALL SELECT 68401, 68, N'5.', N'Ostatné pohľadávky (315AÚ) - (391AÚ)', N'Other receivables (315A) - (391A)', 0, NULL, NULL, 67
    UNION ALL SELECT 68401, 69, N'6.', N'Pohľadávky z nedaňových rozpočtových príjmov (316) - (391AÚ)', N'Receivables from non-tax revenue (316) - (391A)', 0, NULL, NULL, 68
    UNION ALL SELECT 68401, 70, N'7.', N'Pohľadávky z daňových a colných rozpočtových príjmov (317) - (391AÚ)', N'Receivables from tax and customs revenue (317) - (391A)', 0, NULL, NULL, 69
    UNION ALL SELECT 68401, 71, N'8.', N'Pohľadávky z nedaňových príjmov obcí a vyšších územných celkov a rozpočtových organizácií zriadených obcou a vyšším územným celkom (318) - (391AÚ)', N'Receivables from non-tax revenue of municipalities and higher territorial units and state-funded organisations founded by municipality and higher territorial unit (318) - (391A)', 0, NULL, NULL, 70
    UNION ALL SELECT 68401, 72, N'9.', N'Pohľadávky z daňových príjmov obcí a vyšších územných celkov (319) - (391AÚ)', N'Receivables from tax revenue of municipalities and higher territorial units (319) - (391A)', 0, NULL, NULL, 71
    UNION ALL SELECT 68401, 73, N'10.', N'Pohľadávky voči zamestnancom (335AÚ) - (391AÚ)', N'Receivables to employees (335A) - (391A)', 0, NULL, NULL, 72
    UNION ALL SELECT 68401, 74, N'11.', N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia (336) - (391AÚ)', N'Clearing with social and health insurance institutions (336) - (391A)', 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 73
    UNION ALL SELECT 68401, 75, N'12.', N'Daň z príjmov (341) - (391AÚ)', N'Income tax (341) - (391A)', 0, NULL, NULL, 74
    UNION ALL SELECT 68401, 76, N'13.', N'Ostatné priame dane (342) - (391AÚ)', N'Other direct taxes (342) - (391A)', 0, NULL, NULL, 75
    UNION ALL SELECT 68401, 77, N'14.', N'Daň z pridanej hodnoty (343) - (391AÚ)', N'Value added tax (343) - (391A)', 0, NULL, NULL, 76
    UNION ALL SELECT 68401, 78, N'15.', N'Ostatné dane a poplatky (345) - (391AÚ)', N'Other taxes and fees (345) - (391A)', 0, NULL, NULL, 77
    UNION ALL SELECT 68401, 79, N'16.', N'Pohľadávky voči združeniu (369AÚ) - (391AÚ)', N'Receivables from participants in association (369A) - (391A)', 0, NULL, NULL, 78
    UNION ALL SELECT 68401, 80, N'17.', N'Pohľadávky a záväzky z pevných termínovaných operácií (373AÚ) - (391AÚ)', N'Receivables and liabilities from fixed term transactions (373A) - (391A)', 0, NULL, NULL, 79
    UNION ALL SELECT 68401, 81, N'18.', N'Pohľadávky z nájmu (374AÚ) - (391AÚ)', N'Receivables from leasing (374A) - (391A)', 0, NULL, NULL, 80
    UNION ALL SELECT 68401, 82, N'19.', N'Pohľadávky z vydaných dlhopisov (375AÚ) - (391AÚ)', N'Receivables from issued bonds (375A) - (391A)', 0, NULL, NULL, 81
    UNION ALL SELECT 68401, 83, N'20.', N'Nakúpené opcie (376AÚ) - (391AÚ)', N'Options purchased (376A) - (391A)', 0, NULL, NULL, 82
    UNION ALL SELECT 68401, 84, N'21.', N'Iné pohľadávky (378AÚ) - (391AÚ)', N'Other receivables (378A) - (391A)', 0, NULL, NULL, 83
    UNION ALL SELECT 68401, 85, N'22.', N'Spojovací účet pri združení (396AÚ)', N'Control account at association (396A)', 0, NULL, NULL, 84
    UNION ALL SELECT 68401, 86, N'23.', N'Zúčtovanie s Európskou úniou (371AÚ)- (391AÚ)', N'Clearing with the European Union (371A)- (391A)', 0, NULL, NULL, 85
    UNION ALL SELECT 68401, 87, N'24.', N'Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy (372AÚ) - (391AÚ)', N'Transfers and other clearance with entities outside public administration (372A) - (391A)', 0, NULL, NULL, 86
    UNION ALL SELECT 68401, 88, N'B.V.', N'Finančné účty súčet (r. 089 až 100)', N'Financial accounts - total (lines 089 to 100)', 1, NULL, NULL, 87
    UNION ALL SELECT 1201, 35, N'558', N'Tvorba ostatných opravných položiek z prevádzkovej činnosti', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 1201, 36, NULL, N'Rezervy a opravné položky z finančnej činnosti (r. 037+ r. 038)', NULL, 1, NULL, NULL, 35
    UNION ALL SELECT 1201, 37, N'554', N'Tvorba rezerv z finančnej činnosti', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 1201, 38, N'559', N'Tvorba opravných položiek z finančnej činnosti', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 1201, 39, N'555', N'Zúčtovanie komplexných nákladov budúcich období', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 1201, 40, N'56', N'Finančné náklady (r. 041 až r. 049)', NULL, 1, NULL, NULL, 39
    UNION ALL SELECT 1201, 41, N'561', N'Predané cenné papiere a podiely', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 1201, 42, N'562', N'Úroky', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 1201, 43, N'563', N'Kurzové straty', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 1201, 44, N'564', N'Náklady na precenenie cenných papierov', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 1201, 45, N'566', N'Náklady na krátkodobý finančný majetok', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 1201, 46, N'567', N'Náklady na derivátové operácie', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 1201, 47, N'568', N'Ostatné finančné náklady', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 1201, 48, N'569', N'Manká a škody na finančnom majetku', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 1201, 49, NULL, N'Podiel konsolidujúcej účtovnej jednotky na výsledku hospodárenia pridružených účtovných jednotiek verejnej správy', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 1201, 50, N'57', N'Mimoriadne náklady (r. 051 až r. 054)', NULL, 1, NULL, NULL, 49
    UNION ALL SELECT 1201, 51, N'572', N'Škody', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 1201, 52, N'574', N'Tvorba rezerv', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 1201, 53, N'578', N'Ostatné mimoriadne náklady', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 1502, 12, NULL, N'Záväzky', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 1502, 13, NULL, N'z toho: sociálny fond', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 1502, 14, NULL, N'fond prevádzky, údržby a opráv', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1502, 15, NULL, N'Úvery', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1502, 16, NULL, N'Záväzky celkom (súčet r. 12 a r.15)', NULL, 1, NULL, NULL, 4
    UNION ALL SELECT 1502, 17, NULL, N'Rozdiel majetku a záväzkov (r. 11 - r.16)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 1901, 35, N'563', N'Poskytnuté príspevky fyzickým osobám', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 1901, 36, N'565', N'Poskytnuté príspevky z podielu zaplatenej dane', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 1901, 37, N'567', N'Poskytnuté príspevky z verejnej zbierky', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 1901, 38, NULL, N'Účtová trieda 5 spolu r. 01 až r. 37', NULL, 1, NULL, NULL, 37
    UNION ALL SELECT 1901, 994, NULL, N'Kontrolné číslo r. 01 až r. 38', NULL, 1, NULL, NULL, 38
    UNION ALL SELECT 518104, 1, N'1.', N'Priemerný evidenčný počet zamestnancov prepočítaný na plne zamestnaných', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 518104, 2, N'2.', N'Evidenčný počet zamestnancov prepočítaný na plne zamestnaných ku dňu, ku ktorému sa zostavuje účtovná závierka', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 518301, 1, NULL, N'SPOLU MAJETOK (r. 02 + r. 09)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 518301, 2, N'A.', N'Neobežný majetok (r. 03 + r.04 + r. 05 + r. 07 + r. 08)', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 518301, 3, N'A. I.', N'Dlhodobý nehmotný majetok', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 518301, 4, N'A. II.', N'Dlhodobý hmotný majetok', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 518301, 5, N'A. III.', N'Dlhodobý finančný majetok, z toho:', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 518301, 6, N'A. III.1', N'pohľadávky z obchodného styku', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 518301, 7, N'A. IV.', N'Ostatný majetok', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 518301, 8, N'A. V.', N'Odložená daňová pohľadávka', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 518301, 9, N'B.', N'Obežný majetok (r. 10 + r. 11 + r. 14)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 518301, 10, N'B. I.', N'Zásoby', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 518301, 11, N'B. II.', N'Krátkodobý finančný majetok, z toho:', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 518301, 12, N'B. II. 1', N'pohľadávky z obchodného styku', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 518301, 13, N'B. II. 2', N'peniaze a peňažné ekvivalenty', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 518301, 14, N'B. III.', N'Ostatný majetok, z toho:', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 518301, 15, N'B. III. 1', N'majetok klasifikovaný ako držaný na predaj', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 518301, 16, NULL, N'SPOLU VLASTNÉ IMANIE A ZÁVÄZKY(r. 17 + r. 26)', NULL, 1, NULL, NULL, 15
    UNION ALL SELECT 518301, 17, N'C.', N'Vlastné imanie (r. 18 + r. 19 + r. 20 + r. 21 + r. 24 + r. 25)', NULL, 1, NULL, NULL, 16
    UNION ALL SELECT 518301, 18, N'C. I.', N'Základné imanie', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 518301, 19, N'C. II.', N'Kapitálové fondy', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 518301, 20, N'C. III.', N'Rezervné fondy a ostatné fondy tvorené zo zisku', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 518301, 21, N'C. IV.', N'Výsledok hospodárenia minulých rokov (r. 22 + r. 23)', NULL, 1, NULL, NULL, 20
    UNION ALL SELECT 518301, 22, N'C. IV. 1', N'Nerozdelený zisk minulých rokov', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 518301, 23, N'C. IV. 2', N'Neuhradená strata minulých rokov', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 518301, 24, N'C. V.', N'Výsledok hospodárenia za účtovné obdobie po zdanení (+/-)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 518301, 25, N'C. VI.', N'Ostatné zložky vlastného imania', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 518301, 26, N'D.', N'Záväzky (r. 27 + r. 31 + r. 32)', NULL, 1, NULL, NULL, 25
    UNION ALL SELECT 518301, 27, N'D. I.', N'Dlhodobé záväzky, z toho:', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 518301, 28, N'D. I. 1', N'záväzky z obchodného styku', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 518301, 29, N'D. I. 2', N'úvery a pôžičky', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 518301, 30, N'D. I. 3', N'rezervy', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 518301, 31, N'D. II.', N'Odložený daňový záväzok', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 518301, 32, N'D. III.', N'Krátkodobé záväzky, z toho:', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 518301, 33, N'D. III. 1', N'záväzky z obchodného styku', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 518301, 34, N'D. III. 2', N'úvery a pôžičky', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 518301, 35, N'D. III. 3', N'rezervy', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 518301, 36, N'D. III. 4', N'záväzky spojené s majetkom klasifikovaným ako držaný na predaj', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 68401, 1, NULL, N'SPOLU MAJETOK r. 002 + r. 035 + r. 113 + r. 117', N'TOTAL ASSETS line 002 + line 035 + line 113 + line 117', 1, NULL, NULL, 0
    UNION ALL SELECT 68401, 2, N'A.', N'Neobežný majetok r. 003 + r. 012 + r. 025', N'Non-current assets line 003 + line 012 + line 025', 1, NULL, NULL, 1
    UNION ALL SELECT 68401, 3, N'A.I.', N'Dlhodobý nehmotný majetok súčet (r. 004 až 011)', N'Non-current intangible assets - total (lines 004 to 011)', 1, NULL, NULL, 2
    UNION ALL SELECT 68401, 4, N'A.I.1.', N'Aktivované náklady na vývoj (012) - (072+091AÚ)', N'Capitalized development costs (012) - (072+091A)', 0, NULL, NULL, 3
    UNION ALL SELECT 68401, 5, N'2.', N'Softvér (013) - (073+091AÚ)', N'Software (013) - (073+091A)', 0, NULL, NULL, 4
    UNION ALL SELECT 68401, 6, N'3.', N'Oceniteľné práva (014) - (074+091AÚ)', N'Valuable rights (014) - (074+091A)', 0, NULL, NULL, 5
    UNION ALL SELECT 68401, 7, N'4.', N'Goodwill z konsolidácie kapitálu alebo negatívny goodwill z konsolidácie kapitálu (+/-)', N'Goodwill (+/-)', 0, NULL, NULL, 6
    UNION ALL SELECT 68401, 8, N'5.', N'Drobný dlhodobý nehmotný majetok (018) - (078+091AÚ)', N'Small non-current intangible assets (018) - (078+091A)', 0, NULL, NULL, 7
    UNION ALL SELECT 68401, 9, N'6.', N'Ostatný dlhodobý nehmotný majetok (019) - (079+091AÚ)', N'Other non-current intangible assets (019) - (079+091A)', 0, NULL, NULL, 8
    UNION ALL SELECT 68401, 89, N'B.V.1.', N'Pokladnica (211)', N'Cash (211)', 0, NULL, NULL, 88
    UNION ALL SELECT 68401, 90, N'2.', N'Ceniny (213)', N'Stamps and vouchers (213)', 0, NULL, NULL, 89
    UNION ALL SELECT 68401, 91, N'3.', N'Bankové účty (221AÚ +/- 261)', N'Bank accounts (221AA +/- 261)', 0, NULL, NULL, 90
    UNION ALL SELECT 68401, 92, N'4.', N'Účty v bankách s dobou viazanosti dlhšou ako jeden rok (221AÚ)', N'Accounts in banks with term longer than one year (221A)', 0, NULL, NULL, 91
    UNION ALL SELECT 68401, 93, N'5.', N'Výdavkový rozpočtový účet (222)', N'Account of budgetary expenditures (222)', 0, NULL, NULL, 92
    UNION ALL SELECT 68401, 94, N'6.', N'Príjmový rozpočtový účet (223)', N'Account of budgetary revenue (223)', 0, NULL, NULL, 93
    UNION ALL SELECT 68401, 95, N'7.', N'Majetkové cenné papiere na obchodovanie (251) - (291AÚ)', N'Equity securities held for trading (251) - (291A)', 0, NULL, NULL, 94
    UNION ALL SELECT 68401, 96, N'8.', N'Dlhové cenné papiere na obchodovanie (253) - (291AÚ)', N'Debt securities held for trading (253) - (291A)', 0, NULL, NULL, 95
    UNION ALL SELECT 68401, 97, N'9.', N'Dlhové cenné papiere so splatnosťou do jedného roka držané do splatnosti (256) - (291AÚ)', N'Debt securities payable to one year held to maturity (256) - (291A)', 0, NULL, NULL, 96
    UNION ALL SELECT 68401, 98, N'10.', N'Ostatné realizovateľné cenné papiere (257) - (291AÚ)', N'Other realisable securities (257) - (291A)', 0, NULL, NULL, 97
    UNION ALL SELECT 68401, 99, N'11.', N'Obstaranie krátkodobého finančného majetku (259) - (291AÚ)', N'Acquisition of current financial assets (259) - (291A)', 0, NULL, NULL, 98
    UNION ALL SELECT 68401, 100, N'12.', N'Účty Štátnej pokladnice (účtová skupina 28)', N'State Treasury accounts (account group 28)', 0, NULL, NULL, 99
    UNION ALL SELECT 68401, 101, N'B.VI.', N'Poskytnuté návratné finančné výpomoci dlhodobé súčet (r. 102 až r. 106)', N'Provided non-current repayable financial assistance - total (lines 102 to 106)', 1, NULL, NULL, 100
    UNION ALL SELECT 68401, 102, N'B.VI.1.', N'Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku (271AÚ) - (291AÚ)', N'Repayable financial assistance provided to entities within consolidated unit (271A) - (291A)', 0, NULL, NULL, 101
    UNION ALL SELECT 68401, 103, N'2.', N'Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy (272AÚ) - (291AÚ)', N'Repayable financial assistance provided to other entities of general government (272A) - (291A)', 0, NULL, NULL, 102
    UNION ALL SELECT 68401, 104, N'3.', N'Poskytnuté návratné finančné výpomoci podnikateľským subjektom (274AÚ) - (291AÚ)', N'Repayable financial assistance provided to business entities (274A) - (291A)', 0, NULL, NULL, 103
    UNION ALL SELECT 68401, 105, N'4.', N'Poskytnuté návratné finančné výpomoci ostatným organizáciám (275AÚ) - (291AÚ)', N'Repayable financial assistance provided to other organisations (275A) - (291A)', 0, NULL, NULL, 104
    UNION ALL SELECT 68401, 106, N'5.', N'Poskytnuté návratné finančné výpomoci fyzickým osobám (277AÚ) - (291AÚ)', N'Repayable financial assistance provided to natural persons (277A) - (291A)', 0, NULL, NULL, 105
    UNION ALL SELECT 68401, 107, N'B.VII.', N'Poskytnuté návratné finančné výpomoci krátkodobé súčet (r. 108 až r. 112)', N'Provided current repayable financial assistance - total (lines 108 to 112)', 1, NULL, NULL, 106
    UNION ALL SELECT 68401, 108, N'B.VII.1.', N'Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku (271AÚ) - (291AÚ)', N'Repayable financial assistance provided to entities within consolidated unit (271A) - (291A)', 0, NULL, NULL, 107
    UNION ALL SELECT 68401, 109, N'2.', N'Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy (272AÚ) - (291AÚ)', N'Repayable financial assistance provided to other entities of general government (272A) - (291A)', 0, NULL, NULL, 108
    UNION ALL SELECT 68401, 110, N'3.', N'Poskytnuté návratné finančné výpomoci podnikateľským subjektom (274AÚ) - (291AÚ)', N'Repayable financial assistance provided to business entities (274A) - (291A)', 0, NULL, NULL, 109
    UNION ALL SELECT 68401, 111, N'4.', N'Poskytnuté návratné finančné výpomoci ostatným organizáciám (275AÚ) - (291AÚ)', N'Repayable financial assistance provided to other organisations (275A) - (291A)', 0, NULL, NULL, 110
    UNION ALL SELECT 68401, 112, N'5.', N'Poskytnuté návratné finančné výpomoci fyzickým osobám (277AÚ) - (291AÚ)', N'Repayable financial assistance provided to natural persons (277A) - (291A)', 0, NULL, NULL, 111
    UNION ALL SELECT 68401, 113, N'C.', N'Časové rozlíšenie súčet (r. 114 až r. 116)', N'Accruals and deferrals - total (lines 114 to 116)', 1, NULL, NULL, 112
    UNION ALL SELECT 68401, 114, N'C. 1.', N'Náklady budúcich období (381)', N'Prepaid expenses (381)', 0, NULL, NULL, 113
    UNION ALL SELECT 68401, 115, N'2.', N'Komplexné náklady budúcich období (382)', N'Complex prepaid expenses (382)', 0, NULL, NULL, 114
    UNION ALL SELECT 68401, 116, N'3.', N'Príjmy budúcich období (385)', N'Accrued income (385)', 0, NULL, NULL, 115
    UNION ALL SELECT 69601, 1, N'50', N'Spotrebované nákupy (r. 002 až r. 005)', N'Consumed purchases - total (lines 002 to 005)', 1, NULL, NULL, 0
    UNION ALL SELECT 69601, 2, N'501', N'Spotreba materiálu', N'Raw material consumption', 0, NULL, NULL, 1
    UNION ALL SELECT 69601, 3, N'502', N'Spotreba energie', N'Energy consumption', 0, NULL, NULL, 2
    UNION ALL SELECT 69601, 4, N'503', N'Spotreba ostatných neskladovateľných dodávok', N'Consumption of other non-inventory supplies', 0, NULL, NULL, 3
    UNION ALL SELECT 69601, 5, N'504, 507', N'Predaný tovar, Predaná nehnuteľnosť', N'Cost on merchandise sold', 0, NULL, NULL, 4
    UNION ALL SELECT 69601, 6, N'51', N'Služby (r. 007 až r. 010)', N'Services - total (lines 007 to 010)', 1, NULL, NULL, 5
    UNION ALL SELECT 69601, 7, N'511', N'Opravy a udržiavanie', N'Repairs and maintenance', 0, NULL, NULL, 6
    UNION ALL SELECT 69601, 8, N'512', N'Cestovné', N'Travel expenses', 0, NULL, NULL, 7
    UNION ALL SELECT 69601, 9, N'513', N'Náklady na reprezentáciu', N'Representation costs', 0, NULL, NULL, 8
    UNION ALL SELECT 69601, 10, N'518', N'Ostatné služby', N'Other services', 0, NULL, NULL, 9
    UNION ALL SELECT 69601, 11, N'52', N'Osobné náklady (r. 012 až r. 016)', N'Personnel expenses - total (lines 012 to 016)', 1, NULL, NULL, 10
    UNION ALL SELECT 69601, 12, N'521', N'Mzdové náklady', N'Wages and salaries', 0, NULL, NULL, 11
    UNION ALL SELECT 69601, 13, N'524', N'Zákonné sociálne poistenie', N'Legal social insurance', 0, NULL, NULL, 12
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 69601 AS [TableErpId], 14 AS [RowNumber], N'525' AS [Designation], N'Ostatné sociálne poistenie' AS [Text_sk], N'Other social insurance' AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 13 AS [RowOrdinal]
    UNION ALL SELECT 69601, 15, N'527', N'Zákonné sociálne náklady', N'Legal social expenses', 0, NULL, NULL, 14
    UNION ALL SELECT 69601, 16, N'528', N'Ostatné sociálne náklady', N'Other social costs', 0, NULL, NULL, 15
    UNION ALL SELECT 69601, 17, N'53', N'Dane a poplatky (r. 018 až r. 020)', N'Taxes and fees - total (lines 018 to 020)', 1, NULL, NULL, 16
    UNION ALL SELECT 69601, 18, N'531', N'Daň z motorových vozidiel', N'Road tax', 0, NULL, NULL, 17
    UNION ALL SELECT 69601, 31, NULL, N'Rezervy a opravné položky z prevádzkovej činnosti (r. 032 až r. 035)', N'Provisions and adjusting entries to operating expenses - total (lines 032 to 035)', 1, NULL, NULL, 30
    UNION ALL SELECT 69601, 32, N'552', N'Tvorba zákonných rezerv z prevádzkovej činnosti', N'Additions to legal provisions out of operations', 0, NULL, NULL, 31
    UNION ALL SELECT 69601, 33, N'553', N'Tvorba ostatných rezerv z prevádzkovej činnosti', N'Additions to other provisions out of operations', 0, NULL, NULL, 32
    UNION ALL SELECT 69601, 34, N'557', N'Tvorba zákonných opravných položiek z prevádzkovej činnosti', N'Additions to legal adjusting entries out of operations', 0, NULL, NULL, 33
    UNION ALL SELECT 69601, 35, N'558', N'Tvorba ostatných opravných položiek z prevádzkovej činnosti', N'Additions to other adjusting entries out of operations', 0, NULL, NULL, 34
    UNION ALL SELECT 69601, 36, NULL, N'Rezervy a opravné položky z finančnej činnosti (r. 037+ r. 038)', N'Provisions and adjusting entries to financial expenses - total (lines 037 to 038)', 1, NULL, NULL, 35
    UNION ALL SELECT 69601, 37, N'554', N'Tvorba rezerv z finančnej činnosti', N'Additions to provisions out of financial activity', 0, NULL, NULL, 36
    UNION ALL SELECT 69601, 38, N'559', N'Tvorba opravných položiek z finančnej činnosti', N'Additions to adjusting entries out of financial activity', 0, NULL, NULL, 37
    UNION ALL SELECT 69601, 39, N'555', N'Zúčtovanie komplexných nákladov budúcich období', N'Clearing of complex deferred expenses', 0, NULL, NULL, 38
    UNION ALL SELECT 69601, 40, N'56', N'Finančné náklady (r. 041 až r. 049)', N'Financial expenses - total (lines 041 to 049)', 1, NULL, NULL, 39
    UNION ALL SELECT 69601, 41, N'561', N'Predané cenné papiere a podiely', N'Securities and shares sold', 0, NULL, NULL, 40
    UNION ALL SELECT 69601, 42, N'562', N'Úroky', N'Interest expense', 0, NULL, NULL, 41
    UNION ALL SELECT 69601, 43, N'563', N'Kurzové straty', N'Exchange rate losses', 0, NULL, NULL, 42
    UNION ALL SELECT 69601, 44, N'564', N'Náklady na precenenie cenných papierov', N'Costs of securities revaluation', 0, NULL, NULL, 43
    UNION ALL SELECT 69601, 45, N'566', N'Náklady na krátkodobý finančný majetok', N'Costs of current financial assets', 0, NULL, NULL, 44
    UNION ALL SELECT 69601, 46, N'567', N'Náklady na derivátové operácie', N'Costs of derivative operations', 0, NULL, NULL, 45
    UNION ALL SELECT 69601, 47, N'568', N'Ostatné finančné náklady', N'Other financial expenses', 0, NULL, NULL, 46
    UNION ALL SELECT 69601, 48, N'569', N'Manká a škody na finančnom majetku', N'Deficits and damages to financial assets', 0, NULL, NULL, 47
    UNION ALL SELECT 69601, 49, NULL, N'Podiel konsolidujúcej účtovnej jednotky na výsledku hospodárenia pridružených účtovných jednotiek verejnej správy', N'Share of controlling entity on profit/loss of associates', 0, NULL, NULL, 48
    UNION ALL SELECT 69601, 50, N'57', N'Mimoriadne náklady (r. 051 až r. 054)', N'Extraordinary expenses - total (lines 051 to 054)', 1, NULL, NULL, 49
    UNION ALL SELECT 69601, 51, N'572', N'Škody', N'Damages', 0, NULL, NULL, 50
    UNION ALL SELECT 69601, 52, N'574', N'Tvorba rezerv', N'Additions to provisions', 0, NULL, NULL, 51
    UNION ALL SELECT 69601, 53, N'578', N'Ostatné mimoriadne náklady', N'Other extraordinary expenses', 0, NULL, NULL, 52
    UNION ALL SELECT 69601, 54, N'1579', N'Tvorba opravných položiek', N'Additions to adjusting entries', 0, NULL, NULL, 53
    UNION ALL SELECT 69601, 55, N'58', N'Náklady na transfery a náklady z odvodu príjmov (r. 056 až r. 064)', N'Costs of transfers and costs of revenue transfer - total (lines 056 to 064)', 1, NULL, NULL, 54
    UNION ALL SELECT 69601, 56, N'581', N'Náklady na transfery zo štátneho rozpočtu do štátnych rozpočtových organizácií a príspevkových organizácií', N'Cost of transfers from state budget to state-funded and state-subsidized organizations', 0, NULL, NULL, 55
    UNION ALL SELECT 69601, 57, N'582', N'Náklady na transfery zo štátneho rozpočtu ostatným subjektom verejnej správy', N'Cost of transfers from state budget to other entities of general government', 0, NULL, NULL, 56
    UNION ALL SELECT 69601, 58, N'583', N'Náklady na transfery zo štátneho rozpočtu subjektom mimo verejnej správy', N'Cost of transfers from state budget to the entities outside of general government', 0, NULL, NULL, 57
    UNION ALL SELECT 69601, 59, N'584', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku do rozpočtových organizácií a príspevkových organizácií zriadených obcou alebo vyšším územným celkom', N'Cost of transfers from the budget of municipality or higher regional unit to state-funded and state-subsidized organisations founded by the municipality or higher regional unit', 0, NULL, NULL, 58
    UNION ALL SELECT 69601, 60, N'585', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku ostatným subjektom verejnej správy', N'Cost of transfers from the budget of municipality or higher regional unit to other entities of general government', 0, NULL, NULL, 59
    UNION ALL SELECT 69601, 61, N'586', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku subjektom mimo verejnej správy', N'Cost of transfers from the budget of municipality or higher regional unit to the entities outside of general government', 0, NULL, NULL, 60
    UNION ALL SELECT 69601, 62, N'587', N'Náklady na ostatné transfery', N'Costs of other transfers', 0, NULL, NULL, 61
    UNION ALL SELECT 69601, 63, N'588', N'Náklady z odvodu príjmov', N'Cost of budgetary contributions', 0, NULL, NULL, 62
    UNION ALL SELECT 69601, 64, N'589', N'Náklady z budúceho odvodu príjmov', N'Cost of future budgetary contributions', 0, NULL, NULL, 63
    UNION ALL SELECT 69601, 65, NULL, N'Účtové skupiny 50 - 58 súčet (r. 001 + r. 006 + r. 011 + r. 017 + r. 021 + r. 029 + r. 040 + r. 050 + r. 055)', N'Account groups 50 - 58, line 001 + line 006 + line 011 + line 017 + line 021 + line 029 + line 040 + line 050 + line 055', 1, NULL, NULL, 64
    UNION ALL SELECT 71604, 16, NULL, N'Rezervy', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 71604, 17, NULL, N'Záväzky', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 71604, 18, NULL, N'Úvery', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 71604, 19, NULL, N'Opravná položka k nadobudnutému majetku (pasívna)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 71604, 20, NULL, N'Záväzky celkom súčet (r. 16 až 19)', NULL, 1, NULL, NULL, 4
    UNION ALL SELECT 71604, 21, NULL, N'Rozdiel majetku a záväzkov (r. 15 - r. 20)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 73302, 1, NULL, N'Výnosy z prevádzkovej činnosti, z toho:', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 73302, 2, NULL, N'tržby', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 73302, 3, NULL, N'Náklady na prevádzkovú činnosť', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 73302, 4, NULL, N'Výsledok hospodárenia z prevádzkovej činnosti(+/-), (r.01 - r. 03)', NULL, 1, NULL, NULL, 3
    UNION ALL SELECT 73302, 5, NULL, N'Finančné výnosy', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 73302, 6, NULL, N'Finančné náklady', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 73302, 7, NULL, N'Výsledok hospodárenia z pokračujúcich činností pred zdanením (+/-), (r. 04 + r. 05 - r. 06)', NULL, 1, NULL, NULL, 6
    UNION ALL SELECT 73302, 8, NULL, N'Daň z príjmu', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 73302, 9, NULL, N'Výsledok hospodárenia z pokračujúcich činností po zdanení (+/-), (r. 07 - r. 08)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 94301, 1, NULL, N'SPOLU MAJETOK (r. 02 + r. 09)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 94301, 2, N'A.', N'Neobežný majetok (r. 03 + r.04 + r. 05 + r. 07 + r. 08)', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 94301, 3, N'A. I.', N'Dlhodobý nehmotný majetok', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94301, 4, N'A. II.', N'Dlhodobý hmotný majetok', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 94301, 5, N'A. III.', N'Dlhodobý finančný majetok, z toho:', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 94301, 6, N'A. III.1', N'pohľadávky z obchodného styku', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 94301, 7, N'A. IV.', N'Ostatný majetok', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 94301, 8, N'A. V.', N'Odložená daňová pohľadávka', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 94301, 9, N'B.', N'Obežný majetok (r. 10 + r. 11 + r. 14)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 94301, 10, N'B. I.', N'Zásoby', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 94301, 11, N'B. II.', N'Krátkodobý finančný majetok, z toho:', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 94301, 12, N'B. II. 1', N'pohľadávky z obchodného styku', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 94301, 13, N'B. II. 2', N'peniaze a peňažné ekvivalenty', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 94301, 14, N'B. III.', N'Ostatný majetok, z toho:', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 94301, 15, N'B. III. 1', N'majetok klasifikovaný ako držaný na predaj', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 94301, 16, NULL, N'SPOLU VLASTNÉ IMANIE A ZÁVÄZKY(r. 17 + r. 26)', NULL, 1, NULL, NULL, 15
    UNION ALL SELECT 94301, 17, N'C.', N'Vlastné imanie (r. 18 + r. 19 + r. 20 + r. 21 + r. 24 + r. 25)', NULL, 1, NULL, NULL, 16
    UNION ALL SELECT 94301, 18, N'C. I.', N'Základné imanie', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 94301, 19, N'C. II.', N'Kapitálové fondy', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 94301, 20, N'C. III.', N'Rezervné fondy a ostatné fondy tvorené zo zisku', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 94301, 21, N'C. IV.', N'Výsledok hospodárenia minulých rokov (r. 22 + r. 23)', NULL, 1, NULL, NULL, 20
    UNION ALL SELECT 94301, 22, N'C. IV. 1', N'Nerozdelený zisk minulých rokov', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 94301, 23, N'C. IV. 2', N'Neuhradená strata minulých rokov', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 94301, 24, N'C. V.', N'Výsledok hospodárenia za účtovné obdobie po zdanení (+/-)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 94301, 25, N'C. VI.', N'Ostatné zložky vlastného imania', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 94301, 26, N'D.', N'Záväzky (r. 27 + r. 31 + r. 32)', NULL, 1, NULL, NULL, 25
    UNION ALL SELECT 94301, 27, N'D. I.', N'Dlhodobé záväzky, z toho:', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 94301, 28, N'D. I. 1', N'záväzky z obchodného styku', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 94301, 29, N'D. I. 2', N'úvery a pôžičky', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 94301, 30, N'D. I. 3', N'rezervy', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 94301, 31, N'D. II.', N'Odložený daňový záväzok', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 94301, 32, N'D. III.', N'Krátkodobé záväzky, z toho:', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 94301, 33, N'D. III. 1', N'záväzky z obchodného styku', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 94301, 34, N'D. III. 2', N'úvery a pôžičky', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 94301, 35, N'D. III. 3', N'rezervy', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 94301, 36, N'D. III. 4', N'záväzky spojené s majetkom klasifikovaným ako držaný na predaj', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 100103, 2, N'1.', N'Poistné v hrubej výške', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 100103, 3, N'2.', N'Prevedený výsledok z finančného umiestnenia z netechnického účtu', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 100103, 4, N'3.', N'Ostatné technické výnosy', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 100103, 5, N'4.', N'Náklady na poistné plnenia', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 100103, 6, N'4a.', N'Náklady na poistné plnenia v hrubej výške v tom', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 100103, 7, N'4aa.', N'Náklady na ambulantnú zdravotnú starostlivosť', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 100103, 8, N'4ab.', N'Náklady na ústavnú zdravotnú starostlivosť', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 100103, 9, N'4ac.', N'Náklady na lieky', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 100103, 10, N'4ad.', N'Náklady na zdravotnícke pomôcky', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 100103, 11, N'4ae.', N'Náklady na ostatné poistné plnenia', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 100103, 12, N'4b.', N'Nárok na úhradu nákladov od iných subjektov', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 100103, 13, N'4c.', N'Zmena stavu technickej rezervy na poistné plnenia v hrubej výške', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 100103, 14, N'5.', N'Zmena stavu iných technických rezerv', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 100103, 15, N'7.', N'Čistá výška prevádzkových nákladov', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 100103, 16, N'7a.', N'Obstarávacie náklady na poistné zmluvy', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 100103, 17, N'7b.', N'Správna réžia', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 100103, 18, N'8.', N'Ostatné technické náklady', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 100103, 19, N'10.', N'Výsledok technického účtu k neživotnému poisteniu A', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 100103, 20, N'III.', N'NETECHNICKÝ ÚČET', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 100103, 21, N'1.', N'Výsledok technického účtu k neživotnému poisteniu', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 100103, 22, N'3.', N'Výnosy z finančného umiestnenia', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 100103, 23, N'3a.', N'Výnosy z podielových cenných papierov a vkladov a v tom rozhodujúci vplyv', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 100103, 24, N'3b.', N'Výnosy z ostatného finančného umiestnenia a v tom rozhodujúci vplyv', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 100103, 25, N'3ba.', N'Výnosy z pozemkov a stavieb', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 100103, 26, N'3bb.', N'Výnosy z ostatných zložiek finančného umiestnenia', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 100103, 27, N'3c.', N'Použitie opravných položiek k finančnému umiestneniu', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 100103, 28, N'3d.', N'Výnosy z realizácie finančného umiestnenia', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 100103, 29, N'3e.', N'Prírastky hodnoty finančného umiestnenia', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 100103, 30, N'5.', N'Náklady na finančné umiestnenie', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 100103, 31, N'5a.', N'Náklady na finančné umiestnenie', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 38503, 1, N'501', N'Spotreba materiálu', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 38503, 2, N'502', N'Spotreba energie', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 38503, 3, N'504', N'Predaný tovar', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 38503, 4, N'511', N'Opravy a udržiavanie', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 38503, 5, N'512', N'Cestovné', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 38503, 6, N'513', N'Náklady na reprezentáciu', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 38503, 7, N'518', N'Ostatné služby', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 38503, 8, N'521', N'Mzdové náklady', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 38503, 9, N'524', N'Zákonné sociálne poistenie a zdravotné poistenie', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 38503, 10, N'525', N'Ostatné sociálne poistenie', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 38503, 11, N'527', N'Zákonné sociálne náklady', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 38503, 12, N'528', N'Ostatné sociálne náklady', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 38503, 13, N'531', N'Daň z motorových vozidiel', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 38503, 14, N'532', N'Daň z nehnuteľností', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 38503, 15, N'538', N'Ostatné dane a poplatky', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 38503, 16, N'541', N'Zmluvné pokuty a penále', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 38503, 17, N'542', N'Ostatné pokuty a penále', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 38503, 18, N'543', N'Odpísanie pohľadávky', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 38503, 19, N'544', N'Úroky', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 38503, 20, N'545', N'Kurzové straty', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 38503, 21, N'546', N'Dary', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 38503, 22, N'547', N'Osobitné náklady', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 38503, 23, N'548', N'Manká a škody', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 38503, 24, N'549', N'Iné ostatné náklady', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 38503, 25, N'551', N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 38503, 26, N'552', N'Zostatková cena predaného dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 38503, 27, N'553', N'Predané cenné papiere', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 38503, 28, N'554', N'Predaný materiál', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 38503, 29, N'555', N'Náklady na krátkodobý finančný majetok', NULL, 0, NULL, NULL, 28
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 38503 AS [TableErpId], 30 AS [RowNumber], N'556' AS [Designation], N'Tvorba fondov' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 29 AS [RowOrdinal]
    UNION ALL SELECT 38503, 31, N'557', N'Náklady na precenenie cenných papierov', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 38503, 32, N'558', N'Tvorba a zúčtovanie opravných položiek', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 38503, 33, N'561', N'Poskytnuté príspevky organizačným zložkám', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 38503, 34, N'562', N'Poskytnuté príspevky iným účtovným jednotkám', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 38503, 35, N'563', N'Poskytnuté príspevky fyzickým osobám', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 38503, 36, N'565', N'Poskytnuté príspevky z podielu zaplatenej dane', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 38503, 37, N'567', N'Poskytnuté príspevky z verejnej zbierky', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 38503, 38, NULL, N'Účtová trieda 5 spolu r. 01 až r. 37', NULL, 1, NULL, NULL, 37
    UNION ALL SELECT 100103, 1, N'I. A.', N'TECHNICKÝ ÚČET K NEŽIVOTNÉMU POISTENIU - VEREJNÉ ZDRAVOTNÉ POISTENIE', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 68401, 25, N'A.III.', N'Dlhodobý finančný majetok súčet (r. 026 + r. 027 + r. 029 až 034)', N'Non-current financial assets - total (line 026 + line 027 + lines 029 to 034)', 1, NULL, NULL, 24
    UNION ALL SELECT 68401, 26, N'A.III.1.', N'Podielové cenné papiere a podiely v dcérskej účtovnej jednotke (061) - (096AÚ)', N'Shares and ownership interests in subsidiary (061) - (096A)', 0, NULL, NULL, 25
    UNION ALL SELECT 68401, 27, N'2.', N'Podielové cenné papiere a podiely v spoločnosti s podstatným vplyvom (062) - (096AÚ)', N'Shares and ownership interests with significant influence over enterprises (062) - (096A)', 0, NULL, NULL, 26
    UNION ALL SELECT 68401, 28, NULL, N'z toho: goodwill', N'of that: goodwill', 0, NULL, NULL, 27
    UNION ALL SELECT 68401, 29, N'3.', N'Realizovateľné cenné papiere a podiely (063) - (096AÚ)', N'Realisable securities and shares (063) - (096A)', 0, NULL, NULL, 28
    UNION ALL SELECT 68401, 30, N'4.', N'Dlhové cenné papiere držané do splatnosti (065) - (096AÚ)', N'Debt securities held up to their maturity (065) - (096A)', 0, NULL, NULL, 29
    UNION ALL SELECT 68401, 31, N'5.', N'Pôžičky účtovnej jednotke v konsolidovanom celku (066) - (096AÚ)', N'Loans to accounting entity within consolidated unit (066) - (096A)', 0, NULL, NULL, 30
    UNION ALL SELECT 68401, 32, N'6.', N'Ostatné pôžičky (067) - (096AÚ)', N'Other loans (067) - (096AA)', 0, NULL, NULL, 31
    UNION ALL SELECT 68401, 33, N'7.', N'Ostatný dlhodobý finančný majetok (069) - (096AÚ)', N'Other non-current financial assets (069) - (089+092A)', 0, NULL, NULL, 32
    UNION ALL SELECT 68401, 34, N'8.', N'Obstaranie dlhodobého finančného majetku (043) - (096AÚ)', N'Acquisition of non-current financial assets (043) - (096A)', 0, NULL, NULL, 33
    UNION ALL SELECT 68401, 35, N'B.', N'Obežný majetok r. 036 + r. 042 + r. 050 + r. 063 + r. 088+ r. 101 + r. 107', N'Current assets line 036 + line 042 + line 050 + line 063 + line 088 + line 101 + line 107', 1, NULL, NULL, 34
    UNION ALL SELECT 68401, 36, N'B.I.', N'Zásoby súčet (r. 037 až 041)', N'Inventory - total (lines 035 to 039)', 1, NULL, NULL, 35
    UNION ALL SELECT 68401, 37, N'B.I.1.', N'Materiál (112 + 119) - (191)', N'Raw material (112 + 119) - (191)', 0, NULL, NULL, 36
    UNION ALL SELECT 68401, 38, N'2.', N'Nedokončená výroba a polotovary (121 + 122) - (192 + 193)', N'Work in progress and semi-finished products (121 + 122) - (192 + 193)', 0, NULL, NULL, 37
    UNION ALL SELECT 68401, 39, N'3.', N'Výrobky (123) - (194)', N'Finished goods (123) - (194)', 0, NULL, NULL, 38
    UNION ALL SELECT 68401, 40, N'4.', N'Zvieratá (124) - (195)', N'Animals (124) - (195)', 0, NULL, NULL, 39
    UNION ALL SELECT 68401, 41, N'5.', N'Tovar (132 + 133 + 139) - (196)', N'Merchandise (132 + 133 + 139) - (196)', 0, NULL, NULL, 40
    UNION ALL SELECT 68401, 42, N'B.II.', N'Zúčtovanie medzi subjektami verejnej správy súčet (r. 043 až r. 049)', N'Clearance between the public administration entities - total (lines 043 to 049)', 1, NULL, NULL, 41
    UNION ALL SELECT 68401, 43, N'B.II.1.', N'Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa (351)', N'Clearing of state-funded organisation''s contributions to founder''s budget (351)', 0, NULL, NULL, 42
    UNION ALL SELECT 68401, 44, N'2.', N'Zúčtovanie transferov štátneho rozpočtu (353)', N'Clearing of state budget transfers (353)', 0, NULL, NULL, 43
    UNION ALL SELECT 68401, 45, N'3.', N'Zúčtovanie transferov rozpočtu obce a vyššieho územného celku (355)', N'Clearing of transfers of the budget of municipalities and higher territorial units (355)', 0, NULL, NULL, 44
    UNION ALL SELECT 68401, 46, N'4.', N'Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku (356)', N'Clearing of transfers from state budget within consolidated unit (356)', 0, NULL, NULL, 45
    UNION ALL SELECT 68401, 47, N'5.', N'Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku (357)', N'Other clearing of the budget of municipalities and higher territorial units (357)', 0, NULL, NULL, 46
    UNION ALL SELECT 68401, 48, N'6.', N'Zúčtovanie transferov zo štátneho rozpočtu iným subjektom (358)', N'Clearing of transfers from state budget to other entities (358)', 0, NULL, NULL, 47
    UNION ALL SELECT 68401, 49, N'7.', N'Zúčtovanie transferov medzi subjektami verejnej správy a iné zúčtovania (359)', N'Clearance of transfers between the public administration entities and other clearance transactions (359)', 0, NULL, NULL, 48
    UNION ALL SELECT 68401, 50, N'B. III', N'Dlhodobé pohľadávky súčet (r. 051 až 061)', N'Non-current receivables - total (lines 051 to 061)', 1, NULL, NULL, 49
    UNION ALL SELECT 68401, 51, N'B.III.1.', N'Odberatelia (311AÚ) - (391AÚ)', N'Customers (311A) - (391A)', 0, NULL, NULL, 50
    UNION ALL SELECT 68401, 52, N'2.', N'Zmenky na inkaso (312AÚ) - (391AÚ)', N'Bills of exchange to be collected (312A) - (391A)', 0, NULL, NULL, 51
    UNION ALL SELECT 68401, 53, N'3.', N'Pohľadávky za eskontované cenné papiere (313AÚ) - (391AÚ)', N'Receivables for discounted securities (313A) - (391A)', 0, NULL, NULL, 52
    UNION ALL SELECT 68401, 54, N'4.', N'Ostatné pohľadávky (315AÚ) - (391AÚ)', N'Other receivables (315A) - (391A)', 0, NULL, NULL, 53
    UNION ALL SELECT 68401, 55, N'5.', N'Pohľadávky voči zamestnancom (335AÚ) - (391AÚ)', N'Receivables to employees (335A) - (391A)', 0, NULL, NULL, 54
    UNION ALL SELECT 68401, 56, N'6.', N'Pohľadávky voči združeniu (369AÚ) - (391AÚ)', N'Receivables from participants in association (369A) - (391A)', 0, NULL, NULL, 55
    UNION ALL SELECT 68401, 57, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ) - (391AÚ)', N'Receivables and liabilities from fixed term transactions (373A) - (391A)', 0, NULL, NULL, 56
    UNION ALL SELECT 68401, 58, N'8.', N'Pohľadávky z nájmu (374AÚ) - (391AÚ)', N'Receivables from leasing (374A) - (391A)', 0, NULL, NULL, 57
    UNION ALL SELECT 68401, 59, N'9.', N'Pohľadávky z vydaných dlhopisov (375AÚ) - (391AÚ)', N'Receivables from issued bonds (375A) - (391A)', 0, NULL, NULL, 58
    UNION ALL SELECT 68401, 60, N'10.', N'Nakúpené opcie (376AÚ) - (391AÚ)', N'Options purchased (376A) - (391A)', 0, NULL, NULL, 59
    UNION ALL SELECT 68401, 61, N'11.', N'Iné pohľadávky (378AÚ) - (391AÚ)', N'Other receivables (378A) - (391A)', 0, NULL, NULL, 60
    UNION ALL SELECT 68401, 62, NULL, N'z toho: odložená daňová pohľadávka', N'of that: deffered tax assets', 0, NULL, NULL, 61
    UNION ALL SELECT 68401, 63, N'B.IV.', N'Krátkodobé pohľadávky súčet (r. 064 až 087)', N'Current receivables - total (lines 064 to 087)', 1, NULL, NULL, 62
    UNION ALL SELECT 68401, 64, N'B.IV.1.', N'Odberatelia (311AÚ) - (391AÚ)', N'Customers (311A) - (391A)', 0, NULL, NULL, 63
    UNION ALL SELECT 68401, 65, N'2.', N'Zmenky na inkaso (312AÚ) - (391AÚ)', N'Bills of exchange to be collected (312AA) - (391A)', 0, NULL, NULL, 64
    UNION ALL SELECT 73302, 10, NULL, N'Výsledok hospodárenia z ukončených činností pred zdanením (+/-)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 73302, 11, NULL, N'Daň z príjmu', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 73302, 12, NULL, N'Výsledok hospodárenia z ukončených činností po zdanení (+/-), (r. 10 - r. 11)', NULL, 1, NULL, NULL, 11
    UNION ALL SELECT 73302, 13, NULL, N'Výsledok hospodárenia za účtovné obdobie po zdanení (+/-), (r. 09 + r. 12)', NULL, 1, NULL, NULL, 12
    UNION ALL SELECT 73302, 14, NULL, N'Ostatné súčasti komplexného výsledku', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 73302, 15, NULL, N'Celkový komplexný výsledok za účtovné obdobie po zdanení (+/-), (r. 13 + r. 14)', NULL, 1, NULL, NULL, 14
    UNION ALL SELECT 69601, 19, N'532', N'Daň z nehnuteľnosti', N'Real estate tax', 0, NULL, NULL, 18
    UNION ALL SELECT 69601, 20, N'538', N'Ostatné dane a poplatky', N'Other taxes and fees', 0, NULL, NULL, 19
    UNION ALL SELECT 69601, 21, N'54', N'Ostatné náklady na prevádzkovú činnosť (r. 022 až r. 028)', N'Other operating expenses - total (lines 022 to 028)', 1, NULL, NULL, 20
    UNION ALL SELECT 69601, 22, N'541', N'Zostatková cena predaného dlhodobého nehmotného majetku a dlhodobého hmotného majetku', N'Carrying value of non-current intangible and tangible assets sold', 0, NULL, NULL, 21
    UNION ALL SELECT 69601, 23, N'542', N'Predaný materiál', N'Material sold', 0, NULL, NULL, 22
    UNION ALL SELECT 69601, 24, N'544', N'Zmluvné pokuty, penále a úroky z omeškania', N'Contractual fines, penalties, and interest on late payment', 0, NULL, NULL, 23
    UNION ALL SELECT 69601, 25, N'545', N'Ostatné pokuty, penále a úroky z omeškania', N'Other fines, penalties, and interest on late payment', 0, NULL, NULL, 24
    UNION ALL SELECT 69601, 26, N'546', N'Odpis pohľadávky', N'Receivable write off', 0, NULL, NULL, 25
    UNION ALL SELECT 69601, 27, N'548', N'Ostatné náklady na prevádzkovú činnosť', N'Other operating expenses', 0, NULL, NULL, 26
    UNION ALL SELECT 69601, 28, N'549', N'Manká a škody', N'Deficits and damages', 0, NULL, NULL, 27
    UNION ALL SELECT 69601, 29, N'55', N'Odpisy, rezervy a opravné položky z prevádzkovej činnosti a finančnej činnosti a zúčtovanie časového rozlíšenia (r. 030 + r. 031 + r. 036 + r. 039)', N'Depreciation, provisions and adjusting entries to operating and financial expenses, and accrual-based accounting - total (line 030 + line 031 + line 036 + line 039)', 1, NULL, NULL, 28
    UNION ALL SELECT 69601, 30, N'551', N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', N'Depreciation of non-current intangible assets and non-current tangible assets', 0, NULL, NULL, 29
    UNION ALL SELECT 100103, 32, N'5b.', N'Tvorba opravných položiek k finančnému umiestneniu', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 100103, 33, N'5c.', N'Náklady na realizáciu finančného umiestnenia', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 100103, 34, N'5d.', N'Úbytky hodnoty finančného umiestnenia', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 100103, 35, N'6.', N'Prevedené výnosy z finančného umiestnenia na technický účet', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 100103, 36, N'7.', N'Ostatné výnosy', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 100103, 37, N'8.', N'Ostatné náklady', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 100103, 38, N'8a.', N'Ostatné dane a poplatky', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 100103, 39, N'9.', N'Daň z príjmov z bežnej činnosti', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 100103, 40, N'10.', N'Výsledok hospodárenia z bežnej činnosti po zdanení', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 100103, 41, N'11.', N'Mimoriadne výnosy', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 100103, 42, N'12.', N'Mimoriadne náklady', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 100103, 43, N'13.', N'Mimoriadny výsledok hospodárenia', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 100103, 44, N'14.', N'Daň z príjmov z mimoriadnej činnosti', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 100103, 45, N'16.', N'Výsledok hospodárenia za účtovné obdobie', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 100103, 999, NULL, N'Kontrolné číslo', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 68401, 10, N'7.', N'Obstaranie dlhodobého nehmotného majetku (041) - (093)', N'Acquisition of non-current intangible assets (041) - (093)', 0, NULL, NULL, 9
    UNION ALL SELECT 68401, 117, N'D.', N'Vzťahy k účtom klientov Štátnej pokladnice (účtová skupina 20)', N'Relationships to the State Treasury client accounts (account group 20)', 0, NULL, NULL, 116
    UNION ALL SELECT 202, 120, N'A.II.', N'Fondy súčet (r. 121 + r. 122)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 202, 121, N'A.II.1.', N'Zákonný rezervný fond (421)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 202, 122, N'2.', N'Ostatné fondy (427)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 202, 123, N'A.III.', N'Výsledok hospodárenia (+/-) súčet (r. 124 až 125)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 202, 124, N'A.III.1.', N'Nevysporiadaný výsledok hospodárenia minulých rokov (+/– 428)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 202, 125, N'2.', N'Výsledok hospodárenia za účtovné obdobie (+/–) r. 001 - (r. 117 + r. 120 +r.124+ r. 126 + r. 180 + r. 183)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 202, 126, N'B.', N'Záväzky súčet r. 127 + r. 132 + r. 140 + r. 151 + r. 173', NULL, 1, NULL, NULL, 11
    UNION ALL SELECT 202, 127, N'B.I.', N'Rezervy súčet (r. 128 až 131)', NULL, 1, NULL, NULL, 12
    UNION ALL SELECT 202, 128, N'B.I.1.', N'Rezervy zákonné dlhodobé (451AÚ)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 202, 129, N'2.', N'Ostatné rezervy (459AÚ)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 202, 130, N'3.', N'Rezervy zákonné krátkodobé (323AÚ, 451AÚ)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 202, 131, N'4.', N'Ostatné krátkodobé rezervy (323AÚ, 459AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 202, 132, N'B.II.', N'Zúčtovanie medzi subjektami verejnej správy súčet (r. 133 až r. 139)', NULL, 1, NULL, NULL, 17
    UNION ALL SELECT 202, 133, N'B.II.1.', N'Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa (351)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 202, 134, N'2.', N'Zúčtovanie transferov štátneho rozpočtu (353)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 202, 135, N'3.', N'Zúčtovanie transferov rozpočtu obce a vyššieho územného celku (355)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 202, 136, N'4.', N'Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku (356)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 202, 137, N'5.', N'Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku (357)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 202, 138, N'6.', N'Zúčtovanie transferov zo štátneho rozpočtu iným subjektom (358)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 202, 139, N'7.', N'Zúčtovanie transferov medzi subjektami verejnej správy (359)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 202, 140, N'B.III.', N'Dlhodobé záväzky súčet (r. 141 až 150)', NULL, 1, NULL, NULL, 25
    UNION ALL SELECT 202, 141, N'B.III.1.', N'Ostatné dlhodobé záväzky (479AÚ)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 202, 142, N'2.', N'Dlhodobé prijaté preddavky (475AÚ)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 202, 143, N'3.', N'Dlhodobé zmenky na úhradu (478AÚ)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 202, 144, N'4.', N'Záväzky zo sociálneho fondu (472)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 202, 145, N'5.', N'Záväzky z nájmu (474AÚ)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 202, 146, N'6.', N'Dlhodobé nevyfakturované dodávky (476AÚ)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 202, 147, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 202, 148, N'8.', N'Predané opcie (377AÚ)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 202, 149, N'9.', N'Iné záväzky (379AÚ)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 202, 150, N'10.', N'Vydané dlhopisy dlhodobé (473AÚ ) - (255AÚ)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 202, 151, N'B.IV.', N'Krátkodobé záväzky súčet (r. 152 až 172)', NULL, 1, NULL, NULL, 36
    UNION ALL SELECT 202, 152, N'B.IV.1.', N'Dodávatelia (321)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 202, 153, N'2.', N'Zmenky na úhradu (322, 478AÚ)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 202, 154, N'3.', N'Prijaté preddavky (324, 475AÚ)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 202, 155, N'4.', N'Ostatné záväzky (325, 479AÚ)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 202, 156, N'5.', N'Nevyfakturované dodávky (326, 476AÚ)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 202, 157, N'6.', N'Záväzky z nájmu (474AÚ)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 202, 158, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 202, 159, N'8.', N'Predané opcie (377AÚ)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 202, 160, N'9.', N'Iné záväzky (379AÚ)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 202, 161, N'10.', N'Záväzky z upísaných nesplatených cenných papierov a vkladov (367)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 202, 162, N'11.', N'Záväzky voči združeniu (368)', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 202, 163, N'12.', N'Zamestnanci (331)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 202, 164, N'13.', N'Ostatné záväzky voči zamestnancom (333)', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 202, 165, N'14.', N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia (336)', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 202, 166, N'15.', N'Daň z príjmov (341)', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 202, 167, N'16.', N'Ostatné priame dane (342)', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 202, 168, N'17.', N'Daň z pridanej hodnoty (343)', NULL, 0, N'Krátkodobé záväzky - Dan z pridanej hodnoty', NULL, 53
    UNION ALL SELECT 202, 169, N'18.', N'Ostatné dane a poplatky (345)', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 202, 170, N'19.', N'Spojovací účet pri združení (396AÚ)', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 202, 171, N'20.', N'Zúčtovanie s Európskymi spoločenstvami (371AÚ)', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 202, 172, N'21.', N'Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy (372AÚ)', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 202, 173, N'B.V.', N'Bankové úvery a výpomoci súčet (r. 174 až 179)', NULL, 1, NULL, NULL, 58
    UNION ALL SELECT 202, 174, N'B.V.1.', N'Bankové úvery dlhodobé (461AÚ )', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 202, 175, N'2.', N'Bežné bankové úvery (461AÚ, 221AÚ, 231, 232)', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 202, 176, N'3.', N'Vydané dlhopisy krátkodobé (473AÚ, 241 ) - (255AÚ)', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 202, 177, N'4.', N'Ostatné krátkodobé finančné výpomoci (249)', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 202, 178, N'5.', N'Prijaté návratné finančné výpomoci od subjektov verejnej správy dlhodobé (273AÚ)', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 202, 179, N'6.', N'Prijaté návratné finančné výpomoci od subjektov verejnej správy krátkodobé (273AÚ)', NULL, 0, NULL, NULL, 64
    UNION ALL SELECT 202, 180, N'C.', N'Časové rozlíšenie súčet (r. 181 + r. 182)', NULL, 1, NULL, NULL, 65
    UNION ALL SELECT 202, 181, N'C.1.', N'Výdavky budúcich období (383)', NULL, 0, NULL, NULL, 66
    UNION ALL SELECT 202, 182, N'2.', N'Výnosy budúcich období (384)', NULL, 0, NULL, NULL, 67
    UNION ALL SELECT 202, 183, N'D.', N'Vzťahy k účtom klientov štátnej pokladnice (účtová skupina 20)', NULL, 0, NULL, NULL, 68
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 202 AS [TableErpId], 999 AS [RowNumber], NULL AS [Designation], N'KONTROLNÉ ČÍSLO súčet (r. 115 až 183)' AS [Text_sk], NULL AS [Text_en], 1 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 69 AS [RowOrdinal]
    UNION ALL SELECT 702, 5, NULL, N'Nákup materiálu', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 702, 6, NULL, N'Nákup tovaru', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 702, 7, NULL, N'Mzdy', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 702, 8, NULL, N'Platby poistného a príspevkov', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 702, 9, NULL, N'Prevádzková réžia', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 702, 10, NULL, N'Výdavky celkom súčet (r. 05 až 09)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 702, 11, NULL, N'Rozdiel príjmov a výdavkov (r. 04 - r. 10)', NULL, 1, NULL, NULL, 6
    UNION ALL SELECT 52101, 1, N'50', N'Spotrebované nákupy (r. 002 až r. 005)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 52101, 2, N'501', N'Spotreba materiálu', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 52101, 3, N'502', N'Spotreba energie', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 52101, 4, N'503', N'Spotreba ostatných neskladovateľných dodávok', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 52101, 5, N'504, 507', N'Predaný tovar, Predaná nehnuteľnosť', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1101, 1, NULL, N'SPOLU MAJETOK r. 002 + r. 035 + r. 113 + r. 117', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 1101, 2, N'A.', N'Neobežný majetok r. 003 + r. 012 + r. 025', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 1101, 3, N'A.I.', N'Dlhodobý nehmotný majetok súčet (r. 004 až 011)', NULL, 1, NULL, NULL, 2
    UNION ALL SELECT 202, 115, NULL, N'VLASTNÉ IMANIE A ZÁVÄZKY r. 116 + r. 126 + r. 180 + r. 183', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 202, 116, N'A.', N'Vlastné imanie r. 117 + r. 120 + r. 123', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 202, 117, N'A.I.', N'Oceňovacie rozdiely súčet (r. 118 + r. 119)', NULL, 1, NULL, NULL, 2
    UNION ALL SELECT 202, 118, N'A.I.1.', N'Oceňovacie rozdiely z precenenia majetku a záväzkov (+/– 414)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 202, 119, N'2.', N'Oceňovacie rozdiely z kapitálových účastín (+/– 415)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 52101, 6, N'51', N'Služby (r. 007 až r. 010)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 52101, 7, N'511', N'Opravy a udržiavanie', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 52101, 8, N'512', N'Cestovné', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 52101, 9, N'513', N'Náklady na reprezentáciu', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 52101, 10, N'518', N'Ostatné služby', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 52101, 11, N'52', N'Osobné náklady (r. 012 až r. 016)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 52101, 12, N'521', N'Mzdové náklady', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 52101, 13, N'524', N'Zákonné sociálne poistenie', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 52101, 14, N'525', N'Ostatné sociálne poistenie', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 52101, 15, N'527', N'Zákonné sociálne náklady', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 52101, 16, N'528', N'Ostatné sociálne náklady', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 52101, 17, N'53', N'Dane a poplatky (r. 018 až r. 020)', NULL, 1, NULL, NULL, 16
    UNION ALL SELECT 52101, 18, N'531', N'Daň z motorových vozidiel', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 52101, 19, N'532', N'Daň z nehnuteľnosti', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 52101, 20, N'538', N'Ostatné dane a poplatky', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 52101, 21, N'54', N'Ostatné náklady na prevádzkovú činnosť (r. 022 až r. 028)', NULL, 1, NULL, NULL, 20
    UNION ALL SELECT 52101, 22, N'541', N'Zostatková cena predaného dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 52101, 23, N'542', N'Predaný materiál', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 52101, 24, N'544', N'Zmluvné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 52101, 25, N'545', N'Ostatné pokuty, penále a úroky z omeškania', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 52101, 26, N'546', N'Odpis pohľadávky', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 52101, 27, N'548', N'Ostatné náklady na prevádzkovú činnosť', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 52101, 28, N'549', N'Manká a škody', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 52101, 29, N'55', N'Odpisy, rezervy a opravné položky z prevádzkovej činnosti a finančnej činnosti a zúčtovanie časového rozlíšenia (r. 030 + r. 031 + r. 036 + r. 039)', NULL, 1, NULL, NULL, 28
    UNION ALL SELECT 52101, 30, N'551', N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 52101, 31, NULL, N'Rezervy a opravné položky z prevádzkovej činnosti (r. 032 až r. 035)', NULL, 1, NULL, NULL, 30
    UNION ALL SELECT 52101, 32, N'552', N'Tvorba zákonných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 52101, 33, N'553', N'Tvorba ostatných rezerv z prevádzkovej činnosti', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 52101, 34, N'557', N'Tvorba zákonných opravných položiek z prevádzkovej činnosti', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 52101, 35, N'558', N'Tvorba ostatných opravných položiek z prevádzkovej činnosti', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 52101, 36, NULL, N'Rezervy a opravné položky z finančnej činnosti (r. 037+ r. 038)', NULL, 1, NULL, NULL, 35
    UNION ALL SELECT 52101, 37, N'554', N'Tvorba rezerv z finančnej činnosti', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 52101, 38, N'559', N'Tvorba opravných položiek z finančnej činnosti', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 52101, 39, N'555', N'Zúčtovanie komplexných nákladov budúcich období', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 52101, 40, N'56', N'Finančné náklady (r. 041 až r. 048)', NULL, 1, NULL, NULL, 39
    UNION ALL SELECT 52101, 41, N'561', N'Predané cenné papiere a podiely', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 52101, 42, N'562', N'Úroky', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 52101, 43, N'563', N'Kurzové straty', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 52101, 44, N'564', N'Náklady na precenenie cenných papierov', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 52101, 45, N'566', N'Náklady na krátkodobý finančný majetok', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 52101, 46, N'567', N'Náklady na derivátové operácie', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 52101, 47, N'568', N'Ostatné finančné náklady', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 52101, 48, N'569', N'Manká a škody na finančnom majetku', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 52101, 49, N'57', N'Mimoriadne náklady (r. 050 až r. 053)', NULL, 1, NULL, NULL, 48
    UNION ALL SELECT 52101, 50, N'572', N'Škody', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 52101, 51, N'574', N'Tvorba rezerv', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 52101, 52, N'578', N'Ostatné mimoriadne náklady', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 52101, 53, N'579', N'Tvorba opravných položiek', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 52101, 54, N'58', N'Náklady na transfery a náklady z odvodu príjmov (r. 055 až r. 063)', NULL, 1, NULL, NULL, 53
    UNION ALL SELECT 52101, 55, N'581', N'Náklady na transfery zo štátneho rozpočtu do štátnych rozpočtových organizácií a príspevkových organizácií', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 52101, 56, N'582', N'Náklady na transfery zo štátneho rozpočtu ostatným subjektom verejnej správy', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 52101, 57, N'583', N'Náklady na transfery zo štátneho rozpočtu subjektom mimo verejnej správy', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 52101, 58, N'584', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku do rozpočtových organizácií a príspevkových organizácií zriadených obcou alebo vyšším územným celkom', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 52101, 59, N'585', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku ostatným subjektom verejnej správy', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 52101, 60, N'586', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku subjektom mimo verejnej správy', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 52101, 61, N'587', N'Náklady na ostatné transfery', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 52101, 62, N'588', N'Náklady z odvodu príjmov', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 52101, 63, N'589', N'Náklady z budúceho odvodu príjmov', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 52101, 64, NULL, N'Účtové skupiny 50 - 58 celkom súčet (r.001 + r.006 + r.011 + r.017 + r.021 + r.029 + r.040 + r.049 + r.054)', NULL, 1, NULL, NULL, 63
    UNION ALL SELECT 52101, 994, NULL, N'Kontrolné číslo súčet (r. 001 až r. 064)', NULL, 1, NULL, NULL, 64
    UNION ALL SELECT 1101, 60, N'10.', N'Nakúpené opcie (376AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 1101, 61, N'11.', N'Iné pohľadávky (378AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 1101, 62, NULL, N'z toho: odložená daňová pohľadávka', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 1101, 63, N'B.IV.', N'Krátkodobé pohľadávky súčet (r. 064 až 087)', NULL, 1, NULL, NULL, 62
    UNION ALL SELECT 1101, 64, N'B.IV.1.', N'Odberatelia (311AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 1101, 65, N'2.', N'Zmenky na inkaso (312AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 64
    UNION ALL SELECT 1101, 66, N'3.', N'Pohľadávky za eskontované cenné papiere (313AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 65
    UNION ALL SELECT 1101, 67, N'4.', N'Poskytnuté prevádzkové preddavky (314) - (391AÚ)', NULL, 0, NULL, NULL, 66
    UNION ALL SELECT 1101, 68, N'5.', N'Ostatné pohľadávky (315AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 67
    UNION ALL SELECT 1101, 69, N'6.', N'Pohľadávky z nedaňových rozpočtových príjmov (316) - (391AÚ)', NULL, 0, NULL, NULL, 68
    UNION ALL SELECT 1101, 70, N'7.', N'Pohľadávky z daňových a colných rozpočtových príjmov (317) - (391AÚ)', NULL, 0, NULL, NULL, 69
    UNION ALL SELECT 1101, 71, N'8.', N'Pohľadávky z nedaňových príjmov obcí a vyšších územných celkov a rozpočtových organizácií zriadených obcou a vyšším územným celkom (318) - (391AÚ)', NULL, 0, NULL, NULL, 70
    UNION ALL SELECT 1101, 4, N'A.I.1.', N'Aktivované náklady na vývoj (012) - (072+091AÚ)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1101, 5, N'2.', N'Softvér (013) - (073+091AÚ)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 1101, 6, N'3.', N'Oceniteľné práva (014) - (074+091AÚ)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 1101, 7, N'4.', N'Goodwill z konsolidácie kapitálu alebo negatívny goodwill z konsolidácie kapitálu (+/-)', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 1101, 8, N'5.', N'Drobný dlhodobý nehmotný majetok (018) - (078+091AÚ)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 1101, 9, N'6.', N'Ostatný dlhodobý nehmotný majetok (019) - (079+091AÚ)', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 1101, 10, N'7.', N'Obstaranie dlhodobého nehmotného majetku (041) - (093)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 1101, 11, N'8.', N'Poskytnuté preddavky na dlhodobý nehmotný majetok (051) - (095AÚ)', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 1101, 12, N'A.II.', N'Dlhodobý hmotný majetok súčet (r. 013 až 024)', NULL, 1, NULL, NULL, 11
    UNION ALL SELECT 1101, 13, N'A.II.1.', N'Pozemky (031) - (092AÚ)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 1101, 14, N'2.', N'Umelecké diela a zbierky (032) - (092AÚ)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 1101, 15, N'3.', N'Predmety z drahých kovov (033) - (092AÚ)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 1101, 16, N'4.', N'Stavby (021) - (081+092AÚ)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 1101, 17, N'5.', N'Samostatné hnuteľné veci a súbory hnuteľných vecí (022)-(082+092AÚ)', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 1101, 18, N'6.', N'Dopravné prostriedky (023) - (083+092AÚ)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 1101, 19, N'7.', N'Pestovateľské celky trvalých porastov (025) - (085+092AÚ)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 1101, 20, N'8.', N'Základné stádo a ťažné zvieratá (026) - (086+092AÚ)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 1101, 21, N'9.', N'Drobný dlhodobý hmotný majetok (028) - (088+092AÚ)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 1101, 22, N'10.', N'Ostatný dlhodobý hmotný majetok (029) - (089+092AÚ)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 1101, 23, N'11.', N'Obstaranie dlhodobého hmotného majetku (042) - (094)', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 1101, 24, N'12.', N'Poskytnuté preddavky na dlhodobý hmotný majetok (052) - (095AÚ)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 1101, 25, N'A.III.', N'Dlhodobý finančný majetok súčet (r. 026 + r. 27 + r.29 až 034)', NULL, 1, NULL, NULL, 24
    UNION ALL SELECT 1101, 26, N'A.III.1.', N'Podielové cenné papiere a podiely v dcérskej účtovnej jednotke (061) - (096AÚ)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 1101, 27, N'2.', N'Podielové cenné papiere a podiely v spoločnosti s podstatným vplyvom (062) - (096AÚ)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 1101, 28, NULL, N'z toho: goodwill', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 1101, 29, N'3.', N'Realizovateľné cenné papiere a podiely (063) - (096AÚ)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 1101, 30, N'4.', N'Dlhové cenné papiere držané do splatnosti (065) - (096AÚ)', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 1101, 31, N'5.', N'Pôžičky účtovnej jednotke v konsolidovanom celku (066) - (096AÚ)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 1101, 32, N'6.', N'Ostatné pôžičky (067) - (096AÚ)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 1101, 33, N'7.', N'Ostatný dlhodobý finančný majetok (069) - (096AÚ)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 1101, 34, N'8.', N'Obstaranie dlhodobého finančného majetku (043) - (096AÚ)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 1101, 35, N'B.', N'Obežný majetok r. 036 + r. 042 + r. 050 + r. 063 + r. 088+ r. 101 + r. 107', NULL, 1, NULL, NULL, 34
    UNION ALL SELECT 1101, 36, N'B.I.', N'Zásoby súčet (r. 037 až 041)', NULL, 1, NULL, NULL, 35
    UNION ALL SELECT 1101, 37, N'B.I.1.', N'Materiál (112 + 119) - (191)', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 1101, 38, N'2.', N'Nedokončená výroba a polotovary (121 + 122) - (192 + 193)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 1101, 39, N'3.', N'Výrobky (123) - (194)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 1101, 40, N'4.', N'Zvieratá (124) - (195)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 1101, 41, N'5.', N'Tovar (132 + 139) - (196)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 1101, 42, N'B.II.', N'Zúčtovanie medzi subjektami verejnej správy súčet (r. 043 až r. 049)', NULL, 1, NULL, NULL, 41
    UNION ALL SELECT 1101, 43, N'B.II.1.', N'Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa (351)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 1101, 44, N'2.', N'Zúčtovanie transferov štátneho rozpočtu (353)', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 1101, 45, N'3.', N'Zúčtovanie transferov rozpočtu obce a vyššieho územného celku (355)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 1101, 46, N'4.', N'Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku (356)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 1101, 47, N'5.', N'Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku (357)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 1101, 48, N'6.', N'Zúčtovanie transferov zo štátneho rozpočtu iným subjektom (358)', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 1101, 49, N'7.', N'Zúčtovanie transferov medzi subjektami verejnej správy (359)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 1101, 50, N'B. III', N'Dlhodobé pohľadávky súčet (r. 051 až 061)', NULL, 1, NULL, NULL, 49
    UNION ALL SELECT 1101, 51, N'B.III.1.', N'Odberatelia (311AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 1101, 52, N'2.', N'Zmenky na inkaso (312AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 1101, 53, N'3.', N'Pohľadávky za eskontované cenné papiere (313AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 1101, 54, N'4.', N'Ostatné pohľadávky (315AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 1101, 55, N'5.', N'Pohľadávky voči zamestnancom (335AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 1101, 56, N'6.', N'Pohľadávky voči združeniu (369AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 1101, 57, N'7.', N'Pohľadávky a záväzky z pevných termínových operácií (373AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 1101, 58, N'8.', N'Pohľadávky z nájmu (374AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 1101, 59, N'9.', N'Pohľadávky z vydaných dlhopisov (375AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 1602, 12, NULL, N'Záväzky', NULL, 0, NULL, NULL, 0
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 1602 AS [TableErpId], 13 AS [RowNumber], NULL AS [Designation], N'z toho: sociálny fond' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 1 AS [RowOrdinal]
    UNION ALL SELECT 1602, 14, NULL, N'fond prevádzky, údržby a opráv', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 1602, 15, NULL, N'Úvery a pôžičky', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 1602, 16, NULL, N'Záväzky celkom (súčet r. 12 a r.15)', NULL, 1, NULL, NULL, 4
    UNION ALL SELECT 1602, 17, NULL, N'Rozdiel majetku a záväzkov (r. 11 - r.16)', NULL, 1, NULL, NULL, 5
    UNION ALL SELECT 2001, 1, NULL, N'SPOLU MAJETOK r. 002 + r. 031 + r. 061', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 2001, 2, N'A.', N'Neobežný majetok r. 003 + r. 012 + r. 022', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 2001, 3, N'A.I.', N'Dlhodobý nehmotný majetok súčet (r. 004 až 011)', NULL, 1, NULL, NULL, 2
    UNION ALL SELECT 2001, 4, N'A.I.1.', N'Zriaďovacie náklady (011) - /071, 091A/', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 2001, 5, N'2.', N'Aktivované náklady na vývoj (012) - /072, 091A/', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 2001, 6, N'3.', N'Softvér (013) - /073, 091A/', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 2001, 7, N'4.', N'Oceniteľné práva (014) - /074, 091A/', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 2001, 8, N'5.', N'Goodwill (015) - /075, 091A/', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 2001, 9, N'6.', N'Ostatný dlhodobý nehmotný majetok (019, 01X) - /079, 07X, 091A/', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 2001, 10, N'7.', N'Obstarávaný dlhodobý nehmotný majetok (041) - 093', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 2001, 11, N'8.', N'Poskytnuté preddavky na dlhodobý nehmotný majetok (051) - 095A', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 2001, 12, N'A.II.', N'Dlhodobý hmotný majetok súčet (r. 013 až 021)', NULL, 1, NULL, NULL, 11
    UNION ALL SELECT 2001, 13, N'A.II.1.', N'Pozemky (031) - 092A', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 2001, 14, N'2.', N'Stavby (021) - /081, 092A/', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 2001, 15, N'3.', N'Samostatné hnuteľné veci a súbory hnuteľných vecí (022) - /082, 092A/', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 2001, 16, N'4.', N'Pestovateľské celky trvalých porastov (025) - /085, 092A/', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 2001, 17, N'5.', N'Základné stádo a ťažné zvieratá (026) - /086, 092A/', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 2001, 18, N'6.', N'Ostatný dlhodobý hmotný majetok (029, 02X, 032) - /089, 08X, 092A/', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 2001, 19, N'7.', N'Obstarávaný dlhodobý hmotný majetok (042) - 094', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 2001, 20, N'8.', N'Poskytnuté preddavky na dlhodobý hmotný majetok (052) - 095A', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 2001, 21, N'9.', N'Opravná položka k nadobudnutému majetku (+/- 097) +/- 098', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 2001, 22, N'A.III.', N'Dlhodobý finančný majetok súčet (r. 023 až 030)', NULL, 1, NULL, NULL, 21
    UNION ALL SELECT 2001, 23, N'A.III.1.', N'Podielové cenné papiere a podiely v dcérskej účtovnej jednotke (061) - 096A', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 2001, 24, N'2.', N'Podielové cenné papiere a podiely v spoločnosti s podstatným vplyvom (062) - 096A', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 2001, 25, N'3.', N'Ostatné dlhodobé cenné papiere a podiely (063, 065) - 096A', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 2001, 26, N'4.', N'Pôžičky účtovnej jednotke v konsolidovanom celku (066A) - 096A', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 2001, 27, N'5.', N'Ostatný dlhodobý finančný majetok (067A, 069, 06XA) - 096A', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 2001, 28, N'6.', N'Pôžičky s dobou splatnosti najviac jeden rok (066A, 067A, 06XA) - 096A', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 2001, 29, N'7.', N'Obstarávaný dlhodobý finančný majetok (043) - 096A', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 2001, 30, N'8.', N'Poskytnuté preddavky na dlhodobý finančný majetok (053) - 095A', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 2001, 31, N'B.', N'Obežný majetok r. 032 + r. 040 + r. 047 + r. 055', NULL, 1, NULL, NULL, 30
    UNION ALL SELECT 2001, 32, N'B.I.', N'Zásoby súčet (r. 033 až 039)', NULL, 1, NULL, NULL, 31
    UNION ALL SELECT 2001, 33, N'B.I.1.', N'Materiál (112, 119, 11X) - /191, 19X/', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 2001, 34, N'2.', N'Nedokončená výroba a polotovary vlastnej výroby (121, 122, 12X) - /192, 193, 19X/', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 2001, 35, N'3.', N'Zákazková výroba s predpokladanou dobou ukončenia dlhšou ako jeden rok 12X - 192A', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 2001, 36, N'4.', N'Výrobky (123) - 194', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 2001, 37, N'5.', N'Zvieratá (124) - 195', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 2001, 38, N'6.', N'Tovar (132, 13X, 139) - /196, 19X/', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 2001, 39, N'7.', N'Poskytnuté preddavky na zásoby (314A) - 391A', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 2001, 40, N'B.II.', N'Dlhodobé pohľadávky súčet (r. 041 až 046)', NULL, 1, NULL, NULL, 39
    UNION ALL SELECT 2001, 41, N'B.II.1.', N'Pohľadávky z obchodného styku (311A, 312A, 313A, 314A, 315A, 31XA) - 391A', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 2001, 42, N'2.', N'Pohľadávky voči dcérskej účtovnej jednotke a materskej účtovnej jednotke (351A) - 391A', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 2001, 43, N'3.', N'Ostatné pohľadávky v rámci konsolidovaného celku (351A) - 391A', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 2001, 44, N'4.', N'Pohľadávky voči spoločníkom, členom a združeniu (354A, 355A, 358A, 35XA) - 391A', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 2001, 45, N'5.', N'Iné pohľadávky (335A, 33XA, 371A, 373A, 374A, 375A, 376A, 378A) - 391A', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 2001, 46, N'6.', N'Odložená daňová pohľadávka (481A)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 2001, 47, N'B.III.', N'Krátkodobé pohľadávky súčet (r. 048 až 054)', NULL, 1, NULL, NULL, 46
    UNION ALL SELECT 2001, 48, N'B.III.1.', N'Pohľadávky z obchodného styku (311A, 312A, 313A, 314A, 315A, 31XA) - 391A', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 2001, 49, N'2.', N'Pohľadávky voči dcérskej účtovnej jednotke a materskej účtovnej jednotke (351A) - 391A', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 2001, 50, N'3.', N'Ostatné pohľadávky v rámci konsolidovaného celku (351A) - 391A', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 2001, 51, N'4.', N'Pohľadávky voči spoločníkom, členom a združeniu (354A, 355A, 358A, 35XA, 398A) - 391A', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 2001, 52, N'5.', N'Sociálne poistenie (336) - 391A', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 2001, 53, N'6.', N'Daňové pohľadávky a dotácie (341, 342, 343, 345, 346, 347) - 391A', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 2001, 54, N'7.', N'Iné pohľadávky (335A, 33XA, 371A, 373A, 374A, 375A, 376A,378A) - 391A', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 2001, 55, N'B.IV.', N'Finančné účty súčet (r. 056 až r. 060)', NULL, 1, NULL, NULL, 54
    UNION ALL SELECT 2001, 56, N'B.IV.1.', N'Peniaze (211, 213, 21X)', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 2001, 57, N'2.', N'Účty v bankách (221A, 22X +/- 261)', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 2001, 58, N'3.', N'Účty v bankách s dobou viazanosti dlhšou ako jeden rok 22XA', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 2001, 59, N'4.', N'Krátkodobý finančný majetok (251, 253, 256, 257, 25X) - /291, 29X/', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 2001, 60, N'5.', N'Obstarávaný krátkodobý finančný majetok (259, 314A) - 291', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 2001, 61, N'C.', N'Časové rozlíšenie súčet (r. 062 až r. 065)', NULL, 1, NULL, NULL, 60
    UNION ALL SELECT 2001, 62, N'C.1.', N'Náklady budúcich období dlhodobé (381A, 382A)', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 2001, 63, N'2.', N'Náklady budúcich období krátkodobé (381A, 382A)', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 2001, 64, N'3.', N'Príjmy budúcich období dlhodobé (385A)', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 2001, 65, N'4.', N'Príjmy budúcich období krátkodobé (385A)', NULL, 0, NULL, NULL, 64
    UNION ALL SELECT 2902, 56, NULL, N'A. Vlastné zdroje krytia majetku súčet (r. 057 + r. 062 + r. 072)', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 2902, 57, N'1.', N'Fondy účtovnej jednotky súčet (r. 058 až r.061)', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 2902, 58, N'1.', N'Fond dlhodobého majetku (901)', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 2902, 59, N'1.', N'Fond prevádzkových prostriedkov (902)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 2902, 60, N'1.', N'Oceňovacie rozdiely z precenenia majetku a záväzkov (905)', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 2902, 61, N'1.', N'Správny fond (+/- 914)', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 2902, 62, N'2.', N'Osobitné fondy účtovnej jednotky súčet (r. 063 až r. 071)', NULL, 1, NULL, NULL, 6
    UNION ALL SELECT 2902, 63, N'2.', N'Účet tvorby fondov (921)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 2902, 64, N'2.', N'Základný fond nemocenského poistenia (+/- 922)', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 2902, 65, N'2.', N'Základný fond starobného poistenia (+/- 923)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 2902, 66, N'2.', N'Základný fond invalidného poistenia (+/- 924)', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 2902, 67, N'2.', N'Základný fond garančného poistenia (+/- 925)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 2902, 68, N'2.', N'Základný fond poistenia v nezamestnanosti (+/- 926)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 2902, 69, N'2.', N'Základný fond úrazového poistenia (927)', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 2902, 70, N'2.', N'Základný fond príspevkov na starobné dôchodkové sporenie (928)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 2902, 71, N'2.', N'Rezervný fond solidarity (929)', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 2902, 72, N'3.', N'Výsledok hospodárenia súčet (r. 073 + r. 074 + r. 075)', NULL, 1, NULL, NULL, 16
    UNION ALL SELECT 2902, 73, N'3.', N'Účet ziskov a strát (+/- 963)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 2902, 74, N'3.', N'Výsledok hospodárenia v schvaľovaní (+/- 931)', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 18
    UNION ALL SELECT 2902, 75, N'3.', N'Nerozdelený zisk, neuhradená strata minulých rokov (+/- 932)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 1101, 72, N'9.', N'Pohľadávky z daňových príjmov obcí a vyšších územných celkov (319) - (391AÚ)', NULL, 0, NULL, NULL, 71
    UNION ALL SELECT 1101, 73, N'10.', N'Pohľadávky voči zamestnancom (335AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 72
    UNION ALL SELECT 1101, 74, N'11.', N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia (336) - (391AÚ)', NULL, 0, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 73
    UNION ALL SELECT 1101, 75, N'12.', N'Daň z príjmov (341) - (391AÚ)', NULL, 0, NULL, NULL, 74
    UNION ALL SELECT 1101, 76, N'13.', N'Ostatné priame dane (342) - (391AÚ)', NULL, 0, NULL, NULL, 75
    UNION ALL SELECT 1101, 77, N'14.', N'Daň z pridanej hodnoty (343) - (391AÚ)', NULL, 0, NULL, NULL, 76
    UNION ALL SELECT 1101, 78, N'15.', N'Ostatné dane a poplatky (345) - (391AÚ)', NULL, 0, NULL, NULL, 77
    UNION ALL SELECT 1101, 79, N'16.', N'Pohľadávky voči združeniu (369AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 78
    UNION ALL SELECT 1101, 80, N'17.', N'Pohľadávky a záväzky z pevných termínovaných operácií (373AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 79
    UNION ALL SELECT 1101, 81, N'18.', N'Pohľadávky z nájmu (374AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 80
    UNION ALL SELECT 1101, 82, N'19.', N'Pohľadávky z vydaných dlhopisov (375AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 81
    UNION ALL SELECT 1101, 83, N'20.', N'Nakúpené opcie (376AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 82
    UNION ALL SELECT 1101, 84, N'21.', N'Iné pohľadávky (378AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 83
    UNION ALL SELECT 1101, 85, N'22.', N'Spojovací účet pri združení (396AÚ)', NULL, 0, NULL, NULL, 84
    UNION ALL SELECT 1101, 86, N'23.', N'Zúčtovanie s Európskymi spoločenstvami (371AÚ)- (391AÚ)', NULL, 0, NULL, NULL, 85
    UNION ALL SELECT 1101, 87, N'24.', N'Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy (372AÚ) - (391AÚ)', NULL, 0, NULL, NULL, 86
    UNION ALL SELECT 1101, 88, N'B.V.', N'Finančné účty súčet (r. 089 až 100)', NULL, 1, NULL, NULL, 87
    UNION ALL SELECT 1101, 89, N'B.V.1.', N'Pokladnica (211)', NULL, 0, NULL, NULL, 88
    UNION ALL SELECT 1101, 90, N'2.', N'Ceniny (213)', NULL, 0, NULL, NULL, 89
    UNION ALL SELECT 1101, 91, N'3.', N'Bankové účty (221AÚ +/- 261)', NULL, 0, NULL, NULL, 90
    UNION ALL SELECT 1101, 92, N'4.', N'Účty v bankách s dobou viazanosti dlhšou ako jeden rok (221AÚ)', NULL, 0, NULL, NULL, 91
    UNION ALL SELECT 1101, 93, N'5.', N'Výdavkový rozpočtový účet (222)', NULL, 0, NULL, NULL, 92
    UNION ALL SELECT 1101, 94, N'6.', N'Príjmový rozpočtový účet (223)', NULL, 0, NULL, NULL, 93
    UNION ALL SELECT 1101, 95, N'7.', N'Majetkové cenné papiere na obchodovanie (251) - (291AÚ)', NULL, 0, NULL, NULL, 94
    UNION ALL SELECT 1101, 96, N'8.', N'Dlhové cenné papiere na obchodovanie (253) - (291AÚ)', NULL, 0, NULL, NULL, 95
    UNION ALL SELECT 1101, 97, N'9.', N'Dlhové cenné papiere so splatnosťou do jedného roka držané do splatnosti (256) - (291AÚ)', NULL, 0, NULL, NULL, 96
    UNION ALL SELECT 1101, 98, N'10.', N'Ostatné realizovateľné cenné papiere (257) - (291AÚ)', NULL, 0, NULL, NULL, 97
    UNION ALL SELECT 1101, 99, N'11.', N'Obstaranie krátkodobého finančného majetku (259) - (291AÚ)', NULL, 0, NULL, NULL, 98
    UNION ALL SELECT 1101, 100, N'12.', N'Účty štátnej pokladnice (účtová skupina 28)', NULL, 0, NULL, NULL, 99
    UNION ALL SELECT 1101, 101, N'B.VI.', N'Poskytnuté návratné finančné výpomoci dlhodobé súčet (r. 102 až r. 106)', NULL, 1, NULL, NULL, 100
    UNION ALL SELECT 1101, 102, N'B.VI.1.', N'Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku (271AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 101
    UNION ALL SELECT 1101, 103, N'2.', N'Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy (272AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 102
    UNION ALL SELECT 1101, 104, N'3.', N'Poskytnuté návratné finančné výpomoci podnikateľským subjektom (274AÚ )- (291AÚ)', NULL, 0, NULL, NULL, 103
    UNION ALL SELECT 1101, 105, N'4.', N'Poskytnuté návratné finančné výpomoci ostatným organizáciám (275AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 104
    UNION ALL SELECT 1101, 106, N'5.', N'Poskytnuté návratné finančné výpomoci fyzickým osobám (277AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 105
    UNION ALL SELECT 1101, 107, N'B.VII.', N'Poskytnuté návratné finančné výpomoci krátkodobé súčet (r. 108 až r. 112)', NULL, 1, NULL, NULL, 106
    UNION ALL SELECT 1101, 108, N'B.VII.1.', N'Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku (271AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 107
    UNION ALL SELECT 1101, 109, N'2.', N'Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy (272AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 108
    UNION ALL SELECT 1101, 110, N'3.', N'Poskytnuté návratné finančné výpomoci podnikateľským subjektom (274AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 109
    UNION ALL SELECT 1101, 111, N'4.', N'Poskytnuté návratné finančné výpomoci ostatným organizáciám (275AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 110
    UNION ALL SELECT 1101, 112, N'5.', N'Poskytnuté návratné finančné výpomoci fyzickým osobám (277AÚ) - (291AÚ)', NULL, 0, NULL, NULL, 111
    UNION ALL SELECT 1101, 113, N'C.', N'Časové rozlíšenie súčet (r. 114 až r. 116)', NULL, 1, NULL, NULL, 112
    UNION ALL SELECT 1101, 114, N'C. 1.', N'Náklady budúcich období (381)', NULL, 0, NULL, NULL, 113
    UNION ALL SELECT 1101, 115, N'2.', N'Komplexné náklady budúcich období (382)', NULL, 0, NULL, NULL, 114
    UNION ALL SELECT 1101, 116, N'3.', N'Príjmy budúcich období (385)', NULL, 0, NULL, NULL, 115
    UNION ALL SELECT 1101, 117, N'D.', N'Vzťahy k účtom klientov štátnej pokladnice (účtová skupina 20)', NULL, 0, NULL, NULL, 116
    UNION ALL SELECT 1101, 888, NULL, N'KONTROLNÉ ČÍSLO súčet (r. 001 až r. 117)', NULL, 1, NULL, NULL, 117
    UNION ALL SELECT 54202, 61, N'A.', N'Vlastné imanie', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 54202, 62, N'I.', N'Základné imanie, z toho', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 54202, 63, N'1.', N'upísané základné imanie splatené', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 54202, 64, N'2.', N'Vlastné akcie (–)', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 54202, 65, N'lII.', N'Emisné ážio', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 54202, 66, N'III.', N'Oceňovacie rozdiely z ocenenia majetku a záväzkov', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 54202, 67, N'IV.', N'Rezervné fondy a ostatné fondy tvorené zo zisku', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 54202, 68, N'1.', N'Ostatné kapitálové fondy', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 54202, 69, N'V.', N'Výsledok hospodárenia minulých rokov', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 54202, 70, N'VI.', N'Výsledok hospodárenia bežného účtovného obdobia', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 54202, 71, N'B.', N'Podriadené pasíva', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 54202, 72, N'C.', N'Technické rezervy', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 54202, 73, N'1.', N'Technická rezerva na poistné budúcich období', NULL, 0, NULL, NULL, 12
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 54202 AS [TableErpId], 74 AS [RowNumber], N'1a.' AS [Designation], N'Hrubá výška' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], N'Krátkodobé pohladávky - Dan z pridanej hodnoty' AS [Category_sk], NULL AS [MappingCaption_sk], 13 AS [RowOrdinal]
    UNION ALL SELECT 54202, 75, N'1b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 54202, 76, N'3.', N'Technická rezerva na poistné plnenie', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 54202, 77, N'3a.', N'Hrubá výška', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 54202, 78, N'3b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 54202, 79, N'4.', N'Technická rezerva na poistné prémie a zľavy', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 54202, 80, N'4a.', N'Hrubá výška', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 54202, 81, N'4b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 54202, 82, N'6.', N'Iné technické rezervy', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 54202, 83, N'6a.', N'Hrubá výška', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 54202, 84, N'6b.', N'Výška zaistenia (–)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 54202, 85, N'E.', N'Ostatné rezervy', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 54202, 86, N'G.', N'Záväzky, z toho', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 54202, 87, N'I.A.', N'z verejného zdravotného poistenia, z toho', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 54202, 88, N'1.', N'voči poisteným, z toho', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 54202, 89, N'1a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 54202, 90, N'1b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 54202, 91, N'2.', N'voči poskytovateľom zdravotnej starostlivosti', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 54202, 92, N'2a.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka rozhodujúci vplyv', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 54202, 93, N'2b.', N'voči obchodným spoločnostiam, v ktorých má účtovná jednotka podstatný vplyv', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 54202, 94, N'3.', N'voči inej zdravotnej poisťovni, z toho', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 54202, 95, N'3a.', N'z prerozdelenia poistného', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 54202, 96, N'4.', N'voči Úradu pre dohľad nad zdravotnou starostlivosťou', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 54202, 97, N'5.', N'voči Ministerstvu zdravotníctva Slovenskej republiky', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 54202, 98, N'I.B.', N'z individuálneho zdravotného poistenia', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 54202, 99, N'1.', N'voči poisteným', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 54202, 100, N'2.', N'voči sprostredkovateľom poistenia', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 54202, 101, N'3.', N'voči poskytovateľom zdravotnej starostlivosti', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 54202, 102, N'II.', N'zo zaistenia', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 54202, 103, N'III.', N'pôžičky zaručené dlhopisom, z toho', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 54202, 104, N'1.', N'v konvertibilnej mene', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 54202, 105, N'2.', N'krátkodobé pôžičky', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 54202, 106, N'3.', N'dlhodobé pôžičky', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 54202, 107, N'IV.', N'bankové úvery, z toho', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 54202, 108, N'1.', N'krátkodobé úvery', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 54202, 109, N'V.', N'ostatné záväzky, z toho', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 54202, 110, N'1.', N'z daní', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 54202, 111, N'2.', N'záväzky voči zamestnancom celkom', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 54202, 112, N'2a.', N'z toho zo sociálneho poistenia a zdravotného poistenia', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 54202, 113, N'3.', N'z finančného prenájmu', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 54202, 114, N'4.', N'z dotácií zo štátneho rozpočtu a ostatné dotácie', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 54202, 115, N'H.', N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 54202, 116, NULL, N'PASÍVA spolu', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 54202, 999, NULL, N'Kontrolné číslo', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 518202, 1, N'A.A.', N'Peňažné prostriedky a peňažné ekvivalenty', N'Cash and cash equivalents', 0, NULL, NULL, 0
    UNION ALL SELECT 518202, 2, N'A.B.', N'Investície', N'Investments', 0, NULL, NULL, 1
    UNION ALL SELECT 518202, 3, N'A.B.1.', N'Finančné nástroje oceňované reálnou hodnotou cez výsledok hospodárenia (FVTPL)', N'Investments at FVTPL', 0, NULL, NULL, 2
    UNION ALL SELECT 518202, 4, N'A.B.2.', N'Finančné nástroje oceňované reálnou hodnotou cez ostatné súčasti komplexného výsledku (FVOCI)', N'Investments at FVOCI', 0, NULL, NULL, 3
    UNION ALL SELECT 518202, 5, N'A.B.3.', N'Finančné nástroje oceňované amortizovanou hodnotou (AC)', N'Investments at AC', 0, NULL, NULL, 4
    UNION ALL SELECT 518202, 6, N'A.B.4.', N'Podiely v prepojených podnikoch vrátane účastí', N'Investments in affiliated and associated enterprises and joint ventures', 0, NULL, NULL, 5
    UNION ALL SELECT 518202, 7, N'A.C.', N'Hodnota poistných zmlúv ako aktívum', N'Insurance contracts that are assets', 0, NULL, NULL, 6
    UNION ALL SELECT 518202, 8, N'A.C.1.', N'Hodnota poistných zmlúv na zostávajúce krytie', N'Assets for remaining coverage', 0, NULL, NULL, 7
    UNION ALL SELECT 518202, 9, N'A.C.1.1.', N'Súčasná hodnota budúcich peňažných tokov', N'PV FCF', 0, NULL, NULL, 8
    UNION ALL SELECT 518202, 10, N'A.C.1.2.', N'Servisná marža', N'CSM', 0, NULL, NULL, 9
    UNION ALL SELECT 518202, 11, N'A.C.1.3.', N'Riziková prirážka na nefinančné riziká', N'RA', 0, NULL, NULL, 10
    UNION ALL SELECT 518202, 12, N'A.C.1.4.', N'Hodnota poistných zmlúv ocenené podľa PAA modelu', N'Insurance contract assets measured under PAA', 0, NULL, NULL, 11
    UNION ALL SELECT 518202, 13, N'A.C.2.', N'Hodnota poistných zmlúv na vzniknuté poistné udalosti', N'Asset for Incurred Claims AIC', 0, NULL, NULL, 12
    UNION ALL SELECT 518202, 14, N'A.C.2.1.', N'Súčasná hodnota budúcich peňažných tokov', N'PV FCF', 0, NULL, NULL, 13
    UNION ALL SELECT 518202, 15, N'A.C.2.2.', N'Riziková prirážka na nefinančné riziká', N'RA', 0, NULL, NULL, 14
    UNION ALL SELECT 518202, 16, N'A.C.3.', N'Predplatené alebo nealokované obstarávacie náklady na poistné zmluvy', N'Prepaid or not allocated acqusition costs', 0, NULL, NULL, 15
    UNION ALL SELECT 518202, 17, N'A.D.', N'Pasívne zaistenie ako aktívum', N'Reinsurance contracts that are assets', 0, NULL, NULL, 16
    UNION ALL SELECT 518202, 18, N'A.E.', N'Pohľadávky (iné ako z poistenia a zaistenia)', N'Receivables (other than from insurance and reinsurance)', 0, NULL, NULL, 17
    UNION ALL SELECT 518202, 19, N'A.F.', N'Hmotný majetok', N'Property and equipment', 0, NULL, NULL, 18
    UNION ALL SELECT 518202, 20, N'A.G.', N'Nehmotné aktíva', N'Intangible assets', 0, NULL, NULL, 19
    UNION ALL SELECT 518202, 21, N'A.G.1.', N'Goodwill', N'Goodwill', 0, NULL, NULL, 20
    UNION ALL SELECT 518202, 22, N'A.G.2.', N'Softvér', N'Software', 0, NULL, NULL, 21
    UNION ALL SELECT 518202, 23, N'A.G.3.', N'Ostatné nehmotné aktíva', N'Other intangible assets', 0, NULL, NULL, 22
    UNION ALL SELECT 518202, 24, N'A.H.', N'Daňové pohľadávky', N'Tax receivables', 0, NULL, NULL, 23
    UNION ALL SELECT 518202, 25, N'A.H.1.', N'Splatná daň z príjmov - pohľadávka', N'Current tax asset', 0, NULL, NULL, 24
    UNION ALL SELECT 518202, 26, N'A.H.2.', N'Odložené daňové pohľadávky', N'Deferred tax assets', 0, NULL, NULL, 25
    UNION ALL SELECT 518202, 27, N'A.I.', N'Účty časového rozlíšenia (aktívne)', N'Accrual accounts (active)', 0, NULL, NULL, 26
    UNION ALL SELECT 518202, 28, N'A.J.', N'Ostatné aktíva, inde neuvedené', N'Other assets', 0, NULL, NULL, 27
    UNION ALL SELECT 518202, 29, NULL, N'AKTÍVA SPOLU', N'TOTAL ASSETS', 0, NULL, NULL, 28
    UNION ALL SELECT 518202, 30, N'P.A.', N'Finančné záväzky', N'Financial liabilities', 0, NULL, NULL, 29
    UNION ALL SELECT 518202, 31, N'P.A.1.', N'Záväzky z investičných zmlúv', N'Investment contract liabilities', 0, NULL, NULL, 30
    UNION ALL SELECT 518202, 32, N'P.A.2.', N'Prijaté úvery a pôžičky', N'Loans received', 0, NULL, NULL, 31
    UNION ALL SELECT 518202, 33, N'P.B.', N'Hodnota poistných zmlúv ako záväzok', N'Insurance contracts liabilities', 0, NULL, NULL, 32
    UNION ALL SELECT 518202, 34, N'P.B.1.', N'Hodnota poistných zmlúv na zostávajúce krytie', N'Liability for remaining coverage', 0, NULL, NULL, 33
    UNION ALL SELECT 518202, 35, N'P.B.1.1.', N'Súčasná hodnota budúcich peňažných tokov', N'PV FCF', 0, NULL, NULL, 34
    UNION ALL SELECT 518202, 36, N'P.B.1.2.', N'Servisná marža', N'CSM', 0, NULL, NULL, 35
    UNION ALL SELECT 518202, 37, N'P.B.1.3.', N'Riziková prirážka na nefinančné riziká', N'RA', 0, NULL, NULL, 36
    UNION ALL SELECT 518202, 38, N'P.B.1.4.', N'Hodnota poistných zmlúv ocenené podľa PAA modelu', N'Insurance contract liabilities measured under PAA', 0, NULL, NULL, 37
    UNION ALL SELECT 518202, 39, N'P.B.2.', N'Záväzky zo vzniknutých poistných udalostí', N'Liability for incurred claims', 0, NULL, NULL, 38
    UNION ALL SELECT 518202, 45, N'P.F.', N'Rezervy (iné ako z poistenia a zaistenia)', N'Reserves (other than from insurance and reinsurance)', 0, NULL, NULL, 44
    UNION ALL SELECT 518202, 46, N'P.G', N'Podriadené záväzky', N'Subordinated liabilities', 0, NULL, NULL, 45
    UNION ALL SELECT 518202, 47, N'P.H', N'Daňové záväzky', N'Tax payables', 0, NULL, NULL, 46
    UNION ALL SELECT 518202, 48, N'P.H.1.', N'Splatná daň z príjmov - záväzok', N'Current tax liability', 0, NULL, NULL, 47
    UNION ALL SELECT 518202, 49, N'P.H.2.', N'Odložené daňové záväzky', N'Deferred income tax liabilities', 0, NULL, NULL, 48
    UNION ALL SELECT 518202, 50, N'P.I.', N'Účty časového rozlíšenia (pasívne)', N'Accrual accounts (passive)', 0, NULL, NULL, 49
    UNION ALL SELECT 518202, 51, N'P.J.', N'Ostatné pasíva, inde neuvedené', N'Other liabilities', 0, NULL, NULL, 50
    UNION ALL SELECT 518202, 52, NULL, N'ZÁVÄZKY SPOLU', N'TOTAL LIABILITIES', 0, NULL, NULL, 51
    UNION ALL SELECT 518202, 53, N'P.K.', N'Základné imanie', N'Issued capital', 0, NULL, NULL, 52
    UNION ALL SELECT 518202, 54, N'P.K.1.', N'z toho: upísané základné imanie splatené', N'of which: paid-up subscribed capital', 0, NULL, NULL, 53
    UNION ALL SELECT 518202, 55, N'P.L.', N'Vlastné akcie', N'Own shares', 0, NULL, NULL, 54
    UNION ALL SELECT 518202, 56, N'P.M.', N'Emisné ážio', N'Share premium', 0, NULL, NULL, 55
    UNION ALL SELECT 518202, 57, N'P.N.', N'Rezervné fondy a fondy tvorené zo zisku', N'Capital reserve', 0, NULL, NULL, 56
    UNION ALL SELECT 518202, 58, N'P.O.', N'Ostatné kapitálové fondy', N'Other capital funds', 0, NULL, NULL, 57
    UNION ALL SELECT 518202, 59, N'P.P.', N'Oceňovacie rozdiely, z toho:', N'Valuation differences, of which:', 0, NULL, NULL, 58
    UNION ALL SELECT 518202, 60, N'P.P.1.', N'Úpravy vyplývajúce z prepočtu cudzích mien', N'Foreign currency translation adjustments', 0, NULL, NULL, 59
    UNION ALL SELECT 518202, 61, N'P.P.2.', N'Oceňovacie rozdiely z ocenenia finančných nástrojov cez OCI', N'Unrealized gains and losses (OCI)', 0, NULL, NULL, 60
    UNION ALL SELECT 518202, 62, N'P.P.3.', N'Oceňovacie rozdiely z poistných zmlúv a zaistných zmlúv cez OCI', N'(Re)insurance assets and liabilities / (Re)insurance finance reserve', 0, NULL, NULL, 61
    UNION ALL SELECT 518202, 63, N'P.P.4.', N'Očakávané kreditné straty', N'Expected credit loss (ECL)', 0, NULL, NULL, 62
    UNION ALL SELECT 518202, 64, N'P.Q.', N'Výsledok hospodárenia minulých rokov', N'Profit or loss from previous periods', 0, NULL, NULL, 63
    UNION ALL SELECT 518202, 65, N'P.R.', N'Výsledok hospodárenia bežného obdobia', N'Profit or loss for the current period', 0, NULL, NULL, 64
    UNION ALL SELECT 518202, 66, NULL, N'VLASTNÉ IMANIE SPOLU', N'TOTAL EQUITY', 0, NULL, NULL, 65
    UNION ALL SELECT 518202, 67, NULL, N'PASÍVA SPOLU', N'TOTAL EQUITY AND LIABILITIES', 0, NULL, NULL, 66
    UNION ALL SELECT 68701, 1, NULL, N'SPOLU MAJETOK r. 02 + r. 14', NULL, 1, NULL, NULL, 0
    UNION ALL SELECT 68701, 2, N'A.', N'Neobežný majetok r. 03 + r. 04 + r. 09', NULL, 1, NULL, NULL, 1
    UNION ALL SELECT 68701, 3, N'A.I.', N'Dlhodobý nehmotný majetok (012, 013, 014, 015, 019, 01X, 041, 051) - /072, 073, 074, 075, 079, 07X, 091, 093, 095A/', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 68701, 4, N'A.II.', N'Dlhodobý hmotný majetok súčet (r. 05 až r. 08)', NULL, 1, NULL, NULL, 3
    UNION ALL SELECT 68701, 5, N'A.II.1.', N'Pozemky a stavby (021, 031, 042A, 052A) - /081, 092A, 094A, 095A/', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 68701, 6, N'2.', N'Samostatné hnuteľné veci a súbory hnuteľných vecí (022, 02X, 042A, 052A) - /082, 08XA, 092A, 094A, 095A/', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 68701, 7, N'3.', N'Ostatný dlhodobý hmotný majetok (025, 026, 029, 02X, 032, 042A, 052A) - /085, 086, 089, 08XA, 092A, 094A, 095A/', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 68701, 8, N'4.', N'Opravná položka k nadobudnutému majetku (+/- 097 ) - /+/- 098/', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 68701, 9, N'A.III.', N'Dlhodobý finančný majetok súčet (r. 10 až r. 13)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 68701, 10, N'A.III.1.', N'Podielové cenné papiere (061, 062, 063, 043A, 053A) - /095A, 096A/', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 68701, 11, N'2.', N'Ostatný dlhodobý finančný majetok (065A, 066A, 067A, 069, 06XA, 043A, 053A) - /095A, 096A/', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 68701, 12, N'3.', N'Účty v bankách s dobou viazanosti dlhšou ako jeden rok (22XA)', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 68701, 13, N'4.', N'Ostatný dlhodobý finančný majetok so zostatkovou dobou splatnosti najviac jeden rok (065A, 066A, 067A, 06XA) - /096A/', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 68701, 14, N'B.', N'Obežný majetok r. 15 + r. 16 + r. 17 + r. 21', NULL, 1, NULL, NULL, 13
    UNION ALL SELECT 68701, 15, N'B.I.', N'Zásoby (112, 119, 11X, 121, 122, 123, 124, 12X, 132, 133, 13X, 139, 314A) - /191, 192, 193, 194, 195, 196, 19X, 391A/', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 68701, 16, N'B.II.', N'Dlhodobé pohľadávky (311A, 312A, 313A, 314A, 315A, 316A, 31XA, 335A, 336A, 33XA, 354A, 355A, 358A, 35XA, 371A, 374A, 375A, 378A, 381A, 382A, 385A) - 391A', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 68701, 17, N'B.III.', N'Krátkodobé pohľadávky súčet (r. 18 až r. 20)', NULL, 1, NULL, NULL, 16
    UNION ALL SELECT 68701, 18, N'B.III.1.', N'Pohľadávky z obchodného styku (311A, 312A, 313A, 314A, 315A, 316A, 31XA) - /391A/', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 68701, 19, N'2.', N'Sociálne poistenie, daňové pohľadávky a dotácie (336A, 341A, 342A, 343A, 345A, 346A, 347A, 34XA) - /391A/', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 68701, 20, N'3.', N'Ostatné pohľadávky (335A, 336A, 33XA, 354A, 355A, 358A, 35XA, 371A, 374A, 375A, 378A, 381A, 382A, 385A, 398A) - /391A/', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 68701, 21, N'B.IV.', N'Finančný majetok r. 22 + r. 23', NULL, 1, NULL, NULL, 20
    UNION ALL SELECT 68701, 22, N'B.IV.1.', N'Peniaze a účty v bankách (211, 213, 21X, 221A, 22XA, +/- 261)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 68701, 23, N'2.', N'Ostatné finančné účty (251, 252, 253, 256, 257, 25X, 259, 314A) - /291, 29X/', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 69901, 1, NULL, N'SPOLU MAJETOK r. 02 + r. 33 + r. 74', N'TOTAL ASSETS line 02 + line 33 + line 74', 1, N'SPOLU MAJETOK', NULL, 0
    UNION ALL SELECT 69901, 2, N'A.', N'Neobežný majetok r. 03 + r. 11 + r. 21', N'Non-current assets line 03 + line 11 + line 21', 1, N'Neobežný majetok', NULL, 1
    UNION ALL SELECT 69901, 3, N'A.I.', N'Dlhodobý nehmotný majetok súčet (r. 04 až r. 10)', N'Non-current intangible assets - total (lines 04 to 10)', 1, N'Dlhodobý nehmotný majetok', NULL, 2
    UNION ALL SELECT 69901, 4, N'A.I.1.', N'Aktivované náklady na vývoj (012) - /072, 091A/', N'Capitalized development costs (012) - /072, 091A/', 0, NULL, NULL, 3
    UNION ALL SELECT 69901, 5, N'2.', N'Softvér (013) - /073, 091A/', N'Software (013)-/073, 091A/', 0, NULL, NULL, 4
    UNION ALL SELECT 69901, 6, N'3.', N'Oceniteľné práva (014) - /074, 091A/', N'Valuable rights (014)-/074, 091A/', 0, NULL, NULL, 5
    UNION ALL SELECT 69901, 7, N'4.', N'Goodwill (015) - /075, 091A/', N'Goodwill (015) - /075, 091A/', 0, NULL, NULL, 6
    UNION ALL SELECT 69901, 8, N'5.', N'Ostatný dlhodobý nehmotný majetok (019, 01X) - /079, 07X, 091A/', N'Other non-current intangible assets (019, 01X) - /079, 07X, 091A/', 0, NULL, NULL, 7
    UNION ALL SELECT 69901, 9, N'6.', N'Obstarávaný dlhodobý nehmotný majetok (041) - /093/', N'Acquisition of non-current intangible assets (041) - /093/', 0, NULL, NULL, 8
    UNION ALL SELECT 69901, 10, N'7.', N'Poskytnuté preddavky na dlhodobý nehmotný majetok (051) - /095A/', N'Advance payments made for non-current intangible assets (051) - /095A/', 0, NULL, NULL, 9
    UNION ALL SELECT 69901, 11, N'A.II.', N'Dlhodobý hmotný majetok súčet (r. 12 až r. 20)', N'Property, plant and equipment - total (lines 12 to 20)', 1, N'Dlhodobý hmotný majetok', NULL, 10
    UNION ALL SELECT 69901, 12, N'A.II.1.', N'Pozemky (031) - /092A/', N'Land (031) - /092A/', 0, NULL, NULL, 11
    UNION ALL SELECT 69901, 13, N'2.', N'Stavby (021) - /081, 092A/', N'Structures (021) - /081, 092A/', 0, NULL, NULL, 12
    UNION ALL SELECT 69901, 14, N'3.', N'Samostatné hnuteľné veci a súbory hnuteľných vecí (022) - /082, 092A/', N'Individual movable assets and sets of movable assets (022) - /082, 092A/', 0, NULL, NULL, 13
    UNION ALL SELECT 69901, 15, N'4.', N'Pestovateľské celky trvalých porastov (025) - /085, 092A/', N'Perennial crops (025) - /085, 092A/', 0, NULL, NULL, 14
    UNION ALL SELECT 69901, 16, N'5.', N'Základné stádo a ťažné zvieratá (026) - /086, 092A/', N'Livestock (026) - /086, 092A/', 0, NULL, NULL, 15
    UNION ALL SELECT 69901, 17, N'6.', N'Ostatný dlhodobý hmotný majetok (029, 02X, 032) - /089, 08X, 092A/', N'Other property, plant and equipment (029, 02X, 032) - /089, 08X, 092A/', 0, NULL, NULL, 16
    UNION ALL SELECT 69901, 18, N'7.', N'Obstarávaný dlhodobý hmotný majetok (042) - /094/', N'Acquisition of property, plant and equipment (042) - /094/', 0, NULL, NULL, 17
    UNION ALL SELECT 69901, 19, N'8.', N'Poskytnuté preddavky na dlhodobý hmotný majetok (052) - /095A/', N'Advance payments made for property, plant and equipment (052) - /095A/', 0, NULL, NULL, 18
    UNION ALL SELECT 69901, 20, N'9.', N'Opravná položka k nadobudnutému majetku (+/- 097) +/- 098', N'Value adjustment to acquired assets (+/- 097) +/- 098', 0, NULL, NULL, 19
    UNION ALL SELECT 69901, 21, N'A.III.', N'Dlhodobý finančný majetok súčet (r. 22 až r. 32)', N'Non-current financial assets - total (lines 22 to 32)', 1, NULL, NULL, 20
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 69901 AS [TableErpId], 22 AS [RowNumber], N'A.III.1.' AS [Designation], N'Podielové cenné papiere a podiely v prepojených účtovných jednotkách (061A, 062A, 063A) - /096A/' AS [Text_sk], N'Shares and ownership interests in affiliated accounting entities (061A, 062A, 063A) - /096A/' AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 21 AS [RowOrdinal]
    UNION ALL SELECT 69901, 23, N'2.', N'Podielové cenné papiere a podiely s podielovou účasťou okrem v prepojených účtovných jednotkách (062A) - /096A/', N'Shares and ownership interests with participating interest, except for affiliated accounting entities (062A) - /096A/', 0, NULL, NULL, 22
    UNION ALL SELECT 69901, 24, N'3.', N'Ostatné realizovateľné cenné papiere a podiely (063A) - /096A/', N'Other available-for-sale securities and ownership interests (063A) - /096A/', 0, N'Dlhodobý finančný majetok', NULL, 23
    UNION ALL SELECT 69901, 25, N'4.', N'Pôžičky prepojeným účtovným jednotkám (066A) - /096A/', N'Loans to affiliated accounting entities (066A) - /096A/', 0, NULL, NULL, 24
    UNION ALL SELECT 69901, 26, N'5.', N'Pôžičky v rámci podielovej účasti okrem prepojeným účtovným jednotkám (066A) - /096A/', N'Loans within participating interest, except for affiliated accounting entities (066A) - /096A/', 0, NULL, NULL, 25
    UNION ALL SELECT 69901, 27, N'6.', N'Ostatné pôžičky (067A) - /096A/', N'Other loans (067A) - /096A/', 0, NULL, NULL, 26
    UNION ALL SELECT 69901, 28, N'7.', N'Dlhové cenné papiere a ostatný dlhodobý finančný majetok (065A, 069A,06XA) - /096A/', N'Debt securities and other non-current financial assets (065A, 069A, 06XA) - /096A/', 0, NULL, NULL, 27
    UNION ALL SELECT 69901, 29, N'8.', N'Pôžičky a ostatný dlhodobý finančný majetok so zostatkovou dobou splatnosti najviac jeden rok (066A, 067A, 069A, 06XA) - /096A/', N'Loans and other non-current financial assets with remaining maturity of up to one year (066A, 067A, 069A, 06XA) - /096A/', 0, NULL, NULL, 28
    UNION ALL SELECT 69901, 30, N'9.', N'Účty v bankách s dobou viazanosti dlhšou ako jeden rok (22XA)', N'Bank accounts with notice period exceeding one year (22XA)', 0, NULL, NULL, 29
    UNION ALL SELECT 69901, 31, N'10.', N'Obstarávaný dlhodobý finančný majetok (043) - /096A/', N'Acquisition of non-current financial assets(043) - /096A/', 0, NULL, NULL, 30
    UNION ALL SELECT 69901, 32, N'11.', N'Poskytnuté preddavky na dlhodobý finančný majetok (053) - /095A/', N'Advance payments made for non-current financial assets (053) - /095A/', 0, NULL, NULL, 31
    UNION ALL SELECT 69901, 33, N'B.', N'Obežný majetok r. 34 + r. 41 + r. 53 + r. 66 + r. 71', N'Current assets line 34 + line 41 + line 53 + line 66 + line 71', 1, N'Obežný majetok', NULL, 32
    UNION ALL SELECT 69901, 34, N'B.I.', N'Zásoby súčet (r. 35 až r. 40)', N'Inventory - total (lines 35 to 40)', 1, N'Zásoby súčet', NULL, 33
    UNION ALL SELECT 69901, 35, N'B.I.1.', N'Materiál (112, 119, 11X) - /191, 19X/', N'Raw material (112, 119, 11X) - /191, 19X/', 0, NULL, NULL, 34
    UNION ALL SELECT 69901, 36, N'2.', N'Nedokončená výroba a polotovary vlastnej výroby (121, 122, 12X) - /192, 193, 19X/', N'Work in progress and semi-finished products(121, 122, 12X) - /192, 193, 19X/', 0, NULL, NULL, 35
    UNION ALL SELECT 69901, 37, N'3.', N'Výrobky (123) - /194/', N'Finished goods (123) - /194/', 0, NULL, NULL, 36
    UNION ALL SELECT 69901, 38, N'4.', N'Zvieratá (124) - /195/', N'Animals (124) - /195/', 0, NULL, NULL, 37
    UNION ALL SELECT 69901, 39, N'5.', N'Tovar (132, 133, 13X, 139) - /196, 19X/', N'Merchandise (132, 133, 13X, 139) - /196, 19X/', 0, NULL, NULL, 38
    UNION ALL SELECT 69901, 40, N'6.', N'Poskytnuté preddavky na zásoby (314A) - /391A/', N'Advance payments made for inventory (314A) - /391A/', 0, N'Zúčtovanie medzi subjektami verejnej správy', NULL, 39
    UNION ALL SELECT 69901, 44, N'1.b.', N'Pohľadávky z obchodného styku v rámci podielovej účasti okrem pohľadávok voči prepojeným účtovným jednotkám (311A, 312A, 313A, 314A, 315A, 31XA) - /391A/', N'Trade receivables within participating interest, except for receivables from affiliated accounting entities (311A, 312A, 313A, 314A, 315A,31XA) - /391A/', 0, NULL, NULL, 43
    UNION ALL SELECT 69901, 45, N'1.c.', N'Ostatné pohľadávky z obchodného styku (311A, 312A, 313A, 314A, 315A, 31XA) - /391A/', N'Other trade receivables (311A, 312A, 313A, 314A, 315A,31XA) - /391A/', 0, NULL, NULL, 44
    UNION ALL SELECT 69901, 46, N'2.', N'Čistá hodnota zákazky (316A)', N'Net value of contract (316A)', 0, NULL, NULL, 45
    UNION ALL SELECT 69901, 47, N'3.', N'Ostatné pohľadávky voči prepojeným účtovným jednotkám (351A) - /391A/', N'Other receivables from affiliated accounting entities (351A) - /391A/', 0, NULL, NULL, 46
    UNION ALL SELECT 69901, 48, N'4.', N'Ostatné pohľadávky v rámci podielovej účasti okrem pohľadávok voči prepojeným účtovným jednotkám (351A) - /391A/', N'Other receivables within participating interest, except for receivables from affiliated accounting entities (351A) - /391A/', 0, N'Dlhodobé pohľadávky', NULL, 47
    UNION ALL SELECT 69901, 49, N'5.', N'Pohľadávky voči spoločníkom, členom a združeniu (354A, 355A, 358A, 35XA) - 391A', N'Receivables from participants, members, and association (354A, 355A, 358A, 35XA) - /391A/', 0, NULL, NULL, 48
    UNION ALL SELECT 69901, 50, N'6.', N'Pohľadávky z derivátových operácií (373A, 376A)', N'Receivables related to derivative transactions (373A, 376A)', 0, NULL, NULL, 49
    UNION ALL SELECT 69901, 51, N'7.', N'Iné pohľadávky (335A, 336A, 33XA, 371A, 374A, 375A, 378A) - /391A/', N'Other receivables (335A, 336A, 33XA, 371A, 374A, 375A, 378A) - /391A/', 0, NULL, NULL, 50
    UNION ALL SELECT 69901, 52, N'8.', N'Odložená daňová pohľadávka (481A)', N'Deferred tax asset (481A)', 0, NULL, NULL, 51
    UNION ALL SELECT 69901, 53, N'B.III.', N'Krátkodobé pohľadávky súčet (r. 54 + r. 58 až r. 65)', N'Current receivables - total (line 54 + lines 58 to 65)', 1, NULL, NULL, 52
    UNION ALL SELECT 69901, 54, N'B.III.1.', N'Pohľadávky z obchodného styku súčet (r. 55 až r. 57)', N'Trade receivables - total (lines 55 to 57)', 1, NULL, NULL, 53
    UNION ALL SELECT 69901, 55, N'1.a.', N'Pohľadávky z obchodného styku voči prepojeným účtovným jednotkám (311A, 312A, 313A, 314A, 315A, 31XA) - /391A/', N'Trade receivables from affiliated accounting entities (311A, 312A, 313A, 314A, 315A, 31XA) - /391A/', 0, NULL, NULL, 54
    UNION ALL SELECT 69901, 56, N'1.b.', N'Pohľadávky z obchodného styku v rámci podielovej účasti okrem pohľadávok voči prepojeným účtovným jednotkám (311A, 312A, 313A, 314A, 315A, 31XA) - /391A/', N'Trade receivables within participating interest, except for receivables from affiliated accounting entities (311A, 312A, 313A, 314A, 315A, 31XA) - /391A/', 0, NULL, NULL, 55
    UNION ALL SELECT 69901, 57, N'1.c.', N'Ostatné pohľadávky z obchodného styku (311A, 312A, 313A, 314A, 315A, 31XA) - /391A/', N'Other trade receivables (311A, 312A, 313A, 314A, 315A, 31XA) - /391A/', 0, NULL, NULL, 56
    UNION ALL SELECT 69901, 58, N'2.', N'Čistá hodnota zákazky (316A)', N'Net value of contract (316A)', 0, NULL, NULL, 57
    UNION ALL SELECT 69901, 59, N'3.', N'Ostatné pohľadávky voči prepojeným účtovným jednotkám (351A) - /391A/', N'Other receivables from affiliated accounting entities (351A) - /391A/', 0, NULL, NULL, 58
    UNION ALL SELECT 69901, 60, N'4.', N'Ostatné pohľadávky v rámci podielovej účasti okrem pohľadávok voči prepojeným účtovným jednotkám (351A) - /391A/', N'Other receivables within participating interest, except for receivables from affiliated accounting entities (351A) - /391A/', 0, N'Krátkodobé pohľadávky', NULL, 59
    UNION ALL SELECT 69901, 61, N'5.', N'Pohľadávky voči spoločníkom, členom a združeniu (354A, 355A, 358A, 35XA, 398A) - /391A/', N'Receivables from participants, members, and association (354A, 355A, 358A, 35XA, 398A) - /391A/', 0, NULL, NULL, 60
    UNION ALL SELECT 69901, 62, N'6.', N'Sociálne poistenie (336) - /391A/', N'Social security (336A) - /391A/', 0, NULL, NULL, 61
    UNION ALL SELECT 69901, 63, N'7.', N'Daňové pohľadávky a dotácie (341, 342, 343, 345, 346, 347) - /391A/', N'Tax assets and subsidies (341, 342, 343, 345, 346, 347) - /391A/', 0, NULL, NULL, 62
    UNION ALL SELECT 69901, 64, N'8.', N'Pohľadávky z derivátových operácií (373A, 376A)', N'Receivables related to derivative transactions (373A, 376A)', 0, NULL, NULL, 63
    UNION ALL SELECT 69901, 65, N'9.', N'Iné pohľadávky (335A, 33XA, 371A, 374A, 375A, 378A) - /391A/', N'Other receivables (335A, 33XA, 371A, 374A, 375A, 378A) - /391A/', 0, NULL, NULL, 64
    UNION ALL SELECT 69901, 66, N'B.IV.', N'Krátkodobý finančný majetok súčet (r. 67 až r. 70)', N'Current financial assets - total (lines 67 to 70)', 1, NULL, NULL, 65
    UNION ALL SELECT 69901, 67, N'B.IV.1.', N'Krátkodobý finančný majetok v prepojených účtovných jednotkách (251A, 253A, 256A, 257A, 25XA) - /291A, 29XA/', N'Current financial assets in affiliated accounting entities (251A, 253A, 256A, 257A, 25XA) - /291A, 29XA/', 0, NULL, NULL, 66
    UNION ALL SELECT 69901, 68, N'2.', N'Krátkodobý finančný majetok bez krátkodobého finančného majetku v prepojených účtovných jednotkách (251A, 253A, 256A, 257A, 25XA) - /291A, 29XA/', N'Current financial assets, not including current financial assets in affiliated accounting entities (251A, 253A, 256A, 257A, 25XA) - /291A, 29XA/', 0, NULL, NULL, 67
    UNION ALL SELECT 69901, 69, N'3.', N'Vlastné akcie a vlastné obchodné podiely (252)', N'Own shares and own ownership interests (252)', 0, NULL, NULL, 68
    UNION ALL SELECT 69901, 70, N'4.', N'Obstarávaný krátkodobý finančný majetok (259, 314A) - /291A/', N'Acquisition of current financial assets (259, 314A) - /291A/', 0, NULL, NULL, 69
    UNION ALL SELECT 69901, 71, N'B.V.', N'Finančné účty r. 72 + r. 73', N'Financial accounts line 72 + line 73', 1, NULL, NULL, 70
    UNION ALL SELECT 69901, 72, N'B.V.1.', N'Peniaze (211, 213, 21X)', N'Cash (211, 213, 21X)', 0, NULL, NULL, 71
    UNION ALL SELECT 69901, 73, N'2.', N'Účty v bankách (221A, 22X +/- 261)', N'Bank accounts (221A, 22X, +/- 261)', 0, NULL, NULL, 72
    UNION ALL SELECT 69901, 74, N'C.', N'Časové rozlíšenie súčet (r. 75 až r. 78)', N'Accruals/deferrals - total (lines 75 to 78)', 1, N'Krátkodobé pohladávky - Dan z pridanej hodnoty', NULL, 73
    UNION ALL SELECT 69901, 75, N'C.1.', N'Náklady budúcich období dlhodobé (381A, 382A)', N'Prepaid expenses - long-term (381A, 382A)', 0, NULL, NULL, 74
    UNION ALL SELECT 69901, 76, N'2.', N'Náklady budúcich období krátkodobé (381A, 382A)', N'Prepaid expenses - short-term (381A, 382A)', 0, NULL, NULL, 75
    UNION ALL SELECT 69901, 77, N'3.', N'Príjmy budúcich období dlhodobé (385A)', N'Accrued income - long-term (385A)', 0, NULL, NULL, 76
    UNION ALL SELECT 69901, 78, N'4.', N'Príjmy budúcich období krátkodobé (385A)', N'Accrued income - short-term (385A)', 0, NULL, NULL, 77
    UNION ALL SELECT 73802, 21, NULL, N'Vlastné imanie z toho:', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 73802, 22, NULL, N'Základné imanie', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 73802, 23, NULL, N'Vlastné akcie', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 73802, 24, NULL, N'Emisné ážio', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 73802, 25, NULL, N'Finančné zdroje poskytnuté pobočke zahraničnej poisťovne', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 73802, 26, NULL, N'Rezervné fondy a ostatné fondy tvorené zo zisku', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 94207, 1, NULL, N'Technické výnosy spolu', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 94207, 2, NULL, N'Čisté zaslúžené poistné', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 94207, 3, NULL, N'Predpísané poistné v hrubej výške', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94207, 4, NULL, N'Podiel zaisťovateľa na predpísanom poistnom', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 94207, 5, NULL, N'Zmena stavu rezervy na poistné budúcich období v hrubej výške', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 94207, 6, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na poistné budúcich období', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 94207, 7, NULL, N'Ostatné technické výnosy', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 94207, 8, NULL, N'z toho: provízie od zaisťovateľov', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 94207, 9, NULL, N'provízie zo spolupoistenia', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 94207, 10, NULL, N'poplatky', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 94207, 11, NULL, N'Technické náklady spolu', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 94207, 12, NULL, N'Náklady na poistné plnenia', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 94207, 13, NULL, N'Náklady na poistné plnenia v hrubej výške', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 94207, 14, NULL, N'Podiel zaisťovateľa na nákladoch na poistné plnenia', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 94207, 15, NULL, N'Zmena stavu rezervy na poistné plnenie v hrubej výške', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 94207, 16, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na poistné plnenie', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 94207, 17, NULL, N'Zmena stavu ostatných rezerv', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 94207, 18, NULL, N'Zmena stavu rezervy na životné poistenie v hrubej výške', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 94207, 19, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na životné poistenie v hrubej výške', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 94207, 20, NULL, N'Zmena stavu rezervy na poistné prémie a zľavy v hrubej výške', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 94207, 21, NULL, N'Podiel zaisťovateľa na zmene stavu rezervy na poistné prémie a zľavy v hrubej výške', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 94207, 22, NULL, N'Zmena stavu rezervy na úhradu záväzkov voči SKP vznikajúcich z činností podľa osobitného predpisu', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 94207, 23, NULL, N'Zmena stavu ďalších rezerv v hrubej výške', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 94207, 24, NULL, N'Podiel zaisťovateľa na zmene stavu ďalších rezerv', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 94207, 25, NULL, N'Zmena stavu rezervy na krytie rizika z investovania finančných prostriedkov v mene poistených', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 94207, 26, NULL, N'Prevádzkové náklady', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 94207, 27, NULL, N'Obstarávacie náklady na poistné zmluvy', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 94207, 28, NULL, N'z toho: provízie', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 94207, 29, NULL, N'marketing', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 94207, 30, NULL, N'Správna réžia', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 94207, 31, NULL, N'z toho: provízie', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 94207, 32, NULL, N'odpisy', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 94207, 33, NULL, N'Ostatné technické náklady', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 94207, 34, NULL, N'z toho: príspevky SKP', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 94207, 35, NULL, N'príspevky MV SR', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 94207, 36, NULL, N'Technický výsledok', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 94207, 37, NULL, N'Finančné výnosy spolu', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 94207, 38, NULL, N'Výnosy z finančného majetku a investičného majetku, ktoré kryjú technické rezervy', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 94207, 39, NULL, N'Kde riziko z investovaných prostriedkov nesie poisťovňa', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 94207, 40, NULL, N'Kde riziko z investovaných prostriedkov nesie klient', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 94207, 41, NULL, N'Výnosy z finančného majetku a investičného majetku, ktoré nekryjú technické rezervy', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 94207, 42, NULL, N'Ostatné finančné výnosy', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 94207, 43, NULL, N'Finančné náklady spolu', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 94207, 44, NULL, N'Náklady na finančný majetok a investičný majetok, ktorý kryje technické rezervy', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 94207, 45, NULL, N'Kde riziko z investovaných prostriedkov nesie poisťovňa', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 94207, 46, NULL, N'Kde riziko z investovaných prostriedkov nesie klient', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 94207, 47, NULL, N'Náklady na finančný majetok a investičný majetok, ktorý nekryje technické rezervy', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 94207, 48, NULL, N'Ostatné finančné náklady', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 94207, 49, NULL, N'Finančný výsledok', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 94207, 50, NULL, N'Ostatné výnosy', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 94207, 51, NULL, N'Ostatné náklady', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 94207, 52, NULL, N'Hospodársky výsledok pred zdanením', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 94207, 53, NULL, N'Splatná daň', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 94207, 54, NULL, N'Odložená daň', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 94207, 55, NULL, N'Osobitný odvod', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 94207, 56, NULL, N'Hospodársky výsledok po zdanení', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 518202, 40, N'P.B.2.1.', N'Súčasná hodnota budúcich peňažných tokov', N'LIC FCF', 0, NULL, NULL, 39
    UNION ALL SELECT 518202, 41, N'P.B.2.2.', N'Riziková prirážka na nefinančné riziká', N'LIC RA', 0, NULL, NULL, 40
    UNION ALL SELECT 518202, 42, N'P.C.', N'Záväzky z pasívneho zaistenia', N'Reinsurance contracts that are liabilities', 0, NULL, NULL, 41
    UNION ALL SELECT 518202, 43, N'P.D.', N'Záväzky (iné ako z poistenia a zaistenia)', N'Liabilities (other than from insurance and reinsurance)', 0, NULL, NULL, 42
    UNION ALL SELECT 518202, 44, N'P.E.', N'Krátkodobé zamestnanecké požitky', N'Short - term employee benefits', 0, NULL, NULL, 43
    UNION ALL SELECT 73802, 27, NULL, N'Fond vyrovnávacej rezervy', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 73802, 28, NULL, N'Ostatné kapitálové fondy', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 73802, 29, NULL, N'Oceňovacie rozdiely', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 73802, 30, NULL, N'Vlastnosti ľubovoľnej účasti', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 73802, 31, NULL, N'Hospodársky výsledok minulých rokov', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 73802, 32, NULL, N'Hospodársky výsledok vo schvaľovacom období', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 73802, 33, NULL, N'Hospodársky výsledok bežného obdobia', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 73802, 34, NULL, N'Záväzky z toho:', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 73802, 35, NULL, N'Podriadené záväzky', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 73802, 36, NULL, N'Prijaté úvery a pôžičky', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 73802, 37, NULL, N'Vklady pri pasívnom zaistení', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 73802, 38, NULL, N'Záporná reálna hodnota derivátových operácií na obchodovanie', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 73802, 39, NULL, N'Záporná reálna hodnota derivátových operácií na zabezpečenie', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 73802, 40, NULL, N'Rezervy na poistné zmluvy', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 73802, 41, NULL, N'Rezerva na poistné budúcich období', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 73802, 42, NULL, N'Rezerva na poistné plnenia', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 73802, 43, NULL, N'Rezerva na poistné prémie a zľavy', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 73802, 44, NULL, N'Rezerva na úhradu záväzkov voči SKP vznikajúcich z činnosti podľa osobitného predpisu', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 73802, 45, NULL, N'Rezerva na životné poistenie', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 73802, 46, NULL, N'Ďalšie rezervy', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 73802, 47, NULL, N'Rezerva na krytie rizika z investovania finančných prostriedkov v mene poistených', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 73802, 48, NULL, N'Finančné záväzky z investičných zmlúv', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 73802, 49, NULL, N'Netechnické rezervy', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 73802, 50, NULL, N'Záväzky z poistenia a zaistenia', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 73802, 51, NULL, N'Krátkodobé zamestnanecké pôžičky', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 73802, 52, NULL, N'Daňové záväzky', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 73802, 53, NULL, N'Účty časového rozlíšenia', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 73802, 54, NULL, N'Ostatné záväzky', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 73802, 55, NULL, N'PASÍVA spolu', NULL, 0, NULL, NULL, 34
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 69901 AS [TableErpId], 41 AS [RowNumber], N'B.II.' AS [Designation], N'Dlhodobé pohľadávky súčet (r. 42 + r. 46 až r. 52)' AS [Text_sk], N'Non-current receivables - total (line 42 + lines 46 to 52)' AS [Text_en], 1 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 40 AS [RowOrdinal]
    UNION ALL SELECT 69901, 42, N'B.II.1.', N'Pohľadávky z obchodného styku súčet (r. 43 až r. 45)', N'Trade receivables - total (lines 43 to 45)', 1, NULL, NULL, 41
    UNION ALL SELECT 69901, 43, N'1.a.', N'Pohľadávky z obchodného styku voči prepojeným účtovným jednotkám (311A, 312A, 313A, 314A, 315A, 31XA) - /391A/', N'Trade receivables from affiliated accounting entities (311A, 312A, 313A, 314A, 315A, 31XA) - /391A/', 0, NULL, NULL, 42
    UNION ALL SELECT 71602, 5, NULL, N'Zásoby', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 71602, 6, NULL, N'Služby', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 71602, 7, NULL, N'Mzdy', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 71602, 8, NULL, N'Platby poistného a príspevkov', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 71602, 9, NULL, N'Tvorba sociálneho fondu', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 71602, 10, NULL, N'Ostatné výdavky', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 71602, 11, NULL, N'Výdavky celkom súčet (r. 05 až 10)', NULL, 1, NULL, NULL, 6
    UNION ALL SELECT 71602, 12, NULL, N'Rozdiel príjmov a výdavkov (r. 04 - r. 11)', NULL, 1, NULL, NULL, 7
    UNION ALL SELECT 38302, 17, NULL, N'Zásoby', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 38302, 18, NULL, N'Služby', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 38302, 19, NULL, N'Mzdy, poistné a príspevky', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 38302, 20, NULL, N'Dary a príspevky iným subjektom', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 38302, 21, NULL, N'Prevádzková réžia', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 38302, 22, NULL, N'Splátky úverov a pôžičiek', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 38302, 23, NULL, N'Sociálny fond', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 38302, 24, NULL, N'Ostatné', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 38302, 25, NULL, N'Výdavky celkom (súčet r. 17 až r. 24)', NULL, 1, NULL, NULL, 8
    UNION ALL SELECT 38302, 26, NULL, N'Rozdiel príjmov a výdavkov (r. 16 - r. 25)', NULL, 1, NULL, NULL, 9
    UNION ALL SELECT 38302, 27, NULL, N'Daň z príjmov', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 2902, 76, NULL, N'B. Cudzie zdroje súčet (r.077 + r.078 + r.085 + r.099 + r.103)', NULL, 1, NULL, NULL, 20
    UNION ALL SELECT 2902, 77, N'1.', N'Rezervy (941)', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 2902, 78, N'2.', N'Dlhodobé záväzky súčet (r.079 až r.084)', NULL, 1, NULL, NULL, 22
    UNION ALL SELECT 2902, 79, N'2.', N'Návratná finančná výpomoc zo štátneho rozpočtu (952AÚ)', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 2902, 80, N'2.', N'Záväzky z nájmu (954AÚ)', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 2902, 81, N'2.', N'Dlhodobé prijaté preddavky (955)', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 2902, 82, N'2.', N'Sociálny fond (956)', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 2902, 83, N'2.', N'Dlhodobé zmenky na úhradu (958)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 2902, 84, N'2.', N'Ostatné dlhodobé záväzky (959AÚ + 373AÚ)', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 2902, 85, N'3.', N'Krátkodobé záväzky súčet (r.086 až r.098)', NULL, 1, NULL, NULL, 29
    UNION ALL SELECT 2902, 86, N'3.', N'Záväzky z obchodného styku (321 až 325 okrem r. 089)', NULL, 1, NULL, NULL, 30
    UNION ALL SELECT 2902, 87, N'3.', N'Nevyfakturované dodávky (329)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 2902, 88, N'3.', N'Krátkodobé rezervy (323)', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 2902, 89, N'3.', N'Záväzky z poistných vzťahov (326)', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 2902, 90, N'3.', N'Záväzky voči dôchodkovej správcovskej spoločnosti (328)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 2902, 91, N'3.', N'Záväzky voči zamestnancom (331+ 333)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 2902, 92, N'3.', N'Zúčtovanie s inštitúciami sociálneho poistenia a zdravotného poistenia (336)', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 2902, 93, N'3.', N'Daňové záväzky (341+ 342 + 343 + 345)', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 2902, 94, N'3.', N'Dotácie a ostatné zúčtovanie so štátnym rozpočtom (346)', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 2902, 95, N'3.', N'Záväzky z upísaných nesplatených cenných papierov a vkladov (367)', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 2902, 96, N'3.', N'Záväzky voči združeniu (368)', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 2902, 97, N'3.', N'Spojovací účet pri združení (396)', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 2902, 98, N'3.', N'Iné záväzky (379 + 373AÚ + 952AÚ + 954AÚ + 959AÚ)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 2902, 99, N'4.', N'Bankové výpomoci a pôžičky súčet (r.100 až r.102)', NULL, 1, NULL, NULL, 43
    UNION ALL SELECT 2902, 100, N'4.', N'Dlhodobé bankové úvery (951AÚ)', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 2902, 101, N'4.', N'Bežné bankové úvery (231 + 232 + 951AÚ)', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 2902, 102, N'4.', N'Iné krátkodobé finančné výpomoci (249)', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 2902, 103, N'5.', N'Prechodné účty pasív súčet (r.104 + r.105)', NULL, 1, NULL, NULL, 47
    UNION ALL SELECT 2902, 104, N'5.', N'Výdavky budúcich období (383)', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 2902, 105, N'5.', N'Výnosy budúcich období (384)', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 2902, 106, NULL, N'PASÍVA spolu súčet (r. 056 + r. 076)', NULL, 1, NULL, NULL, 50
    UNION ALL SELECT 2902, 993, NULL, N'Kontrolné číslo súčet (r. 056 až r. 106)', NULL, 1, NULL, NULL, 51
    UNION ALL SELECT 518303, 1, NULL, N'Záväzky po lehote splatnosti z pokračujúcej činnosti celkom, z toho:', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 518303, 2, NULL, N'- do 90 dní', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 518303, 3, NULL, N'- od 91 dní do 120 dní', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 518303, 4, NULL, N'- od 121 dní do 150 dní', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 518303, 5, NULL, N'- od 151 dní do 180 dní', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 518303, 6, NULL, N'- od 181 dní do 360 dní', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 518303, 7, NULL, N'- od 361 dní a viac', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 116403, 1, NULL, N'Dlhodobý nehmotný majetok', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 116403, 2, NULL, N'Dlhodobý hmotný majetok', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 116403, 3, NULL, N'Dlhodobý finančný majetok', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 116403, 4, NULL, N'Zásoby', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 116403, 5, NULL, N'Pohľadávky', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 116403, 6, NULL, N'Peniaze', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 116403, 7, NULL, N'Ceniny', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 116403, 8, NULL, N'Priebežné položky (+/-)', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 116403, 9, NULL, N'Bankové účty', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 116403, 10, NULL, N'Krátkodobé cenné papiere a ostatný krátkodobý finančný majetok', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 116403, 11, NULL, N'Majetok celkom (súčet r. 01 až r. 10)', NULL, 1, NULL, NULL, 10
    UNION ALL SELECT 401, NULL, N'01', N'Začiatočný stav fondu privatizácie', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 401, NULL, N'02', N'Tvorba celkom (r. 03 a r. 04 a r. 06 až r. 17)', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 401, NULL, N'03', N'Privatizovaný majetok, ktorý prešiel na fond', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 401, NULL, N'04', N'Zisk z účasti fondu na podnikaní obchodných spoločností', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 401, NULL, N'05', N'z toho: dividendy', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 401, NULL, N'06', N'Výnos z predaja akcií akciových spoločností a podielov', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 401, NULL, N'07', N'Akcie a podiely, ktoré neboli predmetom rozhodnutia o privatizácii a ktoré fond nadobudol ako akcionár alebo spoločník', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 401, NULL, N'08', N'Majetok, ktorý prešiel na fond v dôsledku odstúpenia od zmluvy o privatizácii majetku', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 401, NULL, N'09', N'Výnosy z predaja majetku', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 401, NULL, N'10', N'Iné výnosy z predaja akcií, podielov a majetku (úroky, úroky z omeškania, kurzové rozdiely)', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 401, NULL, N'11', N'Výnos z prenájmu majetku fondu', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 401, NULL, N'12', N'Výnos z rozdielu medzi súpisom majetku a fyzickou inventúrou pri odstúpení fondu od zmluvy o privatizácii majetku', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 401, NULL, N'13', N'Výnos z prijatých úrokov na účtoch fondu v bankách', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 401, NULL, N'14', N'Tržby z predaja a likvidácie majetku zo správnej činnosti fondu', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 401, NULL, N'15', N'Finančné prostriedky, ktoré sú podľa osobitných predpisov príjmom osobitného účtu ministerstva hospodárstva', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 401, NULL, N'16', N'Tvorba fondu z prepočtu majetku, záväzkov a vlastného imania na eurá', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 401, NULL, N'17', N'Ostatné položky', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 401, NULL, N'18', N'Použitie majetku fondu celkom ( r. 19 + r. 56 až r. 61 )', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 401, NULL, N'19', N'Použitie majetku fondu (r. 20 + r. 31 + r. 43 až r. 55)', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 401, NULL, N'20', N'Použitie v súlade s rozhodnutím o privatizácii (súčet r. 21 až r. 30)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 401, NULL, N'21', N'Založenie akciovej spoločnosti alebo inej obchodnej spoločnosti', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 401, NULL, N'22', N'Vklad do obchodnej spoločnosti', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 401, NULL, N'23', N'Predaj majetku podniku alebo jeho časti', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 401, NULL, N'24', N'Predaj majetkovej účasti na podnikaní obchodnej spoločnosti inej právnickej osobe', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 401, NULL, N'25', N'Prevod privatizovaného majetku na obce', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 401, NULL, N'26', N'Prevod privatizovaného majetku na účely zdravotného, nemocenského poistenia a dôchodkového zabezpečenia a na účely aktívnej politiky zamestnanosti', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 401, NULL, N'27', N'Prevod majetku na Slovenský pozemkový fond', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 401, NULL, N'28', N'Prevod majetku oprávnenej osobe', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 401, NULL, N'29', N'Prevod na Reštitučný investičný fond', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 401, NULL, N'30', N'Úhrada nákladov vynaložených nadobúdateľom na vysporiadanie ekologických záväzkov vzniknutých pred privatizáciou', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 401, NULL, N'31', N'Použitie v súlade s rozhodnutím vlády (súčet r. 32 až r. 42)', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 401, NULL, N'32', N'Plnenie záväzkov podnikov určených na privatizáciu', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 401, NULL, N'33', N'Posilnenie zdrojov bánk a sporiteľní určených na poskytnutie úverov', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 401, NULL, N'34', N'Splnenie záruk za úvery obchodných spoločností, v ktorých má fond aspoň 34 % majetkovú účasť', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 401, NULL, N'35', N'Podpora rozvojových programov Slovenskej republiky', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 401, NULL, N'36', N'Plnenie štátnych záruk za bankové úvery', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 401, NULL, N'37', N'Financovanie splácania štátneho dlhu v priebehu rozpočtového roka', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 401, NULL, N'38', N'Doplnenie zdrojov v systéme financovania zdravotníctva', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 401, NULL, N'39', N'Úhrada častí záväzkov Štátneho fondu cestného hospodárstva Slovenskej republiky', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 401, NULL, N'40', N'Posilnenie štátnych finančných aktív', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 401, NULL, N'41', N'Vysporiadanie cenových rozdielov k cenám tepla a vody', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 401, NULL, N'42', N'Finančné kompenzácie nákladov obcí na plynárenské zariadenia, ktoré boli bez náhrady prevedené do vlastníctva štátu', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 401, NULL, N'43', N'Zvýšenie základného imania obchodných spoločností', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 401, NULL, N'44', N'Náklady spojené so správnou činnosťou fondu v rozsahu určenom rozpočtom', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 401, NULL, N'45', N'Finančná náhrada subjektom, voči ktorým mal privatizovaný podnik zodpovednosť za nedostatky, pričom táto zodpovednosť neprešla na nadobúdateľa', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 401, NULL, N'46', N'Úhrada nákladov spojených s podporou privatizácie', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 401, NULL, N'47', N'Nákup majetku a majetkových účastí, na ktoré má fond predkupné právo', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 401, NULL, N'48', N'Na uspokojovanie nárokov oprávnených osôb podľa osobitných predpisov a úhradu nákladov reštitučných a privatizačných súdnych sporov', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 401, NULL, N'49', N'Na úhradu na ťarchu osobitného účtu ministerstva hospodárstva Slovenskej republiky', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 401, NULL, N'50', N'Na prevod prostriedkov do majetku obcí v rozsahu 25% podielu na úhrnnom čistom výnose z predaja prevádzkových jednotiek', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 401, NULL, N'51', N'Na úhradu neuspokojenej časti pohľadávok štátu z hľadiska životného prostredia voči úpadcovi', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 401, NULL, N'52', N'Na úhradu nákladov spojených s emisiou, splatením dlhopisov fondu a ich výnosov', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 401, NULL, N'53', N'Na úhradu nákladov vzniknutých v dôsledku odstúpenia od zmluvy alebo na uzatvárenie zmlúv o nájme takto získaného majetku', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 401, NULL, N'54', N'Na nakladanie s majetkovými účasťami fondu nadobudnutými fondom', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 401, NULL, N'55', N'Na ďalšie účely, ak tak ustanoví osobitný zákon', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 401, NULL, N'56', N'Odpustenie časti kúpnej ceny a započítanie investícií', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 401, NULL, N'57', N'Platené úroky a poplatky bankám a k úverom', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 401, NULL, N'58', N'Strata zo zrušenia spoločnosti založenej fondom, odpísanie pohľadávok na základe súdneho rozhodnutia', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 401, NULL, N'59', N'Nároky z ručenia', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 401, NULL, N'60', N'Ostatné použitie majetku fondu', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 401, NULL, N'61', N'Použitie fondu z prepočtu majetku, záväzkov a vlastného imania na eurá', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 401, NULL, N'62', N'Rozdiel medzi tvorbou a použitím fondu (r. 01 + r. 02 - r. 18)', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 501, NULL, N'01', N'Začiatočný stav fondu privatizácie', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 501, NULL, N'02', N'Tvorba celkom (r. 03 až r. 17 - 16)', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 501, NULL, N'03', N'Privatizovaný majetok, ktorý prešiel na fond', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 501, NULL, N'04', N'Cenné papiere prevedené na fond od fyzických osôb', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 501, NULL, N'05', N'Zisk z účasti fondu na podnikaní obchodných spoločností', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 501, NULL, N'06', N'z toho: dividendy', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 501, NULL, N'07', N'Výnos z predaja akcií akciových spoločností a podielov', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 501, NULL, N'08', N'Akcie a podiely, ktoré neboli predmetom rozhodnutia o privatizácii a ktoré fond nadobudol ako akcionár alebo spoločník', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 501, NULL, N'09', N'Majetok, ktorý prešiel na fond v dôsledku odstúpenia od zmluvy o privatizácii majetku', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 501, NULL, N'10', N'Výnosy z predaja majetku', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 501, NULL, N'11', N'Iné výnosy z predaja akcií, podielov a majetku (úroky, úroky z omeškania, kurzové rozdiely)', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 501, NULL, N'12', N'Výnos z prenájmu majetku fondu', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 501, NULL, N'13', N'Výnos z rozdielu medzi súpisom majetku a fyzickou inventúrou pri odstúpení fondu od zmluvy o privatizácii majetku', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 501, NULL, N'14', N'Výnos z prijatých úrokov na účtoch fondu v bankách', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 501, NULL, N'15', N'Tržby z predaja a likvidácie majetku zo správnej činnosti fondu', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 501, NULL, N'16', N'Finančné prostriedky, ktoré sú podľa osobitných predpisov príjmom osobitného účtu Ministerstva hospodárstva', NULL, 0, NULL, NULL, 15
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 501 AS [TableErpId], NULL AS [RowNumber], N'17' AS [Designation], N'Ostatné položky' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 16 AS [RowOrdinal]
    UNION ALL SELECT 501, NULL, NULL, N'Kontrolné číslo (súčet r. 01 až r. 17)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 501, NULL, N'18', N'Použitie majetku fondu celkom ( r. 19 + r. 20 )', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 501, NULL, N'19', N'Použitie majetku fondu (r. 31 a r. 43 až 61)', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 501, NULL, N'20', N'Použitie v súlade s rozhodnutím o privatizácii (súčet r. 21 až r. 30)', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 501, NULL, N'21', N'Založenie akciovej spoločnosti alebo inej obchodnej spoločnosti', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 501, NULL, N'22', N'Vklad do obchodnej spoločnosti', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 501, NULL, N'23', N'Predaj majetku podniku alebo jeho časti', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 501, NULL, N'24', N'Predaj majetkovej účasti na podnikaní obchodnej spoločnosti inej právnickej osobe', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 501, NULL, N'25', N'Prevod privatizovaného majetku na obce', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 501, NULL, N'26', N'Prevod privatizovaného majetku na účely zdravotného, nemocenského poistenia a dôchodkového zabezpečenia a na účely aktívnej politiky zamestnanosti', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 501, NULL, N'27', N'Prevod majetku na Slovenský pozemkový fond', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 501, NULL, N'28', N'Prevod majetku oprávnenej osobe', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 501, NULL, N'29', N'Prevod na Reštitučný investičný fond', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 501, NULL, N'30', N'Úhrada nákladov vynaložených nadobúdateľom na vysporiadanie ekologických záväzkov vzniknutých pred privatizáciou', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 501, NULL, N'31', N'Použitie v súlade s rozhodnutím vlády (súčet r. 32 až r. 42)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 501, NULL, N'32', N'Plnenie záväzkov podnikov určených na privatizáciu', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 501, NULL, N'33', N'Posilnenie zdrojov bánk určených na poskytnutie úverov', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 501, NULL, N'34', N'Splnenie záruk za úvery obchodných spoločností, v ktorých má fond aspoň 34 % majetkovú účasť', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 501, NULL, N'35', N'Podpora rozvojových programov Slovenskej republiky', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 501, NULL, N'36', N'Plnenie štátnych záruk za bankové úvery', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 501, NULL, N'37', N'Financovanie splácania štátneho dlhu v priebehu rozpočtového roka', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 501, NULL, N'38', N'Doplnenie zdrojov v systéme financovania zdravotníctva', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 501, NULL, N'39', N'Úhrada častí záväzkov Štátneho fondu cestného hospodárstva Slovenskej republiky', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 501, NULL, N'40', N'Posilnenie štátnych finančných aktív', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 501, NULL, N'41', N'Vysporiadanie cenových rozdielov k cenám tepla a vody', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 501, NULL, N'42', N'Finančné kompenzácie nákladov obcí na plynárenské zariadenia, ktoré boli bez náhrady prevedené do vlastníctva štátu', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 501, NULL, N'43', N'Zvýšenie základného imania obchodných spoločností', NULL, 0, NULL, NULL, 43
    UNION ALL SELECT 501, NULL, N'44', N'Náklady spojené so správnou činnosťou fondu v rozsahu určenom rozpočtom', NULL, 0, NULL, NULL, 44
    UNION ALL SELECT 501, NULL, N'45', N'Finančná náhrada subjektom, voči ktorým mal privatizovaný podnik zodpovednosť za nedostatky, pričom táto zodpovednosť neprešla na nadobúdateľa', NULL, 0, NULL, NULL, 45
    UNION ALL SELECT 501, NULL, N'46', N'Úhrada nákladov spojených s podporou privatizácie', NULL, 0, NULL, NULL, 46
    UNION ALL SELECT 501, NULL, N'47', N'Nákup majetku a majetkových účastí, na ktoré má fond predkupné právo', NULL, 0, NULL, NULL, 47
    UNION ALL SELECT 501, NULL, N'48', N'Na uspokojovanie nárokov oprávnených osôb podľa osobitných predpisov a úhradu nákladov reštitučných a privatizačných súdnych sporov', NULL, 0, NULL, NULL, 48
    UNION ALL SELECT 501, NULL, N'49', N'Na úhradu na ťarchu osobitného účtu Ministerstva hospodárstva', NULL, 0, NULL, NULL, 49
    UNION ALL SELECT 501, NULL, N'50', N'Na prevod prostriedkov do majetku obcí v rozsahu 25% podielu na úhrnnom čistom výnose z predaja prevádzkových jednotiek', NULL, 0, NULL, NULL, 50
    UNION ALL SELECT 501, NULL, N'51', N'Na úhradu neuspokojenej časti pohľadávok štátu z hľadiska životného prostredia voči úpadcovi', NULL, 0, NULL, NULL, 51
    UNION ALL SELECT 501, NULL, N'52', N'Na úhradu nákladov spojených s emisiou, splatením dlhopisov fondu a ich výnosov', NULL, 0, NULL, NULL, 52
    UNION ALL SELECT 501, NULL, N'53', N'Na úhradu nákladov vzniknutých v dôsledku odstúpenia od zmluvy alebo na uzatvárenie zmlúv o nájme takto získaného majetku', NULL, 0, NULL, NULL, 53
    UNION ALL SELECT 501, NULL, N'54', N'Na nakladanie s majetkovými účasťami fondu nadobudnutými fondom', NULL, 0, NULL, NULL, 54
    UNION ALL SELECT 501, NULL, N'55', N'Na úhradu nákladov spojených s cennými papiermi od fyzických osôb', NULL, 0, NULL, NULL, 55
    UNION ALL SELECT 501, NULL, N'56', N'Na ďalšie účely, ak tak ustanoví osobitný zákon', NULL, 0, NULL, NULL, 56
    UNION ALL SELECT 501, NULL, N'57', N'Odpustenie časti kúpnej ceny a započítanie investícií', NULL, 0, NULL, NULL, 57
    UNION ALL SELECT 501, NULL, N'58', N'Platené úroky a poplatky bankám a k úverom', NULL, 0, NULL, NULL, 58
    UNION ALL SELECT 501, NULL, N'59', N'Strata zo zrušenia spoločnosti založenej fondom, odpísanie pohľadávok na základe súdneho rozhodnutia', NULL, 0, NULL, NULL, 59
    UNION ALL SELECT 501, NULL, N'60', N'Nároky z ručenia', NULL, 0, NULL, NULL, 60
    UNION ALL SELECT 501, NULL, N'61', N'Ostatné použitie majetku fondu', NULL, 0, NULL, NULL, 61
    UNION ALL SELECT 501, NULL, N'62', N'Rozdiel medzi tvorbou a použitím fondu (r. 01 + r. 02 - r. 18)', NULL, 0, NULL, NULL, 62
    UNION ALL SELECT 501, NULL, NULL, N'Kontrolné číslo (súčet r. 18 až r. 62)', NULL, 0, NULL, NULL, 63
    UNION ALL SELECT 601, NULL, N'01', N'Náklady na materiál a služby súčet (r. 02 až r. 12)', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 601, NULL, N'02', N'spotreba materiálu a palív', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 601, NULL, N'03', N'spotreba energie', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 601, NULL, N'04', N'opravy a udržovanie', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 601, NULL, N'05', N'cestovné', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 601, NULL, N'06', N'reprezentačné', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 601, NULL, N'07', N'výkony spojov', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 601, NULL, N'08', N'nájomné', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 601, NULL, N'09', N'inzercia', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 601, NULL, N'10', N'audit', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 601, NULL, N'11', N'preprava', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 601, NULL, N'12', N'ostatné podľa schváleného rozpočtu', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 601, NULL, N'13', N'Osobné náklady súčet (r. 14 až r. 17)', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 601, NULL, N'14', N'mzdové náklady', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 601, NULL, N'15', N'odmeny členov orgánov fondu', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 601, NULL, N'16', N'zákonné sociálne poistenie', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 601, NULL, N'17', N'doplnkové dôchodkové sporenie', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 601, NULL, N'18', N'Zákonné sociáln náklady (r. 19 až r. 26)', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 601, NULL, N'19', N'školenie', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 601, NULL, N'20', N'odstupné', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 601, NULL, N'21', N'odchodné', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 601, NULL, N'22', N'práce neschopnosť', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 601, NULL, N'23', N'príspevok na stravovanie', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 601, NULL, N'24', N'tvorba sociálneho fondu', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 601, NULL, N'25', N'ostatné sociálne náklady', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 601, NULL, N'26', N'zdravotná starostlivosť', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 601, NULL, N'27', N'Dane a poplatky', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 601, NULL, N'28', N'Finančné náklady súčet (r. 29 až r. 31)', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 601, NULL, N'29', N'úroky', NULL, 0, NULL, NULL, 28
    UNION ALL SELECT 601, NULL, N'30', N'kurzové straty', NULL, 0, NULL, NULL, 29
    UNION ALL SELECT 601, NULL, N'31', N'ostatné finančné náklady', NULL, 0, NULL, NULL, 30
    UNION ALL SELECT 601, NULL, N'32', N'Iné náklady na činnosť súčet (r. 33 a r. 34)', NULL, 0, NULL, NULL, 31
    UNION ALL SELECT 601, NULL, N'33', N'škody', NULL, 0, NULL, NULL, 32
    UNION ALL SELECT 601, NULL, N'34', N'ostatné mimoriadne náklady', NULL, 0, NULL, NULL, 33
    UNION ALL SELECT 601, NULL, N'35', N'Prevádzkové náklady celkom súčet (r. 01 + r. 13 + r. 18 + r. 27 + r. 28 + r. 32)', NULL, 0, NULL, NULL, 34
    UNION ALL SELECT 601, NULL, N'36', N'Dlhodobý majetok celkom súčet (r. 37 až r. 42)', NULL, 0, NULL, NULL, 35
    UNION ALL SELECT 601, NULL, N'37', N'dlhodobý nehmotný majetok', NULL, 0, NULL, NULL, 36
    UNION ALL SELECT 601, NULL, N'38', N'pozemky', NULL, 0, NULL, NULL, 37
    UNION ALL SELECT 601, NULL, N'39', N'investície k bytom', NULL, 0, NULL, NULL, 38
    UNION ALL SELECT 601, NULL, N'40', N'samostatné hnuteľné veci', NULL, 0, NULL, NULL, 39
    UNION ALL SELECT 601, NULL, N'41', N'doprava', NULL, 0, NULL, NULL, 40
    UNION ALL SELECT 601, NULL, N'42', N'výpočtová technika', NULL, 0, NULL, NULL, 41
    UNION ALL SELECT 601, NULL, N'43', N'Náklady na správnu činnosť celkom (r. 35 + r. 36)', NULL, 0, NULL, NULL, 42
    UNION ALL SELECT 518101, NULL, N'1.', N'Peňažné prostriedky a ekvivalenty peňažných prostriedkov', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 518101, NULL, N'2.', N'Pohľadávky z obchodného styku', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 518101, NULL, N'3.', N'Finančný majetok oceňovaný reálnou hodnotou proti zisku/strate', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 518101, NULL, N'4.', N'Finančný majetok so zaisťovaním jeho reálnej hodnoty', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 518101, NULL, N'4.a.', N'reálna hodnota zaisteného finančného majetku', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 518101, NULL, N'4.b.', N'reálna hodnota zaisťovacích nástrojov', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 518101, NULL, N'5.', N'Finančný majetok oceňovaný reálnou hodnotou proti inému úplnému výsledku', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 518101, NULL, N'6.', N'Finančný majetok oceňovaný umorovanou hodnotou', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 518101, NULL, N'7.', N'Investičný nehnuteľný majetok', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 518101, NULL, N'7.a.', N'oceňované reálnou hodnotou', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 518101, NULL, N'7.b.', N'oceňované obstarávacou cenou', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 518101, NULL, N'8.', N'Prenajatý majetok', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 518101, NULL, N'8.a.', N'nehnuteľnosti', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 518101, NULL, N'8.b.', N'ostatný majetok', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 518101, NULL, N'9.', N'Podriadené úvery', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 518101, NULL, N'10.', N'Podielové účasti', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 518101, NULL, N'10.a.', N'v podnikoch združených v skupine', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 518101, NULL, N'10.b.', N'v spoločných podnikoch', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 518101, NULL, N'10.c.', N'v pridružených podnikoch', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 518101, NULL, N'10.d.', N'obstarané na účel zabezpečenia dlhodobého vplvyvu v podniku', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 518101, NULL, N'11.', N'Dlhodobý hmotný majetok', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 518101, NULL, N'12.', N'Dlhodobý nehmotný majetok', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 518101, NULL, N'13.', N'Goodwill', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 518101, NULL, N'14.', N'Majetok na predaj', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 518101, NULL, N'15.', N'Daňové pohľadávky', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 518101, NULL, N'16.', N'Ostatný majetok', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 518101, NULL, N'17.', N'Vzťahy s ostatnými organizačnými zložkami, aktívne zostatky', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 518101, NULL, N'A', N'AKTÍVA SPOLU', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 518102, NULL, N'18.', N'Záväzky z obchodného styku', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 518102, NULL, N'19.', N'Finančné záväzky oceňované reálnou hodnotou proti zisku/strate', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 518102, NULL, N'20.', N'Finančné záväzky so zaisťovaním ich reálnej hodnoty', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 518102, NULL, N'20.a.', N'reálna hodnota zaistených finančných záväzkov', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 518102, NULL, N'20.b.', N'reálna hodnota zaisťovacích nástrojov', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 518102, NULL, N'21.', N'Finančné záväzky oceňované umorovanou hodnotou', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 518102, NULL, N'22.', N'Záväzky z prenájmov', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 518102, NULL, N'22.a.', N'nehnuteľností', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 518102, NULL, N'22.b.', N'iného majetku', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 518102, NULL, N'23.', N'Rezervy na podsúvahové záväzky', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 518102, NULL, N'24.', N'Podriadené záväzky', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 518102, NULL, N'25.', N'Daňové záväzky', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 518102, NULL, N'26.', N'Ostatné záväzky', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 518102, NULL, N'27.', N'Vzťahy s ostatnými organizačnými zložkami, pasívne zostatky', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 518102, NULL, N'Z.', N'Záväzky spolu', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 518102, NULL, N'28.', N'Vlastné imanie bez fondov z ocenenia, ziskov/strát minulých účtovných období a zistku/straty bežného roka', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 518102, NULL, N'29.', N'Fondy z ocenenia', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 518102, NULL, N'30.', N'Zisky/straty minulých účtovných období', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 518102, NULL, N'30.a.', N'zisky minulých účtovných období', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 518102, NULL, N'30.b.', N'straty minulých účtovných období', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 518102, NULL, N'31.', N'Zisk/strata bežného roka', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 518102, NULL, N'VI.', N'Vlastné imanie spolu', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 518102, NULL, N'P', N'PASÍVA SPOLU', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 518103, NULL, N'1.', N'Výnosy z odplát a provízií', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 518103, NULL, N'a.', N'Náklady na odplaty a provízie', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 518103, NULL, N'2.', N'Výnosy z úrokov', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 518103, NULL, N'b.', N'Náklady na úroky', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 518103, NULL, N'3.', N'Dividendy', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 518103, NULL, N'4./c.', N'Zisk alebo strata z operácií s finančným majetkom', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 518103, NULL, N'5./d.', N'Čistý zisk/strata z investičného nehnuteľného majetku', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 518103, NULL, N'6./e.', N'Zisk alebo strata zo ziskov zo zrušenia zníženia hodnoty finančného majetku a z už odpísaného finančného majetku a straty zo zníženia hodnoty finančného majetku a z odpísania finančného majetku', NULL, 0, NULL, NULL, 7
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 518103 AS [TableErpId], NULL AS [RowNumber], N'7./f.' AS [Designation], N'Zisk alebo strata zo zrušenia rezerv a z tvorby rezerv na podsúvahové záväzky' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 8 AS [RowOrdinal]
    UNION ALL SELECT 518103, NULL, N'A', N'Zisk alebo strata z bežnej činnosti', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 518103, NULL, N'8./g.', N'Zisk alebo strata z predaja nefinančného majetku a z prevodu nefinančného majetku', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 518103, NULL, N'h.', N'Personálne náklady', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 518103, NULL, N'i.', N'Odpisy', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 518103, NULL, N'j.', N'Administratívne náklady', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 518103, NULL, N'9./k.', N'Zisk alebo strata zo zrušenia zníženia hodnoty prevádzkového majetku a zo zníženia hodnoty prevádzkového majetku', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 518103, NULL, N'10./l.', N'Ostatné náklady alebo výnosy', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 518103, NULL, N'11.', N'Záporný goodwill', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 518103, NULL, N'12./m.', N'Podiel na zisku/strate v spoločných podnikoch a pridružených podnikoch', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 518103, NULL, N'B', N'Zisk alebo strata za účtovné obdobie pred zdanením', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 518103, NULL, N'n.', N'Náklady na daň z príjmov, z toho', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 518103, NULL, N'n.1.', N'uhradenú zrážkou', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 518103, NULL, N'n.2.', N'daň splatná za zdaňovacie obdobie podľa daňového priznania', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 518103, NULL, N'n.3.', N'zúčtovanie odloženej dane', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 518103, NULL, N'C', N'Zisk alebo strata za účtovné obdobie', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 112101, NULL, N'1.', N'Peňažné prostriedky a ekvivalenty peňažných prostriedkov', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 112101, NULL, N'2.', N'Pohľadávky z obchodného styku', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 112101, NULL, N'3.', N'Finančný majetok oceňovaný reálnou hodnotou proti zisku/strate', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 112101, NULL, N'4.', N'Finančný majetok so zaisťovaním jeho reálnej hodnoty', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 112101, NULL, N'4.a.', N'reálna hodnota zaisteného finančného majetku', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 112101, NULL, N'4.b.', N'reálna hodnota zaisťovacích nástrojov', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 112101, NULL, N'5.', N'Finančný majetok oceňovaný reálnou hodnotou proti inému úplnému výsledku', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 112101, NULL, N'6.', N'Finančný majetok oceňovaný umorovanou hodnotou', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 112101, NULL, N'7.', N'Investičný nehnuteľný majetok', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 112101, NULL, N'7.a.', N'oceňované reálnou hodnotou', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 112101, NULL, N'7.b.', N'oceňované obstarávacou cenou', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 112101, NULL, N'8.', N'Prenajatý majetok', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 112101, NULL, N'8.a.', N'nehnuteľnosti', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 112101, NULL, N'8.b.', N'ostatný majetok', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 112101, NULL, N'9.', N'Podriadené úvery', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 112101, NULL, N'10.', N'Podielové účasti', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 112101, NULL, N'10.a.', N'v podnikoch združených v skupine', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 112101, NULL, N'10.b.', N'v spoločných podnikoch', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 112101, NULL, N'10.c.', N'v pridružených podnikoch', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 112101, NULL, N'10.d.', N'obstarané na účel zabezpečenia dlhodobého vplvyvu v podniku', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 112101, NULL, N'11.', N'Dlhodobý hmotný majetok', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 112101, NULL, N'12.', N'Dlhodobý nehmotný majetok', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 112101, NULL, N'13.', N'Goodwill', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 112101, NULL, N'14.', N'Majetok na predaj', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 112101, NULL, N'15.', N'Daňové pohľadávky', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 112101, NULL, N'16.', N'Ostatný majetok', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 112101, NULL, N'17.', N'Vzťahy s ostatnými organizačnými zložkami, aktívne zostatky', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 112101, NULL, N'A', N'AKTÍVA SPOLU', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 112102, NULL, N'18.', N'Záväzky z obchodného styku', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 112102, NULL, N'19.', N'Finančné záväzky oceňované reálnou hodnotou proti zisku/strate', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 112102, NULL, N'20.', N'Finančné záväzky so zaisťovaním ich reálnej hodnoty', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 112102, NULL, N'20.a.', N'reálna hodnota zaistených finančných záväzkov', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 112102, NULL, N'20.b.', N'reálna hodnota zaisťovacích nástrojov', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 112102, NULL, N'21.', N'Finančné záväzky oceňované umorovanou hodnotou', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 112102, NULL, N'22.', N'Záväzky z prenájmov', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 112102, NULL, N'22.a.', N'nehnuteľností', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 112102, NULL, N'22.b.', N'iného majetku', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 112102, NULL, N'23.', N'Rezervy na podsúvahové záväzky', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 112102, NULL, N'24.', N'Podriadené záväzky', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 112102, NULL, N'25.', N'Daňové záväzky', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 112102, NULL, N'26.', N'Ostatné záväzky', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 112102, NULL, N'27.', N'Vzťahy s ostatnými organizačnými zložkami, pasívne zostatky', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 112102, NULL, N'Z.', N'Záväzky spolu', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 112102, NULL, N'28.', N'Vlastné imanie bez fondov z ocenenia, ziskov/strát minulých účtovných období a zistku/straty bežného roka', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 112102, NULL, N'29.', N'Fondy z ocenenia', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 112102, NULL, N'30.', N'Zisky/straty minulých účtovných období', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 112102, NULL, N'30.a.', N'zisky minulých účtovných období', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 112102, NULL, N'30.b.', N'straty minulých účtovných období', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 112102, NULL, N'31.', N'Zisk/strata bežného roka', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 112102, NULL, N'VI.', N'Vlastné imanie spolu', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 112102, NULL, N'P', N'PASÍVA SPOLU', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 112103, NULL, N'1.', N'Výnosy z odplát a provízií', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 112103, NULL, N'a.', N'Náklady na odplaty a provízie', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 112103, NULL, N'2.', N'Výnosy z úrokov', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 112103, NULL, N'b.', N'Náklady na úroky', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 112103, NULL, N'3.', N'Dividendy', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 112103, NULL, N'4./c.', N'Zisk alebo strata z operácií s finančným majetkom', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 112103, NULL, N'5./d.', N'Čistý zisk/strata z investičného nehnuteľného majetku', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 112103, NULL, N'6./e.', N'Zisk alebo strata zo ziskov zo zrušenia zníženia hodnoty finančného majetku a z už odpísaného finančného majetku a straty zo zníženia hodnoty finančného majetku a z odpísania finančného majetku', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 112103, NULL, N'7./f.', N'Zisk alebo strata zo zrušenia rezerv a z tvorby rezerv na podsúvahové záväzky', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 112103, NULL, N'A', N'Zisk alebo strata z bežnej činnosti', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 112103, NULL, N'8./g.', N'Zisk alebo strata z predaja nefinančného majetku a z prevodu nefinančného majetku', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 112103, NULL, N'h.', N'Personálne náklady', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 112103, NULL, N'i.', N'Odpisy', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 112103, NULL, N'j.', N'Administratívne náklady', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 112103, NULL, N'9./k.', N'Zisk alebo strata zo zrušenia zníženia hodnoty prevádzkového majetku a zo zníženia hodnoty prevádzkového majetku', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 112103, NULL, N'10./l.', N'Ostatné náklady alebo výnosy', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 112103, NULL, N'11.', N'Záporný goodwill', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 112103, NULL, N'12./m.', N'Podiel na zisku/strate v spoločných podnikoch a pridružených podnikoch', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 112103, NULL, N'B', N'Zisk alebo strata za účtovné obdobie pred zdanením', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 112103, NULL, N'n.', N'Náklady na daň z príjmov, z toho', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 112103, NULL, N'n.1.', N'uhradenú zrážkou', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 112103, NULL, N'n.2.', N'daň splatná za zdaňovacie obdobie podľa daňového priznania', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 112103, NULL, N'n.3.', N'zúčtovanie odloženej dane', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 112103, NULL, N'C', N'Zisk alebo strata za účtovné obdobie', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 66301, NULL, N'1.', N'Peňažné prostriedky a ekvivalenty peňažných prostriedkov', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 66301, NULL, N'2.', N'Pohľadávky z obchodného styku', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 66301, NULL, N'3.', N'Finančné nástroje oceňované reálnou hodnotou proti zisku/strate', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 66301, NULL, N'4.', N'Finančné nástroje na predaj', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 66301, NULL, N'5.', N'Deriváty s kladnou hodnotou', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 66301, NULL, N'6.', N'Finančné nástroje držané do splatnosti', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 66301, NULL, N'7.', N'Úvery', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 66301, NULL, N'8.', N'Podriadené úvery', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 66301, NULL, N'9.', N'Dcérske účtovné jednotky, spoločné účtovné jednotky a pridružené účtovné jednotky', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 66301, NULL, N'10.', N'Hmotný majetok', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 66301, NULL, N'11.', N'Nehmotný majetok', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 66301, NULL, N'13.', N'Goodwill', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 66301, NULL, N'14.', N'Majetok na predaj', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 66301, NULL, N'15.', N'Daňové pohľadávky', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 66301, NULL, N'16.', N'Ostatný majetok', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 66301, NULL, N'17.', N'Vzťahy s ostatnými organizačnými zložkami, aktívne zostatky', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 66301, NULL, N'A', N'Aktíva spolu', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 66302, NULL, N'18.', N'Záväzky z obchodného styku', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 66302, NULL, N'19.', N'Finančné záväzky oceňované reálnou hodnotou', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 66302, NULL, N'20.', N'Deriváty so zápornou hodnotou', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 66302, NULL, N'21.', N'Finančné záväzky oceňované umorovanou hodnotou', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 66302, NULL, N'22.', N'Rezervy na podsúvahové záväzky', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 66302, NULL, N'23.', N'Podriadené záväzky', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 66302, NULL, N'24.', N'Daňové záväzky', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 66302, NULL, N'25.', N'Ostatné záväzky', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 66302, NULL, N'26.', N'Vzťahy s ostatnými organizačnými zložkami, pasívne zostatky', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 66302, NULL, N'Z.', N'Záväzky spolu', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 66302, NULL, N'27.', N'Vlastné imanie bez fondov z ocenenia a zisku/straty bežného roka', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 66302, NULL, N'28.', N'Fondy z ocenenia', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 66302, NULL, N'29.', N'Zisk/strata bežného roku', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 66302, NULL, N'VI.', N'Vlastné imanie spolu', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 66302, NULL, N'P', N'Pasíva spolu', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 66303, NULL, N'1.', N'Výnosy z odplát a provízií', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 66303, NULL, N'a.', N'Náklady na odplaty a provízie', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 66303, NULL, N'2.', N'Výnosy z úrokov', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 66303, NULL, N'b.', N'Náklady na úroky', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 66303, NULL, N'3.', N'Dividendy', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 66303, NULL, N'4./c.', N'Zisk/strata z operácií s finančným majetkom', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 66303, NULL, N'5./d.', N'Zisk/strata z čistého zrušenia zníženia hodnoty/zníženia hodnoty finančného majetku a z odpísaného/odpísania finančného majetku', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 66303, NULL, N'6./e.', N'Zisk/strata z čistého zrušenia/čistej tvorby rezerv na podsúvahové záväzky', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 66303, NULL, N'7./f.', N'Zisk alebo strata z predaja iného majetku a z prevodu majetku', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 66303, NULL, N'g.', N'Personálne náklady', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 66303, NULL, N'h.', N'Odpisy', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 66303, NULL, N'8./i.', N'Zisk/strata z čistého zrušenia zníženia hodnoty/zníženia hodnoty nefinančného majetku', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 66303, NULL, N'9./j.', N'Ostatné prevádzkové náklady/výnosy', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 66303, NULL, N'10.', N'Záporný goodwill', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 66303, NULL, N'A.', N'Zisk alebo strata za účtovné obdobie pred zdanením', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 66303, NULL, N'k.', N'Daň z príjmov', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 66303, NULL, N'B.', N'Zisk alebo strata za účtovné obdobie po zdanení', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 71101, NULL, N'1.', N'Peňažné prostriedky a ekvivalenty peňažných prostriedkov', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 71101, NULL, N'2.', N'Pohľadávky z obchodného styku', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 71101, NULL, N'3.', N'Finančné nástroje oceňované reálnou hodnotou proti zisku/strate', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 71101, NULL, N'4.', N'Finančné nástroje na predaj', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 71101, NULL, N'5.', N'Deriváty s kladnou hodnotou', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 71101, NULL, N'6.', N'Finančné nástroje držané do splatnosti', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 71101, NULL, N'7.', N'Úvery', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 71101, NULL, N'8.', N'Podriadené úvery', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 71101, NULL, N'9.', N'Dcérske účtovné jednotky, spoločné účtovné jednotky a pridružené účtovné jednotky', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 71101, NULL, N'10.', N'Hmotný majetok', NULL, 0, NULL, NULL, 9
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 71101 AS [TableErpId], NULL AS [RowNumber], N'11.' AS [Designation], N'Nehmotný majetok' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 10 AS [RowOrdinal]
    UNION ALL SELECT 71101, NULL, N'13.', N'Goodwill', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 71101, NULL, N'14.', N'Majetok na predaj', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 71101, NULL, N'15.', N'Daňové pohľadávky', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 71101, NULL, N'16.', N'Ostatný majetok', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 71101, NULL, N'17.', N'Vzťahy s ostatnými organizačnými zložkami, aktívne zostatky', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 71101, NULL, N'A', N'Aktíva spolu', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 71102, NULL, N'18.', N'Záväzky z obchodného styku', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 71102, NULL, N'19.', N'Finančné záväzky oceňované reálnou hodnotou', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 71102, NULL, N'20.', N'Deriváty so zápornou hodnotou', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 71102, NULL, N'21.', N'Finančné záväzky oceňované umorovanou hodnotou', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 71102, NULL, N'22.', N'Rezervy na podsúvahové záväzky', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 71102, NULL, N'23.', N'Podriadené záväzky', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 71102, NULL, N'24.', N'Daňové záväzky', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 71102, NULL, N'25.', N'Ostatné záväzky', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 71102, NULL, N'26.', N'Vzťahy s ostatnými organizačnými zložkami, pasívne zostatky', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 71102, NULL, N'Z.', N'Záväzky spolu', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 71102, NULL, N'27.', N'Vlastné imanie bez fondov z ocenenia a zisku/straty bežného roka', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 71102, NULL, N'28.', N'Fondy z ocenenia', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 71102, NULL, N'29.', N'Zisk/strata bežného roku', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 71102, NULL, N'VI.', N'Vlastné imanie spolu', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 71102, NULL, N'P', N'Pasíva spolu', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 71103, NULL, N'1.', N'Výnosy z odplát a provízií', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 71103, NULL, N'a.', N'Náklady na odplaty a provízie', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 71103, NULL, N'2.', N'Výnosy z úrokov', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 71103, NULL, N'b.', N'Náklady na úroky', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 71103, NULL, N'3.', N'Dividendy', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 71103, NULL, N'4./c.', N'Zisk/strata z operácií s finančným majetkom', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 71103, NULL, N'5./d.', N'Zisk/strata z čistého zrušenia zníženia hodnoty/zníženia hodnoty finančného majetku a z odpísaného/odpísania finančného majetku', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 71103, NULL, N'6./e.', N'Zisk/strata z čistého zrušenia/čistej tvorby rezerv na podsúvahové záväzky', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 71103, NULL, N'7./f.', N'Zisk alebo strata z predaja iného majetku a z prevodu majetku', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 71103, NULL, N'g.', N'Personálne náklady', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 71103, NULL, N'h.', N'Odpisy', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 71103, NULL, N'8./i.', N'Zisk/strata z čistého zrušenia zníženia hodnoty/zníženia hodnoty nefinančného majetku', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 71103, NULL, N'9./j.', N'Ostatné prevádzkové náklady/výnosy', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 71103, NULL, N'10.', N'Záporný goodwill', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 71103, NULL, N'A.', N'Zisk alebo strata za účtovné obdobie pred zdanením', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 71103, NULL, N'k.', N'Daň z príjmov', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 71103, NULL, N'B.', N'Zisk alebo strata za účtovné obdobie po zdanení', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 72301, NULL, N'1.', N'Bežný účet v Národnej banke Slovenska a peňažné prostriedky v pokladni', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 72301, NULL, N'2.', N'Termínované vklady v Národnej banke Slovenska', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 72301, NULL, N'3.', N'Pohľadávky z úverov poskytnutých záručným fondom', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 72301, NULL, N'4.', N'Štátne dlhopisy', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 72301, NULL, N'5.', N'Pohľadávky za vyplatené náhrady', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 72301, NULL, N'6.', N'Pohľadávky voči prispievateľom', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 72301, NULL, N'7.', N'Obstaranie hmotného majetku a nehmotného majetku', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 72301, NULL, N'8.', N'Nehmotný majetok', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 72301, NULL, N'9.', N'Hmotný majetok', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 72301, NULL, N'a)', N'neodpisovaný', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 72301, NULL, N'b)', N'odpisovaný', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 72301, NULL, N'10.', N'Ostatný majetok', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 72301, NULL, N'11.', N'Strata', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 72301, NULL, NULL, N'Aktíva spolu', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 72302, NULL, N'1.', N'Záväzky z úverov voči Národnej banke Slovenska', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 72302, NULL, N'2.', N'Záväzky voči bankám', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 72302, NULL, N'a)', N'z krátkodobých úverov', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 72302, NULL, N'b)', N'z dlhodobých úverov', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 72302, NULL, N'3.', N'Záväzky z úverov voči záručným fondom', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 72302, NULL, N'4.', N'Záväzky na vyplatenie náhrad', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 72302, NULL, N'5.', N'Ostatné záväzky', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 72302, NULL, N'6.', N'Daňové záväzky', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 72302, NULL, N'a)', N'splatná daň z príjmov', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 72302, NULL, N'b)', N'odložený daňový záväzok', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 72302, NULL, N'7.', N'Fond príspevkov', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 72302, NULL, N'8.', N'Zisk', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 72302, NULL, NULL, N'Pasíva spolu', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 72303, NULL, N'a.', N'Personálne náklady', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 72303, NULL, N'a.1.', N'mzdové náklady a sociálne náklady', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 72303, NULL, N'a.2.', N'ostatné personálne náklady', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 72303, NULL, N'b.', N'Ostatné náklady na prevádzku', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 72303, NULL, N'c.', N'Odpisy', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 72303, NULL, N'd.', N'Čisté zníženie hodnoty majetku a odpísanie majetku', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 72303, NULL, N'e.', N'Ostatné náklady', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 72303, NULL, N'1.', N'Výnosy z úrokov', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 72303, NULL, N'2./f.', N'Zisk/strata z operácií s dlhopismi', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 72303, NULL, N'3./g.', N'Zisk/strata z predaja majetku a z prevodu majetku', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 72303, NULL, N'4.', N'Ostatné výnosy', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 72303, NULL, N'I.', N'Náklady na správu záručného fondu', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 72303, NULL, N'h.', N'Náklady na úroky a podobné náklady', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 72303, NULL, N'i.', N'Tvorba rezerv na záväzky na vyplácanie náhrad', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 72303, NULL, N'j.', N'Daň z príjmov', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 72303, NULL, N'II.', N'Náklady na správu Národného fondu', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 72303, NULL, N'III.', N'Náklady na financovanie záručného fondu', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 72303, NULL, N'A.', N'Zisk alebo strata za účtovné obdobie po zdanení', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 94101, NULL, N'1.', N'Peňažné prostriedky a ekvivalenty peňažných prostriedkov', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 94101, NULL, N'2.', N'Pohľadávky z obchodného styku', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 94101, NULL, N'3.', N'Finančné nástroje oceňované reálnou hodnotou proti zisku/strate', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94101, NULL, N'4.', N'Finančné nástroje na predaj', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 94101, NULL, N'5.', N'Deriváty na obchodovanie s kladnou hodnotou', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 94101, NULL, N'6.', N'Deriváty na zabezpečenie s kladnou hodnotou', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 94101, NULL, N'7.', N'Finančný majetok oceňovaný umorovanou hodnotou', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 94101, NULL, N'8.', N'Podriadené úvery', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 94101, NULL, N'9.', N'Dcérske účtovné jednotky, spoločné účtovné jednotky a pridružené účtovné jednotky', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 94101, NULL, N'10.', N'Hmotný majetok', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 94101, NULL, N'11.', N'Nehmotný majetok', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 94101, NULL, N'12.', N'Goodwill', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 94101, NULL, N'13.', N'Majetok na predaj', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 94101, NULL, N'14.', N'Ostatný majetok', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 94101, NULL, N'15.', N'Vzťahy s ostatnými organizačnými zložkami, aktívne zostatky', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 94101, NULL, N'A', N'Aktíva spolu', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 94102, NULL, N'16.', N'Záväzky z obchodného styku', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 94102, NULL, N'17.', N'Finančné záväzky oceňované reálnou hodnotou', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 94102, NULL, N'18.', N'Deriváty na obchodovanie so zápornou hodnotou', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94102, NULL, N'19.', N'Deriváty na zabezpečenie so zápornou hodnotou', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 94102, NULL, N'20.', N'Finančné záväzky oceňované umorovanou hodnotou', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 94102, NULL, N'21.', N'Rezervy na podsúvahové záväzky', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 94102, NULL, N'22.', N'Podriadené záväzky', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 94102, NULL, N'23.', N'Ostatné záväzky', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 94102, NULL, N'24.', N'Vzťahy s ostatnými organizačnými zložkami, pasívne zostatky', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 94102, NULL, N'Z.', N'Záväzky spolu', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 94102, NULL, N'25.', N'Vlastné imanie bez fondov z ocenenia a zisku/straty bežného roku', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 94102, NULL, N'26.', N'Fondy z ocenenia', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 94102, NULL, N'27.', N'Zisk/strata bežného roku', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 94102, NULL, N'VI.', N'Vlastné imanie spolu', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 94102, NULL, N'P', N'Pasíva spolu', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 94103, NULL, N'1.', N'Výnosy z odplát a provízií', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 94103, NULL, N'a.', N'Náklady na odplaty a provízie', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 94103, NULL, N'2.', N'Výnosy z úrokov', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 94103, NULL, N'b.', N'Náklady na úroky', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 94103, NULL, N'3.', N'Dividendy', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 94103, NULL, N'4./c.', N'Zisk/strata z operácií s finančným majetkom', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 94103, NULL, N'5./d.', N'Zisk alebo strata zo zrušenia zníženia hodnoty a zo zníženia hodnoty finančného majetku a z odpísaného a z odpísania finančného majetku', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 94103, NULL, N'6./e.', N'Zisk alebo strata zo zrušenia rezerv a z tvorby rezerv na podsúvahové záväzky', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 94103, NULL, N'A.', N'Zisk alebo strata z bežnej činnosti', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 94103, NULL, N'7./f.', N'Zisk alebo strata z predaja iného majetku a z prevodu majetku', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 94103, NULL, N'g.', N'Personálne náklady', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 94103, NULL, N'h.', N'Odpisy', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 94103, NULL, N'8./i.', N'Zisk alebo strata zo zrušenia zníženia hodnoty a zo zníženia hodnoty nefinančného majetku', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 94103, NULL, N'j./9.', N'Ostatné náklady/výnosy', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 94103, NULL, N'10.', N'Záporný goodwill', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 94103, NULL, N'k.', N'Náklady na poplatky', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 94103, NULL, N'l.', N'Náklady na daň z príjmov, z toho', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 94103, NULL, N'l.1.', N'platenú zrážkou', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 94103, NULL, N'l.2.', N'podľa daňového priznania', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 94103, NULL, N'm./11.', N'Náklady na odloženú daň z príjmov', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 94103, NULL, N'B.', N'Zisk alebo strata za účtovné obdobie', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 102101, NULL, N'01.', N'Peňažné prostriedky a ich ekvivalenty', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 102101, NULL, N'02.', N'Pohľadávky voči bankám', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 102101, NULL, N'03.', N'Finančný majetok na obchodovanie', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 102101, NULL, N'04.', N'Finančný majetok na predaj', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 102101, NULL, N'05.', N'Derivátové finančné pohľadávky', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 102101, NULL, N'06.', N'Pohľadávky voči klientom', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 102101, NULL, N'07.', N'Pohľadávky z poistenia', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 102101, NULL, N'08.', N'Majetok a pohľadávky zo zaistenia', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 102101, NULL, N'09.', N'Investície držané do splatnosti', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 102101, NULL, N'10.', N'Hmotný majetok', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 102101, NULL, N'11.', N'Nehmotný majetok', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 102101, NULL, N'12.', N'Splatná daňová pohľadávka', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 102101, NULL, N'13.', N'Odložená daňová pohľadávka', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 102101, NULL, N'14.', N'Ostatný majetok', NULL, 0, NULL, NULL, 13
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 102101 AS [TableErpId], NULL AS [RowNumber], N'15.' AS [Designation], N'Náklady a príjmy budúcich období' AS [Text_sk], NULL AS [Text_en], 0 AS [IsSumRow], NULL AS [Category_sk], NULL AS [MappingCaption_sk], 14 AS [RowOrdinal]
    UNION ALL SELECT 102101, NULL, N'16.', N'MAJETOK spolu:', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 102102, NULL, N'17.', N'Záväzky voči bankám', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 102102, NULL, N'18.', N'Záväzky z obchodovania', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 102102, NULL, N'19.', N'Derivátové finančné záväzky', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 102102, NULL, N'20.', N'Záväzky voči klientom', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 102102, NULL, N'21.', N'Záväzky zo zaistenia', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 102102, NULL, N'22.', N'Emitované dlhové cenné papiere', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 102102, NULL, N'23.', N'Technické rezervy na poistenie', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 102102, NULL, N'24.', N'Ostatné finančné záväzky', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 102102, NULL, N'25.', N'Ostatné záväzky', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 102102, NULL, N'26.', N'Splatný daňový záväzok', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 102102, NULL, N'27.', N'Odložený daňový záväzok', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 102102, NULL, N'28.', N'Ostatné rezervy', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 102102, NULL, N'29.', N'Výnosy a výdavky budúcich období', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 102102, NULL, N'30.', N'Záväzky spolu:', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 102102, NULL, N'31.', N'Základné imanie', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 102102, NULL, N'32.', N'Kapitálové fondy', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 102102, NULL, N'33.', N'Oceňovacie rozdiely z ocenenia majetku a záväzkov', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 102102, NULL, N'34.', N'Výsledok hospodárenia minulých rokov', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 102102, NULL, N'35.', N'Výsledok hospodárenia bežného účtovného obdobia', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 102102, NULL, N'36.', N'Vlastné imanie spolu:', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 102102, NULL, N'37.', N'VLASTNÉ IMANIE A ZÁVÄZKY spolu:', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 102103, NULL, N'01.', N'Úrokové výnosy', NULL, 0, NULL, NULL, 0
    UNION ALL SELECT 102103, NULL, N'02.', N'Úrokové náklady', NULL, 0, NULL, NULL, 1
    UNION ALL SELECT 102103, NULL, N'03.', N'Čisté úrokové výnosy', NULL, 0, NULL, NULL, 2
    UNION ALL SELECT 102103, NULL, N'04.', N'Predpísané poistné očistené o podiel zaisťovateľov', NULL, 0, NULL, NULL, 3
    UNION ALL SELECT 102103, NULL, N'05.', N'Rezerva na poistné budúcich období očistená o podiel zaisťovateľov', NULL, 0, NULL, NULL, 4
    UNION ALL SELECT 102103, NULL, N'06.', N'Rezerva na neukončené riziká', NULL, 0, NULL, NULL, 5
    UNION ALL SELECT 102103, NULL, N'07.', N'Čisté zaslúžené poistné', NULL, 0, NULL, NULL, 6
    UNION ALL SELECT 102103, NULL, N'08.', N'Čistý zisk/strata (-) z finančných operácií', NULL, 0, NULL, NULL, 7
    UNION ALL SELECT 102103, NULL, N'09.', N'Výnosy z poplatkov z bankových záruk', NULL, 0, NULL, NULL, 8
    UNION ALL SELECT 102103, NULL, N'10.', N'Výnosy z poplatkov a provízií', NULL, 0, NULL, NULL, 9
    UNION ALL SELECT 102103, NULL, N'11.', N'Ostatné výnosy', NULL, 0, NULL, NULL, 10
    UNION ALL SELECT 102103, NULL, N'12.', N'Poistné plnenia', NULL, 0, NULL, NULL, 11
    UNION ALL SELECT 102103, NULL, N'13.', N'Rezervy na poistné plnenia', NULL, 0, NULL, NULL, 12
    UNION ALL SELECT 102103, NULL, N'14.', N'Podiel zaisťovateľov na poistných plneniach', NULL, 0, NULL, NULL, 13
    UNION ALL SELECT 102103, NULL, N'15.', N'Podiel zaisťovateľov na rezervách na poistné plnenia', NULL, 0, NULL, NULL, 14
    UNION ALL SELECT 102103, NULL, N'16.', N'Rezervy na záruky', NULL, 0, NULL, NULL, 15
    UNION ALL SELECT 102103, NULL, N'17.', N'Iné rezervy', NULL, 0, NULL, NULL, 16
    UNION ALL SELECT 102103, NULL, N'18.', N'Všeobecné prevádzkové náklady', NULL, 0, NULL, NULL, 17
    UNION ALL SELECT 102103, NULL, N'19.', N'Odpisy', NULL, 0, NULL, NULL, 18
    UNION ALL SELECT 102103, NULL, N'20.', N'Opravné položky k majetku z toho', NULL, 0, NULL, NULL, 19
    UNION ALL SELECT 102103, NULL, N'21.', N'k pohľadávkam voči klientom', NULL, 0, NULL, NULL, 20
    UNION ALL SELECT 102103, NULL, N'22.', N'VÝSLEDOK HOSPODÁRENIA pred zdanením', NULL, 0, NULL, NULL, 21
    UNION ALL SELECT 102103, NULL, N'23.', N'Daň z príjmov', NULL, 0, NULL, NULL, 22
    UNION ALL SELECT 102103, NULL, N'24.', N'VÝSLEDOK HOSPODÁRENIA (zisk/strata (-)) po zdanení', NULL, 0, NULL, NULL, 23
    UNION ALL SELECT 102103, NULL, N'25.', N'Ostatné súčasti súhrnného výsledku hospodárenia za účtovné obdobie', NULL, 0, NULL, NULL, 24
    UNION ALL SELECT 102103, NULL, N'26.', N'Oceňovacie rozdiely', NULL, 0, NULL, NULL, 25
    UNION ALL SELECT 102103, NULL, N'27.', N'Daň z príjmov vzťahujúca sa na ostatné súčasti súhrnného výsledku hospodárenia', NULL, 0, NULL, NULL, 26
    UNION ALL SELECT 102103, NULL, N'28.', N'Ostatné súčasti súhrnného výsledku hospodárenia za účtovné obdobie po zdanení', NULL, 0, NULL, NULL, 27
    UNION ALL SELECT 102103, NULL, N'29.', N'SÚHRNNÝ VÝSLEDOK HOSPODÁRENIA ZA ÚČTOVNÉ OBDOBIE', NULL, 0, NULL, NULL, 28
)
INSERT INTO [Template].[Rows]
(
    [TableId], [RowNumber], [Designation], [Text_sk], [Text_en], [IsSumRow], [Category_sk], [MappingCaption_sk], [RowOrdinal]
)
SELECT
    t.[Id], n.[RowNumber], n.[Designation], n.[Text_sk], n.[Text_en], n.[IsSumRow], n.[Category_sk], n.[MappingCaption_sk], n.[RowOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Tables] AS t ON t.[TableErpId] = n.[TableErpId]
LEFT JOIN [Template].[Rows] AS e ON e.[TableId] = t.[Id] AND e.[RowOrdinal] = n.[RowOrdinal]
WHERE e.[Id] IS NULL;

COMMIT TRANSACTION;
PRINT '051 template-row population completed.';
GO
