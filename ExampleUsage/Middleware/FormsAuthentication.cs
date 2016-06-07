using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This middleware example demonstrates the following techniques:
    /// * It injects the IAuthentication feature into the OWIN context
    /// * It is configurable
    /// * It has a dependency on the ISession feature which must be present
    ///   in the OWIN context. The builder will throw an exception at startup
    ///   if there are no middleware components configured to inject ISession
    /// </summary>
    public class FormsAuthentication: IMiddleware<IAuthentication>, IConfigurable
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public FormsAuthentication()
        {
            Dependencies = new List<IDependency>();
            this.RunAfter<ISession>();
        }

        /// <summary>
        /// Note that implementing IConfigurable is optional in your middleware
        /// </summary>
        void IConfigurable.Configure(IConfiguration configuration, string path)
        {
            var registration = configuration.Register(
                path,
                cfg =>
                {
                    Console.WriteLine("Forms authentication middleware configured");
                },
                string.Empty);
            registration.Dispose();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine("Forms authentication middleware invoked");

            var session = context.GetFeature<ISession>();
            // Check session to see if the user is authenticated

            context.SetFeature<IAuthentication>(new Authentication());
            return next.Invoke();
        }

        private class Authentication : IAuthentication
        {
        }
    }
}
