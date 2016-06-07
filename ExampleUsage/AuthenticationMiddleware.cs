using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Interfaces;
using OwinFramework.Builder;

namespace ExampleUsage
{
    public class AuthenticationMiddleware: IMiddleware<IAuthentication>, IConfigurable
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public AuthenticationMiddleware()
        {
            Dependencies = new List<IDependency>();
            this.RunAfter<ISession>();
        }

        public void Configure(IConfiguration configuration, string path)
        {
            var registration = configuration.Register(path, ConfigurationChanged, string.Empty);
            registration.Dispose();
        }

        private void ConfigurationChanged(string configuration)
        {
            Console.WriteLine("Authentication middleware configured");
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine("Authentication middleware invoked");

            var session = context.GetFeature<ISession>();
            // Do stuff with session here

            context.SetFeature<IAuthentication>(new Authentication());
            return next.Invoke();
        }

        private class Authentication : IAuthentication
        {
        }
    }
}
