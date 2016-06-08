using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This illustrates the minimum code required to be a valid middleware component
    /// </summary>
    public class DoNothing : IMiddleware<IMiddleware>
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public DoNothing()
        {
            Dependencies = new List<IDependency>();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            return next.Invoke();
        }
    }
}
