using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using PulseCheck.Data.Common.Caching;
using PulseCheck.Data.Common.Database;
using PulseCheck.Data.Common.Repositories;
using PulseCheck.QCPR.Data.AccessObject;
using PulseCheck.Utilities.Web.Json;

namespace PulseCheck.QCPR.Data.Repository
{
    public class ImportRepository : SqlDatabaseRepository, IImportRepository
    {
        private IRedisCache _redisCache = null;
        private readonly string QCPR_PROCEDURES_KEY = "QcprProceduresKey";
        private readonly string QCPR_PRODUCTS_KEY = "QcprProductsKey";

        public ImportRepository(IIbexArchiveConnectionSettings connectionSettings)
        : base(connectionSettings)
        {
        }

        public ImportRepository(IIbexArchiveConnectionSettings connectionSettings, IRedisCache redisCache)
            : base(connectionSettings)
        {
            _redisCache = redisCache ?? throw new ArgumentNullException(nameof(redisCache));
        }

        public ImportRepository(IDbConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        public ImportRepository(ISqlDatabaseHandler sqlDatabaseHandler)
            : base(sqlDatabaseHandler)
        {
        }

        public long ArchiveJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(json));

            if (!JsonValidation.IsValid(json))
                throw new ArgumentOutOfRangeException($"{nameof(json)} does not contain a valid json string");

            ImportArchive archive = new ImportArchive();
            archive.Json = json;

            return SqlDatabaseHandler.Insert(archive);

        }

        public void SaveImportData(Procedure[] procedures)
        {
            if (procedures == null)
                throw new ArgumentNullException(nameof(procedures));

            if (procedures.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(procedures));


            var siteMappings = SqlDatabaseHandler.Query<SiteMapping>("select * from dbo.qcprsitemapping")?.ToArray();

            if(siteMappings == null || siteMappings.Length <= 0)
                throw new InvalidOperationException("No site mappings were defined for the integrations: QcprSiteMapping table");

            List<Procedure> proceduresToCache = new List<Procedure>();
            List<Product> productsToCache = new List<Product>();

            SqlDatabaseHandler.Execute("DELETE FROM dbo.QcprRoute");
            foreach (Procedure procedure in procedures)
            {
                var map = siteMappings.FirstOrDefault(x => x.FacilityName.ToLowerInvariant() == procedure.Facility.ToLowerInvariant());

                procedure.SiteId = map?.SiteId;

                var existingProcedure = SqlDatabaseHandler.QuerySingleOrDefault<Procedure>("select QcprProcedureId from dbo.QcprProcedure where SiteId=@SiteId and Code=@Code", new { procedure.SiteId, procedure.Code });
                if (existingProcedure != null)
                {
                    procedure.QcprProcedureId = existingProcedure.QcprProcedureId;
                    SqlDatabaseHandler.Update(procedure);
                }
                else
                {
                    procedure.QcprProcedureId = SqlDatabaseHandler.Insert(procedure);
                }
                var removableProducts = SqlDatabaseHandler.Query<Product>("select * from QcprProduct where QcprProcedureId=@QcprProcedureId", new { procedure.QcprProcedureId }).ToList();

                proceduresToCache.Add(procedure);
                foreach (var product in procedure.Products)
                {
                    product.QcprProcedureId = procedure.QcprProcedureId;

                    var existingProduct = SqlDatabaseHandler.QuerySingleOrDefault<Product>("select * from dbo.QcprProduct where QcprProcedureId=@QcprProcedureId and Code=@Code", new { product.QcprProcedureId, product.Code });

                    if (existingProduct != null)
                    {
                        product.QcprProductId = existingProduct.QcprProductId;
                        SqlDatabaseHandler.Update(product);
                    }
                    else
                    {
                        product.QcprProductId = SqlDatabaseHandler.Insert(product);
                    }
                    
                    removableProducts.RemoveAll(p => p.QcprProductId == product.QcprProductId);
                    productsToCache.Add(product);
                    // Routes can safely get a new list everytime, because they aren't tied to anything in PulseCheck
                    foreach (Route route in product.Routes)
                    {
                        route.QcprProductId = product.QcprProductId;
                        route.Id = SqlDatabaseHandler.Insert(route);
                    }
                }

                // Remove the products that were not updated/inserted
                foreach (var removableProduct in removableProducts)
                    SqlDatabaseHandler.Delete(removableProduct);
            }           

            if (_redisCache != null && _redisCache.IsConnected())
            {
                _redisCache.Delete(QCPR_PROCEDURES_KEY);
                _redisCache.Delete(QCPR_PRODUCTS_KEY);

                _redisCache.Add(QCPR_PROCEDURES_KEY, proceduresToCache);
                _redisCache.Add(QCPR_PRODUCTS_KEY, productsToCache);
            }
        }

        public IEnumerable<Procedure> GetProcedureByName(byte siteId, string procedureName)
        {
            if (string.IsNullOrEmpty(procedureName))
                throw new ArgumentNullException(nameof(procedureName));

            IEnumerable<Procedure> retVal = GetProceduresFromCache(siteId, procedureName);

            if (retVal != null)
                return retVal;

            retVal = GetProcedureFromTable(siteId, procedureName);

            if (retVal == null)
                throw new RowNotInTableException($"ProcedureName = {procedureName}");

            return retVal;
        }

