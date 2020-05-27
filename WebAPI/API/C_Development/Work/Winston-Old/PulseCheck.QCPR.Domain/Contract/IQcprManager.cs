using System.Collections.Generic;
using System.Threading.Tasks;
using PulseCheck.QCPR.Domain.Data;

namespace PulseCheck.QCPR.Domain.Contract
{
    public interface IQcprManager
    {
        void SaveImportData(IQcprImportData importData);
        void SaveImportData(string json);
        IEnumerable<GetProdceduresResponse> GetProceduresByName(string procedureName);
        IEnumerable<GetProductsResponse> GetProductsByName(string productName);
        Task ReloadCachedImportDataFromTable();
        string GetQcprJsonFromVendor();

        IEnumerable<GetProductsResponse> GetProductsById(long id);
    }
}
