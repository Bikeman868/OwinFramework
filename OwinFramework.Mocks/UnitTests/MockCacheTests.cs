using System;
using System.Threading;
using NUnit.Framework;
using OwinFramework.InterfacesV2.Facilities;
using OwinFramework.Mocks.V2.Facilities;

namespace OwinFramework.Mocks.UnitTests
{
    /// <summary>
    /// Unit tests for mocks in a bit further than most people like to take it
    /// but there is no harm, right.
    /// </summary>
    [TestFixture]
    public class MockCacheTests
    {
        private MockCache _mockCache;
        private ICache _cache;

        [SetUp]
        public void Setup()
        {
            _mockCache = new MockCache();
            _cache = _mockCache.GetImplementation<ICache>(null);
        }

        [Test]
        public void Should_store_and_retrieve_values()
        {
            var exist1 = _cache.Replace("key1", "value1", null);
            var exist2 = _cache.Replace("key2", "value2", null);
            var exist3 = _cache.Replace("key1", "value3", null);

            Assert.IsFalse(exist1, "Key 1 does not exist");
            Assert.IsFalse(exist2, "Key 2 does not exist");
            Assert.IsTrue(exist3, "Key 1 does exist");

            Assert.AreEqual("value3", _cache.Get("key1", ""));
            Assert.AreEqual("value2", _cache.Get("key2", ""));
        }

        [Test]
        public void Should_lock_values_for_update()
        {
            var lockTime = TimeSpan.FromSeconds(1);
            var sleepTime = TimeSpan.FromMilliseconds(50);

            Action threadAction = () =>
            {
                for (var i = 0; i < 10; i++)
                {
                    var value = _cache.Get("key", 0, lockTime);
                    Thread.Sleep(sleepTime);
                    _cache.Replace("key", value + 1);
                }
            };

            var threads = new[]
            {
                new Thread(() => threadAction()),
                new Thread(() => threadAction()),
                new Thread(() => threadAction()),
                new Thread(() => threadAction())
            };

            foreach (var thread in threads) thread.Start();
            foreach (var thread in threads) thread.Join(TimeSpan.FromMinutes(1));

            Assert.AreEqual(40, _cache.Get("key", 0));
        }

        [Test]
        public void Should_expire_content()
        {
            _cache.Replace("key1", 1);
            _cache.Replace("key2", 2, TimeSpan.FromMilliseconds(250));
            _cache.Replace("key3", 3, TimeSpan.FromMilliseconds(750));

            Assert.AreEqual(1, _cache.Get("key1", 0));
            Assert.AreEqual(2, _cache.Get("key2", 0));
            Assert.AreEqual(3, _cache.Get("key3", 0));

            Thread.Sleep(TimeSpan.FromMilliseconds(500));

            Assert.AreEqual(1, _cache.Get("key1", 0));
            Assert.AreEqual(0, _cache.Get("key2", 0));
            Assert.AreEqual(3, _cache.Get("key3", 0));

            Thread.Sleep(TimeSpan.FromMilliseconds(500));

            Assert.AreEqual(1, _cache.Get("key1", 0));
            Assert.AreEqual(0, _cache.Get("key2", 0));
            Assert.AreEqual(0, _cache.Get("key3", 0));
        }
    }
}
