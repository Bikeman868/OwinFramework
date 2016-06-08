using System;
using Microsoft.Owin;

namespace OwinFramework.Interfaces.Routing
{
    /// <summary>
    /// Middleware components implement this interface when they want to be included
    /// in the routing stage of request processing. The routing stage establishes the
    /// route that the request will take through the middleware components, once this
    /// stage is complete there is a second request processing stage.
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
        void RouteRequest(IOwinContext context, Action next);
    }
}
