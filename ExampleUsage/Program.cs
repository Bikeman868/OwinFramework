using System;
using Microsoft.Owin.Hosting;
using Owin;
using OwinFramework.Builder;

namespace ExampleUsage
{
    class Program
    {
        static void Main(string[] args)
        {
            using (WebApp.Start<Startup>("http://localhost:12345"))
            {
                Console.ReadLine();
            }
        }
    }

    public class StartupSimple
    {
        public void Configuration(IAppBuilder app)
        {
            // Note that I did not use an IOC container here to keep things as
            // simple and focused as possible. You should use IOC in your application.

            var dependencyTreeFactory = new DependencyTreeFactory();
            var builder = new Builder(dependencyTreeFactory);
            var configuration = new Configuration();

            // This next part defines the concrete implementation of the various
            // OWIN middleware components you want to use in your application. The
            // order that these will be chained into the OWIN pipeline will be
            // determined from the dependencies defined within the components. This
            // is a simplified example, in a real application you should use IOC
            // to build your middleware components and supply their dependencies.

            builder.Register(new AuthenticationMiddleware())
                .As("authentication")
                .ConfigureWith(configuration, "/owin/authentication");

            builder.Register(new SessionMiddleware())
                .As("session")
                .ConfigureWith(configuration, "/owin/session");

            // This is standard OWIN configuration statements. As well as using the
            // builder to chain middleware into the pipeline, you can also chain
            // any other middleware here

            app.UseBuilder(builder);
            app.UseErrorPage();
            app.UseWelcomePage("/");
        }
    }
}
