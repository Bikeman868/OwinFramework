using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExampleUsage.Middleware
{
    public class LegacyMiddleware1
    {
        private readonly Func<IDictionary<string, object>, Task> _next;

        public LegacyMiddleware1(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            Console.WriteLine("Before Legacy 1");
            return _next(environment)
                .ContinueWith(t => Console.WriteLine("After Legacy 1"));
        }
    }
}
