# Ives structured exports

Use the same staged importer and runner with `PdfLayouts/Ives/Compact`.

Supported samples: 17-column semicolon CP1250 journal CSV; Crystal Reports XML journal, detailed general ledger, and account framework. Four small JSON definitions map source columns/FieldName values. XML visual Field.Name values are not identifiers.

CSV structure is shared with an IfoSoft export. Selecting this Ives catalogue supplies the producer context; the column structure alone does not prove the producer. IČO and entity name come from the CSV header, year from posting dates. The supplied U_DENNIK_00323217_202512.CSV contains 2,310 records dated 2026, despite its filename.

Crystal XML is read section by section. Level 1 stops once a matching record is found (at most 4,096 sections). Level 2 scans transaction dates/counts without retaining rows. Levels 3–5 extract, validate, and map into the existing canonical models. Journal continuation fields are preserved. GL imports account summaries, not individual transactions or synthetic subtotals. AJ/GL totals are checked separately for each activity group before canonical publication. All source FieldName/Value pairs of imported records are retained.

XML samples contain no verified IČO or entity name; these remain unset with a warning. AF also has no fiscal year: validity dates and filenames are not used as evidence. AJ/GL years describe observed transaction years, not a separately declared reporting period. Mixed-year journals require an auditor-selected year for calculation eligibility. Invalid dates remain in the journal, flagged and excluded; invalid amounts or failing controls block canonical output.

The supplied HL_KNIHA_00325465_2025.xml is byte-identical to REKAP_00325465_2025.xml and is intentionally unrecognized. The supported detailed ledger is HL_KNIHA_00325465_2025_a.xml. ZBORNIK remains unsupported.

Validation on the supplied XML/PDF pairs: all 6,391 AJ records match on document number, date, both accounts and amounts; all 808 GL records match on account, opening/closing net and debit/credit turnover; all 3,203 AF records match on account and type. XML text may retain content clipped in PDF output.

No CSV GL or AF sample was present in this ZIP, so no unverified Ives mappings for those formats are included.
