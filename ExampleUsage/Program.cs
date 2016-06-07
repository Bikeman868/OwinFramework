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

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Note that I did not use an IOC container here to keep things as
            // simple and focused as possible. You should use IOC in your application

            var dependencyTreeFactory = new DependencyTreeFactory();
            var builder = new Builder(dependencyTreeFactory);
            var configuration = new Configuration();

            builder.Register(new AuthenticationMiddleware())
                .As("authentication")
                .ConfigureWith(configuration, "/owin/authentication");

            builder.Register(new SessionMiddleware())
                .As("session")
                .ConfigureWith(configuration, "/owin/session");

            app.UseBuilder(builder);
            app.UseErrorPage();
            app.UseWelcomePage("/");
        }
    }
}
