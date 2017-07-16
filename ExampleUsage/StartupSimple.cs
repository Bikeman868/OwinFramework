using ExampleUsage.Middleware;
using Ioc.Modules;
using Ioc.Modules.Ninject;
using Ninject;
using Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;

namespace ExampleUsage
{
    /// <summary>
    /// This example demonstrates a very simple configuration of the Owin Framework. It 
    /// has these features:
    /// - Uses IoC.Modules and a Niject IoC container.
    /// - Builds a simple Owin pipeline with one route.
    /// - Does not configure any of the middleware components.
    /// </summary>
    public class StartupSimple
    {
        public void Configuration(IAppBuilder app)
        {
            // This example shows how to use the Ninject IoC container to construct the
            // Owin pipeline buider. You can use any other IoC container supported by 
            // the Ioc.Modules package with just one line of code change. You can also
            // choose not to use IoC, or configure any other IoC container you like.
            var packageLocator = new PackageLocator().ProbeAllLoadedAssemblies();
            var ninject = new StandardKernel(new Module(packageLocator));
            var builder = ninject.Get<IBuilder>().EnableTracing(RequestsToTrace.QueryString);

            // This next part defines the concrete implementation of the various
            // OWIN middleware components you want to use in your application. The
            // order that these will be chained into the OWIN pipeline will be
            // determined from the dependencies defined within the components.
            
            // This is a simplified example, in a real application you should provide a 
            // configuration mechanism. This example will run with all default 
            // configuration values.

            builder.Register(ninject.Get<NotFoundError>());
            builder.Register(ninject.Get<PrintRequest>());
            builder.Register(ninject.Get<ReportExceptions>());
            builder.Register(ninject.Get<FormsIdentification>());
            builder.Register(ninject.Get<TemplatePageRendering>());
            builder.Register(ninject.Get<AllowEverythingAuthorization>());
            builder.Register(ninject.Get<InProcessSession>());

            // The next few lines build the Owin pipeline. Note that the
            // Owin Framework builder adds to the pipeline but other middleware 
            // can also be added to the pipeline before or after it.

            app.UseErrorPage();
            app.UseBuilder(builder);
            app.UseWelcomePage("/");
        }
    }
}
