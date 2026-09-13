using PdfLayoutEngine.Grouping;

namespace PdfLayoutEngine.Records;

public interface IBaselineRecordContinuationPolicy
{
    bool ContinuesRecord(BaselineRecord currentRecord, BaselineGroup nextGroup);
}
