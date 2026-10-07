# IfoSoft DBF imports

Supported: observed non-memo dBASE III (version 03) AJ, GL and AF exports. The reader opens source files read-only; no DBF driver installation is needed.

`dbf-journal.json` recognizes field names/types and maps source fields to canonical journal fields. CP852 is explicit because the samples have no declared code page. Account codes stay strings. MSUMA and DSUMA are independent debit/credit amounts; SUMA is retained and validated, not substituted for them. All source fields are preserved with physical record provenance in canonical rows.

Levels:
1. Read header and recognize field structure.
2. Scan active DATVY values; return observed accounting-date range and year, without creating journal rows. The range does not prove a complete month/year. No filename-derived year. DATVY is used provisionally for accounting-period assignment; its exact source label remains unconfirmed. DATUM remains the canonical PostingDate and both original fields are preserved. Entity name and CIN remain unavailable; CISLO/FIRMA are preserved without assuming their meaning.
3. Extract active records. Deleted records are excluded and counted.
4. Validate dates, amounts and account presence. Missing, implausible or out-of-year dates produce review warnings. Mixed DATVY years require fiscal-year selection but do not block normalization. These are import checks, not accounting reconciliation.
5. Produce JournalImport if there are no blocking extraction/amount/account errors. Date exceptions remain in AJ with DateExceptionResolution.Excluded, which makes UsedForReportCalculation false. Without a resolved fiscal year, FiscalYear is 0 and all rows are excluded. Missing/unparseable PostingDate uses DateTime.MinValue because the existing model is nonnullable; original text is preserved in SourceFields and must be used for review/display. An optional fifth runner argument selects the fiscal year (use on a single file or same-year manifest). ExpectedYear remains only a test expectation and never selects data. Opening/closing business classification and reconciliation are not added in this first DBF implementation; RecordKind retains its default value.

Use the existing CompactLayoutImporter and SimpleRunner with the Compact directory. PDF recognition excludes files named dbf-*.json. Existing directory scans now include .dbf. For DBF, decoded page count is zero.

Tests cover stages, CP852 text, account codes, invalid dates/amounts, deleted records, mixed years, truncation and cancellation. To include the supplied real files, set IFOSOFT_DBF_TEST_ROOT to a folder containing U_DENNIK_00323292_202510.DBF and U_DENNIK_00323292_202512.DBF; the optional sample check returns without reading files when the variable is absent.

## General ledger (GL)

`dbf-general-ledger.json` recognizes UCET/MES and the numeric balance/turnover fields.
The mapping follows the existing IfoSoft CSV canonical conventions:

| Source | Canonical |
| --- | --- |
| PM / PD | OpeningDebit / OpeningCredit |
| RM / RD | AnnualDebitTurnover / AnnualCreditTurnover |
| MM / MD | PeriodDebitTurnover / PeriodCreditTurnover |
| KM / KD | ClosingDebit / ClosingCredit |
| PLAN | Plan |
| ODD / POL / KZ / PROG / STR / ZAK | Section / Item / FundingSource / Program / CostCenter / Order |

Separate debit/credit values, including negative values, remain unchanged. Repeated
accounts with different dimensions remain separate records; no subtotal aggregation
or business reconciliation is performed. All original fields are retained in
SourceDimensions, and SourceRecordNumber is the physical DBF record number.
Canonical AccountCode uses the existing whitespace normalizer; raw UCET remains
available. For GL, its first three source positions (trimmed) supply SyntheticCode;
the remainder supplies AnalyticalCode. Unusual codes are retained with warnings.

L2 scans MES only, returning its observed year and period. This is an end-period
identifier, not proof of full-year coverage. Supported month/period numbers are
00 through 14. L4 rejects malformed periods, mixed period snapshots, blank account
codes and invalid amounts. An explicit caller year cannot override a conflicting
MES year. Entity identity is unavailable and must come from confirmed context.

## Account framework (AF)

`dbf-account-framework.json` maps UCET, SYN, ANA, NAZOV, TYP, D, SALDO and DPH.
SYN and ANA remain strings; leading zeros and letter suffixes are preserved.
ANA = **** marks headings; a blank account/name placeholder is Empty. Both remain
in the L3 diagnostic rows but are omitted from canonical account rows. All other
active records are retained, including unusual SYN values (with warnings).
UCET must agree with SYN + ANA after whitespace normalization; a disagreement is
reported as a blocking error rather than guessed. Canonical rows preserve original
fields, including unmapped R/P and alternate names, without assigning unverified
meanings. Synthetic and analytical account row kinds remain distinct.

AF has no source year: identifiers.fiscalYear stays null; canonical FiscalYear is
0 unless supplied explicitly by the caller. A supplied year is reported separately
as selectedFiscalYear. Neither filenames nor DBF header update dates determine it.
No entity identification is inferred from file paths.

Use the same five levels and runner arguments for all three categories. L1 reads
headers only. L2 counts active/deleted records and scans only identification fields;
it creates no source/canonical rows. L3 extracts, L4 validates, L5 normalizes.

For the seven supplied GL/AF binary sample regressions, set
IFOSOFT_DBF_ACCOUNT_TEST_ROOT to their folder before running the SimpleTests project.
These optional sample cases return without opening files if that variable is absent;
the synthetic GL/AF tests always execute. Required sample filenames are listed in
DbfAccountTests.cs. No Add-In UI or business-validation wiring is changed here.
