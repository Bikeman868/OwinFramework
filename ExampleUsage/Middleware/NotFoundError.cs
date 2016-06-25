using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This illustrates a middleware component that needs to be at the back of the
    /// pipeline after all other middleware has run. It will always return a 404 
    /// response
    /// </summary>
    public class NotFoundError : IMiddleware<object>
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get { return _dependencies; } }

        private readonly IList<IDependency> _dependencies = new List<IDependency>();

        public NotFoundError()
        {
            // Tell the builder that this should be the last middleware to run
            this.RunLast();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine("PROCESS: Not found error");

            context.Response.StatusCode = 404;
            context.Response.ReasonPhrase = "Not Found";
            return context.Response.WriteAsync("<html><head><title>Not Found</title></head><body>The page was not found</body></html>");
        }
    }
}
