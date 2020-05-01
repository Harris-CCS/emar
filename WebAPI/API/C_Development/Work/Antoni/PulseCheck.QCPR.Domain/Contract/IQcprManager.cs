using System.Collections.Generic;
using System.Threading.Tasks;
using PulseCheck.QCPR.Domain.Data;

namespace PulseCheck.QCPR.Domain.Contract
{
    public interface IQcprManager
    {
        void SaveImportData(IQcprImportData importData);
        void SaveImportData(string json);
        IEnumerable<GetProceduresResponse> GetProceduresByName(byte siteId, string procedureName);
        IEnumerable<GetProductsResponse> GetProductsByName(byte siteId, string productName);
        Task ReloadCachedImportDataFromTable();
        string GetQcprJsonFromVendor();
        IEnumerable<GetProductsResponse> GetProductsByProcedureId(long procedureId);
        IEnumerable<GetProductsResponse> GetProductById(long id);
    }
}
