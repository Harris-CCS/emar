using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PulseCheck.QCPR.Data.Repository;
using PulseCheck.QCPR.Domain.Contract;
using PulseCheck.QCPR.Domain.Data;
using PulseCheck.Utilities.Web.Json;
using System.Runtime.Caching;
using PulseCheck.QCPR.Domain.Constants;

namespace PulseCheck.QCPR.Logic.Managers
{
    public class QcprManager : Manager, IQcprManager
    {
        private IImportRepository _importRepository;

        private static NameValueCollection _memCache = new NameValueCollection();
        private MemoryCache _mcQcprCache = new MemoryCache("qcpr_manager_cache", _memCache);
        private IQcprInterfaceRepository _qcprInterfaceRepository;

        public QcprManager(IImportRepository importRepository, IQcprInterfaceRepository qcprInterfaceRepository)
        {
            _importRepository = importRepository ?? throw new ArgumentNullException(nameof(importRepository));
            _qcprInterfaceRepository = qcprInterfaceRepository ?? throw new ArgumentNullException(nameof(qcprInterfaceRepository));
        }

        public QcprManager(IImportRepository importRepository)
        {
            _importRepository = importRepository ?? throw new ArgumentNullException(nameof(importRepository));
            _qcprInterfaceRepository = null;
        }

        public string GetQcprJsonFromVendor()
        {
            if(_qcprInterfaceRepository == null)
                throw new ConfigurationErrorsException($"appsetting {AppSettingConstants.QcprBaseUrl} has not been configured.");

            return _qcprInterfaceRepository.GetProceduresJson();
        }

        public void SaveImportData(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(json);

            if (!JsonValidation.IsValid(json))
                throw new ArgumentOutOfRangeException($"{nameof(json)} does not contain a valid json value");

            var qcpr = JsonConvert.DeserializeObject<QcprImportData>(json);

            if (qcpr == null)
                throw new InvalidOperationException("The json string failed to deserialize to a valid object");

            long archiveId = _importRepository.ArchiveJson(json);

            if (archiveId <= 0)
                throw new InvalidOperationException("An error occured archiving the import json");

            qcpr.SetImportArchiveId(archiveId);

            SaveImportData(qcpr);
        }


        public void SaveImportData(IQcprImportData importData)
        {
            QcprImportData.Validate(importData);
            _importRepository.SaveImportData(AutoMapper.Mapper.Map<QCPR.Data.AccessObject.Procedure[]>(importData.data.procedure));
        }

        public IEnumerable<Procedure> GetProceduresByCode(string code)
        {
            IEnumerable<Procedure> procedures = null;

            var res = (IEnumerable<Procedure>)_mcQcprCache.Get("procedureCode");
            if (res != null)
            {
                procedures = res;
            }
            else
            {
                //procedures = _importRepository.GetProcedureByCode(code);
                _mcQcprCache.Add(code, procedures, DateTimeOffset.UtcNow.AddDays(1));
            }

            return procedures;
        }

        public IEnumerable<GetProdceduresResponse> GetProceduresByName(string procedureName)
        {
            return AutoMapper.Mapper.Map<IEnumerable<GetProdceduresResponse>>(_importRepository.GetProcedureByName(procedureName));
        }

        public IEnumerable<GetProductsResponse> GetProductsByName(string productName)
        {
            return AutoMapper.Mapper.Map<IEnumerable<GetProductsResponse>>(_importRepository.GetProductByName(productName));
        }

        public IEnumerable<GetProductsResponse> GetProductsById(long id)
        {
            return AutoMapper.Mapper.Map<IEnumerable<GetProductsResponse>>(_importRepository.GetProductById(id));
        }

        public async Task ReloadCachedImportDataFromTable()
        {
            await _importRepository.ReloadCachedImportDataFromTable();
        }

    }
}
