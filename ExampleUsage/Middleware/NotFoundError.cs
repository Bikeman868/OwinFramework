using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.InterfacesV1.Capability;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This illustrates a middleware component that needs to be at the back of the
    /// pipeline after all other middleware has run. It will always return a 404 
    /// response
    /// </summary>
    public class NotFoundError : IMiddleware<object>, IConfigurable
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get { return _dependencies; } }

        private readonly IList<IDependency> _dependencies = new List<IDependency>();
        private Configuration _configuration;
        private IDisposable _configurationRegistration;

        public NotFoundError()
        {
            // Tell the builder that this should be the last middleware to run
            this.RunLast();

            // Establish default configuration for the case where the application
            // does not provide a configuration mechanism
            _configuration = new Configuration();
        }

        /// <summary>
        /// Note that implementing IConfigurable is optional in your middleware
        /// </summary>
        void IConfigurable.Configure(IConfiguration configuration, string path)
        {
            _configurationRegistration = configuration.Register(
                path,
                cfg =>
                {
                    _configuration = cfg;
                    Console.WriteLine("CONFIGURE: not found error '" + Name + "' from " + path);
                },
                new Configuration());
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine("PROCESS: Not found error " + Name);

            context.Response.StatusCode = 404;
            context.Response.ReasonPhrase = "Not Found";
            return context.Response.WriteAsync(
                "<html><head><title>Not Found</title></head><body>" +
                _configuration.Body+"</body></html>");
        }

        public class Configuration
        {
            public string Body { get; set; }

            public Configuration()
            {
                Body = "The page was not found";
            }
        }
    }
}
