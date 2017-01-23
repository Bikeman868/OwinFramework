using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.InterfacesV1.Upstream;
using OwinFramework.MiddlewareHelpers;
using OwinFramework.MiddlewareHelpers.ResponseRewriter;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This example shows how you would go about writing an output caching
    /// middleware. This sample does no caching, it just illustrates what
    /// pieces you need, and how they should be arranged.
    /// </summary>
    public class OutputCache : IMiddleware<IOutputCache>, IRoutingProcessor
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get { return _dependencies; } }
        private readonly IList<IDependency> _dependencies = new List<IDependency>();

        /// <summary>
        /// During the routing phase the output cache indicates if cached content is
        /// available, and if so how long it has been cached for.
        /// </summary>
        public Task RouteRequest(IOwinContext context, Func<Task> next)
        {
            // This should be populated with cached data if available with
            // information about when it was cached.
            var cacheContext = new CacheContext
            {
                Priority = CachePriority.Never,
                MaximumCacheTime = TimeSpan.FromHours(1),
                Category = "",
                CachedContentIsAvailable = true,
                TimeInCache = TimeSpan.FromMinutes(3),
                UseCachedContent = false,
                CachedContent = Encoding.UTF8.GetBytes("This is some cached content"),
            };

            context.SetFeature<IUpstreamOutputCache>(cacheContext);

            return next();
        }

        /// <summary>
        /// During the request processing phase, if the downstream middleware indicated that
        /// the cached content can be used then the output cache handles the request by returning
        /// the cached content to the browser.
        /// Otherwise the output cache adds an IOutputCache to the context so that downstream
        /// rendering middleware can give clues to the output cache to help it decide whether to
        /// cache the response o not.
        /// </summary>
        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            var cacheContext = context.GetFeature<IUpstreamOutputCache>() as CacheContext;
            if (cacheContext == null)
                return next();

            if (cacheContext.CachedContentIsAvailable && cacheContext.UseCachedContent)
            {
                Console.WriteLine("Responding with cached content");
                return context.Response.WriteAsync(cacheContext.CachedContent);
            }

            context.SetFeature<IOutputCache>(cacheContext);
            cacheContext.StartCapture(context);

            return next().ContinueWith(t =>
            {
                var updateCache = cacheContext.Priority != CachePriority.Never;
                cacheContext.Send(updateCache);

                if (updateCache)
                {
                    Console.WriteLine("Updating cache with new content");
                    // TODO: Write cacheContext.CachedContent to the cache
                }
            });
        }

        private class CacheContext: IOutputCache, IUpstreamOutputCache
        {
            public CachePriority Priority { get; set; }
            public TimeSpan MaximumCacheTime { get; set; }
            public string Category { get; set; }
            public bool CachedContentIsAvailable { get; set; }
            public TimeSpan? TimeInCache { get; set; }
            public bool UseCachedContent { get; set; }
            public ResponseCapture ResponseCapture { get; set; }
            public byte[] CachedContent { get; set; }

            public byte[] OutputBuffer 
            {
                get { return ResponseCapture.OutputBuffer; }
                set { ResponseCapture.OutputBuffer = value; }
            }

            public void StartCapture(IOwinContext owinContext)
            {
                ResponseCapture = new ResponseCapture(owinContext);
            }

            public void Send(bool copyToCache)
            {
                if (ResponseCapture == null)
                    return;

                if (copyToCache)
                {
                    // We need to copy the output buffer because other middleware upstream
                    // might make further modifications - compression, encryption etc
                    var buffer = ResponseCapture.OutputBuffer;
                    CachedContent = new byte[buffer.Length];
                    buffer.CopyTo(CachedContent, 0);
                }

                ResponseCapture.Send();
            }

            void IOutputCache.Clear()
            {
            }

            void IOutputCache.Clear(string urlRegex)
            {
            }
        }
    }
}
