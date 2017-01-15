using ExampleUsage.Middleware;
using Owin;
using OwinFramework.Builder;
using OwinFramework.Configuration;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Utility;
using OwinFramework.Routing;
using OwinFramework.Utility;

namespace ExampleUsage
{
    /// <summary>
    /// This example demonstrates a a more complex scenario with routes and
    /// different authentication mechanisms for each route. The resulting
    /// OWIN pipeline will be built like this:
    /// 
    /// session --> aspx pages --ui---> /secure --secure--> forms auth -----> template rendering
    ///          |                  |                                      |
    ///          |                  --> not /secure --public---------------^
    ///          |
    ///          -> non aspx --api--> cert auth ----> REST service rendering
    /// 
    /// Try these URLs and look at the putput in the console window:
    ///   http://localhost:12345/test.aspx
    ///   http://localhost:12345/secure/test.aspx
    ///   http://localhost:12345/api/user/98765
    ///   http://localhost:12345/test.jpg
    ///   http://localhost:12345/api/test.jpg
    ///   http://localhost:12345/secure/test.jpg
    /// </summary>
    public class StartupRouting
    {
        public void Configuration(IAppBuilder app)
        {
            // This demonstrates how you would configure the builder without using IoC
            // There are other startup examples in this project that demonstrate the IoC version
            IDependencyGraphFactory dependencyGraphFactory = new DependencyGraphFactory();
            ISegmenterFactory segmenterFactory = new SegmenterFactory(dependencyGraphFactory);
            IBuilder builder = new Builder(dependencyGraphFactory, segmenterFactory);
            IConfiguration configuration = new DefaultValueConfiguration();

            // Note that the middleware components below can be registerd with the builder
            // in any order. The builder will resolve dependencies and add middleware into 
            // the OWIN pipeline so that all dependencies are satisfied. If there are
            // circular dependencies an exception will be thrown.

            // This middleware is built to run at the front of the Owin pipeline
            // so not much more to set up here.
            builder.Register(new ReportExceptions());

            // Configure different 404 behaviours for different routes.
            // Note that NotFoundError middleware is built to run at the end of the Owin
            // pipeline after all other middleware. If no other middleware handled the 
            // request then it returns a 404 response.
            builder.Register(new NotFoundError())
                .As("apiNotFoundError")
                .RunOnRoute("api")
                .ConfigureWith(configuration, "/owin/notFound/api");

            builder.Register(new NotFoundError())
                .As("uiNotFoundError")
                .RunOnRoute("ui")
                .ConfigureWith(configuration, "/owin/notFound/ui");

            builder.Register(new NotFoundError())
                .As("staticFilesNotFoundError")
                .RunOnRoute("staticFiles")
                .ConfigureWith(configuration, "/owin/notFound/staticFiles");

            builder.Register(new NotFoundError())
                .As("invalidNotFoundError")
                .RunOnRoute("invalid")
                .ConfigureWith(configuration, "/owin/notFound/invalid");

            // This says that we want to use forms based identification, that
            // we will refer to it by the name 'loginId', and it will
            // only be configured for the 'secure' route. This also defines
            // the configureation mechanism and specifies where to get config
            // for this instance in the config file.
            builder.Register(new FormsIdentification())
                .As("loginId")
                .ConfigureWith(configuration, "/owin/auth/forms")
                .RunOnRoute("secure");

            // This says that we want to use certificate based identification, that
            // we will refer to it by the name 'certificateId'.
            builder.Register(new CertificateIdentification())
                .As("certificateId")
                .ConfigureWith(configuration, "/owin/auth/cert");

            // This specifies the mechanism we want to use to store session.
            // We don't need to specify anything else because each middleware knows
            // already whether it needs session or not and the builder will ensure
            // that session is included in the pipeline before anything that needs it.
            builder.Register(new InProcessSession())
                .ConfigureWith(configuration, "/owin/session");

            // This configures a routing element that will split the OWIN pipeline into
            // two routes. There is a 'ui' route that has forms based authentication and 
            // template based rendering. There is an 'api' route that has certifcate based
            // authentication and REST service rendering.
            // Since this router does not have any route dependencies it will run directly
            // off the incomming request.
            builder.Register(new Router(dependencyGraphFactory))
                .AddRoute("ui", context => context.Request.Path.Value.EndsWith(".aspx"))
                .AddRoute("staticFiles", context =>
                {
                    var path = context.Request.Path.Value;
                    var fileExtensionIndex = path.LastIndexOf('.');
                    if (fileExtensionIndex < 0) return false;

                    var fileExtension = path.Substring(fileExtensionIndex).ToLower();
                    if (fileExtension == ".html") return true;
                    if (fileExtension == ".css") return true;
                    if (fileExtension == ".js") return true;
                    if (fileExtension == ".jpg") return true;
                    if (fileExtension == ".png") return true;
                    return false;
                })
                .AddRoute("api", context => context.Request.Path.Value.StartsWith("/api/"))
                .AddRoute("invalid", context => true);

            // This configures another routing split that divides the 'ui' route into 
            // 'secure' and 'public' routes.
            builder.Register(new Router(dependencyGraphFactory))
                .AddRoute("secure", context => context.Request.Path.Value.StartsWith("/secure"))
                .AddRoute("public", context => true)
                .RunOnRoute("ui");

            // This specifies that we want to use the template page rendering
            // middleware and that it should run on both the "public" route and
            // the "secure" route. This creates a join bewteen the routes.
            builder.Register(new TemplatePageRendering())
                .RunOnRoute("public")
                .RunOnRoute("secure")
                .ConfigureWith(configuration, "/owin/templates");

            // This specifies that we want to use the REST service mapper on the 'api' route
            // and that it should run after the 'certificateId' middleware
            builder.Register(new RestServiceMapper())
                .RunAfter("certificateId")
                .RunOnRoute("api")
                .ConfigureWith(configuration, "/owin/rest");

            // This is an example of how to include middleware that was not built to work
            // with the OWIN Framework. This technique allows any other middleware to work
            // with the OWIN Framework without modification.
            var welcomePageWrapper = new LegacyMiddlewareWrapper();
            welcomePageWrapper.UseWelcomePage("/");
            builder.Register(welcomePageWrapper)
                .RunFirst();

            // This statement will add all of the middleware registered with the builder into
            // the OWIN pipeline. The builder will add middleware to the pipeline in an order
            // that ensures all dependencies are met.
            // The builder will also create splits in the OWIN pipeline where there are routing 
            // components configured, and joins where middleware has a dependency on multiple 
            // routes.
            // If you want to see exactly how the Owin pipeline got built, there is a 
            // PipelineVisualizer middleware in the OwinFramework.Middleware package that you
            // can add to your configuraton. This middleware will return an SVG vizualization
            // of the pipeline including configurations and analytics.
            app.UseBuilder(builder);

            // Anything that you do with the builder after this point will have no effect on
            // to Owin pipeline which has already been built.
        }
    }
}
