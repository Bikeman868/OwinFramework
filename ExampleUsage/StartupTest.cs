using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExampleUsage.Middleware;
using Microsoft.Owin;
using Owin;
using OwinFramework.Builder;
using OwinFramework.Configuration;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Utility;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.Routing;
using OwinFramework.Utility;

namespace ExampleUsage
{
    // This startup was used to test and debug different scenarios. Since this
    // class is constantly changing it is not documented. For documented examples
    // refer to one of the other sample strtup classes.
    public class StartupTest
    {
        public void Configuration(IAppBuilder app)
        {
            IDependencyGraphFactory dependencyGraphFactory = new DependencyGraphFactory();
            ISegmenterFactory segmenterFactory = new SegmenterFactory(dependencyGraphFactory);
            IBuilder builder = new Builder(dependencyGraphFactory, segmenterFactory);
            IConfiguration config = new DefaultValueConfiguration();

            var uiPath = new PathString("/ui");
            builder.Register(new Router(dependencyGraphFactory))
                .AddRoute("ui", c => c.Request.Path.StartsWithSegments(uiPath))
                .As("Router");

            builder.Register(new OutputCache())
                .As("OutputCache")
                .RunOnRoute("ui")
                .ConfigureWith(config, "/ui/outputCache");

            builder.Register(new TestMiddleware2())
                .As("Versioning")
                .RunOnRoute("ui")
                .ConfigureWith(config, "/ui/versioning");

            builder.Register(new TestMiddleware2())
                .As("Dart")
                .RunOnRoute("ui")
                .ConfigureWith(config, "/ui/dart");

            builder.Register(new TestMiddleware2())
                .As("Less")
                .RunAfter("Dart")
                .RunAfter("Versioning")
                .RunOnRoute("ui")
                .ConfigureWith(config, "/ui/less");

            builder.Register(new TestMiddleware3()).
                As("Static files")
                .RunAfter("Dart")
                .RunAfter("Versioning")
                .RunOnRoute("ui")
                .ConfigureWith(config, "/ui/staticFiles");

            builder.Register(new TestMiddleware1())
                .As("Route visualizer")
                .RunOnRoute("ui")
                .ConfigureWith(config, "/ui/visualizer");

            app.UseBuilder(builder);
        }

        private class TestMiddleware: IMiddleware<object>
        {
            public string Name { get; set; }

            private readonly IList<IDependency> _dependencies = new List<IDependency>();
            public IList<IDependency> Dependencies { get { return _dependencies; } }

            public Task Invoke(IOwinContext context, Func<Task> next)
            {
                return context.Response.WriteAsync(Name);
            }
        }

        private class TestMiddleware1 : TestMiddleware
        {
            public TestMiddleware1()
            {
                this.RunAfter<IAuthorization>(null, false);
            }
        }

        private class TestMiddleware2 : TestMiddleware
        {
            public TestMiddleware2()
            {
                this.RunAfter<IOutputCache>(null, false);
            }
        }

        private class TestMiddleware3 : TestMiddleware
        {
            public TestMiddleware3()
            {
                this.RunAfter<IOutputCache>(null, false);
                this.RunAfter<IAuthorization>(null, false);
            }
        }
    }
}
