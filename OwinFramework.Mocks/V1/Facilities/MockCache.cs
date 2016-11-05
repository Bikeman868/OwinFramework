﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Moq.Modules;
using OwinFramework.InterfacesV1.Facilities;

namespace OwinFramework.Mocks.V1.Facilities
{
    public class MockCache : ConcreteImplementationProvider<ICache>
    {
        private readonly TestCache _cache = new TestCache();

        protected override ICache GetImplementation(IMockProducer mockProducer)
        {
            return _cache;
        }

        public void Clear()
        {
            _cache.Clear();
        }

        private class TestCache: ICache
        {
            private readonly IDictionary<string, CacheEntry> _cache = new ConcurrentDictionary<string, CacheEntry>();

            public void Clear()
            {
                _cache.Clear();
            }

            bool ICache.Delete(string key)
            {
                return _cache.Remove(key);
            }

            T ICache.Get<T>(string key, T defaultValue, TimeSpan? lockTime)
            {
                while (true)
                {
                    CacheEntry cacheEntry;
                    if (!_cache.TryGetValue(key, out cacheEntry))
                        return defaultValue;

                    if (cacheEntry.Expires.HasValue && DateTime.UtcNow > cacheEntry.Expires)
                        return defaultValue;

                    if (!cacheEntry.LockedUntil.HasValue || DateTime.UtcNow > cacheEntry.LockedUntil)
                    {
                        cacheEntry.LockedUntil = DateTime.UtcNow + lockTime;
                        return (T)cacheEntry.Data;
                    }

                    Thread.Sleep(1);
                }
            }

            bool ICache.Put<T>(string key, T value, TimeSpan? lifespan)
            {
                var exists = _cache.ContainsKey(key);
                _cache[key] = new CacheEntry
                {
                    Data = value,
                    Expires = DateTime.UtcNow + lifespan
                };
                return exists;
            }

            private class CacheEntry
            {
                public object Data;
                public DateTime? Expires;
                public DateTime? LockedUntil;
            }
        }
    }
}
