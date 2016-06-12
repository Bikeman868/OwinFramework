using ExampleUsage.Middleware;
using Owin;
using OwinFramework.Builder;
using OwinFramework.Utility;

namespace ExampleUsage
{
    /// <summary>
    /// This example demonstrates a very simple configuration
    /// </summary>
    public class StartupSimple
    {
        public void Configuration(IAppBuilder app)
        {
            // Note that I did not use an IOC container here to keep things as
            // simple and focused as possible. You should use IOC in your application.

            var dependencyGraphFactory = new DependencyGraphFactory();
            var builder = new Builder(dependencyGraphFactory);

            // This next part defines the concrete implementation of the various
            // OWIN middleware components you want to use in your application. The
            // order that these will be chained into the OWIN pipeline will be
            // determined from the dependencies defined within the components.
            
            // This is a simplified example, in a real application you should use IOC
            // to build your middleware components, and you should provide a configuration
            // for them.

            builder.Register(new NotFoundError());
            builder.Register(new ReportExceptions());
            builder.Register(new FormsIdentification());
            builder.Register(new TemplatePageRendering());
            builder.Register(new AllowEverythingAuthorization());
            builder.Register(new InProcessSession());

            // As well as using the builder to chain middleware into the pipeline, you can also 
            // chain any other middleware here. Below are some standard OWIN configuration 
            // statements that show this.

            app.UseBuilder(builder);
            app.UseErrorPage();
            app.UseWelcomePage("/");
        }
    }
}
