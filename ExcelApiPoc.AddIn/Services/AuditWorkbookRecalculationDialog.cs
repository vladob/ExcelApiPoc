using ExcelApiPoc.AddIn.Models;
using System;
using System.Text;
using System.Windows.Forms;

namespace ExcelApiPoc.AddIn.Services
{
    internal static class AuditWorkbookRecalculationDialog
    {
        public static void Show(AuditWorkbookRecalculationResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var message = new StringBuilder();
            message.AppendLine("Audit report calculation completed.");
            message.AppendLine();
            message.AppendLine($"Accounts: {result.AccountCount:N0}");
            message.AppendLine($"Mapping rules: {result.MappingRuleCount:N0}");
            message.AppendLine($"Calculation dependencies: {result.CalculationDependencyCount:N0}");
            message.AppendLine();
            message.AppendLine($"Mapped analytical accounts: {result.MappingSelections.MappedCount:N0}");
            message.AppendLine($"Excluded analytical accounts: {result.MappingSelections.ExcludedCount:N0}");
            message.AppendLine($"Unresolved analytical accounts: {result.MappingSelections.UnresolvedAccountCodes.Count:N0}");
            message.AppendLine();
            message.AppendLine($"Calculated report rows: {result.Calculation.Rows.Count:N0}");
            message.AppendLine($"Pending analytical requirements: {result.Calculation.AnalyticalRequirements.Count:N0}");
            message.AppendLine($"Calculation complete: {(result.Calculation.IsComplete ? "Yes" : "No")}");

            MessageBoxIcon icon = result.Calculation.IsComplete
                ? MessageBoxIcon.Information
                : MessageBoxIcon.Warning;

            MessageBox.Show(
                message.ToString(),
                "Audit Report Calculation",
                MessageBoxButtons.OK,
                icon);
        }
    }
}
