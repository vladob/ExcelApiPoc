# IfoSoft print-layout signatures (initial recognition definitions)

The JSON files describe observable report signatures and candidate record lines.
They are input to the shared layout engine for both PDF and OpenXPS; they do not
yet map printed values into `JournalImport`, `GeneralLedgerImport`, or framework
rows. Import must reject ambiguous lines until each format has full-file checks.

| Definition | Source layout | Report kind | Balance presentation |
| --- | --- | --- | --- |
| journal-dennik | `_DENNIK.GMX` | journal by document | n/a |
| journal-dennik1 | `_DENNIK1.GMX` (both supplied versions) | journal | n/a |
| general-ledger-hlknia4 | `_HLKNIA4.GMX` (both supplied versions) | detailed ledger | signed net opening/closing |
| general-ledger-hlknia4b | `_HLKNIA4B.GMX` | ledger by account | signed net opening/closing |
| general-ledger-hlknia4c | `_HLKNIA4C.GMX` | ledger variant | confirm field semantics with matching print sample |
| general-ledger-hlknia4d | `_HLKNIA4D.GMX` | ledger by account | signed net opening/closing |
| general-ledger-predvas3 | `_PREDVAS3.GMX` | analytical ledger | separate opening/closing sides |
| account-movements-obr-ucet | `_OBR_UCET.GMX` | movements grouped by account | n/a |
| accounting-framework | printed `ÚČTOVÝ ROZVRH ANALYTICKÝCH ÚČTOV` | framework | n/a |

The same three-digit account code can be a posted account or a subtotal. The
candidate record rules deliberately do not treat either as an imported account
until the enclosing parser identifies the row type. The accounting framework
has no GMX marker in the supplied samples; its title is the signature.

Fiscal year in a corpus filename is derived from its folder and is not used for
recognition. The printed report period is preferred when present. A framework
without a printed period has an unverified year. Additional print layouts may
need separate definitions as more samples are examined.
