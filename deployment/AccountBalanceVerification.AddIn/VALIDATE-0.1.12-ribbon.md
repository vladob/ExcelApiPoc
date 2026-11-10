# Conclusion rows and ribbon availability

Apply after the 0.1.12 PDF retry/signature patch. Rebuild with Build.ps1 and restart Excel.

- New Rekapitulácia section conclusion rows are unmerged, with text in A, WrapText off, and a 20-point row height. B:E remain blank and editable. Auditors can insert worksheet rows for more lines. Existing workbooks are not rewritten: unmerge the desired conclusion rows and turn off Wrap Text manually to retain all edits.
- Output buttons require the active workbook to contain recognized parameters, title/signature anchors and at least one account worksheet. Decree buttons additionally require the decree formula sheet. Existing content validation still runs when printing/exporting.
- All commands check edit mode before acting. A UI-thread timer detects edit-state changes at 500 ms intervals; workbook/sheet activation and command completion invalidate ribbon state. A click-time check covers the interval before Excel updates the ribbon. No keystrokes are sent and no cell edits are committed or cancelled automatically.
- The timer and application event subscriptions are disposed on add-in unload. The native edit-state API is discovered on ExcelDnaUtil; older assemblies fall back to the built-in New command's enabled state.
- The legacy command is now labelled 'Doplniť dekréty do staršieho zošita'. It only prepares decree/person support. It does not upgrade the parameter layout, reload data, or build a recapitulation. It is disabled when decree support and the six-column people table are already present.

## Address deployment check

The initial 0.1.12 source already includes EntityAddress in MkSoftRepository.FindEntitiesAsync, both entity SQL wrapper TVFs, MkSoftEntity, and the assignment to P_AeAddress. No additional address SQL change is needed in this patch.

Apply Database/AuditAddIn/Migrations/MkSoftVerificationV1/040_RecapAddress.sql, deploy the rebuilt ExcelApiPoc.Api and its dependent assemblies, and restart/recycle its IIS application as appropriate. Reload entity details before creating a workbook. A previously generated workbook does not fetch the address retroactively. File imports do not perform a MkSoft address lookup.

For the example client/year, compare in SSMS:

```sql
SELECT ULICA, PSC, OBEC
FROM [MkSoftRaw].[Data].[UDAJEFIRMY](11, 2026);

SELECT ClientErpId, ICO, EntityAddress
FROM [AuditAddIn].[MkSoft].[EntityInfo](11, 2026);
```

If the wrapper returns an address but a new MkSoft workbook stays blank, check that the configured API URL points to the updated deployment and that its entity response includes entityAddress. A successful old API response may omit that property and deserialize to an empty address.

## Windows checks still required

1. Start with no workbook or an ordinary workbook: Create is available; output and legacy preparation are disabled.
2. Switch to a completed Inventarizácia workbook: normal print/PDF and decree buttons enable. Switch away: they disable.
3. Enter edit mode using typing, F2 and the formula bar. Buttons disable after the polling interval. Test an immediate click too: the message should request Enter/Esc without creating an extra workbook or changing the pending edit. Commit/cancel manually and verify re-enabling.
4. Create a workbook, cancel creation, switch windows, close all workbooks and unload/reload the add-in. Verify no stale state or extra timer remains.
5. On an older workbook, check legacy preparation enables; after preparing it, decree buttons enable and preparation disables.
6. Generate a new report. Check section conclusions are unmerged, Wrap Text off, and spill into the empty cells to the right. Insert a row, add another line, and export.
7. Recheck PDF retry/cancel and normal print/PDF after the ribbon changes.

Static source/XML checks only were possible here. No .NET compilation or Excel COM tests were run. Visual Studio may stop on a thrown COM exception before a catch handler runs when 'Break when this exception type is thrown' is checked; the new edit guard aims to avoid that exception in normal use.
