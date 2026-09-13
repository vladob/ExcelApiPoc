using PdfLayoutEngine.Grouping;

namespace PdfLayoutEngine.Records;

public interface IBaselineRecordContinuationPolicy
{
    RecordContinuationDecision Evaluate(BaselineRecord currentRecord, BaselineGroup nextGroup);
}
