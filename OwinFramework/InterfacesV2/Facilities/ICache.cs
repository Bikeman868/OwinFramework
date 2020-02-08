using System;

namespace OwinFramework.InterfacesV2.Facilities
{
    /// <summary>
    /// An object of this type is returned by the cache facility
    /// </summary>
    public interface ICache: InterfacesV1.Facilities.ICache
    {
        /// <summary>
        /// Overwrites data in the cache and unlocks it if locked. This is the same behavour as
        /// the Put() method from the V1 interface, it was renamed in V2 to avoid misunderstanding
        /// about what it does.
        /// </summary>
        /// <typeparam name="T">The type of data to store in the cache. Must be serializable for distributed caches</typeparam>
        /// <param name="key">A key that identifies a piece of data in the cache</param>
        /// <param name="value">The data to store in the cache</param>
        /// <param name="lifespan">How long to keep the data in cache. If you pass null the cache will decide</param>
        /// <param name="category">Optional category. Cache implementations can choose different caching strategies for different categories of data</param>
        /// <returns>True if the data was overwritten and False if data was inserted</returns>
        bool Replace<T>(string key, T value, TimeSpan? lifespan = null, string category = null);

        /// <summary>
        /// Returns true if this cache implementation supports merge operations. If this
        /// property is false and you call the Merge method it will throw a NotImplementedException
        /// </summary>
        bool CanMerge { get; }

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
