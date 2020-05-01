using System;
using System.Runtime.Caching;
using System.Data.SqlClient;
using System.Data;
using PulseCheck.Constants;
using PulseCheck.Utilities;

namespace Host.Configuration
{
    /// <summary>
    /// Retrieve address information for different services used by the API
    /// </summary>
    public static class Addresses
    {
        /// <summary>
        /// Retrieve the base address for a specific version of the API
        /// </summary>
        /// <returns>API base address string</returns>
        public static string GetAPIBaseAddress()
        {
            ObjectCache cache = MemoryCache.Default;

            var apiBaseAddress = cache.Get("APIBaseAddress") as string;
            if (apiBaseAddress == null)
            {
                apiBaseAddress = new DB.Select
                {
                    Sql = "SELECT api_base_address FROM api_settings WHERE api_major_version=@majorv AND api_minor_version=@minorv AND active=1",
                    Parameters = new SqlParameter[] {
                        new SqlParameter("@majorv", SqlDbType.Int) { Value = Identifiers.APIMajorVersion },
                        new SqlParameter("@minorv", SqlDbType.Int) { Value = Identifiers.APIMinorVersion }
                    }
                }.RunForScalar().ToString();

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddDays(1) };
                cache.Add("APIBaseAddress", apiBaseAddress, policy);
            }

            return apiBaseAddress;
        }

        /// <summary>
        /// Retrieve the base address of the Identity Server for a specific version of the API
        /// </summary>
        /// <returns>Identity Server base address string</returns>
        public static string GetIDServerBaseAddress()
        {
            ObjectCache cache = MemoryCache.Default;

            var idBaseAddress = cache.Get("IdentityServerBaseAddress") as string;
            if (idBaseAddress == null)
            {
                idBaseAddress = new DB.Select
                {
                    Sql = "SELECT id_server_base_address FROM api_settings WHERE api_major_version=@majorv AND api_minor_version=@minorv AND active=1",
                    Parameters = new SqlParameter[] {
                        new SqlParameter("@majorv", SqlDbType.Int) { Value = Identifiers.APIMajorVersion },
                        new SqlParameter("@minorv", SqlDbType.Int) { Value = Identifiers.APIMinorVersion }
                    }
                }.RunForScalar().ToString();

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddDays(1) };
                cache.Add("IdentityServerBaseAddress", idBaseAddress, policy);
            }

            return idBaseAddress;
        }
    }
}
