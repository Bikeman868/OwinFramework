using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExampleUsage.Middleware
{
    public class LegacyMiddleware3
    {
        private Func<IDictionary<string, object>, Task> _next;

        public void Initialize(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            Console.WriteLine("Before Legacy 3");
            return _next(environment)
                .ContinueWith(t => Console.WriteLine("After Legacy 3"));
        }
    }
}
