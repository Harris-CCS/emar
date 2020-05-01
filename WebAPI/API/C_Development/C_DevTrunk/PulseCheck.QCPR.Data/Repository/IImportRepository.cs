using System.Collections.Generic;
using System.Threading.Tasks;
using PulseCheck.QCPR.Data.AccessObject;

namespace PulseCheck.QCPR.Data.Repository
{
    public interface IImportRepository
    {
        void SaveImportData(Procedure[] procedures);
        long ArchiveJson(string json);
        IEnumerable<Procedure> GetProcedureByName(byte siteId, string procedureName);
        IEnumerable<Product> GetProductByName(byte siteId, string productName);
        Task ReloadCachedImportDataFromTable();
        IEnumerable<Product> GetProductById(long productId);
        IEnumerable<Product> GetProductsByProcedureId(long procedureId);
    }
}