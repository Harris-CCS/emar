using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace PulseCheck.Data.Common.Caching
{
    public interface IRedisCache
    {
        void Delete(string key);

        void Add(string key, string value);
        Task AddAsync(string key, string value);
        void Add<T>(string key, T value);
        Task AddAsync<T>(string key, T value);

        string Get(string key);
        T Get<T>(string key);


        bool IsConnected();
    }
}
