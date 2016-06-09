using System;
using System.Collections.Generic;
using Microsoft.Owin;
using OwinFramework.Interfaces.Builder;
using System.Threading.Tasks;

namespace OwinFramework.Interfaces.Routing
{
    public interface IRoutingSegment : IRoutingProcessor, IRoute
    {
        string Name { get; }
        Func<IOwinContext, bool> Filter { get; }
        IList<IMiddleware> Middleware { get; }

        /// <summary>
        /// Adds a middleware component to this routing segment
        /// </summary>
        void Add(IMiddleware middleware, Type middlewareType);

        /// <summary>
        /// You must resolve dependencies after adding middleware and before
        /// processing requests
        /// </summary>
        void ResolveDependencies();

        /// <summary>
        /// Processes a request by passing it down the pipe of middleware in this segment
        /// </summary>
        Task Invoke(IOwinContext context, Func<Task> next);
    }
}
