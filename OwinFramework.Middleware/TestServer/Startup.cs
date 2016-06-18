using Owin;
using OwinFramework.Builder;
using OwinFramework.RouteVisualizer;
using OwinFramework.Utility;

namespace TestServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var dependencyGraphFactory = new DependencyGraphFactory();
            var segmenterFactory = new SegmenterFactory(dependencyGraphFactory);
            var configuration = new DefaultValueConfiguration();
            var builder = new Builder(dependencyGraphFactory, segmenterFactory);

            builder.Register(new RouteVisualizer())
                .As("RouteVisualizer")
                .ConfigureWith(configuration, "/routeVisualizer")
                .RunFirst();

            app.UseBuilder(builder);
        }
    }
}
