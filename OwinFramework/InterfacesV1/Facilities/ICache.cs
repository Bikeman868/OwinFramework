using System;
using System.Collections.Generic;

namespace OwinFramework.InterfacesV1.Facilities
{
    /// <summary>
    /// An object of this type is returned by the cache facility
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Tries to get a value from the cache if it is present
        /// </summary>
        /// <typeparam name="T">The type of data stored the cache against this key</typeparam>
        /// <param name="key">A key that identifies a piece of data in the cache</param>
        /// <param name="defaultValue">The value to return if the cache does not contain this key</param>
        /// <param name="lockTime">Optional time to lock the value in the cache. Updating the cache will clear the lock</param>
        /// <returns>The cached value or the default value if not in cache</returns>
        T Get<T>(string key, T defaultValue = default(T), TimeSpan? lockTime = null);

        /// <summary>
        /// Overwrites data in the cache and unlocks it if locked
        /// </summary>
        /// <typeparam name="T">The type of data to store in the cache. Must be serializable for distributed caches</typeparam>
        /// <param name="key">A key that identifies a piece of data in the cache</param>
        /// <param name="value">The data to store in the cache</param>
        /// <param name="lifespan">How long to keep the data in cache. If you pass null the cache will decide</param>
        /// <returns>True if the data was overwritten and False if data was inserted</returns>
        bool Put<T>(string key, T value, TimeSpan? lifespan = null);

        /// <summary>
        /// Deletes an entry in the cache
        /// </summary>
        /// <param name="key">A key that identifies a piece of data in the cache</param>
        /// <returns>True if the data was deleted and False if data was not in the cache</returns>
        bool Delete(string key);
    }
}
