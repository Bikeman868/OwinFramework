using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Interfaces.Builder;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This illustrates the minimum code required to be a valid middleware component.
    /// This middleware does nothing and is not configurable.
    /// </summary>
    public class DoNothing : IMiddleware<object>
    {
        /// <summary>
        /// The Name property is used to specify dependencies and also for documentation
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Dependencies property is required so that the application can specify
        /// dependencies between middleware components.
        /// </summary>
        public IList<IDependency> Dependencies { get; private set; }

        public DoNothing()
        {
            // The Dependencies property is not allowed to be null, it is used by
            // the algorithms that resolves middleware dependencies.
            Dependencies = new List<IDependency>();
        }

        /// <summary>
        /// This method is called once for each request received by the server. The code
        /// should either return an async task, or call the next middleware in the pipeline.
        /// This is standard OWIN, nothing special here.
        /// </summary>
        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            // Invoke the next middleware in the OWIN pipeline
            return next();
        }
    }
}
