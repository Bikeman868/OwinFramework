using ExampleUsage.Middleware;
using Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces;

namespace ExampleUsage
{
    /// <summary>
    /// This example demonstrates a a more complex scenario with routes and
    /// different authentication mechanisms for each route
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

            // This says that we want to use forms based authentication and that
            // we will refer to it by the name FormsAuthentication
            builder.Register(new FormsAuthentication())
                .As("FormsAuthentication")
                .ConfigureWith(configuration, "/owin/auth/forms");

            // This says that we want to use certificate based authentication and that
            // we will refer to it by the name CertificateAuthentication
            builder.Register(new CertificateAuthentication())
                .As("CertificateAuthentication")
                .ConfigureWith(configuration, "/owin/auth/cert");

            // This specifies the mechanism we want to use to store session
            builder.Register(new SessionMiddleware())
                .ConfigureWith(configuration, "/owin/session");

            // This specifies that we want to use the template page rendering
            // middleware and that it should run after FormsAuthentication middleware
            builder.Register(new TemplatePageRendering())
                .RunAfter<IAuthentication>("FormsAuthentication")
                .ConfigureWith(configuration, "/owin/templates");

            // This specifies that we want to use the REST service mapper
            // middleware and that it should run after CertificateAuthentication middleware
            builder.Register(new RestServiceMapper())
                .RunAfter<IAuthentication>("CertificateAuthentication")
                .ConfigureWith(configuration, "/owin/rest");

            // This statement will add all of the middleware registered with the builder into
            // the OWIN pipeline. The builder will add middleware to the pipeline in an order
            // that ensures all dependencies are met. The builder will also create a split in the
            // OWIN pipeline where there are routing middleware components configured.
            app.UseBuilder(builder);

            app.UseErrorPage();
            app.UseWelcomePage("/");
        }
    }
}
