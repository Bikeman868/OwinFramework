using OwinFramework.Builder;

namespace ExampleUsage
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new Builder();
            var configuration = new Configuration();

            var authentication = new AuthenticationMiddleware(builder)
                .As("authentication")
                .ConfigureWith(configuration, "/owin/authentication");

            var session = new SessionMiddleware(builder)
                .As("session")
                .ConfigureWith(configuration, "/owin/session");

            builder.Build();
        }
    }
}
