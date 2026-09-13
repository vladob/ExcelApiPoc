namespace ExcelApiPoc.AccountingImport.Models.Reporting
{
    public sealed class ImportPerformanceMetrics
    {
        public long ElapsedMilliseconds { get; set; }
        public long ManagedMemoryBeforeBytes { get; set; }
        public long ManagedMemoryAfterBytes { get; set; }
        public int SourceFileCount { get; set; }
        public int CanonicalRowCount { get; set; }

        public long ManagedMemoryDeltaBytes
        {
            get { return ManagedMemoryAfterBytes - ManagedMemoryBeforeBytes; }
        }
    }
}
