using System;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace OwinFramework.Interfaces.Routing
{
    /// <summary>
    /// Middleware components implement this interface when they want to be included
    /// in the routing phase of request processing. The routing phase establishes the
    /// route that the request will take through the middleware pipeline, once this
    /// phase is complete there is a second request processing phase.
    /// 
    /// There are several uses for this interface.
    /// 1) Routing components will test the incomming request against filters to determine
    ///    how to route the request.
    /// 2) Middleware that supports upstream communication will add objects to the OWIN
    ///    context that can be used to communicate with it prior to request processing.
    ///    For example the session middleware will add an object to the OWIN context that
    ///    other middleware can use to indicate whether session is required or not.
    /// 3) Middleware will retrieve the upstream communication objects and communicate
    ///    back upstream, for example the presentation layer may have a page level setting
    ///    indicating if session is required, the presentation middleware can use this
    ///    interface to access the object that allows it to communicate this information
    ///    to the session middleware.
    /// </summary>
    public interface IRoutingProcessor
    {
        /// <summary>
        /// This method of your middleware will be called during the routing phase
        /// of request processing. Your middleware should decide if this request
        /// should be handled by this middleware or not, and store the result of
        /// this decision in the owin context.
        /// If the request is for this middleware you should perform any upstream
        /// communication then return null.
        /// If the request is not for this middleware then you should call the next
        /// middleware and return whatever it returns.
        /// </summary>
        /// <param name="context">The owin context</param>
        /// <param name="next">The next middleware in the chain that implements 
        /// IRoutingProcessor</param>
        /// <returns>The result of calling next() or null</returns>
        Task RouteRequest(IOwinContext context, Func<Task> next);
    }
}
