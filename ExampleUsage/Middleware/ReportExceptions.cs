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
    public class ReportExceptions : IMiddleware<object>
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get { return _dependencies; } }

        private readonly IList<IDependency> _dependencies = new List<IDependency>();

        public ReportExceptions()
        {
            // Tell the builder that this should be the first middleware to run
            this.RunFirst();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            try
            {
                return next();
            }
            catch (Exception)
            {
                Console.WriteLine("PROCESS: Exception reporter");

                context.Response.StatusCode = 200;
                context.Response.ReasonPhrase = "OK";
                return context.Response.WriteAsync("<html><head><title>Exception</title></head><body>An exception occurred</body></html>");
            }
        }
    }
}
