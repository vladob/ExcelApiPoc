/* RegisterUZ template catalog and table structure. */
USE [AuditAddIn];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

;WITH [NewData] AS
(
    SELECT 1 AS [ErpId], N'Výkaz ziskov a strát Úč ROPO SFOV 2-01' AS [Name_sk], NULL AS [Name_en], N'MF/25755/2007-31' AS [MfSpecification], CONVERT(date, '2009-01-01') AS [ValidFrom], NULL AS [ValidTo]
    UNION ALL SELECT 2, N'Súvaha Úč ROPO SFOV 1-01', NULL, N'MF/24241/2009-31', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 3, N'Súvaha Úč FNM SR 1-01', NULL, N'MF/23960/2008-74', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 4, N'Úč FNM SR 1-01', NULL, N'MF/23960/2008-74', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 5, N'Úč FNM SR 1-01', NULL, N'MF/24037/2011-74', CONVERT(date, '2011-12-31'), NULL
    UNION ALL SELECT 6, N'Úč FNM SR 2-01', NULL, N'MF/23960/2008-74', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 7, N'Úč FO 1-01', NULL, N'MF/27076/1/2007-74/1', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 8, N'Úč FO 2-01', NULL, N'MF/27076/2/2007-74/1', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 9, N'Kons uj Úč RO 1-01', NULL, N'MF/22110/2009-31', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 10, N'Kons uj Úč RO 2-01', NULL, N'MF/22110/2009-31', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 11, N'Kons uj Úč ROPO OV 1-01', NULL, N'MF/22110/2009-31', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 12, N'Kons uj Úč ROPO OV 2-01', NULL, N'MF/22110/2009-31', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 13, N'Výkaz NO Uč. 1-01', NULL, N'MF/24764/2007-74', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 14, N'Výkaz Uč NO 1-01', NULL, N'MF/24975/2010-74', CONVERT(date, '2011-01-01'), NULL
    UNION ALL SELECT 15, N'Výkaz NO Uč. 2-01', NULL, N'MF/24764/2007-74', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 16, N'Výkaz Uč NO 2-01', NULL, N'MF/24975/2010-74', CONVERT(date, '2011-01-01'), NULL
    UNION ALL SELECT 17, N'Súvaha Úč NUJ 1-01', NULL, N'MF/25682/2007-74', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 18, N'Výsledovka Úč NUJ 2-01', NULL, N'MF/25682/2007-74', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 19, N'Výsledovka Úč NUJ 2-01', NULL, N'MF/25238/2009-74', CONVERT(date, '2010-01-01'), NULL
    UNION ALL SELECT 20, N'Súvaha Úč POD 1-01', NULL, N'MF/24219/1/2008', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 21, N'Súvaha Úč POD 1-01', NULL, N'MF/25947/1/2010', CONVERT(date, '2011-01-01'), NULL
    UNION ALL SELECT 22, N'Výkaz ziskov a strát Úč POD 2-01', NULL, N'MF/24219/3/2008', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 27, N'Výkaz ziskov a strát Úč POI 4-01', NULL, N'MF/24443/2008-74', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 29, N'Súvaha Úč SP 1-01', NULL, N'MF/26940/2005-92', CONVERT(date, '2006-01-01'), NULL
    UNION ALL SELECT 30, N'Výsledovka Úč SP 2-01', NULL, N'MF/24641/2007-74', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 61, N'Úč FO 1-01', NULL, N'MF/26567/2011-74/3/2012', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 62, N'Úč FO 1-01', NULL, N'MF/26567/2011-74/1/2013', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 81, N'Úč FO 2-01', NULL, N'MF/26567/2011-74/4/2012', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 82, N'Úč FO 2-01', NULL, N'MF/26567/2011-74/2/2013', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 301, N'Konsolidovaná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 303, N'Ročná finančná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 341, N'Účtovná závierka', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 342, N'Individuálna výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 364, N'Oznámenie o schválení účtovnej závierky', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 365, N'Správa audítora', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 369, N'Oznámenie o schválení účtovnej závierky', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 383, N'Úč NO', NULL, N'MF/17695/2013-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 384, N'Správa audítora', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 385, N'Úč NUJ', NULL, N'MF/17616/2013-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 386, N'Oznámenie o schválení účtovnej závierky', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 387, N'Správa audítora', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 391, N'Oznámenie o schválení účtovnej závierky', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 392, N'Správa audítora', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 393, N'Oznámenie o schválení účtovnej závierky', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 394, N'Správa audítora', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 395, N'Poznámky Úč POD 3-04', NULL, N'MF/24013/2011-74', CONVERT(date, '2010-01-01'), NULL
    UNION ALL SELECT 396, N'Oznámenie o schválení účtovnej závierky', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 397, N'Správa audítora', NULL, N'2013', CONVERT(date, '2010-01-01'), NULL
    UNION ALL SELECT 401, N'Oznámenie o schválení účtovnej závierky', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 402, N'Správa audítora', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 404, N'Výkaz vybraných údajov VÚ-B kons. 1-01', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 405, N'Výkaz vybraných údajov', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 407, N'Výkaz vybraných údajov VÚ-P kons. 1-01', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 409, N'Výkaz vybraných údajov VÚ POD kons. 1-01', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 413, N'Oznámenie o schválení účtovnej závierky', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 414, N'Správa audítora', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 421, N'Správa audítora', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 422, N'Súvaha Úč Fondy 1-02', NULL, N'MF/23778/2012_74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 423, N'VZS Úč Fondy 2-02', NULL, N'MF/23778/2012_74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 424, N'Poznámky Úč Fondy 3-02', NULL, N'MF/23778/2012_74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 425, N'Súvaha Úč Fondy nehnutelností 1-02', NULL, N'MF/23778/2012_74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 426, N'VZS Úč Fondy nehnutelností 2-02', NULL, N'MF/23778/2012_74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 427, N'Poznámky Úč Fondy nehnutelností 3-02', NULL, N'MF/23778/2012_74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 428, N'Súvaha Úč PI & IEP 1-04', NULL, N'MF/23781/2012-74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 429, N'VZS Úč PI & IEP 2-04', NULL, N'MF/23781/2012-74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 430, N'Poznámky Úč PI & IEP 3-04', NULL, N'MF/23781/2012-74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 431, N'Súvaha Úč OCP 1-04', NULL, N'MF/23779/2012-74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 432, N'VZS Úč OCP 2-04', NULL, N'MF/23779/2012-74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 433, N'Poznámky Úč OCP 3-04', NULL, N'MF/23779/2012-74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 434, N'Súvaha Úč PZFI 1-04', NULL, N'MF/23781/2012-74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 435, N'VZS Úč PZFI 2-04', NULL, N'MF/23781/2012-74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 436, N'Poznámky Úč PZFI 3-04', NULL, N'MF/23781/2012-74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 437, N'Súvaha Úč ZF 1-01', NULL, N'MF/23781/2012-74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 438, N'VZS Úč ZF 2-01', NULL, N'MF/23781/2012-74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 439, N'Poznámky Úč ZF 3-01', NULL, N'MF/23781/2012-74', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 441, N'IFRS účtovná závierka', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 442, N'Oznámenie o schválení účtovnej závierky', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 443, N'Poznámky', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 444, N'Správa audítora', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 445, N'IFRS účtovná závierka', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 446, N'Oznámenie o schválení účtovnej závierky', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 447, N'Poznámky', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 448, N'Správa audítora', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 482, N'Správa audítora', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 483, N'Poznámky', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 521, N'Výkaz ziskov a strát Úč ROPO SFOV 2-01', NULL, N'MF/19301/2012-31', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 522, N'Súvaha Úč ROPO SFOV 1-01', NULL, N'MF/19301/2012-31', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 541, N'Úč POI 3-01', NULL, N'MF/24443/2008-74', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 542, N'Úč POI 3-01', NULL, N'MF/19523/2009-74', CONVERT(date, '2010-01-01'), NULL
    UNION ALL SELECT 561, N'Oznámenie o schválení účtovnej závierky', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 661, N'Výkaz vybraných údajov', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 662, N'Výkaz vybraných údajov', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 663, N'Výkaz vybraných údajov', NULL, N'2013', CONVERT(date, '2012-01-01'), NULL
    UNION ALL SELECT 681, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 682, N'Výkaz vybraných údajov', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 683, N'IFRS účtovná závierka', NULL, N'MF/16232I2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 684, N'Kons S UJ VS Úč 1-01', NULL, N'MF/21230/2014-31', CONVERT(date, '2014-01-01'), NULL
    UNION ALL SELECT 685, N'Úč PI & IEP', NULL, N'MF/16232I2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 686, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 687, N'Úč MUJ', NULL, N'MF/18008/2014', CONVERT(date, '2014-01-01'), NULL
    UNION ALL SELECT 688, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 689, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 690, N'Súvaha Úč ROPO SFOV 1-01', NULL, N'MF/21227/2014-31', CONVERT(date, '2014-01-01'), NULL
    UNION ALL SELECT 691, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 692, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 693, N'Úč OCP', NULL, N'MF/016228/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 694, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 695, N'IFRS účtovná závierka', NULL, N'MF/18007/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 696, N'Kons VZaS UJ VS Úč 2-01', NULL, N'MF/21230/2014-31', CONVERT(date, '2014-01-01'), NULL
    UNION ALL SELECT 697, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 698, N'Výkaz vybraných údajov VÚ POD kons. 1-01', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 699, N'Úč POD', NULL, N'MF/18009/2014-74', CONVERT(date, '2014-01-01'), NULL
    UNION ALL SELECT 700, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 701, N'Úč Fondy', NULL, N'MF/18007/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 702, N'Úč FONDYNEH', NULL, N'MF/18007/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 703, N'IFRS účtovná závierka', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 704, N'IFRS účtovná závierka', NULL, N'MF/016228/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 705, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 706, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 707, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 708, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 709, N'IFRS účtovná závierka', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 710, N'Výkaz vybraných údajov VÚ-B kons. 1-01', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 711, N'Výkaz vybraných údajov', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 712, N'Výkaz vybraných údajov', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 713, N'Výkaz vybraných údajov', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 714, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 715, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 716, N'Úč FO', NULL, N'MF/15523/2014-74', CONVERT(date, '2014-01-01'), NULL
    UNION ALL SELECT 717, N'IFRS účtovná závierka', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 718, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 719, N'Výkaz vybraných údajov', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 720, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 721, N'Výkaz vybraných údajov', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 722, N'Výkaz vybraných údajov VÚ-P kons. 1-01', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 723, N'Úč ZFOND', NULL, N'MF/16232/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 724, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 725, N'Titulná strana', NULL, N'2013', CONVERT(date, '2014-01-01'), NULL
    UNION ALL SELECT 726, N'Úč SKISPS', NULL, N'MF/18007/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 727, N'Výkaz ziskov a strát Úč ROPO SFOV 2-01', NULL, N'MF/21227/2014-31', CONVERT(date, '2014-01-01'), NULL
    UNION ALL SELECT 728, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 729, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 730, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 731, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 732, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 733, N'Výkaz vybraných údajov VÚ POD 1-01', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 734, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 735, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 736, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 737, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 738, N'Výkaz vybraných údajov VÚ-P 1-04', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 739, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 740, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 741, N'Úč PZFI', NULL, N'MF/16232/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 742, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 743, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 744, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 745, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/22497/2014-74', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 801, N'Účtovná závierka zdravotnej poisťovne', NULL, N'MF/19553/2014-74', CONVERT(date, '2014-01-01'), NULL
    UNION ALL SELECT 841, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 902, N'Súhrnná výročná správa', NULL, N'2015', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 923, N'Správa audítora', NULL, N'2014', CONVERT(date, '2013-01-01'), NULL
    UNION ALL SELECT 941, N'Výkaz vybraných údajov VÚ-B 1-01', NULL, N'2015', CONVERT(date, '2015-12-31'), NULL
    UNION ALL SELECT 942, N'Výkaz vybraných údajov VÚ-P 1-04', NULL, N'2015', CONVERT(date, '2015-12-31'), NULL
    UNION ALL SELECT 943, N'Výkaz vybraných údajov VÚ POD 1-01', NULL, N'2015', CONVERT(date, '2015-12-31'), NULL
    UNION ALL SELECT 1001, N'Účtovná závierka zdravotnej poisťovne', NULL, N'MF/14950/2015-74', CONVERT(date, '2015-01-01'), NULL
    UNION ALL SELECT 1021, N'Úč Exportno-importnej banky SR', NULL, N'MF/22164/2014-74', CONVERT(date, '2014-01-01'), NULL
    UNION ALL SELECT 1061, N'Individuálna výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1062, N'Individuálna výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1063, N'Individuálna výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1064, N'Ročná finančná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1065, N'Individuálna výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1066, N'Individuálna výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1067, N'Konsolidovaná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1068, N'Konsolidovaná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1069, N'Ročná finančná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1070, N'Individuálna výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1071, N'Individuálna výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1072, N'Ročná finančná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1073, N'Konsolidovaná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1074, N'Konsolidovaná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1075, N'Ročná finančná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1076, N'Ročná finančná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1077, N'Individuálna výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1078, N'Ročná finančná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1079, N'Ročná finančná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1080, N'Individuálna výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1081, N'Individuálna výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1082, N'Ročná finančná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1083, N'Individuálna výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1084, N'Individuálna výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1085, N'Ročná finančná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1086, N'Ročná finančná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1087, N'Ročná finančná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1088, N'Konsolidovaná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1089, N'Ročná finančná výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1090, N'Individuálna výročná správa', NULL, N'2013', CONVERT(date, '2009-01-01'), NULL
    UNION ALL SELECT 1101, N'Účtovná závierka zdravotnej poisťovne', NULL, N'MF/14950/2015-74/2', CONVERT(date, '2016-01-01'), NULL
    UNION ALL SELECT 1121, N'Výkaz vybraných údajov VÚ-B 1-01', NULL, N'2019', CONVERT(date, '2019-12-31'), NULL
    UNION ALL SELECT 1141, N'Súvaha Úč SP 1-01', NULL, N'MF/007222/2020-74', CONVERT(date, '2020-05-01'), NULL
    UNION ALL SELECT 1142, N'Výkaz ziskov a strát Úč SP 2-01', NULL, N'MF/007222/2020-74', CONVERT(date, '2020-05-01'), NULL
    UNION ALL SELECT 1161, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1162, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1163, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1164, N'Úč NO', NULL, N'MF/011077/2021-74', CONVERT(date, '2022-01-01'), NULL
    UNION ALL SELECT 1165, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1166, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1167, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1168, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1169, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1170, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1171, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1172, N'Správa audítora', NULL, N'2021', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1173, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1174, N'Individuálna výročná správa', NULL, N'2021', CONVERT(date, '2009-01-02'), NULL
    UNION ALL SELECT 1175, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1176, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1177, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1178, N'Správa audítora', NULL, N'2021', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1179, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1180, N'Úč NUJ', NULL, N'MF/011079/2021-74', CONVERT(date, '2022-01-01'), NULL
    UNION ALL SELECT 1181, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1182, N'Oznámenie o dátume schválenia účtovnej závierky', NULL, N'MF/011080/2021-74', CONVERT(date, '2013-01-02'), NULL
    UNION ALL SELECT 1183, N'Individuálna výročná správa', NULL, N'2021', CONVERT(date, '2009-01-02'), NULL
    UNION ALL SELECT 5181, N'Výkaz vybraných údajov VÚ-B 1-01', NULL, N'2023', CONVERT(date, '2023-12-31'), NULL
    UNION ALL SELECT 5182, N'Výkaz vybraných údajov VÚ-P 1-04', NULL, N'2023', CONVERT(date, '2023-12-31'), NULL
    UNION ALL SELECT 5183, N'Výkaz vybraných údajov VÚ POD 1-01', NULL, N'2023', CONVERT(date, '2023-01-01'), NULL
    UNION ALL SELECT 5184, N'Účtovná závierka zdravotnej poisťovne', NULL, N'MF/013185/2022-74', CONVERT(date, '2023-12-31'), NULL
    UNION ALL SELECT 11181, N'Správa o uistení v oblasti vykazovania informácií o udržateľnosti', NULL, N'105/2024 Z. z.', CONVERT(date, '2024-01-01'), NULL
    UNION ALL SELECT 11182, N'Správa o uistení v oblasti vykazovania informácií o udržateľnosti', NULL, N'105/2024 Z. z.', CONVERT(date, '2024-01-01'), NULL
    UNION ALL SELECT 11183, N'Správa o uistení v oblasti vykazovania informácií o udržateľnosti', NULL, N'105/2024 Z. z.', CONVERT(date, '2024-01-01'), NULL
    UNION ALL SELECT 11184, N'Správa o uistení v oblasti vykazovania informácií o udržateľnosti', NULL, N'105/2024 Z. z.', CONVERT(date, '2024-01-01'), NULL
    UNION ALL SELECT 11185, N'Správa o uistení v oblasti vykazovania informácií o udržateľnosti', NULL, N'105/2024 Z. z.', CONVERT(date, '2024-01-01'), NULL
    UNION ALL SELECT 11186, N'Správa o udržateľnosti', NULL, N'105/2024 Z. z.', CONVERT(date, '2024-01-01'), NULL
    UNION ALL SELECT 11187, N'Správa o udržateľnosti', NULL, N'105/2024 Z. z.', CONVERT(date, '2024-01-01'), NULL
    UNION ALL SELECT 11188, N'Dokument s názorom týkajúcim sa uistenia v oblasti vykazovania informácií o udržateľnosti', NULL, N'105/2024 Z. z.', CONVERT(date, '2024-01-01'), NULL
    UNION ALL SELECT 11189, N'Dokument s názorom týkajúcim sa uistenia v oblasti vykazovania informácií o udržateľnosti', NULL, N'105/2024 Z. z.', CONVERT(date, '2024-01-01'), NULL
    UNION ALL SELECT 11190, N'Dokument s názorom týkajúcim sa uistenia v oblasti konsolidovaného vykazovania informácií o udržateľnosti', NULL, N'105/2024 Z. z.', CONVERT(date, '2024-01-01'), NULL
    UNION ALL SELECT 11191, N'Dokument s názorom týkajúcim sa uistenia v oblasti konsolidovaného vykazovania informácií o udržateľnosti', NULL, N'105/2024 Z. z.', CONVERT(date, '2024-01-01'), NULL
    UNION ALL SELECT 11192, N'Konsolidované vykazovanie informácií o udržateľnosti materskej UJ so sídlom v tretích krajinách', NULL, N'105/2024 Z. z.', CONVERT(date, '2024-01-01'), NULL
    UNION ALL SELECT 11193, N'Konsolidované vykazovanie informácií o udržateľnosti materskej UJ so sídlom v tretích krajinách', NULL, N'105/2024 Z. z.', CONVERT(date, '2024-01-01'), NULL
    UNION ALL SELECT 13181, N'Správa s informáciami o dani z príjmov ', NULL, N'407/2022 Z. z.', CONVERT(date, '2024-06-01'), NULL
    UNION ALL SELECT 13182, N'Správa s informáciami o dani z príjmov ', NULL, N'407/2022 Z. z.', CONVERT(date, '2024-06-01'), NULL
    UNION ALL SELECT 15181, N'Výkaz pre mimovládne neziskové organizácie', NULL, N'MF/008470/2025-74', CONVERT(date, '2025-12-31'), NULL
    UNION ALL SELECT 15182, N'Výkaz pre mimovládne neziskové organizácie', NULL, N'MF/008470/2025-74', CONVERT(date, '2025-12-31'), NULL
)
INSERT INTO [Template].[Templates]
(
    [ErpId], [Name_sk], [Name_en], [MfSpecification], [ValidFrom], [ValidTo]
)
SELECT
    n.[ErpId], n.[Name_sk], n.[Name_en], n.[MfSpecification], n.[ValidFrom], n.[ValidTo]
