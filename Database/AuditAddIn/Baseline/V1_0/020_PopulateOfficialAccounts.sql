/* Official reference accounts imported from the reviewed OCR CSV files. */
USE [AuditAddIn];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

/* Data is intentionally insert-only; differences against an existing import are reported by the validation script. */

;WITH [NewData] AS
(
    SELECT N'GOV_LOCAL' AS [FrameworkCode], N'2023-01-01' AS [VersionCode], N'0' AS [AccountCode], N'Dlhodobý majetok' AS [AccountName_sk], 1 AS [AccountLevel], NULL AS [ParentAccountCode], 1 AS [SortOrder]
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'01', N'Dlhodobý nehmotný majetok', 2, N'0', 2
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'012', N'Aktivované náklady na vývoj', 3, N'01', 3
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'013', N'Softvér', 3, N'01', 4
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'014', N'Oceniteľné práva', 3, N'01', 5
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'018', N'Drobný dlhodobý nehmotný majetok', 3, N'01', 6
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'019', N'Ostatný dlhodobý nehmotný majetok', 3, N'01', 7
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'02', N'Dlhodobý hmotný majetok odpisovaný', 2, N'0', 8
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'021', N'Stavby', 3, N'02', 9
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'022', N'Samostatné hnuteľné veci a súbory hnuteľných vecí', 3, N'02', 10
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'023', N'Dopravné prostriedky', 3, N'02', 11
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'025', N'Pestovateľské celky trvalých porastov', 3, N'02', 12
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'026', N'Základné stádo a ťažné zvieratá', 3, N'02', 13
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'028', N'Drobný dlhodobý hmotný majetok', 3, N'02', 14
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'029', N'Ostatný dlhodobý hmotný majetok', 3, N'02', 15
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'03', N'Dlhodobý hmotný majetok neodpisovaný', 2, N'0', 16
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'031', N'Pozemky', 3, N'03', 17
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'032', N'Umelecké diela a zbierky', 3, N'03', 18
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'033', N'Predmety z drahých kovov', 3, N'03', 19
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'04', N'Obstaranie dlhodobého majetku', 2, N'0', 20
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'041', N'Obstaranie dlhodobého nehmotného majetku', 3, N'04', 21
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'042', N'Obstaranie dlhodobého hmotného majetku', 3, N'04', 22
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'043', N'Obstaranie dlhodobého finančného majetku', 3, N'04', 23
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'05', N'Poskytnuté preddavky na dlhodobý nehmotný majetok a dlhodobý hmotný majetok', 2, N'0', 24
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'051', N'Poskytnuté preddavky na dlhodobý nehmotný majetok', 3, N'05', 25
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'052', N'Poskytnuté preddavky na dlhodobý hmotný majetok', 3, N'05', 26
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'06', N'Dlhodobý finančný majetok', 2, N'0', 27
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'061', N'Podielové cenné papiere a podiely v dcérskej účtovnej jednotke', 3, N'06', 28
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'062', N'Podielové cenné papiere a podiely v spoločnosti s podstatným vplyvom', 3, N'06', 29
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'063', N'Realizovateľné cenné papiere', 3, N'06', 30
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'065', N'Dlhové cenné papiere držané do splatnosti', 3, N'06', 31
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'066', N'Pôžičky účtovnej jednotke v konsolidovanom celku', 3, N'06', 32
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'067', N'Ostatné pôžičky', 3, N'06', 33
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'069', N'Ostatný dlhodobý finančný majetok', 3, N'06', 34
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'07', N'Oprávky k dlhodobému nehmotnému majetku', 2, N'0', 35
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'072', N'Oprávky k aktivovaným nákladom na vývoj', 3, N'07', 36
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'073', N'Oprávky k softvéru', 3, N'07', 37
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'074', N'Oprávky k oceniteľným právam', 3, N'07', 38
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'078', N'Oprávky k drobnému dlhodobému nehmotnému majetku', 3, N'07', 39
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'079', N'Oprávky k ostatnému dlhodobému nehmotnému majetku', 3, N'07', 40
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'08', N'Oprávky k dlhodobému hmotnému majetku', 2, N'0', 41
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'081', N'Oprávky k stavbám', 3, N'08', 42
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'082', N'Oprávky k samostatným hnuteľným veciam a súborom hnuteľných vecí', 3, N'08', 43
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'083', N'Oprávky k dopravným prostriedkom', 3, N'08', 44
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'085', N'Oprávky k pestovateľským celkom trvalých porastov', 3, N'08', 45
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'086', N'Oprávky k základnému stádu a ťažným zvieratám', 3, N'08', 46
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'088', N'Oprávky k drobnému dlhodobému hmotnému majetku', 3, N'08', 47
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'089', N'Oprávky k ostatnému dlhodobému hmotnému majetku', 3, N'08', 48
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'09', N'Opravné položky k dlhodobému majetku', 2, N'0', 49
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'091', N'Opravná položka k dlhodobému nehmotnému majetku', 3, N'09', 50
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'092', N'Opravná položka k dlhodobému hmotnému majetku', 3, N'09', 51
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'093', N'Opravná položka k nedokončenému dlhodobému nehmotnému majetku', 3, N'09', 52
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'094', N'Opravná položka k nedokončenému dlhodobému hmotnému majetku', 3, N'09', 53
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'095', N'Opravná položka k poskytnutým preddavkom', 3, N'09', 54
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'096', N'Opravná položka k dlhodobému finančnému majetku', 3, N'09', 55
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'1', N'Zásoby', 1, NULL, 56
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'11', N'Materiál', 2, N'1', 57
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'111', N'Obstaranie materiálu', 3, N'11', 58
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'112', N'Materiál na sklade', 3, N'11', 59
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'119', N'Materiál na ceste', 3, N'11', 60
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'12', N'Zásoby vlastnej výroby', 2, N'1', 61
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'121', N'Nedokončená výroba', 3, N'12', 62
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'122', N'Polotovary vlastnej výroby', 3, N'12', 63
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'123', N'Výrobky', 3, N'12', 64
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'124', N'Zvieratá', 3, N'12', 65
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'13', N'Tovar', 2, N'1', 66
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'131', N'Obstaranie tovaru', 3, N'13', 67
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'132', N'Tovar na sklade a v predajniach', 3, N'13', 68
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'133', N'Nehnuteľnosť na predaj', 3, N'13', 69
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'139', N'Tovar na ceste', 3, N'13', 70
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'19', N'Opravné položky k zásobám', 2, N'1', 71
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'191', N'Opravná položka k materiálu', 3, N'19', 72
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'192', N'Opravná položka k nedokončenej výrobe', 3, N'19', 73
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'193', N'Opravná položka k polotovarom vlastnej výroby', 3, N'19', 74
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'194', N'Opravná položka k výrobkom', 3, N'19', 75
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'195', N'Opravná položka k zvieratám', 3, N'19', 76
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'196', N'Opravná položka k tovaru', 3, N'19', 77
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'2', N'Finančné účty', 1, NULL, 78
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'20', N'Vzťahy k účtom klientov štátnej pokladnice', 2, N'2', 79
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'21', N'Peniaze', 2, N'2', 80
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'211', N'Pokladnica', 3, N'21', 81
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'213', N'Ceniny', 3, N'21', 82
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'22', N'Účty v bankách', 2, N'2', 83
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'221', N'Bankové účty', 3, N'22', 84
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'222', N'Výdavkový rozpočtový účet', 3, N'22', 85
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'223', N'Príjmový rozpočtový účet', 3, N'22', 86
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'224', N'Účet štátnych rozpočtových príjmov', 3, N'22', 87
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'225', N'Účet štátnych rozpočtových výdavkov', 3, N'22', 88
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'23', N'Bežné bankové úvery', 2, N'2', 89
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'231', N'Krátkodobé bankové úvery', 3, N'23', 90
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'232', N'Eskontné úvery', 3, N'23', 91
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'24', N'Iné krátkodobé finančné výpomoci', 2, N'2', 92
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'241', N'Vydané krátkodobé dlhopisy', 3, N'24', 93
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'249', N'Ostatné krátkodobé finančné výpomoci', 3, N'24', 94
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'25', N'Krátkodobý finančný majetok', 2, N'2', 95
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'251', N'Majetkové cenné papiere na obchodovanie', 3, N'25', 96
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'253', N'Dlhové cenné papiere na obchodovanie', 3, N'25', 97
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'255', N'Vlastné dlhopisy', 3, N'25', 98
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'256', N'Dlhové cenné papiere so splatnosťou do jedného roka držané do splatnosti', 3, N'25', 99
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'257', N'Ostatné realizovateľné cenné papiere', 3, N'25', 100
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'259', N'Obstaranie krátkodobého finančného majetku', 3, N'25', 101
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'26', N'Prevody medzi finančnými účtami', 2, N'2', 102
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'261', N'Peniaze na ceste', 3, N'26', 103
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'27', N'Návratné finančné výpomoci', 2, N'2', 104
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'271', N'Poskytnuté návratné finančné výpomoci subjektom v rámci konsolidovaného celku', 3, N'27', 105
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'272', N'Poskytnuté návratné finančné výpomoci ostatným subjektom verejnej správy', 3, N'27', 106
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'273', N'Prijaté návratné finančné výpomoci od subjektov verejnej správy', 3, N'27', 107
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'274', N'Poskytnuté návratné finančné výpomoci podnikateľským subjektom', 3, N'27', 108
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'275', N'Poskytnuté návratné finančné výpomoci ostatným organizáciám', 3, N'27', 109
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'277', N'Poskytnuté finančné výpomoci fyzickým osobám', 3, N'27', 110
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'28', N'Účty štátnej pokladnice', 2, N'2', 111
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'29', N'Opravné položky ku krátkodobému finančnému majetku', 2, N'2', 112
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'291', N'Opravné položky ku krátkodobému finančnému majetku', 3, N'29', 113
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'3', N'Zúčtovacie vzťahy', 1, NULL, 114
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'31', N'Pohľadávky', 2, N'3', 115
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'311', N'Odberatelia', 3, N'31', 116
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'312', N'Zmenky na inkaso', 3, N'31', 117
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'313', N'Pohľadávky za eskontované cenné papiere', 3, N'31', 118
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'314', N'Poskytnuté prevádzkové preddavky', 3, N'31', 119
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'315', N'Ostatné pohľadávky', 3, N'31', 120
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'316', N'Pohľadávky z nedaňových rozpočtových príjmov', 3, N'31', 121
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'317', N'Pohľadávky z daňových a colných rozpočtových príjmov', 3, N'31', 122
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'318', N'Pohľadávky z nedaňových príjmov obcí a vyšších územných celkov a rozpočtových organizácií zriadených obcou a vyšším územným celkom', 3, N'31', 123
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'319', N'Pohľadávky z daňových príjmov obcí a vyšších územných celkov', 3, N'31', 124
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'32', N'Záväzky', 2, N'3', 125
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'321', N'Dodávatelia', 3, N'32', 126
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'322', N'Zmenky na úhradu', 3, N'32', 127
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'323', N'Krátkodobé rezervy', 3, N'32', 128
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'324', N'Prijaté preddavky', 3, N'32', 129
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'325', N'Ostatné záväzky', 3, N'32', 130
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'326', N'Nevyfakturované dodávky', 3, N'32', 131
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'33', N'Zúčtovanie so zamestnancami a orgánmi sociálneho poistenia a zdravotného poistenia', 2, N'3', 132
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'331', N'Zamestnanci', 3, N'33', 133
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'333', N'Ostatné záväzky voči zamestnancom', 3, N'33', 134
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'335', N'Pohľadávky voči zamestnancom', 3, N'33', 135
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'336', N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia', 3, N'33', 136
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'34', N'Zúčtovanie daní a poplatkov', 2, N'3', 137
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'341', N'Daň z príjmov', 3, N'34', 138
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'342', N'Ostatné priame dane', 3, N'34', 139
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'343', N'Daň z pridanej hodnoty', 3, N'34', 140
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'345', N'Ostatné dane a poplatky', 3, N'34', 141
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'35', N'Zúčtovanie medzi subjektami verejnej správy', 2, N'3', 142
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'351', N'Zúčtovanie odvodov príjmov rozpočtových organizácií do rozpočtu zriaďovateľa', 3, N'35', 143
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'352', N'Zúčtovanie z financovania zo štátneho rozpočtu', 3, N'35', 144
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'353', N'Zúčtovanie transferov štátneho rozpočtu', 3, N'35', 145
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'354', N'Zúčtovanie z financovania z rozpočtu obce a vyššieho územného celku', 3, N'35', 146
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'355', N'Zúčtovanie transferov rozpočtu obce a vyššieho územného celku', 3, N'35', 147
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'356', N'Zúčtovanie transferov zo štátneho rozpočtu v rámci konsolidovaného celku', 3, N'35', 148
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'357', N'Ostatné zúčtovanie rozpočtu obce a vyššieho územného celku', 3, N'35', 149
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'358', N'Zúčtovanie transferov zo štátneho rozpočtu iným subjektom', 3, N'35', 150
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'359', N'Zúčtovanie transferov medzi subjektami verejnej správy a iné zúčtovania', 3, N'35', 151
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'36', N'Záväzky z upísaných nesplatených cenných papierov a vkladov, záväzky a pohľadávky voči združeniu', 2, N'3', 152
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'367', N'Záväzky z upísaných nesplatených cenných papierov a vkladov', 3, N'36', 153
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'368', N'Záväzky voči združeniu', 3, N'36', 154
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'369', N'Pohľadávky voči združeniu', 3, N'36', 155
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'37', N'Iné pohľadávky a záväzky', 2, N'3', 156
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'371', N'Zúčtovanie s Európskou úniou', 3, N'37', 157
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'372', N'Transfery a ostatné zúčtovanie so subjektami mimo verejnej správy', 3, N'37', 158
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'373', N'Pohľadávky a záväzky z pevných termínových operácií', 3, N'37', 159
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'374', N'Pohľadávky z nájmu', 3, N'37', 160
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'375', N'Pohľadávky z vydaných dlhopisov', 3, N'37', 161
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'376', N'Nakúpené opcie', 3, N'37', 162
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'377', N'Predané opcie', 3, N'37', 163
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'378', N'Iné pohľadávky', 3, N'37', 164
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'379', N'Iné záväzky', 3, N'37', 165
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'38', N'Časové rozlíšenie nákladov a výnosov', 2, N'3', 166
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'381', N'Náklady budúcich období', 3, N'38', 167
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'382', N'Komplexné náklady budúcich období', 3, N'38', 168
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'383', N'Výdavky budúcich období', 3, N'38', 169
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'384', N'Výnosy budúcich období', 3, N'38', 170
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'385', N'Príjmy budúcich období', 3, N'38', 171
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'39', N'Opravná položka k zúčtovacím vzťahom a vnútorné zúčtovanie', 2, N'3', 172
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'391', N'Opravná položka k pohľadávkam', 3, N'39', 173
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'395', N'Vnútorné zúčtovanie', 3, N'39', 174
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'396', N'Spojovací účet pri združení', 3, N'39', 175
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'4', N'Vlastné imanie a dlhodobé záväzky', 1, NULL, 176
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'41', N'Oceňovacie rozdiely', 2, N'4', 177
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'414', N'Oceňovacie rozdiely z precenenia majetku a záväzkov', 3, N'41', 178
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'415', N'Oceňovacie rozdiely z kapitálových účastín', 3, N'41', 179
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'42', N'Fondy tvorené z kladného výsledku hospodárenia a prevedené výsledky hospodárenia', 2, N'4', 180
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'421', N'Zákonný rezervný fond', 3, N'42', 181
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'427', N'Ostatné fondy', 3, N'42', 182
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'428', N'Nevysporiadaný výsledok hospodárenia minulých rokov', 3, N'42', 183
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'43', N'Výsledok hospodárenia', 2, N'4', 184
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'431', N'Výsledok hospodárenia', 3, N'43', 185
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'45', N'Rezervy', 2, N'4', 186
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'451', N'Rezervy zákonné', 3, N'45', 187
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'459', N'Ostatné rezervy', 3, N'45', 188
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'46', N'Bankové úvery', 2, N'4', 189
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'461', N'Bankové úvery', 3, N'46', 190
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'47', N'Dlhodobé záväzky', 2, N'4', 191
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'472', N'Záväzky zo sociálneho fondu', 3, N'47', 192
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'473', N'Vydané dlhopisy', 3, N'47', 193
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'474', N'Záväzky z nájmu', 3, N'47', 194
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'475', N'Dlhodobé prijaté preddavky', 3, N'47', 195
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'476', N'Dlhodobé nevyfakturované dodávky', 3, N'47', 196
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'478', N'Dlhodobé zmenky na úhradu', 3, N'47', 197
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'479', N'Ostatné dlhodobé záväzky', 3, N'47', 198
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'5', N'Náklady', 1, NULL, 199
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'50', N'Spotrebované nákupy', 2, N'5', 200
)
INSERT INTO [Accounts].[OfficialAccounts]
(
    [AccountFrameworkVersionId], [AccountCode], [AccountName_sk], [AccountLevel], [ParentAccountCode], [SortOrder]
)
SELECT
    afv.[Id], n.[AccountCode], n.[AccountName_sk], n.[AccountLevel], n.[ParentAccountCode], n.[SortOrder]
