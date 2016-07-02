using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This middleware writes the request url to the console output
    /// </summary>
    public class PrintRequest : IMiddleware<object>, IRoutingProcessor
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public PrintRequest()
        {
            Dependencies = new List<IDependency>();
            this.RunFirst();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine();
            Console.WriteLine("Processing " + context.Request.Uri);
            return next();
        }

        public Task RouteRequest(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine();
            Console.WriteLine("Routing " + context.Request.Uri);
            return next();
        }
    }
}
