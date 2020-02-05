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
        /// <param name="category">Optional category. Cache implementations can choose different caching strategies for different categories of data</param>
        /// <returns>The cached value or the default value if not in cache</returns>
        T Get<T>(string key, T defaultValue = default(T), TimeSpan? lockTime = null, string category = null);

        /// <summary>
        /// This is obsolete, use the more aptly named Replace instead. Note that there
        /// is also a new Merge method that applies changes to an object rather than
        /// replacing it completely.
        /// </summary>
        [Obsolete("Use the Replace method instead, this name more accurately describes what it does")]
        bool Put<T>(string key, T value, TimeSpan? lifespan = null, string category = null);

        /// <summary>
        /// Deletes an entry in the cache
        /// </summary>
        /// <param name="key">A key that identifies a piece of data in the cache</param>
        /// <param name="category">Optional category. Cache implementations can choose different caching strategies for different categories of data</param>
        /// <returns>True if the data was deleted and False if data was not in the cache</returns>
        bool Delete(string key, string category = null);

        /// <summary>
        /// Overwrites data in the cache and unlocks it if locked
        /// </summary>
        /// <typeparam name="T">The type of data to store in the cache. Must be serializable for distributed caches</typeparam>
        /// <param name="key">A key that identifies a piece of data in the cache</param>
        /// <param name="value">The data to store in the cache</param>
        /// <param name="lifespan">How long to keep the data in cache. If you pass null the cache will decide</param>
        /// <param name="category">Optional category. Cache implementations can choose different caching strategies for different categories of data</param>
        /// <returns>True if the data was overwritten and False if data was inserted</returns>
        bool Replace<T>(string key, T value, TimeSpan? lifespan = null, string category = null);

        /// <summary>
        /// Merges changes into existing cached data. Unlocks the cache entry if it is locked. The purpose
        /// of this method is to update only modified properties of the cached object. This allows multiple
        /// callers to write different changes into the cache for the same object without overwriting
        /// each others changes.
        /// </summary>
        /// <typeparam name="T">The type of data to store in the cache. Must be serializable for distributed caches</typeparam>
        /// <param name="key">A key that identifies a piece of data in the cache</param>
        /// <param name="value">The data to merge into the cache. The data will be merged by overwriting property 
        /// values in the cached object with corresponding property values from the supplied value that have non-default values.</param>
        /// <param name="lifespan">How long to keep the data in cache. If you pass null the cache will decide</param>
        /// <param name="category">Optional category. Cache implementations can choose different caching strategies for different categories of data</param>
        /// <returns>True if the data was merged with existing data and False if there was no 
        /// existing data in the cache. In most cases when this method returns false the update 
        /// should be repeated with a fully populated object rather than merging just the changed
        /// property values.</returns>
        bool Merge<T>(string key, T value, TimeSpan? lifespan = null, string category = null);
    }
}
