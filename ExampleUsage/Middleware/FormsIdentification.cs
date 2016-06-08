using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces;
using OwinFramework.Interfaces.Builder;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This middleware example demonstrates the following techniques:
    /// * It injects the IIdentification feature into the OWIN context
    /// * It is configurable
    /// </summary>
    public class FormsIdentification: IMiddleware<IIdentification>, IConfigurable
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public FormsIdentification()
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
                    Console.WriteLine("CONFIGURE: Forms identification");
                },
                string.Empty);
            registration.Dispose();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine("PROCESS: Forms identification");

            context.SetFeature<IIdentification>(new Identification());

            return next.Invoke();
        }

        /// <summary>
        /// Basic implementation of IIdentification for illustration purposes only
        /// </summary>
        private class Identification : IIdentification
        {
            public string Identity { get; private set; }
            public bool IsAnonymous { get { return true; } }

            public Identification()
            {
                Identity = Guid.NewGuid().ToString("N");
            }
        }
    }
}
