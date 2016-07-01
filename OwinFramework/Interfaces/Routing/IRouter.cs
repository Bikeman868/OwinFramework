using System;
using System.Collections.Generic;
using Microsoft.Owin;
using OwinFramework.Interfaces.Builder;

namespace OwinFramework.Interfaces.Routing
{
    /// <summary>
    /// The framework router implements this interfae. You can resolve it from
    /// your IoC container to create routes then you can add these routes
    /// to your Owin pipeline to create a multi-way split of the pipelne
    /// into multiple pipelines.
    /// </summary>
    public interface IRouter : IMiddleware<IRoute>, IRoutingProcessor
    {
        /// <summary>
        /// Fluid interface for adding routes to the router
        /// </summary>
        /// <param name="routeName">Unique name for this route</param>
        /// <param name="filterExpression">An expression that determines which
        /// requests should be routed down this route</param>
        IRouter Add(string routeName, Func<IOwinContext, bool> filterExpression);

        /// <summary>
        /// This is for internal use. It is also used by some diagnostic
        /// middleware that traverses the routing graph
        /// </summary>
        IList<IRoutingSegment> Segments { get; }
    }
}
