using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Interfaces.Builder;
using OwinFramework.InterfacesV1.Middleware;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This example always outputs a very simple JSON response because I want to keep the
    /// focus on the OWIN configuration and not clutter this example with complex
    /// application logic.
    /// 
    /// In a real application the middleware would examine the OWIN context and produce 
    /// different output for different requests.
    /// </summary>
    public class RestServiceMapper : IMiddleware<IPresentation>
    {
        public string Name { get; set; }

        private readonly IList<IDependency> _dependencies = new List<IDependency>();
        public IList<IDependency> Dependencies { get { return _dependencies; } }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine("PROCESS: REST service wrapper");

            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(@"{""name"":""myName""}");
        }
    }
}
