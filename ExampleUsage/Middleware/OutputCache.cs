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
                ResponseRewriter = context.GetFeature<IResponseRewriter>()
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
                if (cacheContext.ResponseRewriter == null)
                {
                    // If there is no proir response writer then we have to write the response.
                    Console.WriteLine("Responding with cached content");
                    return context.Response.WriteAsync(cacheContext.CachedContent);
                }
                // If there is a prior response writer then we need to replace the contents
                // of the output buffer so that it will send this back to the browser
                return Task.Factory.StartNew(() =>
                {
                    cacheContext.ResponseRewriter.OutputBuffer = cacheContext.CachedContent;
                });
            }

            context.SetFeature<IOutputCache>(cacheContext);
            return next().ContinueWith(t =>
            {
                if (cacheContext.ResponseRewriter == null)
                {
                    // Since we captured the output from downstream, we are responsible for sending it to the browser
                    // TODO: Set the response body back to the original stream
                    context.Response.Write(cacheContext.OutputBuffer);
                }
                if (cacheContext.Priority != CachePriority.Never)
                    Console.WriteLine("Saving response in output cache");
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
            public IResponseRewriter ResponseRewriter { get; set; }
            public byte[] CachedContent { get; set; }

            /// <summary>
            /// This only gets used when there is no other response rewriter further upstream
            /// </summary>
            private byte[] _outputBuffer;

            /// <summary>
            /// When there is a long chain of middleware that buffers the output and
            /// modifies it before sending the response to the broswer then these should
            /// refer upstream to the first middleware of this type so that the output
            /// is only captured once and only sent back to the browser once.
            /// </summary>
            public byte[] OutputBuffer 
            {
                get 
                {
                    return ResponseRewriter == null ? _outputBuffer : ResponseRewriter.OutputBuffer; 
                }
                set
                {
                    if (ResponseRewriter == null)
                        _outputBuffer = value;
                    else
                        ResponseRewriter.OutputBuffer = value;
                }
            }

            public CacheContext()
            {
                if (ResponseRewriter == null)
                {
                    // TODO: Capture the output from the downstream middleware into _outputBuffer;
                }
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
