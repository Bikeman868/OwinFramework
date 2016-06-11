using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;
using OwinFramework.Interfaces.Upstream;
using OwinFramework.Interfaces.Utility;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This example always outputs "Hello, world" because I want to keep the focus on the OWIN configuration.
    /// This middleware demonstrates the following features:
    /// * It has an optional dependency on IIdentification, ISession and IAuthorization featuers. If these features 
    ///   are configured in the OWIN pipeline the builder will put them before this middleware, 
    ///   but if they are not configured this is not an error.
    /// </summary>
    public class TemplatePageRendering : IMiddleware<IPresentation>, IRoutingProcessor
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public TemplatePageRendering()
        {
            Dependencies = new List<IDependency>();

            // These statements will add to the Dependencies list
            this.RunAfter<IIdentification>(null, false);
            this.RunAfter<ISession>(null, false);
            this.RunAfter<IAuthorization>(null, false);
        }

        /// <summary>
        /// This is called during the routing phase, before the request processing starts
        /// and allows this middleware to alter the behavour of middleware further up the pipeline
        /// </summary>
        public void RouteRequest(IOwinContext context, Action next)
        {
            Console.WriteLine("ROUTE: Template page rendering");

            // At this point a real template rendering middleware would figure out from
            // the request which template it is going to render (if any) and from that
            // figure out what's needed in the pipeline - for example does the request
            // have to come fromm a logged on authenticated user with certain permissions
            // For the purpose of this example I am saying any aspx page is a template
            if (context.Request.Method == "GET" 
                && context.Request.Path.HasValue
                && context.Request.Path.Value.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
            {
                context.Set("templateToRender", context.Request.Path.Value);

                // Get upstream communication interfaces if available
                var upstreamSession = context.GetFeature<IUpstreamSession>();
                var upstreamIdentification = context.GetFeature<IUpstreamIdentification>();

                // Tell the session middleware that a session is required for this request
                if (upstreamSession != null)
                    upstreamSession.SessionRequired = true;

                // Tell the identification middleware that a anonymous users are ok for this request
                if (upstreamIdentification != null)
                    upstreamIdentification.AllowAnonymous = true;
            }

            // Invoke the next middleware in the chain
            next();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            // This value was set during the routing phase only if this is 
            // request for a template page 
            var templateToRender = context.Get<string>("templateToRender");

            if (string.IsNullOrEmpty(templateToRender))
                return next();

            Console.WriteLine("PROCESS: Template page rendering");

            var authentication = context.GetFeature<IAuthorization>();
            if (authentication != null)
            {
                Console.WriteLine("  Authorization is available");
            }

            var identification = context.GetFeature<IIdentification>();
            if (identification != null)
            {
                Console.WriteLine("  Identification is available");
                if (identification.IsAnonymous)
                    Console.WriteLine("  Rendering template page for anonymous user");
                else
                    Console.WriteLine("  Rendering template page for user " + identification.Identity);
            }

            var session = context.GetFeature<ISession>();
            if (session != null)
            {
                Console.WriteLine("  Session feature is available");
                if (session.HasSession)
                    Console.WriteLine("  User has a session");
                else
                    Console.WriteLine("  User does not have a session");
            }

            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync("<html><head><title>Example Usage</title></head><body>Hello, world</body></html>");
        }
    }
}