FROM [NewData] AS n
INNER JOIN [Accounts].[AccountFramework] AS af ON af.[Code] = n.[FrameworkCode]
INNER JOIN [Accounts].[AccountFrameworkVersion] AS afv ON afv.[AccountFrameworkId] = af.[Id] AND afv.[VersionCode] = n.[VersionCode]
LEFT JOIN [Accounts].[OfficialAccounts] AS e ON e.[AccountFrameworkVersionId] = afv.[Id] AND e.[AccountCode] = n.[AccountCode]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT N'GOV_LOCAL' AS [FrameworkCode], N'2023-01-01' AS [VersionCode], N'501' AS [AccountCode], N'Spotreba materiálu' AS [AccountName_sk], 3 AS [AccountLevel], N'50' AS [ParentAccountCode], 201 AS [SortOrder]
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'502', N'Spotreba energie', 3, N'50', 202
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'503', N'Spotreba ostatných neskladovateľných dodávok', 3, N'50', 203
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'504', N'Predaný tovar', 3, N'50', 204
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'507', N'Predaná nehnuteľnosť', 3, N'50', 205
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'51', N'Služby', 2, N'5', 206
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'511', N'Opravy a udržiavanie', 3, N'51', 207
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'512', N'Cestovné', 3, N'51', 208
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'513', N'Náklady na reprezentáciu', 3, N'51', 209
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'518', N'Ostatné služby', 3, N'51', 210
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'52', N'Osobné náklady', 2, N'5', 211
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'521', N'Mzdové náklady', 3, N'52', 212
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'524', N'Zákonné sociálne poistenie', 3, N'52', 213
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'525', N'Ostatné sociálne poistenie', 3, N'52', 214
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'527', N'Zákonné sociálne náklady', 3, N'52', 215
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'528', N'Ostatné sociálne náklady', 3, N'52', 216
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'53', N'Dane a poplatky', 2, N'5', 217
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'531', N'Daň z motorových vozidiel', 3, N'53', 218
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'532', N'Daň z nehnuteľností', 3, N'53', 219
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'538', N'Ostatné dane a poplatky', 3, N'53', 220
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'54', N'Ostatné náklady na prevádzkovú činnosť', 2, N'5', 221
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'541', N'Zostatková cena predaného dlhodobého nehmotného majetku a dlhodobého hmotného majetku', 3, N'54', 222
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'542', N'Predaný materiál', 3, N'54', 223
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'544', N'Zmluvné pokuty, penále a úroky z omeškania', 3, N'54', 224
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'545', N'Ostatné pokuty, penále a úroky z omeškania', 3, N'54', 225
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'546', N'Odpis pohľadávky', 3, N'54', 226
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'548', N'Ostatné náklady na prevádzkovú činnosť', 3, N'54', 227
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'549', N'Manká a škody', 3, N'54', 228
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'55', N'Odpisy, rezervy a opravné položky z prevádzkovej a finančnej činnosti a zúčtovanie časového rozlíšenia', 2, N'5', 229
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'551', N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', 3, N'55', 230
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'552', N'Tvorba zákonných rezerv z prevádzkovej činnosti', 3, N'55', 231
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'553', N'Tvorba ostatných rezerv z prevádzkovej činnosti', 3, N'55', 232
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'554', N'Tvorba rezerv z finančnej činnosti', 3, N'55', 233
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'555', N'Zúčtovanie komplexných nákladov budúcich období', 3, N'55', 234
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'557', N'Tvorba zákonných opravných položiek z prevádzkovej činnosti', 3, N'55', 235
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'558', N'Tvorba ostatných opravných položiek z prevádzkovej činnosti', 3, N'55', 236
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'559', N'Tvorba ostatných opravných položiek z finančnej činnosti', 3, N'55', 237
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'56', N'Finančné náklady', 2, N'5', 238
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'561', N'Predané cenné papiere a podiely', 3, N'56', 239
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'562', N'Úroky', 3, N'56', 240
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'563', N'Kurzové straty', 3, N'56', 241
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'564', N'Náklady na precenenie cenných papierov', 3, N'56', 242
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'566', N'Náklady na krátkodobý finančný majetok', 3, N'56', 243
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'567', N'Náklady na derivátové operácie', 3, N'56', 244
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'568', N'Ostatné finančné náklady', 3, N'56', 245
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'569', N'Manká a škody na finančnom majetku', 3, N'56', 246
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'57', N'Mimoriadne náklady', 2, N'5', 247
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'572', N'Škody', 3, N'57', 248
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'574', N'Tvorba rezerv', 3, N'57', 249
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'578', N'Ostatné mimoriadne náklady', 3, N'57', 250
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'579', N'Tvorba opravných položiek', 3, N'57', 251
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'58', N'Náklady na transfery a náklady z odvodov príjmov', 2, N'5', 252
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'581', N'Náklady na transfery zo štátneho rozpočtu do štátnych rozpočtových organizácií a príspevkových organizácií', 3, N'58', 253
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'582', N'Náklady na transfery zo štátneho rozpočtu ostatným subjektom verejnej správy', 3, N'58', 254
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'583', N'Náklady na transfery zo štátneho rozpočtu subjektom mimo verejnej správy', 3, N'58', 255
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'584', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku do rozpočtových organizácií a príspevkových organizácií zriadených obcou alebo vyšším územným celkom', 3, N'58', 256
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'585', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku ostatným subjektom verejnej správy', 3, N'58', 257
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'586', N'Náklady na transfery z rozpočtu obce alebo z rozpočtu vyššieho územného celku subjektom mimo verejnej správy', 3, N'58', 258
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'587', N'Náklady na ostatné transfery', 3, N'58', 259
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'588', N'Náklady z odvodu príjmov', 3, N'58', 260
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'589', N'Náklady z budúceho odvodu príjmov', 3, N'58', 261
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'59', N'Dane z príjmov', 2, N'5', 262
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'591', N'Splatná daň z príjmov', 3, N'59', 263
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'595', N'Dodatočne platená daň z príjmov', 3, N'59', 264
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'6', N'Výnosy', 1, NULL, 265
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'60', N'Tržby za vlastné výkony a tovar', 2, N'6', 266
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'601', N'Tržby za vlastné výrobky', 3, N'60', 267
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'602', N'Tržby z predaja služieb', 3, N'60', 268
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'604', N'Tržby za tovar', 3, N'60', 269
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'607', N'Výnosy z nehnuteľnosti na predaj', 3, N'60', 270
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'61', N'Zmena stavu vnútroorganizačných zásob', 2, N'6', 271
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'611', N'Zmena stavu nedokončenej výroby', 3, N'61', 272
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'612', N'Zmena stavu polotovarov', 3, N'61', 273
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'613', N'Zmena stavu výrobkov', 3, N'61', 274
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'614', N'Zmena stavu zvierat', 3, N'61', 275
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'62', N'Aktivácia', 2, N'6', 276
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'621', N'Aktivácia materiálu a tovaru', 3, N'62', 277
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'622', N'Aktivácia vnútroorganizačných služieb', 3, N'62', 278
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'623', N'Aktivácia dlhodobého nehmotného majetku', 3, N'62', 279
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'624', N'Aktivácia dlhodobého hmotného majetku', 3, N'62', 280
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'63', N'Daňové a colné výnosy a výnosy z poplatkov', 2, N'6', 281
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'631', N'Daňové a colné výnosy štátu', 3, N'63', 282
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'632', N'Daňové výnosy samosprávy', 3, N'63', 283
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'633', N'Výnosy z poplatkov', 3, N'63', 284
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'64', N'Ostatné výnosy', 2, N'6', 285
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'641', N'Tržby z predaja dlhodobého nehmotného majetku a dlhodobého hmotného majetku', 3, N'64', 286
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'642', N'Tržby z predaja materiálu', 3, N'64', 287
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'644', N'Zmluvné pokuty, penále a úroky z omeškania', 3, N'64', 288
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'645', N'Ostatné pokuty, penále a úroky z omeškania', 3, N'64', 289
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'646', N'Výnosy z odpísaných pohľadávok', 3, N'64', 290
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'648', N'Ostatné výnosy z prevádzkovej činnosti', 3, N'64', 291
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'65', N'Zúčtovanie rezerv a opravných položiek z prevádzkovej a finančnej činnosti a zúčtovanie časového rozlíšenia', 2, N'6', 292
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'652', N'Zúčtovanie zákonných rezerv z prevádzkovej činnosti', 3, N'65', 293
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'653', N'Zúčtovanie ostatných rezerv z prevádzkovej činnosti', 3, N'65', 294
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'654', N'Zúčtovanie rezerv z finančnej činnosti', 3, N'65', 295
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'655', N'Zúčtovanie komplexných nákladov budúcich období', 3, N'65', 296
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'657', N'Zúčtovanie zákonných opravných položiek z prevádzkovej činnosti', 3, N'65', 297
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'658', N'Zúčtovanie ostatných opravných položiek z prevádzkovej činnosti', 3, N'65', 298
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'659', N'Zúčtovanie opravných položiek z finančnej činnosti', 3, N'65', 299
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'66', N'Finančné výnosy', 2, N'6', 300
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'661', N'Tržby z predaja cenných papierov a podielov', 3, N'66', 301
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'662', N'Úroky', 3, N'66', 302
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'663', N'Kurzové zisky', 3, N'66', 303
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'664', N'Výnosy z precenenia cenných papierov', 3, N'66', 304
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'665', N'Výnosy z dlhodobého finančného majetku', 3, N'66', 305
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'666', N'Výnosy z krátkodobého finančného majetku', 3, N'66', 306
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'667', N'Výnosy z derivátových operácií', 3, N'66', 307
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'668', N'Ostatné finančné výnosy', 3, N'66', 308
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'67', N'Mimoriadne výnosy', 2, N'6', 309
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'672', N'Náhrady škôd', 3, N'67', 310
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'674', N'Zúčtovanie rezerv', 3, N'67', 311
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'678', N'Ostatné mimoriadne výnosy', 3, N'67', 312
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'679', N'Zúčtovanie opravných položiek', 3, N'67', 313
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'68', N'Výnosy z transferov a rozpočtových príjmov v štátnych rozpočtových organizáciách a príspevkových organizáciách', 2, N'6', 314
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'681', N'Výnosy z bežných transferov zo štátneho rozpočtu', 3, N'68', 315
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'682', N'Výnosy z kapitálových transferov zo štátneho rozpočtu', 3, N'68', 316
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'683', N'Výnosy z bežných transferov od ostatných subjektov verejnej správy', 3, N'68', 317
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'684', N'Výnosy z kapitálových transferov od ostatných subjektov verejnej správy', 3, N'68', 318
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'685', N'Výnosy z bežných transferov od Európskych spoločenstiev', 3, N'68', 319
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'686', N'Výnosy z kapitálových transferov od Európskych spoločenstiev', 3, N'68', 320
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'687', N'Výnosy z bežných transferov od ostatných subjektov mimo verejnej správy', 3, N'68', 321
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'688', N'Výnosy z kapitálových transferov od ostatných subjektov mimo verejnej správy', 3, N'68', 322
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'689', N'Výnosy z odvodu rozpočtových príjmov', 3, N'68', 323
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'69', N'Výnosy z transferov a rozpočtových príjmov v obciach a vyšších územných celkoch a v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom', 2, N'6', 324
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'691', N'Výnosy z bežných transferov z rozpočtu obce alebo z rozpočtu vyššieho územného celku v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom', 3, N'69', 325
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'692', N'Výnosy z kapitálových transferov z rozpočtu obce alebo z rozpočtu vyššieho územného celku v rozpočtových organizáciách a príspevkových organizáciách zriadených obcou alebo vyšším územným celkom', 3, N'69', 326
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'693', N'Výnosy samosprávy z bežných transferov zo štátneho rozpočtu a od iných subjektov verejnej správy', 3, N'69', 327
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'694', N'Výnosy samosprávy z kapitálových transferov zo štátneho rozpočtu a od iných subjektov verejnej správy', 3, N'69', 328
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'695', N'Výnosy samosprávy z bežných transferov od Európskych spoločenstiev', 3, N'69', 329
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'696', N'Výnosy samosprávy z kapitálových transferov od Európskych spoločenstiev', 3, N'69', 330
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'697', N'Výnosy samosprávy z bežných transferov od ostatných subjektov mimo verejnej správy', 3, N'69', 331
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'698', N'Výnosy samosprávy z kapitálových transferov od ostatných subjektov mimo verejnej správy', 3, N'69', 332
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'699', N'Výnosy samosprávy z odvodu rozpočtových príjmov', 3, N'69', 333
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'7', N'Uzávierkové účty a podsúvahové účty', 1, NULL, 334
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'70', N'Súvahové uzávierkové účty', 2, N'7', 335
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'701', N'Začiatočný účet súvahový', 3, N'70', 336
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'702', N'Konečný účet súvahový', 3, N'70', 337
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'71', N'Výsledkový uzávierkový účet', 2, N'7', 338
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'710', N'Účet výsledku hospodárenia', 3, N'71', 339
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'75', N' Podsúvahové účty', 2, N'7', 340
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'76', N' Podsúvahové účty', 2, N'7', 341
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'77', N' Podsúvahové účty', 2, N'7', 342
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'78', N' Podsúvahové účty', 2, N'7', 343
    UNION ALL SELECT N'GOV_LOCAL', N'2023-01-01', N'79', N' Podsúvahové účty', 2, N'7', 344
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'0', N'Dlhodobý majetok', 1, NULL, 1
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'01', N'Dlhodobý nehmotný majetok', 2, N'0', 2
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'012', N'Aktivované náklady na vývoj', 3, N'01', 3
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'013', N'Softvér', 3, N'01', 4
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'014', N'Oceniteľné práva', 3, N'01', 5
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'015', N'Goodwill', 3, N'01', 6
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'019', N'Ostatný dlhodobý nehmotný majetok', 3, N'01', 7
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'02', N'Dlhodobý hmotný majetok — odpisovaný', 2, N'0', 8
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'021', N'Stavby', 3, N'02', 9
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'022', N'Samostatné hnuteľné veci a súbory hnuteľných vecí', 3, N'02', 10
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'025', N'Pestovateľské celky trvalých porastov', 3, N'02', 11
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'026', N'Základné stádo a ťažné zvieratá', 3, N'02', 12
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'029', N'Ostatný dlhodobý hmotný majetok', 3, N'02', 13
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'03', N'Dlhodobý hmotný majetok — neodpisovaný', 2, N'0', 14
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'031', N'Pozemky', 3, N'03', 15
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'032', N'Umelecké diela a zbierky', 3, N'03', 16
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'04', N'Obstaranie dlhodobého majetku', 2, N'0', 17
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'041', N'Obstaranie dlhodobého nehmotného majetku', 3, N'04', 18
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'042', N'Obstaranie dlhodobého hmotného majetku', 3, N'04', 19
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'043', N'Obstaranie dlhodobého finančného majetku', 3, N'04', 20
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'05', N'Poskytnuté preddavky na dlhodobý majetok', 2, N'0', 21
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'051', N'Poskytnuté preddavky na dlhodobý nehmotný majetok', 3, N'05', 22
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'052', N'Poskytnuté preddavky na dlhodobý hmotný majetok', 3, N'05', 23
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'053', N'Poskytnuté preddavky na dlhodobý finančný majetok', 3, N'05', 24
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'06', N'Dlhodobý finančný majetok', 2, N'0', 25
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'061', N'Podielové cenné papiere a podiely v dcérskej účtovnej jednotke', 3, N'06', 26
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'062', N'Podielové cenné papiere a podiely v spoločnosti alebo družstve s podielovou účasťou (zmena názvu účtu od 31.12.2015)', 3, N'06', 27
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'063', N'Realizovateľné cenné papiere a podiely', 3, N'06', 28
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'065', N'Dlhové cenné papiere držané do splatnosti', 3, N'06', 29
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'066', N'Pôžičky prepojeným účtovným jednotkám a účtovným jednotkám v rámci podielovej účasti (zmena názvu účtu od 31.12.2015)', 3, N'06', 30
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'067', N'Ostatné pôžičky', 3, N'06', 31
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'069', N'Ostatný dlhodobý finančný majetok', 3, N'06', 32
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'07', N'Oprávky k dlhodobému nehmotnému majetku', 2, N'0', 33
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'072', N'Oprávky k aktivovaným nákladom na vývoj', 3, N'07', 34
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'073', N'Oprávky k softvéru', 3, N'07', 35
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'074', N'Oprávky k oceniteľným právam', 3, N'07', 36
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'075', N'Oprávky ku goodwillu', 3, N'07', 37
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'079', N'Oprávky k ostatnému dlhodobému nehmotnému majetku', 3, N'07', 38
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'08', N'Oprávky k dlhodobému hmotnému majetku', 2, N'0', 39
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'081', N'Oprávky k stavbám', 3, N'08', 40
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'082', N'Oprávky k samostatným hnuteľným veciam a k súboru hnuteľných vecí', 3, N'08', 41
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'085', N'Oprávky k pestovateľským celkom trvalých porastov', 3, N'08', 42
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'086', N'Oprávky k základnému stádu a ťažným zvieratám', 3, N'08', 43
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'089', N'Oprávky k ostatnému dlhodobému hmotnému majetku', 3, N'08', 44
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'09', N'Opravné položky k dlhodobému majetku', 2, N'0', 45
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'091', N'Opravné položky k dlhodobému nehmotnému majetku', 3, N'09', 46
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'092', N'Opravné položky k dlhodobému hmotnému majetku', 3, N'09', 47
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'093', N'Opravné položky k nedokončenému dlhodobému nehmotnému majetku', 3, N'09', 48
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'094', N'Opravné položky k nedokončenému dlhodobému hmotnému majetku', 3, N'09', 49
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'095', N'Opravné položky k poskytnutým preddavkom na dlhodobý majetok', 3, N'09', 50
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'096', N'Opravné položky k dlhodobému finančnému majetku', 3, N'09', 51
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'097', N'Opravné položky k nadobudnutému majetku', 3, N'09', 52
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'098', N'Oprávky k opravnej položke k nadobudnutému majetku', 3, N'09', 53
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'1', N'Zásoby', 1, NULL, 54
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'11', N'Materiál', 2, N'1', 55
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'111', N'Obstaranie materiálu', 3, N'11', 56
)
INSERT INTO [Accounts].[OfficialAccounts]
(
    [AccountFrameworkVersionId], [AccountCode], [AccountName_sk], [AccountLevel], [ParentAccountCode], [SortOrder]
)
SELECT
    afv.[Id], n.[AccountCode], n.[AccountName_sk], n.[AccountLevel], n.[ParentAccountCode], n.[SortOrder]