FROM [NewData] AS n
LEFT JOIN [Template].[Templates] AS e ON e.[ErpId] = n.[ErpId]
WHERE e.[Id] IS NULL;

;WITH [NewData] AS
(
    SELECT 101 AS [TableErpId], 1 AS [TemplateErpId], N'Náklady' AS [Name_sk], NULL AS [Name_en], 7 AS [NumberOfColumns], 4 AS [NumberOfDataColumns], 0 AS [DontHaveRowNumbers], 0 AS [TableOrdinal]
    UNION ALL SELECT 102, 1, N'Výnosy', NULL, 7, 4, 0, 1
    UNION ALL SELECT 201, 2, N'Strana aktív', NULL, 7, 4, 0, 0
    UNION ALL SELECT 202, 2, N'Strana pasív', NULL, 5, 2, 0, 1
    UNION ALL SELECT 301, 3, N'Strana aktív', NULL, 7, 4, 0, 0
    UNION ALL SELECT 302, 3, N'Strana pasív', NULL, 5, 2, 0, 1
    UNION ALL SELECT 401, 4, N'Položky', NULL, 4, 2, 1, 0
    UNION ALL SELECT 501, 5, N'Položky', NULL, 4, 2, 1, 0
    UNION ALL SELECT 601, 6, N'Položky', NULL, 5, 3, 1, 0
    UNION ALL SELECT 701, 7, N'Príjmy', NULL, 3, 1, 0, 0
    UNION ALL SELECT 702, 7, N'Výdavky', NULL, 3, 1, 0, 1
    UNION ALL SELECT 801, 8, N'Majetok', NULL, 4, 2, 0, 0
    UNION ALL SELECT 802, 8, N'Záväzky', NULL, 4, 2, 0, 1
    UNION ALL SELECT 901, 9, N'Strana aktív', NULL, 5, 2, 0, 0
    UNION ALL SELECT 902, 9, N'Strana pasív', NULL, 5, 2, 0, 1
    UNION ALL SELECT 1001, 10, N'Náklady', NULL, 7, 4, 0, 0
    UNION ALL SELECT 1002, 10, N'Výnosy', NULL, 7, 4, 0, 1
    UNION ALL SELECT 1101, 11, N'Strana aktív', NULL, 5, 2, 0, 0
    UNION ALL SELECT 1102, 11, N'Strana pasív', NULL, 5, 2, 0, 1
    UNION ALL SELECT 1201, 12, N'Náklady', NULL, 7, 4, 0, 0
    UNION ALL SELECT 1202, 12, N'Výnosy', NULL, 7, 4, 0, 1
    UNION ALL SELECT 1301, 13, N'Príjmy', NULL, 4, 2, 0, 0
    UNION ALL SELECT 1302, 13, N'Výdavky', NULL, 4, 2, 0, 1
    UNION ALL SELECT 1401, 14, N'Príjmy', NULL, 4, 2, 0, 0
    UNION ALL SELECT 1402, 14, N'Výdavky', NULL, 4, 2, 0, 1
    UNION ALL SELECT 1501, 15, N'Majetok', NULL, 4, 2, 0, 0
    UNION ALL SELECT 1502, 15, N'Záväzky', NULL, 4, 2, 0, 1
    UNION ALL SELECT 1601, 16, N'Majetok', NULL, 4, 2, 0, 0
    UNION ALL SELECT 1602, 16, N'Záväzky', NULL, 4, 2, 0, 1
    UNION ALL SELECT 1701, 17, N'Strana aktív', NULL, 7, 4, 0, 0
    UNION ALL SELECT 1702, 17, N'Strana pasív', NULL, 5, 2, 0, 1
    UNION ALL SELECT 1801, 18, N'Náklady', NULL, 7, 4, 0, 0
    UNION ALL SELECT 1802, 18, N'Výnosy', NULL, 7, 4, 0, 1
    UNION ALL SELECT 1901, 19, N'Náklady', NULL, 7, 4, 0, 0
    UNION ALL SELECT 1902, 19, N'Výnosy', NULL, 7, 4, 0, 1
    UNION ALL SELECT 2001, 20, N'Strana aktív', NULL, 7, 4, 0, 0
    UNION ALL SELECT 2002, 20, N'Strana pasív', NULL, 5, 2, 0, 1
    UNION ALL SELECT 2101, 21, N'Strana aktív', NULL, 7, 4, 0, 0
    UNION ALL SELECT 2102, 21, N'Strana pasív', NULL, 5, 2, 0, 1
    UNION ALL SELECT 2201, 22, N'Výkaz ziskov a strát', NULL, 5, 2, 0, 0
    UNION ALL SELECT 2701, 27, N'Výkaz ziskov a strát', NULL, 7, 4, 0, 0
    UNION ALL SELECT 2901, 29, N'Aktíva', NULL, 7, 4, 0, 0
    UNION ALL SELECT 2902, 29, N'Pasíva', NULL, 5, 2, 0, 1
    UNION ALL SELECT 3001, 30, N'Výkaz ziskov a strát', NULL, 7, 4, 0, 0
    UNION ALL SELECT 6101, 61, N'Príjmy', NULL, 3, 1, 0, 0
    UNION ALL SELECT 6102, 61, N'Výdavky', NULL, 3, 1, 0, 1
    UNION ALL SELECT 6201, 62, N'Príjmy', NULL, 3, 1, 0, 0
    UNION ALL SELECT 6202, 62, N'Výdavky', NULL, 3, 1, 0, 1
    UNION ALL SELECT 8101, 81, N'Majetok', NULL, 4, 2, 0, 0
    UNION ALL SELECT 8102, 81, N'Záväzky', NULL, 4, 2, 0, 1
    UNION ALL SELECT 8201, 82, N'Majetok', NULL, 4, 2, 0, 0
    UNION ALL SELECT 8202, 82, N'Záväzky', NULL, 4, 2, 0, 1
    UNION ALL SELECT 38301, 383, N'Príjmy', NULL, 4, 2, 0, 0
    UNION ALL SELECT 38302, 383, N'Výdavky', NULL, 4, 2, 0, 1
    UNION ALL SELECT 38303, 383, N'Majetok', NULL, 4, 2, 0, 2
    UNION ALL SELECT 38304, 383, N'Záväzky', NULL, 4, 2, 0, 3
    UNION ALL SELECT 38501, 385, N'Strana aktív', NULL, 7, 4, 0, 0
    UNION ALL SELECT 38502, 385, N'Strana pasív', NULL, 5, 2, 0, 1
    UNION ALL SELECT 38503, 385, N'Náklady', NULL, 7, 4, 0, 2
    UNION ALL SELECT 38504, 385, N'Výnosy', NULL, 7, 4, 0, 3
    UNION ALL SELECT 52101, 521, N'Náklady', NULL, 7, 4, 0, 0
    UNION ALL SELECT 52102, 521, N'Výnosy', NULL, 7, 4, 0, 1
    UNION ALL SELECT 52201, 522, N'Strana aktív', NULL, 7, 4, 0, 0
    UNION ALL SELECT 52202, 522, N'Strana pasív', NULL, 5, 2, 0, 1
    UNION ALL SELECT 54101, 541, N'A K T Í V A', NULL, 7, 4, 0, 0
    UNION ALL SELECT 54102, 541, N'P A S Í V A', NULL, 5, 2, 0, 1
    UNION ALL SELECT 54201, 542, N'A K T Í V A', NULL, 7, 4, 0, 0
    UNION ALL SELECT 54202, 542, N'P A S Í V A', NULL, 5, 2, 0, 1
    UNION ALL SELECT 66101, 661, N'Časť I.: Tabuľka č. 1', NULL, 5, 2, 0, 0
    UNION ALL SELECT 66102, 661, N'Časť II.: Tabuľka č. 1', NULL, 4, 2, 0, 1
    UNION ALL SELECT 66201, 662, N'AKTÍVA', NULL, 6, 4, 0, 0
    UNION ALL SELECT 66202, 662, N'PASÍVA', NULL, 4, 2, 0, 1
    UNION ALL SELECT 66203, 662, N'NÁKLADY A VÝNOSY', NULL, 6, 4, 0, 2
    UNION ALL SELECT 66301, 663, N'AKTÍVA', NULL, 4, 2, 1, 0
    UNION ALL SELECT 66302, 663, N'PASÍVA', NULL, 4, 2, 1, 1
    UNION ALL SELECT 66303, 663, N'Vybrané údaje z výnosov a nákladov', NULL, 4, 2, 1, 2
    UNION ALL SELECT 68401, 684, N'Strana aktív', N'Assets', 5, 2, 0, 0
    UNION ALL SELECT 68402, 684, N'Strana pasív', N'Liabilities and Equity', 5, 2, 0, 1
    UNION ALL SELECT 68701, 687, N'Strana aktív', NULL, 5, 2, 0, 0
    UNION ALL SELECT 68702, 687, N'Strana pasív', NULL, 5, 2, 0, 1
    UNION ALL SELECT 68703, 687, N'Výkaz ziskov a strát', NULL, 5, 2, 0, 2
    UNION ALL SELECT 69001, 690, N'Strana aktív', N'Assets', 7, 4, 0, 0
    UNION ALL SELECT 69002, 690, N'Strana pasív', N'Liabilities and Equity', 5, 2, 0, 1
    UNION ALL SELECT 69601, 696, N'Náklady', N'Expences', 7, 4, 0, 0
    UNION ALL SELECT 69602, 696, N'Výnosy', N'Revenues', 7, 4, 0, 1
    UNION ALL SELECT 69901, 699, N'Strana aktív', N'Assets', 7, 4, 0, 0
    UNION ALL SELECT 69902, 699, N'Strana pasív', N'Liabilities and equity', 5, 2, 0, 1
    UNION ALL SELECT 69903, 699, N'Výkaz ziskov a strát', N'Income statement', 5, 2, 0, 2
    UNION ALL SELECT 71101, 711, N'AKTÍVA', NULL, 4, 2, 1, 0
    UNION ALL SELECT 71102, 711, N'PASÍVA', NULL, 4, 2, 1, 1
    UNION ALL SELECT 71103, 711, N'Vybrané údaje z výnosov a nákladov', NULL, 4, 2, 1, 2
    UNION ALL SELECT 71601, 716, N'Príjmy', NULL, 3, 1, 0, 0
    UNION ALL SELECT 71602, 716, N'Výdavky', NULL, 3, 1, 0, 1
    UNION ALL SELECT 71603, 716, N'Majetok', NULL, 4, 2, 0, 2
    UNION ALL SELECT 71604, 716, N'Záväzky', NULL, 4, 2, 0, 3
    UNION ALL SELECT 72301, 723, N'Aktíva', NULL, 6, 4, 1, 0
    UNION ALL SELECT 72302, 723, N'Pasíva', NULL, 4, 2, 1, 1
    UNION ALL SELECT 72303, 723, N'VÝKAZ ZISKOV A STRÁT', NULL, 4, 2, 1, 2
    UNION ALL SELECT 72701, 727, N'Náklady', N'Expenses', 7, 4, 0, 0
    UNION ALL SELECT 72702, 727, N'Výnosy', N'Revenues', 7, 4, 0, 1
    UNION ALL SELECT 73301, 733, N'Časť I.: Tabuľka č. 1', NULL, 5, 2, 0, 0
    UNION ALL SELECT 73302, 733, N'Časť II.: Tabuľka č. 1', NULL, 4, 2, 0, 1
    UNION ALL SELECT 73801, 738, N'AKTÍVA', NULL, 6, 4, 0, 0
    UNION ALL SELECT 73802, 738, N'PASÍVA', NULL, 4, 2, 0, 1
    UNION ALL SELECT 73803, 738, N'NÁKLADY A VÝNOSY', NULL, 6, 4, 0, 2
    UNION ALL SELECT 80101, 801, N'A K T Í V A', NULL, 7, 4, 0, 0
    UNION ALL SELECT 80102, 801, N'P A S Í V A', NULL, 5, 2, 0, 1
    UNION ALL SELECT 80103, 801, N'Výkaz ziskov a strát', NULL, 7, 4, 0, 2
    UNION ALL SELECT 94101, 941, N'AKTÍVA', NULL, 4, 2, 1, 0
    UNION ALL SELECT 94102, 941, N'PASÍVA', NULL, 4, 2, 1, 1
    UNION ALL SELECT 94103, 941, N'Vybrané údaje z výnosov a nákladov', NULL, 4, 2, 1, 2
    UNION ALL SELECT 94201, 942, N'I. a) A', NULL, 9, 7, 0, 0
    UNION ALL SELECT 94202, 942, N'I. a) P', NULL, 7, 5, 0, 1
    UNION ALL SELECT 94203, 942, N'I. b) A', NULL, 9, 7, 0, 2
    UNION ALL SELECT 94204, 942, N'I. b) P', NULL, 7, 5, 0, 3
    UNION ALL SELECT 94205, 942, N'I. c)', NULL, 5, 3, 0, 4
    UNION ALL SELECT 94206, 942, N'I. d)', NULL, 4, 2, 0, 5
    UNION ALL SELECT 94207, 942, N'II. a)', NULL, 9, 7, 0, 6
    UNION ALL SELECT 94208, 942, N'II. b)', NULL, 9, 7, 0, 7
    UNION ALL SELECT 94301, 943, N'I.: Tab. č. 1', NULL, 5, 2, 0, 0
    UNION ALL SELECT 94302, 943, N'I.: Tab. č. 2', NULL, 4, 2, 0, 1
    UNION ALL SELECT 94303, 943, N'I.: Tab. č. 3', NULL, 4, 2, 0, 2
    UNION ALL SELECT 94304, 943, N'II.: Tab. č. 1', NULL, 4, 2, 0, 3
    UNION ALL SELECT 94305, 943, N'II.: Tab. č. 2', NULL, 4, 2, 0, 4
    UNION ALL SELECT 100101, 1001, N'A K T Í V A', NULL, 7, 4, 0, 0
    UNION ALL SELECT 100102, 1001, N'P A S Í V A', NULL, 5, 2, 0, 1
    UNION ALL SELECT 100103, 1001, N'Výkaz ziskov a strát', NULL, 7, 4, 0, 2
    UNION ALL SELECT 102101, 1021, N'MAJETOK', NULL, 5, 2, 1, 0
    UNION ALL SELECT 102102, 1021, N'VLASTNÉ IMANIE A ZÁVÄZKY', NULL, 5, 2, 1, 1
    UNION ALL SELECT 102103, 1021, N'VÝKAZ ZISKOV A STRÁT', NULL, 5, 2, 1, 2
    UNION ALL SELECT 110101, 1101, N'A K T Í V A', NULL, 7, 4, 0, 0
    UNION ALL SELECT 110102, 1101, N'P A S Í V A', NULL, 5, 2, 0, 1
    UNION ALL SELECT 110103, 1101, N'Výkaz ziskov a strát', NULL, 7, 4, 0, 2
    UNION ALL SELECT 112101, 1121, N'AKTÍVA', NULL, 4, 2, 1, 0
    UNION ALL SELECT 112102, 1121, N'PASÍVA', NULL, 4, 2, 1, 1
    UNION ALL SELECT 112103, 1121, N'Vybrané údaje z výnosov a nákladov', NULL, 4, 2, 1, 2
    UNION ALL SELECT 114101, 1141, N'Aktíva', NULL, 7, 4, 0, 0
    UNION ALL SELECT 114102, 1141, N'Pasíva', NULL, 5, 2, 0, 1
    UNION ALL SELECT 114201, 1142, N'Náklady', NULL, 7, 4, 0, 0
    UNION ALL SELECT 114202, 1142, N'Výnosy', NULL, 7, 4, 0, 1
    UNION ALL SELECT 116401, 1164, N'Príjmy', NULL, 4, 2, 0, 0
    UNION ALL SELECT 116402, 1164, N'Výdavky', NULL, 4, 2, 0, 1
    UNION ALL SELECT 116403, 1164, N'Majetok', NULL, 4, 2, 0, 2
    UNION ALL SELECT 116404, 1164, N'Záväzky', NULL, 4, 2, 0, 3
    UNION ALL SELECT 118001, 1180, N'Strana aktív', NULL, 7, 4, 0, 0
    UNION ALL SELECT 118002, 1180, N'Strana pasív', NULL, 5, 2, 0, 1
    UNION ALL SELECT 118003, 1180, N'Náklady', NULL, 7, 4, 0, 2
    UNION ALL SELECT 118004, 1180, N'Výnosy', NULL, 7, 4, 0, 3
    UNION ALL SELECT 518101, 5181, N'AKTÍVA', NULL, 4, 2, 1, 0
    UNION ALL SELECT 518102, 5181, N'PASÍVA', NULL, 4, 2, 1, 1
    UNION ALL SELECT 518103, 5181, N'Vybrané údaje z výnosov a nákladov', NULL, 4, 2, 1, 2
    UNION ALL SELECT 518104, 5181, N'III.: Tab. č. 2', NULL, 4, 2, 1, 3
    UNION ALL SELECT 518201, 5182, N'I. BS_BUO', NULL, 9, 6, 0, 0
    UNION ALL SELECT 518202, 5182, N'I. BS_PUO', NULL, 9, 6, 0, 1
    UNION ALL SELECT 518203, 5182, N'II. PL_BUO', NULL, 11, 8, 0, 2
    UNION ALL SELECT 518204, 5182, N'II. PL_PUO', NULL, 11, 8, 0, 3
    UNION ALL SELECT 518205, 5182, N'III.: Tab. č. 1', NULL, 4, 2, 0, 4
    UNION ALL SELECT 518206, 5182, N'IV.: Tab. č. 1', NULL, 3, 1, 0, 5
    UNION ALL SELECT 518301, 5183, N'I.: Tab. č. 1', NULL, 5, 2, 0, 0
    UNION ALL SELECT 518302, 5183, N'I.: Tab. č. 2', NULL, 4, 2, 0, 1
    UNION ALL SELECT 518303, 5183, N'I.: Tab. č. 3', NULL, 4, 2, 0, 2
    UNION ALL SELECT 518304, 5183, N'II.: Tab. č. 1', NULL, 4, 2, 0, 3
    UNION ALL SELECT 518305, 5183, N'II.: Tab. č. 2', NULL, 4, 2, 0, 4
    UNION ALL SELECT 518306, 5183, N'III.: Tab. č. 1', NULL, 4, 2, 0, 5
    UNION ALL SELECT 518401, 5184, N'A K T Í V A', NULL, 7, 4, 0, 0
    UNION ALL SELECT 518402, 5184, N'P A S Í V A', NULL, 5, 2, 0, 1
    UNION ALL SELECT 518403, 5184, N'Výkaz ziskov a strát', NULL, 5, 2, 0, 2
)
INSERT INTO [Template].[Tables]
(
    [TableErpId], [TemplateId], [Name_sk], [Name_en], [NumberOfColumns], [NumberOfDataColumns], [DontHaveRowNumbers], [TableOrdinal]
)
SELECT
    n.[TableErpId], t.[Id], n.[Name_sk], n.[Name_en], n.[NumberOfColumns], n.[NumberOfDataColumns], n.[DontHaveRowNumbers], n.[TableOrdinal]
FROM [NewData] AS n
INNER JOIN [Template].[Templates] AS t ON t.[ErpId] = n.[TemplateErpId]
LEFT JOIN [Template].[Tables] AS e ON e.[TableErpId] = n.[TableErpId]
WHERE e.[Id] IS NULL;

COMMIT TRANSACTION;
PRINT '040 template-catalog population completed.';
GO
