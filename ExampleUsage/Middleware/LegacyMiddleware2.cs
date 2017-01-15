using System;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace ExampleUsage.Middleware
{
    public class LegacyMiddleware2: OwinMiddleware
    {
        private readonly OwinMiddleware _next;

        public LegacyMiddleware2(OwinMiddleware next)
            : base(next)
        {
            _next = next;
        }

        public override Task Invoke(IOwinContext owinContext)
        {
            Console.WriteLine("Before Legacy 2");
            return _next.Invoke(owinContext)
                .ContinueWith(t => Console.WriteLine("After Legacy 2"));
        }
    }
}
