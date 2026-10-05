# IfoSoft CSV and XML layouts

Supported: Windows-1250 semicolon CSV for AJ/GL/AF, and UCT_VETA XML for AJ.
XLS/XLSX are excluded. These definitions describe specific IfoSoft exports, not arbitrary tables.

JSON contains recognition headers/title and column mappings (CSV), or root/document type and element paths (XML). Existing CSV tokenization and AJ row parsers are reused. Existing public importer entry points are unchanged.

Levels:
1. Recognize from CSV first three logical records or XML identification header.
2. Read identifiers. CSV GL/AF use the report title. CSV AJ scans posting dates; XML AJ scans record rok values. These scans retain no journal rows, but do read the body. Multiple years require caller selection; filenames never establish identity or year. XML export-header year is retained separately and discrepancies are warned.
3. Extract original fields and source locations. AF headings/placeholders are diagnostic rows only.
4. Validate/prepare typed values, dates and XML control totals. Errors prevent canonical publication.
5. Publish canonical AJ/GL/AF model. XML retains each source accounting entry instead of merging adjacent debit/credit entries. Compare per-account amounts rather than row counts.

Missing IČO produces a warning. Caller-selected year never proves entity identity. Journal dates needing review remain excluded from calculation. Business reconciliation across AJ/GL/AF remains a later shared-engine responsibility.

SimpleRunner accepts a single CSV export or a manifest with a FilePath header. Directory scans include PDF/XPS/OXPS/DBF/CSV/XML. Prefer a manifest if the directory also contains summary CSV files.

Optional sample tests: set IFOSOFT_STRUCTURED_TEST_ROOT to the folder containing the six Kamienka CSV/XML samples before dotnet test.

## CSV/XML follow-up

Additional business-export layouts: GL PM/PD/RM/RD/MM/MD/KM/KD; AJ Ucet/Protiucet with either Datum or DatumUskut; extended AF. Ucet maps to debit account and Protiucet to credit account. DatumUskut supplies posting date; DatumVystav remains in original source fields. Extra columns are retained under source:<index>:<header>, without adding canonical properties.

CSV quote recovery is limited to a known name/description column on a single physical line, with exact column count and valid quoting in every other column. Ambiguous recovery is rejected. Standard escaped quotes, embedded delimiters, and multiline quoted records remain supported. Recovery emits csvQuotesRecovered. SourceCsvRecord retains the raw logical record.

Invalid/inconsistent XML date fields are retained verbatim, with PostingDate unset (DateTime.MinValue), the entry excluded from calculation, and a warning. Amount/account validation still applies, and these entries remain in XML control totals. No date correction is guessed. Valid record dates establish observed years. Multiple valid years still require an explicit fiscal-year selection.

Unsupported by agreement: the spreadsheet-resaved Tušická Nová Ves 2024 GL, Kačanov invoice listing, and the two Krčava Crystal Reports XML exports. Structurally broken XML remains blocked.