FROM [NewData] AS n
INNER JOIN [Accounts].[AccountFramework] AS af ON af.[Code] = n.[FrameworkCode]
INNER JOIN [Accounts].[AccountFrameworkVersion] AS afv ON afv.[AccountFrameworkId] = af.[Id] AND afv.[VersionCode] = n.[VersionCode]
LEFT JOIN [Accounts].[OfficialAccounts] AS e ON e.[AccountFrameworkVersionId] = afv.[Id] AND e.[AccountCode] = n.[AccountCode]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT N'PROFIT' AS [FrameworkCode], N'2022-01-01' AS [VersionCode], N'112' AS [AccountCode], N'Materiál na sklade' AS [AccountName_sk], 3 AS [AccountLevel], N'11' AS [ParentAccountCode], 57 AS [SortOrder]
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'119', N'Materiál na ceste', 3, N'11', 58
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'12', N'Zásoby vlastnej výroby', 2, N'1', 59
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'121', N'Nedokončená výroba', 3, N'12', 60
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'122', N'Polotovary vlastnej výroby', 3, N'12', 61
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'123', N'Výrobky', 3, N'12', 62
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'124', N'Zvieratá', 3, N'12', 63
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'13', N'Tovar', 2, N'1', 64
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'131', N'Obstaranie tovaru', 3, N'13', 65
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'132', N'Tovar na sklade a v predajniach', 3, N'13', 66
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'133', N'Nehnuteľnosť na predaj (účet doplnený od 1.1.2011)', 3, N'13', 67
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'139', N'Tovar na ceste', 3, N'13', 68
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'19', N'Opravné položky k zásobám', 2, N'1', 69
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'191', N'Opravné položky k materiálu', 3, N'19', 70
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'192', N'Opravné položky k nedokončenej výrobe', 3, N'19', 71
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'193', N'Opravné položky k polotovarom vlastnej výroby', 3, N'19', 72
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'194', N'Opravné položky k výrobkom', 3, N'19', 73
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'195', N'Opravné položky k zvieratám', 3, N'19', 74
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'196', N'Opravné položky k tovaru', 3, N'19', 75
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'2', N'Finančné účty', 1, NULL, 76
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'21', N'Peniaze', 2, N'2', 77
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'211', N'Pokladnica', 3, N'21', 78
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'213', N'Ceniny', 3, N'21', 79
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'22', N'Účty v bankách', 2, N'2', 80
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'221', N'Bankové účty', 3, N'22', 81
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'23', N'Bežné bankové úvery', 2, N'2', 82
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'231', N'Krátkodobé bankové úvery', 3, N'23', 83
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'232', N'Eskontné úvery', 3, N'23', 84
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'24', N'Iné krátkodobé finančné výpomoci', 2, N'2', 85
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'241', N'Vydané krátkodobé dlhopisy', 3, N'24', 86
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'249', N'Ostatné krátkodobé finančné výpomoci', 3, N'24', 87
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'25', N'Krátkodobý finančný majetok', 2, N'2', 88
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'251', N'Majetkové cenné papiere na obchodovanie', 3, N'25', 89
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'252', N'Vlastné akcie a vlastné obchodné podiely', 3, N'25', 90
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'253', N'Dlhové cenné papiere na obchodovanie', 3, N'25', 91
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'255', N'Vlastné dlhopisy', 3, N'25', 92
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'256', N'Dlhové cenné papiere so splatnosťou do jedného roka držané do splatnosti', 3, N'25', 93
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'257', N'Ostatné realizovateľné cenné papiere', 3, N'25', 94
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'259', N'Obstaranie krátkodobého finančného majetku', 3, N'25', 95
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'26', N'Prevody medzi finančnými účtami', 2, N'2', 96
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'261', N'Peniaze na ceste', 3, N'26', 97
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'29', N'Opravné položky ku krátkodobému finančnému majetku', 2, N'2', 98
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'291', N'Opravné položky ku krátkodobému finančnému majetku', 3, N'29', 99
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'3', N'Zúčtovacie vzťahy', 1, NULL, 100
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'31', N'Pohľadávky', 2, N'3', 101
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'311', N'Odberatelia', 3, N'31', 102
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'312', N'Zmenky na inkaso', 3, N'31', 103
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'313', N'Pohľadávky za eskontované cenné papiere', 3, N'31', 104
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'314', N'Poskytnuté preddavky', 3, N'31', 105
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'315', N'Ostatné pohľadávky', 3, N'31', 106
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'316', N'Čistá hodnota zákazky (účet doplnený od 1.1.2011)', 3, N'31', 107
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'32', N'Záväzky', 2, N'3', 108
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'321', N'Dodávatelia', 3, N'32', 109
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'322', N'Zmenky na úhradu', 3, N'32', 110
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'323', N'Krátkodobé rezervy', 3, N'32', 111
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'324', N'Prijaté preddavky', 3, N'32', 112
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'325', N'Ostatné záväzky', 3, N'32', 113
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'326', N'Nevyfakturované dodávky', 3, N'32', 114
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'33', N'Zúčtovanie so zamestnancami a orgánmi sociálneho poistenia a zdravotného poistenia', 2, N'3', 115
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'331', N'Zamestnanci', 3, N'33', 116
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'333', N'Ostatné záväzky voči zamestnancom', 3, N'33', 117
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'335', N'Pohľadávky voči zamestnancom', 3, N'33', 118
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'336', N'Zúčtovanie s orgánmi sociálneho poistenia a zdravotného poistenia', 3, N'33', 119
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'34', N'Zúčtovanie daní a dotácií', 2, N'3', 120
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'341', N'Daň z príjmov', 3, N'34', 121
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'342', N'Ostatné priame dane', 3, N'34', 122
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'343', N'Daň z pridanej hodnoty', 3, N'34', 123
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'345', N'Ostatné dane a poplatky', 3, N'34', 124
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'346', N'Dotácie zo štátneho rozpočtu', 3, N'34', 125
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'347', N'Ostatné dotácie', 3, N'34', 126
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'35', N'Pohľadávky voči spoločníkom a združeniu', 2, N'3', 127
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'351', N'Pohľadávky voči prepojeným účtovným jednotkám a účtovným jednotkám v rámci podielovej účasti (zmena názvu účtu od 31.12.2015)', 3, N'35', 128
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'353', N'Pohľadávky za upísané vlastné imanie', 3, N'35', 129
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'354', N'Pohľadávky voči spoločníkom a členom pri úhrade straty', 3, N'35', 130
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'355', N'Ostatné pohľadávky voči spoločníkom a členom', 3, N'35', 131
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'358', N'Pohľadávky voči účastníkom združenia', 3, N'35', 132
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'36', N'Záväzky voči spoločníkom a združeniu', 2, N'3', 133
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'361', N'Záväzky voči prepojeným účtovným jednotkám a účtovným jednotkám v rámci podielovej účasti (zmena názvu účtu od 31.12.2015)', 3, N'36', 134
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'364', N'Záväzky voči spoločníkom a členom pri rozdeľovaní zisku', 3, N'36', 135
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'365', N'Ostatné záväzky voči spoločníkom a členom', 3, N'36', 136
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'366', N'Záväzky voči spoločníkom a členom zo závislej činnosti', 3, N'36', 137
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'367', N'Záväzky z upísaných nesplatených cenných papierov a vkladov', 3, N'36', 138
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'368', N'Záväzky voči účastníkom združenia', 3, N'36', 139
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'37', N'Iné pohľadávky a iné záväzky', 2, N'3', 140
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'371', N'Pohľadávky z predaja podniku', 3, N'37', 141
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'372', N'Záväzky z kúpy podniku', 3, N'37', 142
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'373', N'Pohľadávky a záväzky z pevných termínových operácií', 3, N'37', 143
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'374', N'Pohľadávky z nájmu', 3, N'37', 144
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'375', N'Pohľadávky z vydaných dlhopisov', 3, N'37', 145
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'376', N'Nakúpené opcie', 3, N'37', 146
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'377', N'Predané opcie', 3, N'37', 147
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'378', N'Iné pohľadávky', 3, N'37', 148
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'379', N'Iné záväzky', 3, N'37', 149
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'38', N'Časové rozlíšenie nákladov a výnosov', 2, N'3', 150
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'381', N'Náklady budúcich období', 3, N'38', 151
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'382', N'Komplexné náklady budúcich období', 3, N'38', 152
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'383', N'Výdavky budúcich období', 3, N'38', 153
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'384', N'Výnosy budúcich období', 3, N'38', 154
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'385', N'Príjmy budúcich období', 3, N'38', 155
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'39', N'Opravná položka k zúčtovacím vzťahom a vnútorné zúčtovanie', 2, N'3', 156
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'391', N'Opravné položky k pohľadávkam', 3, N'39', 157
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'395', N'Vnútorné zúčtovanie', 3, N'39', 158
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'398', N'Spojovací účet pri združení', 3, N'39', 159
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'4', N'Kapitálové účty a dlhodobé záväzky', 1, NULL, 160
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'41', N'Základné imanie a kapitálové fondy', 2, N'4', 161
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'411', N'Základné imanie', 3, N'41', 162
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'412', N'Emisné ážio', 3, N'41', 163
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'413', N'Ostatné kapitálové fondy', 3, N'41', 164
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'414', N'Oceňovacie rozdiely z precenenia majetku a záväzkov', 3, N'41', 165
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'415', N'Oceňovacie rozdiely z kapitálových účastín', 3, N'41', 166
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'416', N'Oceňovacie rozdiely z precenenia pri zlúčení, splynutí a rozdelení', 3, N'41', 167
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'417', N'Zákonný rezervný fond z kapitálových vkladov (účet doplnený od 1.1.2003)', 3, N'41', 168
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'418', N'Nedeliteľný fond z kapitálových vkladov (účet doplnený od 1.1.2003)', 3, N'41', 169
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'419', N'Zmeny základného imania', 3, N'41', 170
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'42', N'Fondy tvorené zo zisku a prevedené výsledky hospodárenia', 2, N'4', 171
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'421', N'Zákonný rezervný fond', 3, N'42', 172
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'422', N'Nedeliteľný fond', 3, N'42', 173
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'423', N'Štatutárne fondy', 3, N'42', 174
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'427', N'Ostatné fondy', 3, N'42', 175
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'428', N'Nerozdelený zisk minulých rokov', 3, N'42', 176
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'429', N'Neuhradená strata minulých rokov', 3, N'42', 177
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'43', N'Výsledok hospodárenia', 2, N'4', 178
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'431', N'Výsledok hospodárenia v schvaľovaní', 3, N'43', 179
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'45', N'Rezervy', 2, N'4', 180
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'451', N'Rezervy zákonné', 3, N'45', 181
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'459', N'Ostatné rezervy', 3, N'45', 182
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'46', N'Bankové úvery', 2, N'4', 183
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'461', N'Bankové úvery', 3, N'46', 184
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'47', N'Dlhodobé záväzky', 2, N'4', 185
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'471', N'Dlhodobé záväzky voči prepojeným účtovným jednotkám a účtovným jednotkám v rámci podielovej účasti (zmena názvu účtu od 31.12.2015)', 3, N'47', 186
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'472', N'Záväzky zo sociálneho fondu', 3, N'47', 187
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'473', N'Vydané dlhopisy', 3, N'47', 188
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'474', N'Záväzky z nájmu', 3, N'47', 189
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'475', N'Dlhodobé prijaté preddavky', 3, N'47', 190
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'476', N'Dlhodobé nevyfakturované dodávky', 3, N'47', 191
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'478', N'Dlhodobé zmenky na úhradu', 3, N'47', 192
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'479', N'Ostatné dlhodobé záväzky', 3, N'47', 193
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'48', N'Odložený daňový záväzok a odložená daňová pohľadávka', 2, N'4', 194
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'481', N'Odložený daňový záväzok a odložená daňová pohľadávka', 3, N'48', 195
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'49', N'Fyzická osoba — podnikateľ', 2, N'4', 196
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'491', N'Vlastné imanie fyzickej osoby — podnikateľa', 3, N'49', 197
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'5', N'Náklady', 1, NULL, 198
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'50', N'Spotrebované nákupy', 2, N'5', 199
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'501', N'Spotreba materiálu', 3, N'50', 200
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'502', N'Spotreba energie', 3, N'50', 201
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'503', N'Spotreba ostatných neskladovateľných dodávok', 3, N'50', 202
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'504', N'Predaný tovar', 3, N'50', 203
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'505', N'Tvorba a zúčtovanie opravných položiek k zásobám', 3, N'50', 204
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'507', N'Predaná nehnuteľnosť (účet doplnený od 1.1.2011)', 3, N'50', 205
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'51', N'Služby', 2, N'5', 206
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'511', N'Opravy a udržiavanie', 3, N'51', 207
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'512', N'Cestovné', 3, N'51', 208
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'513', N'Náklady na reprezentáciu', 3, N'51', 209
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'518', N'Ostatné služby', 3, N'51', 210
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'52', N'Osobné náklady', 2, N'5', 211
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'521', N'Mzdové náklady', 3, N'52', 212
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'522', N'Príjmy spoločníkov a členov zo závislej činnosti', 3, N'52', 213
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'523', N'Odmeny členom orgánov spoločnosti a družstva', 3, N'52', 214
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'524', N'Zákonné sociálne poistenie', 3, N'52', 215
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'525', N'Ostatné sociálne poistenie', 3, N'52', 216
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'526', N'Sociálne náklady fyzickej osoby — podnikateľa', 3, N'52', 217
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'527', N'Zákonné sociálne náklady', 3, N'52', 218
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'528', N'Ostatné sociálne náklady', 3, N'52', 219
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'53', N'Dane a poplatky', 2, N'5', 220
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'531', N'Daň z motorových vozidiel', 3, N'53', 221
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'532', N'Daň z nehnuteľnosti', 3, N'53', 222
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'538', N'Ostatné dane a poplatky', 3, N'53', 223
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'54', N'Iné náklady na hospodársku činnosť', 2, N'5', 224
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'541', N'Zostatková cena predaného dlhodobého nehmotného majetku a dlhodobého hmotného majetku', 3, N'54', 225
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'542', N'Predaný materiál', 3, N'54', 226
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'543', N'Dary', 3, N'54', 227
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'544', N'Zmluvné pokuty, penále a úroky z omeškania', 3, N'54', 228
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'545', N'Ostatné pokuty, penále a úroky z omeškania', 3, N'54', 229
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'546', N'Odpis pohľadávky', 3, N'54', 230
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'547', N'Tvorba a zúčtovanie opravných položiek k pohľadávkam', 3, N'54', 231
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'548', N'Ostatné náklady na hospodársku činnosť', 3, N'54', 232
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'549', N'Manká a škody', 3, N'54', 233
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'55', N'Odpisy a opravné položky k dlhodobému majetku', 2, N'5', 234
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'551', N'Odpisy dlhodobého nehmotného majetku a dlhodobého hmotného majetku', 3, N'55', 235
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'553', N'Tvorba a zúčtovanie opravných položiek k dlhodobému majetku', 3, N'55', 236
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'555', N'Zúčtovanie komplexných nákladov budúcich období', 3, N'55', 237
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'557', N'Zúčtovanie oprávky k opravnej položke k nadobudnutému majetku', 3, N'55', 238
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'56', N'Finančné náklady', 2, N'5', 239
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'561', N'Predané cenné papiere a podiely', 3, N'56', 240
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'562', N'Úroky', 3, N'56', 241
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'563', N'Kurzové straty', 3, N'56', 242
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'564', N'Náklady na precenenie cenných papierov', 3, N'56', 243
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'565', N'Tvorba a zúčtovanie opravných položiek k finančnému majetku', 3, N'56', 244
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'566', N'Náklady na krátkodobý finančný majetok', 3, N'56', 245
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'567', N'Náklady na derivátové operácie', 3, N'56', 246
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'568', N'Ostatné finančné náklady', 3, N'56', 247
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'569', N'Manká a škody na finančnom majetku', 3, N'56', 248
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'59', N'Dane z príjmov a prevodové účty', 2, N'5', 249
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'591', N'Splatná daň z príjmov (zmena názvu účtu od 31.12.2014)', 3, N'59', 250
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'592', N'Odložená daň z príjmov (zmena názvu účtu od 31.12.2014)', 3, N'59', 251
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'595', N'Dodatočné odvody dane z príjmov', 3, N'59', 252
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'596', N'Prevod podielov na výsledku hospodárenia spoločníkom', 3, N'59', 253
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'6', N'Výnosy', 1, NULL, 254
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'60', N'Tržby za vlastné výkony a tovar', 2, N'6', 255
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'601', N'Tržby za vlastné výrobky', 3, N'60', 256
)
INSERT INTO [Accounts].[OfficialAccounts]
(
    [AccountFrameworkVersionId], [AccountCode], [AccountName_sk], [AccountLevel], [ParentAccountCode], [SortOrder]
)
SELECT
    afv.[Id], n.[AccountCode], n.[AccountName_sk], n.[AccountLevel], n.[ParentAccountCode], n.[SortOrder]
