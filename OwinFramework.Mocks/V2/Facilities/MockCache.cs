using System;
using System.Collections.Generic;
using System.Threading;
using Moq.Modules;

namespace OwinFramework.Mocks.V2.Facilities
{
    public class MockCache : ConcreteImplementationProvider<InterfacesV2.Facilities.ICache>
    {
        private readonly TestCache _cache = new TestCache();

        protected override InterfacesV2.Facilities.ICache GetImplementation(IMockProducer mockProducer)
        {
            return _cache;
        }

        public void Clear()
        {
            _cache.Clear();
        }

        private class TestCache: InterfacesV2.Facilities.ICache
        {
            private readonly IDictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();

            public void Clear()
            {
                lock(_cache)
                    _cache.Clear();
            }

            #region V1 interface

            bool InterfacesV1.Facilities.ICache.Delete(string key, string category)
            {
                lock (_cache)
                    return _cache.Remove(key);
            }

            T InterfacesV1.Facilities.ICache.Get<T>(string key, T defaultValue, TimeSpan? lockTime, string category)
            {
                while (true)
                {
                    lock (_cache)
                    {
                        CacheEntry cacheEntry;
                        if (!_cache.TryGetValue(key, out cacheEntry) 
                            || (cacheEntry.Expires.HasValue && DateTime.UtcNow > cacheEntry.Expires))
                        {
                            if (lockTime.HasValue)
                            {
                                _cache[key] = new CacheEntry
                                {
                                    Data = defaultValue,
                                    Expires = DateTime.UtcNow + lockTime,
                                    LockedUntil = DateTime.UtcNow + lockTime
                                };
                            }
                            return defaultValue;
                        }

                        if (!cacheEntry.LockedUntil.HasValue || DateTime.UtcNow > cacheEntry.LockedUntil)
                        {
                            cacheEntry.LockedUntil = DateTime.UtcNow + lockTime;
                            return (T) cacheEntry.Data;
                        }
                    }
                    Thread.Sleep(5);
                }
            }

            bool InterfacesV1.Facilities.ICache.Put<T>(string key, T value, TimeSpan? lifespan, string category)
            {
                return ((InterfacesV2.Facilities.ICache)this).Replace(key, value, lifespan, category);
            }

            #endregion

            #region V2 interface

            bool InterfacesV2.Facilities.ICache.Replace<T>(string key, T value, TimeSpan? lifespan, string category)
            {
                lock (_cache)
                {
                    var exists = _cache.ContainsKey(key);
                    _cache[key] = new CacheEntry
                    {
                        Data = value,
                        Expires = DateTime.UtcNow + lifespan
                    };
                    return exists;
                }
            }

            bool InterfacesV2.Facilities.ICache.CanMerge { get { return false; } }

            bool InterfacesV2.Facilities.ICache.Merge<T>(string key, T value, TimeSpan? lifespan, string category)
            {
                throw new NotImplementedException();
            }

            #endregion

            private class CacheEntry
            {
                public object Data;
                public DateTime? Expires;
                public DateTime? LockedUntil;
            }
        }
    }
}
