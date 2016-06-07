using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This example always outputs "Hello, world" because I want to keep the focus on the OWIN configuration.
    /// This middleware demonstrates the following features:
    /// * It has an optional dependency on ISession and IAuthentication featuers. If these features 
    ///   are configured in the OWIN pipeline the builder will put them before this middleware, 
    ///   but if they are not configured this is not an error.
    /// </summary>
    public class TemplatePageRendering : IMiddleware<IRendering>
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public TemplatePageRendering()
        {
            Dependencies = new List<IDependency>();

            // These statements will add to the Dependencies list
            this.RunAfter<ISession>(null, false);
            this.RunAfter<IAuthentication>(null, false);
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine("Template page rendering middleware invoked");

            var authentication = context.GetFeature<IAuthentication>();
            if (authentication != null)
            {
                // Authentication is available
            }

            var session = context.GetFeature<ISession>();
            if (session != null)
            {
                // Session is available
            }

            context.Response.ContentType = "text/html";
            return context.Response.WriteAsync("<html><head><title>Example Usage</title></head><body>Hello, world</body></html>");
        }
    }
}
