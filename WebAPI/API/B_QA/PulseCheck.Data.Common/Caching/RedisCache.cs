using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace PulseCheck.Data.Common.Caching
{
    public class RedisCache : IRedisCache
    {
        private static ConfigurationOptions _configurationOptions;
        private readonly string _prefix;

        public RedisCache(string prefix = "")
        {
            _configurationOptions = new ConfigurationOptions
            {
                EndPoints = { "localhost" },
                ConnectTimeout = 10000
            };

            _prefix = prefix;
        }

        public bool IsConnected()
        {
            return Connection.IsConnected;
        }

        private static IDatabase Cache
        {
            get { return Connection.GetDatabase(); }
        }

        private static readonly Lazy<ConnectionMultiplexer> LazyConnection
            = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_configurationOptions));

        public static ConnectionMultiplexer Connection
        {
            get { return LazyConnection.Value; }
        }

        public void Delete(string key)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            Cache.KeyDelete(GetKey(key));
        }

        public void Add(string key, string value)
        {
            Cache.StringSet(GetKey(key), value);
        }

        public async Task AddAsync(string key, string value)
        {
            await Cache.StringSetAsync(GetKey(key), value);
        }

        public void Add<T>(string key, T value)
        {
            Cache.StringSet(GetKey(key), JsonConvert.SerializeObject(value));
        }

        public async Task AddAsync<T>(string key, T value)
        {
            await Cache.StringSetAsync(GetKey(key), JsonConvert.SerializeObject(value));
        }

        public string Get(string key)
        {
            return Cache.StringGet(GetKey(key));
        }

        public T Get<T>(string key)
        {
            return JsonConvert.DeserializeObject<T>(Cache.StringGet(GetKey(key)));
        }

        private string GetKey(string key)
        {
            return _prefix + key;
        }
    }
}