using ExampleUsage.Middleware;
using Ioc.Modules;
using Ninject;
using Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;

namespace ExampleUsage
{
    /// <summary>
    /// This example demonstrates a very simple configuration
    /// </summary>
    public class StartupSimple
    {
        public void Configuration(IAppBuilder app)
        {
            // This example shows how to use the Ninject IoC container to construct the
            // Owin pipeline buider. You can use any other IoC container supported by 
            // the Ioc.Modules package with just one line of code change.
            var packageLocator = new PackageLocator().ProbeAllLoadedAssemblies();
            var ninject = new StandardKernel(new Ioc.Modules.Ninject.Module(packageLocator));
            var builder = ninject.Get<IBuilder>();

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
