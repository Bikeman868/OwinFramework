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
    /// </summary>
    public class CertificateAuthentication: IMiddleware<IAuthentication>, IConfigurable
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public CertificateAuthentication()
        {
            Dependencies = new List<IDependency>();
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
                    Console.WriteLine("Certificate authentication middleware configured");
                },
                string.Empty);
            registration.Dispose();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine("Certificate authentication middleware invoked");

            // Check client certificates to ensure client is authenticated

            context.SetFeature<IAuthentication>(new Authentication());
            return next.Invoke();
        }

        private class Authentication : IAuthentication
        {
        }
    }
}
