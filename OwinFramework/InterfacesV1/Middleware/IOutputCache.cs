using System;

namespace OwinFramework.InterfacesV1.Middleware
{
    /// <summary>
    /// Output cache middleware will inject this interface into the OWIN
    /// context before chaining to the rest of the pipeline. Downstream
    /// middleware can use this interface to give hints to the output
    /// cache so that it can optimize its performance.
    /// The output cache can decide to cache the output and serve the
    /// next identical request itself from cache rather than passing
    /// control down the chain.
    /// Output caching algorithms are extremely difficult to get right
    /// so I expect a lot of different implementations to be available.
    /// As an application developer you should test a few alternatives
    /// with production traffic.
    /// </summary>
    public interface IOutputCache : IResponseRewriter
    {
        /// <summary>
        /// Rendering middleware should set this to indicate how valulable
        /// it would be to have this content cached. Another way of looking
        /// at this is how expensive is it to render this content again.
        /// </summary>
        CachePriority Priority {  get; set; }

        /// <summary>
        /// Specifies the maximum amount of time that this content can
        /// be cached before the cached data has no value.
        /// </summary>
        TimeSpan MaximumCacheTime { get; set; }

        /// <summary>
        /// It is assumed that caching algorithms will look at the frequency
        /// of requests, request processing time and size of response when
        /// deciding what to cache. It is not necessary for the cache to
        /// capture these statistics for every URL. By setting this
        /// category property the output cache can calculate stats on each
        /// category instead of each URL. The rendering middleware can set this
        /// property, or the application developer can write middleware for the
        /// express purpose of categorizing requests.
        /// </summary>
        string Category { get; set; }

        /// <summary>
        /// Clears all content from the cache. You might want to do this
        /// if you deployed a new version of your web site and there is
        /// potential for all content to be different.
        /// </summary>
        void Clear();

        /// <summary>
        /// Clears any cached URLs that match the supplied Regular Expression
        /// </summary>
        /// <param name="urlRegex">A Regular Expression to match againt the URL</param>
        void Clear(string urlRegex);
    }

    /// <summary>
    /// Defines possible values for specifying cache priority to the output 
    /// caching middleware
    /// </summary>
    public enum CachePriority
    {
        /// <summary>
        /// Rendering middleware should set this value when the content
        /// it is returning can never be cached, for example when it is
        /// returning real-time stock market prices, or when a user is
        /// requesting their own user profile immediately after posting
        /// changes to it.
        /// </summary>
        Never,

        /// <summary>
        /// Indicates that there is low value in caching this content.
        /// Rendering middleware should set this value when the cost of
        /// a cache miss is very low, for example because the data is
        /// already available in memory
        /// </summary>
        Low,

        /// <summary>
        /// Indicates that there is reasonable value in caching, for example
        /// if caching the output will avoid a call to a local database.
        /// </summary>
        Medium,

        /// <summary>
        /// Indicates that there is very high value in caching this content
        /// for example because it involves multiple complex database queries
        /// or service calls to external systems.
        /// </summary>
        High,

        /// <summary>
        /// Rendering middleware should set this value when the content
        /// never changes and is guaranteed to always be the same. For example
        /// if you version JavaScript files and increase the version number on
        /// each change, old versions are guaranteed to remian unchanged.
        /// </summary>
        Always
    }
}
