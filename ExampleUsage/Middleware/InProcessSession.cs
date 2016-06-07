using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This middleware demonstrates the minimum footprint for a middleware
    /// component. It has no dependencies and can not be configured. It does
    /// inject the ISession feature into the OWIN context, this will make
    /// this middleware run before any middleware that depends on ISession
    /// </summary>
    public class InProcessSession : IMiddleware<ISession>
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public InProcessSession()
        {
            Dependencies = new List<IDependency>();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine("Session middleware invoked");

            context.SetFeature<ISession>(new Session());
            return next.Invoke();
        }

        private class Session: ISession
        {
        }
    }
}