FROM [NewData] AS n
INNER JOIN [Accounts].[AccountFramework] AS af ON af.[Code] = n.[FrameworkCode]
INNER JOIN [Accounts].[AccountFrameworkVersion] AS afv ON afv.[AccountFrameworkId] = af.[Id] AND afv.[VersionCode] = n.[VersionCode]
LEFT JOIN [Accounts].[OfficialAccounts] AS e ON e.[AccountFrameworkVersionId] = afv.[Id] AND e.[AccountCode] = n.[AccountCode]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT N'PROFIT' AS [FrameworkCode], N'2022-01-01' AS [VersionCode], N'602' AS [AccountCode], N'Tržby z predaja služieb' AS [AccountName_sk], 3 AS [AccountLevel], N'60' AS [ParentAccountCode], 257 AS [SortOrder]
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'604', N'Tržby za tovar', 3, N'60', 258
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'606', N'Výnosy zo zákazky (doplnený účet od 1.1.2011)', 3, N'60', 259
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'607', N'Výnosy z nehnuteľnosti na predaj (doplnený účet od 1.1.2011)', 3, N'60', 260
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'61', N'Zmeny stavu vnútroorganizačných zásob', 2, N'6', 261
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'611', N'Zmena stavu nedokončenej výroby', 3, N'61', 262
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'612', N'Zmena stavu polotovarov', 3, N'61', 263
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'613', N'Zmena stavu výrobkov', 3, N'61', 264
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'614', N'Zmena stavu zvierat', 3, N'61', 265
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'62', N'Aktivácia', 2, N'6', 266
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'621', N'Aktivácia materiálu a tovaru', 3, N'62', 267
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'622', N'Aktivácia vnútroorganizačných služieb', 3, N'62', 268
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'623', N'Aktivácia dlhodobého nehmotného majetku', 3, N'62', 269
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'624', N'Aktivácia dlhodobého hmotného majetku', 3, N'62', 270
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'64', N'Iné výnosy z hospodárskej činnosti', 2, N'6', 271
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'641', N'Tržby z predaja dlhodobého nehmotného majetku a dlhodobého hmotného majetku', 3, N'64', 272
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'642', N'Tržby z predaja materiálu', 3, N'64', 273
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'644', N'Zmluvné pokuty, penále a úroky z omeškania', 3, N'64', 274
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'645', N'Ostatné pokuty, penále a úroky z omeškania', 3, N'64', 275
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'646', N'Výnosy z odpísaných pohľadávok', 3, N'64', 276
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'648', N'Ostatné výnosy z hospodárskej činnosti', 3, N'64', 277
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'65', N'Zúčtovanie niektorých položiek z hospodárskej činnosti', 2, N'6', 278
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'653', N'Zúčtovanie komplexných nákladov budúcich období', 3, N'65', 279
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'657', N'Zúčtovanie oprávky k opravnej položke k nadobudnutému majetku', 3, N'65', 280
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'66', N'Finančné výnosy', 2, N'6', 281
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'661', N'Tržby z predaja cenných papierov a podielov', 3, N'66', 282
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'662', N'Úroky', 3, N'66', 283
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'663', N'Kurzové zisky', 3, N'66', 284
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'664', N'Výnosy z precenenia cenných papierov', 3, N'66', 285
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'665', N'Výnosy z dlhodobého finančného majetku', 3, N'66', 286
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'666', N'Výnosy z krátkodobého finančného majetku', 3, N'66', 287
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'667', N'Výnosy z derivátových operácií', 3, N'66', 288
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'668', N'Ostatné finančné výnosy', 3, N'66', 289
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'7', N'Uzávierkové účty a podsúvahové účty', 1, NULL, 290
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'70', N'Súvahové uzávierkové účty', 2, N'7', 291
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'701', N'Začiatočný účet súvahový', 3, N'70', 292
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'702', N'Konečný účet súvahový', 3, N'70', 293
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'71', N'Výsledkový uzávierkový účet', 2, N'7', 294
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'710', N'Účet ziskov a strát', 3, N'71', 295
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'711', N'Začiatočný účet nákladov a výnosov', 3, N'71', 296
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'75', N' Podsúvahové účty', 2, N'7', 297
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'76', N' Podsúvahové účty', 2, N'7', 298
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'77', N' Podsúvahové účty', 2, N'7', 299
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'78', N' Podsúvahové účty', 2, N'7', 300
    UNION ALL SELECT N'PROFIT', N'2022-01-01', N'79', N' Podsúvahové účty', 2, N'7', 301
)
INSERT INTO [Accounts].[OfficialAccounts]
(
    [AccountFrameworkVersionId], [AccountCode], [AccountName_sk], [AccountLevel], [ParentAccountCode], [SortOrder]
)
SELECT
    afv.[Id], n.[AccountCode], n.[AccountName_sk], n.[AccountLevel], n.[ParentAccountCode], n.[SortOrder]
FROM [NewData] AS n
INNER JOIN [Accounts].[AccountFramework] AS af ON af.[Code] = n.[FrameworkCode]
INNER JOIN [Accounts].[AccountFrameworkVersion] AS afv ON afv.[AccountFrameworkId] = af.[Id] AND afv.[VersionCode] = n.[VersionCode]
LEFT JOIN [Accounts].[OfficialAccounts] AS e ON e.[AccountFrameworkVersionId] = afv.[Id] AND e.[AccountCode] = n.[AccountCode]
WHERE e.[Id] IS NULL;

COMMIT TRANSACTION;
PRINT '020 official-account population completed.';
GO
