using PulseCheck.QCPR.Domain.Data;

namespace PulseCheck.QCPR.Domain.Contract
{
    public interface IQcprImportData
    {
        Data.DataInfo data { get; set; }
        Status status { get; set; }
    }
}