        private IEnumerable<Procedure> GetProcedureFromTable(byte siteId, string procedureName)
        {
            if (string.IsNullOrEmpty(procedureName))
                throw new ArgumentNullException(nameof(procedureName));

            DynamicParameters p = new DynamicParameters();
            p.Add("siteid", siteId, DbType.Byte);
            p.Add("name", procedureName, DbType.AnsiString);

            return SqlDatabaseHandler.ExecuteStoredProcedure<Procedure>("QcprProcedureGetByName", p);
        }

        private IEnumerable<Procedure> GetProceduresFromCache(byte siteId, string name)
        {
            var procs = _redisCache?.Get<Procedure[]>(QCPR_PROCEDURES_KEY);

            if (procs == null || procs.Length == 0)
                return null;

            return from p in procs
                   where p.SiteId.Equals(siteId)
                   where p.Name.ToLowerInvariant().Contains(name.ToLowerInvariant())
                   orderby p.Name
                   select p;
        }

        public IEnumerable<Product> GetProductByName(byte siteId, string productName)
        {
            if (string.IsNullOrEmpty(productName))
                throw new ArgumentNullException(nameof(productName));

            IEnumerable<Product> retVal = GetProductsFromCache(siteId, productName);

            if (retVal != null)
                return retVal;

            retVal = GetProductFromTable(siteId, productName);

            if (retVal == null)
                throw new RowNotInTableException($"ProductName = {productName}");

            return retVal;
        }

        public IEnumerable<Product> GetProductById(long productId)
        {
            if (productId <= 0)
                throw new ArgumentOutOfRangeException(nameof(productId));

            IEnumerable<Product> retVal = GetProductsFromCache(productId);

            if (retVal != null)
                return retVal;

            retVal = GetProductFromTable(productId);

            if (retVal == null)
                throw new RowNotInTableException($"ProductId = {productId}");

            return retVal;
        }

        public IEnumerable<Product> GetProductsByProcedureId(long procedureId)
        {
            if (procedureId <= 0)
                throw new ArgumentOutOfRangeException(nameof(procedureId));

            IEnumerable<Product> retVal = GetProductsByProcedureIdFromCache(procedureId);

            if (retVal != null)
                return retVal;

            retVal = GetProductsByProcedureFromTable(procedureId);

            if (retVal == null)
                throw new RowNotInTableException($"ProcedureId = {procedureId}");

            return retVal.OrderBy(p => p.Name);
        }


        private IEnumerable<Product> GetProductFromTable(byte siteId, string productName)
        {
            if (string.IsNullOrEmpty(productName))
                throw new ArgumentNullException(nameof(productName));

            DynamicParameters p = new DynamicParameters();
            p.Add("site", siteId, DbType.Byte);
            p.Add("name", productName, DbType.AnsiString);
            
            return SqlDatabaseHandler.ExecuteStoredProcedure<Product>("QcprProductGetByName", p).OrderBy(prod => prod.Name);
        }

        private IEnumerable<Product> GetProductFromTable(long productId)
        {
            if (productId <= 0)
                throw new ArgumentOutOfRangeException(nameof(productId)); 

            DynamicParameters p = new DynamicParameters();
            p.Add("qcprProductId", productId, DbType.Int64);

            return SqlDatabaseHandler.ExecuteStoredProcedure<Product>("QcprProductGetByQcprProductId", p);
        }


        private IEnumerable<Product> GetProductsByProcedureFromTable(long procedureId)
        {
            if (procedureId <= 0)
                throw new ArgumentOutOfRangeException(nameof(procedureId));

            DynamicParameters p = new DynamicParameters();
            p.Add("qcprProcedureId", procedureId, DbType.Int64);

            return SqlDatabaseHandler.ExecuteStoredProcedure<Product>("QcprProductGetByQcprProcedureId", p);
        }

        private IEnumerable<Product> GetProductsFromCache(byte siteId, string name)
        {
            var products = _redisCache?.Get<Product[]>(QCPR_PRODUCTS_KEY);

            if (products == null || products.Length == 0)
                return null;

            return from p in products
                   where p.SiteId.Equals(siteId)
                   where p.Name.ToLowerInvariant().Contains(name.ToLowerInvariant())
                   orderby p.Name
                   select p;
        }

        private IEnumerable<Product> GetProductsFromCache(long id)
        {
            var products = _redisCache?.Get<Product[]>(QCPR_PRODUCTS_KEY);

            if (products == null || products.Length == 0)
                return null;

            return from p in products where p.QcprProductId == id select p;
        }

        private IEnumerable<Product> GetProductsByProcedureIdFromCache(long procedureId)
        {
            var products = _redisCache?.Get<Product[]>(QCPR_PRODUCTS_KEY);

            if (products == null || products.Length == 0)
                return null;

            return from prod in products
                   where prod.QcprProcedureId == procedureId
                   orderby prod.Name
                   select prod;
        }

        public async Task ReloadCachedImportDataFromTable()
        {
            if (_redisCache.IsConnected())
                return;

            string procedureSql = "Select * From QcprProcedures";
            string productSql = "Select * From QcprProducts";

            var procedures = SqlDatabaseHandler.Query<Procedure>(procedureSql);
            await _redisCache.AddAsync(QCPR_PROCEDURES_KEY, procedures);

            var products = SqlDatabaseHandler.Query<Product>(productSql);
            await _redisCache.AddAsync(QCPR_PRODUCTS_KEY, products);
        }
    }
}
