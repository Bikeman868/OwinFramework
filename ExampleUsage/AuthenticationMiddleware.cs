using System.Collections.Generic;
using OwinFramework.Interfaces;
using OwinFramework.Builder;

namespace ExampleUsage
{
    public class AuthenticationMiddleware: IMiddleware<IAuthentication>, IConfigurable
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public AuthenticationMiddleware(IBuilder builder)
        {
            Dependencies = new List<IDependency>();
            this.RunAfter<ISession>();
            builder.Register(this);
        }

        public void Configure(IConfiguration configuration, string path)
        {
            var registration = configuration.Register(path, ConfigurationChanged, string.Empty);
            registration.Dispose();
        }

        private void ConfigurationChanged(string configuration)
        {
        }
    }
}
