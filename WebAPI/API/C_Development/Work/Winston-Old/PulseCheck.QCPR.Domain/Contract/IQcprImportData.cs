using PulseCheck.QCPR.Domain.Data;

namespace PulseCheck.QCPR.Domain.Contract
{
    public interface IQcprImportData
    {
        Data.Data data { get; set; }
        Status status { get; set; }
    }
}