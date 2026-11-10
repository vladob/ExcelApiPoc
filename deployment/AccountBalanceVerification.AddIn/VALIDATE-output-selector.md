# Output selector update

Apply after ABV-0.1.12-ribbon-conclusions.patch. Rebuild and restart Excel.

The ribbon has Create Workbook plus a Print and Export group containing the document dropdown, Print and Export to PDF. The legacy preparation command and dedicated decree buttons are removed. The selection defaults to Inventarizácia and is retained for the Excel session.

- Inventarizácia: title + account worksheets only; suggested PDF filename Inventarizacia.pdf.
- Rekapitulácia: only that worksheet; suggested PDF filename Rekapitulacia.pdf.
- Menovacie dekréty: existing decree preview / individual PDF workflow; folder selection and one PDF per recipient. The PDF tooltip explains this; there is no extra label.

Existing workbooks work without regeneration. An older workbook without recap/decrees can still print its inventory; unavailable output choices disable the two action buttons. The dropdown remains usable so the user can switch to an available document. Edit-mode guards and normal PDF retry handling remain.

Windows checks:
1. Verify the three dropdown choices and exactly two output buttons, with the old preparation command absent.
2. Inventory preview/PDF: no recap or decrees. Recap preview/PDF: no title/accounts. Check each PDF filename suggestion and retry with an open destination PDF.
3. Decrees: preview all recipients, export one PDF per recipient, verify the PDF button tooltip changes with selection.
4. Switch workbooks; selection remains, availability follows the active workbook. Test an older workbook without recap/decrees and an unrelated workbook.
5. Enter and leave cell edit mode; command availability and click-time guards still work.

Source/XML checks passed in the implementation environment. .NET compilation and live Excel ribbon/print/export tests must run on Windows.
