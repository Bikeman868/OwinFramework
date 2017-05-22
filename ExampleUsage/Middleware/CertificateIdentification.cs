using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.MiddlewareHelpers.Identification;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This middleware example demonstrates the following techniques:
    /// * It injects the IIdentification feature into the OWIN context
    /// * It is configurable
    /// </summary>
    public class CertificateIdentification: IMiddleware<IIdentification>, IConfigurable
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public CertificateIdentification()
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
                cfg => Console.WriteLine("CONFIGURE: Certificate identification '" + Name + "' from " + path),
                string.Empty);
            registration.Dispose();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine("PROCESS: Certificate identification");

            // A real implementation would check client certificates associated with
            // the request at this point to establish verified claims based on the
            // contents of the certificate
            context.SetFeature<IIdentification>(new Identification(
                Guid.NewGuid().ToString("N"), 
                new []{
                    new IdentityClaim (ClaimNames.Domain, "www.certdomain.com", ClaimStatus.Verified)
                    }));

            return next();
        }

    }
}
