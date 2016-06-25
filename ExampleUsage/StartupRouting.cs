using ExampleUsage.Middleware;
using Owin;
using OwinFramework.Builder;
using OwinFramework.Configuration;
using OwinFramework.Interfaces;
using OwinFramework.Interfaces.Middleware;
using OwinFramework.Interfaces.Routing;
using OwinFramework.Routing;
using OwinFramework.Utility;

namespace ExampleUsage
{
    /// <summary>
    /// This example demonstrates a a more complex scenario with routes and
    /// different authentication mechanisms for each route. The resulting
    /// OWIN pipeline will be built like this:
    /// 
    /// session --> aspx pages --ui---> /secure --SecureUI--> forms auth -----> template rendering
    ///          |                  |                                        |
    ///          |                  --> not /secure --PublicUI---------------^
    ///          |
    ///          -> non aspx --api--> cert auth ----> REST service rendering
    /// </summary>
    public class StartupRouting
    {
        public void Configuration(IAppBuilder app)
        {
            // This demonstrates how you would configure the builder without using IoC
            // The other startup example demonstrates the IoC version
            var dependencyGraphFactory = new DependencyGraphFactory();
            var segmenterFactory = new SegmenterFactory(dependencyGraphFactory);
            var builder = new Builder(dependencyGraphFactory, segmenterFactory);
            var configuration = new DefaultValueConfiguration();

            // Note that the middleware components below can be registerd with the builder
            // in any order. The builder will resolve dependencies and add middleware into 
            // the OWIN pipeline so that all dependencies are satisfied. If there are
            // circular dependencies an exception will be thrown.

            // These middleware are configured to run at the front and back of the OWIN
            // pipeline so not much to configure here.
            builder.Register(new NotFoundError());
            builder.Register(new ReportExceptions());

            // This says that we want to use forms based identification, that
            // we will refer to it by the name 'loginId', and it will
            // only be configured for the 'secure' route.
            builder.Register(new FormsIdentification())
                .As("loginId")
                .ConfigureWith(configuration, "/owin/auth/forms")
                .RunAfter<IRoute>("secure");

            // This says that we want to use certificate based identification, that
            // we will refer to it by the name 'certificateId', and it will
            // only be configured for the 'api' route.
            builder.Register(new CertificateIdentification())
                .As("certificateId")
                .ConfigureWith(configuration, "/owin/auth/cert")
                .RunAfter<IRoute>("api");

            // This specifies the mechanism we want to use to store session
            builder.Register(new InProcessSession())
                .RunAfter<IIdentification>("loginId") // TODO: remove when route construction is working
                .ConfigureWith(configuration, "/owin/session");

            // This configures a routing element that will split the OWIN pipeline into
            // two routes. There is a 'ui' route that has forms based authentication and 
            // template based rendering. There is an 'api' route that has certifcate based
            // authentication and REST service rendering.
            builder.Register(new Router(dependencyGraphFactory))
                .AddRoute("ui", context => context.Request.Path.Value.EndsWith(".aspx"))
                .AddRoute("api", context => true);

            // This configures another routing split that divides the 'ui' route into 
            // 'secure' and 'public' routes.
            builder.Register(new Router(dependencyGraphFactory))
                .AddRoute("secure", context => context.Request.Path.Value.StartsWith("/secure"))
                .AddRoute("public", context => true)
                .RunAfter<IRoute>("ui");

            // This specifies that we want to use the template page rendering
            // middleware and that it should run on both the "public" route and
            // the "secure" route. This creates a join bewteen the routes.
            builder.Register(new TemplatePageRendering())
                .RunAfter<IRoute>("public")
                .RunAfter<IRoute>("secure")
                .ConfigureWith(configuration, "/owin/templates");

            // This specifies that we want to use the REST service mapper
            // middleware and that it should run after the 'certificateId' middleware
            // Note that we could also have told it to RunAfter<IRoute>("api") and the
            // resulting OWIN pipeline would be the same. For belts and braces we could
            // also add both dependencies.
            builder.Register(new RestServiceMapper())
                .RunAfter("certificateId")
                .ConfigureWith(configuration, "/owin/rest");

            // This statement will add all of the middleware registered with the builder into
            // the OWIN pipeline. The builder will add middleware to the pipeline in an order
            // that ensures all dependencies are met. The builder will also create splits in the
            // OWIN pipeline where there are routing components configured, and joins where
            // middleware has a dependency on multiple routes.
            app.UseBuilder(builder);

            app.UseErrorPage();
            app.UseWelcomePage("/");
        }
    }
}
