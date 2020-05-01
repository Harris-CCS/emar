using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PulseCheck.Data.Common.Caching;
using PulseCheck.QCPR.Data.AccessObject;
using PulseCheck.QCPR.Logic.Bindings;
using StackExchange.Redis;

namespace PulseCheck.Data.Common.Tests
{
    [TestClass]
    public class RedisHandlerTests
    {
        private static readonly AutoFacQcprLogicRegistrations _autoFacQcprLogicRegistrations = new AutoFacQcprLogicRegistrations();
        private static IRedisCache _cacheHandler;
        private static readonly string RedisKey = "RedisHandlerTests_Key";

        [TestInitialize]
        public void Init()
        {
            _autoFacQcprLogicRegistrations.LoadContainer();
            _cacheHandler = _autoFacQcprLogicRegistrations.GetType<IRedisCache>();
        }

        [TestMethod]
        public void Save_Success()
        {
            string value = "Test12321";
            _cacheHandler.Add(RedisKey, value);
            var result = _cacheHandler.Get(RedisKey);

            Assert.IsNotNull(result);
            Assert.IsTrue(result == value);
        }

        [TestMethod]
        public void Save_Success_Generic()
        {
            Procedure[] procs = {new Procedure(){Name = "Test1"}, new Procedure() { Name = "Test2" } }; 
            
            _cacheHandler.Add<Procedure[]>(RedisKey, procs);
            var result = _cacheHandler.Get<Procedure[]>(RedisKey);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length == 2);
            Assert.IsTrue(result.Any(x=>x.Name == "Test1"));


        }

    }
}
