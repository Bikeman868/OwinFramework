using ExampleUsage.Middleware;
using Owin;
using OwinFramework;
using OwinFramework.Builder;
using OwinFramework.Interfaces;
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
            var dependencyTreeFactory = new DependencyTreeFactory();
            var builder = new Builder(dependencyTreeFactory);
            var configuration = new Configuration();

            // Note that the middleware components below can be registerd with the builder
            // in ant order. The builder will resolve dependencies to add middleware into 
            // the OWIN pipeline so that all dependencies are satisfied.

            // This says that we want to use forms based authentication, that
            // we will refer to it by the name 'FormsAuthentication', and it will
            // only be configured for the 'SecureUI' route.
            
            builder.Register(new FormsIdentification())
                .As("FormsAuthentication")
                .ConfigureWith(configuration, "/owin/auth/forms")
                .RunAfter<IRoute>("SecureUI");

            // This says that we want to use certificate based authentication, that
            // we will refer to it by the name 'CertificateAuthentication', and it will
            // only be configured for the 'API' route.
            builder.Register(new CertificateIdentification())
                .As("CertificateAuthentication")
                .ConfigureWith(configuration, "/owin/auth/cert")
                .RunAfter<IRoute>("API");

            // This specifies the mechanism we want to use to store session
            builder.Register(new InProcessSession())
                .ConfigureWith(configuration, "/owin/session");

            // This specifies that we want to use the template page rendering
            // middleware and that it should run on both the "PublicUI" route and
            // the "SecureUI" route.
            builder.Register(new TemplatePageRendering())
                .RunAfter<IRoute>("PublicUI")
                .RunAfter<IRoute>("SecureUI")
                .ConfigureWith(configuration, "/owin/templates");

            // This specifies that we want to use the REST service mapper
            // middleware and that it should run after CertificateAuthentication middleware
            // Note that we could also have told it to RunAfter<IRoute>("API") and the
            // resulting OWIN pipeline would be the same. For belts and braces we could
            // also add both dependencies.
            builder.Register(new RestServiceMapper())
                .RunAfter<IAuthorization>("CertificateAuthentication")
                .ConfigureWith(configuration, "/owin/rest");

            // This configures a routing element that will split the OWIN pipeline into
            // two routes. There is a 'UI' route that has forms based authentication and 
            // template based rendering. There is an 'API' route that has certifcate based
            // authentication and REST service rendering. If also specifies that routing
            // runs after session, so both routes will use the same session middleware. If
            // you condifure more than one session middleware in this scenario then an exception
            // will be thrown at startup.
            builder.Register(new Router(dependencyTreeFactory))
                .AddRoute("UI", context => context.Request.Path.Value.EndsWith(".aspx"))
                .AddRoute("API", context => true)
                .RunAfter<ISession>(null, false);

            // This configures another routing split that divides the 'UI' route into secure
            // and public routes called 'SecureUI' and 'PublicUI'.
            builder.Register(new Router(dependencyTreeFactory))
                .AddRoute("SecureUI", context => context.Request.Path.Value.StartsWith("/secure"))
                .AddRoute("PublicUI", context => true)
                .RunAfter<IRoute>("UI");

            // This statement will add all of the middleware registered with the builder into
            // the OWIN pipeline. The builder will add middleware to the pipeline in an order
            // that ensures all dependencies are met. The builder will also create splits in the
            // OWIN pipeline where there are routing components configured.
            app.UseBuilder(builder);

            app.UseErrorPage();
            app.UseWelcomePage("/");
        }
    }
}
