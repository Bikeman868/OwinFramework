using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Upstream;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This example always outputs "Hello, world" because I want to keep the focus on the OWIN configuration.
    /// This middleware demonstrates the following features:
    /// * It has an optional dependency on IIdentification, ISession and IAuthorization featuers. If these features 
    ///   are configured in the OWIN pipeline the builder will put them before this middleware, 
    ///   but if they are not configured this is not an error.
    /// </summary>
    public class TemplatePageRendering : IMiddleware<IPresentation>, IUpstreamCommunicator
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
        public void InvokeUpstream(IOwinContext context)
        {
            Console.WriteLine("Template page rendering middleware upstream invoked");

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

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine("Template page rendering middleware invoked");

            var authentication = context.GetFeature<IAuthorization>();
            if (authentication != null)
            {
                Console.WriteLine("Authorization is available to template page rendering middleware");
            }

            var identification = context.GetFeature<IIdentification>();
            if (identification != null)
            {
                Console.WriteLine("Identification is available to template page rendering middleware");
                if (identification.IsAnonymous)
                    Console.WriteLine("Rendering template page for anonymous user");
                else
                    Console.WriteLine("Rendering template page for user " + identification.Identity);
            }

            var session = context.GetFeature<ISession>();
            if (session != null)
            {
                Console.WriteLine("Session is available to template page rendering middleware");
                if (session.HasSession)
                    Console.WriteLine("User has a session in template page rendering middleware");
            }

            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync("<html><head><title>Example Usage</title></head><body>Hello, world</body></html>");
        }
    }
}
