using System;

namespace OwinFramework.InterfacesV1.Upstream
{
    /// <summary>
    /// This interface is injected into the OWIN context by the output cache
    /// during the routing phase. The output cache indicates if a cached version
    /// is available, and the rendering middleware can tell the output cache
    /// whether to serve the cached content or pass the request to the owin
    /// pipeline for rendering.
    /// For example a request comes in for a user profile and there is a
    /// cached version available should we serve the response from cache? That
    /// depends on the identity of the user making the request. If this
    /// request is from a user requesting their own profile then it should
    /// be rendered every time from backing store, but otherwise it can be
    /// served from cache.
    /// </summary>
    public interface IUpstreamOutputCache
    {
        /// <summary>
        /// The output cache sets this to true if it has a cached version
        /// of the content available and false otherwise. Downstream
        /// middleware should check this and perform no further checking
        /// of cachability if it is false.
        /// </summary>
        bool CachedContentIsAvailable { get; }

        /// <summary>
        /// The output cache sets this to the amount of time that elapsed
        /// since this content was added to the cache. Typically middleware
        /// will be configured to cache different types of content for different
        /// amounts of time. This type of middleware will look at the request
        /// and the time it has been cacehd for, then set the UseCachedContent
        /// property appropriately.
        /// </summary>
        TimeSpan? TimeInCache { get; }

        /// <summary>
        /// Middleware downstream of the output cache can set this to false
        /// to tell the output cache not to serve the cached content, but
        /// render the output again from scratch.
        /// </summary>
        bool UseCachedContent { get; set; }
    }
}
