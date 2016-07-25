using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Interfaces.Builder;

namespace OwinFramework.Interfaces.Routing
{
    /// <summary>
    /// Represents a list of middleware that all execute or none execute
    /// for a given route.
    /// </summary>
    public interface IRoutingSegment : IRoutingProcessor, IRoute
    {
        /// <summary>
        /// Returns the name of this segment of the routing graph
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns the filter expression that determines is this segment should be
        /// executed for a request.
        /// </summary>
        Func<IOwinContext, bool> Filter { get; }

        /// <summary>
        /// A list of the middleware to execute if the filter matches the request
        /// </summary>
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
