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
        /// middleware should check this and perform no further processing
        /// if it is False.
        /// </summary>
        bool CachedContentIsAvailable { get; }

        /// <summary>
        /// Middleware downstream of the output cache can set this to false
        /// to tell the output cache not to serve the cached content, but
        /// render the output again from scratch.
        /// </summary>
        bool UseCachedContent { get; set; }
    }
}
