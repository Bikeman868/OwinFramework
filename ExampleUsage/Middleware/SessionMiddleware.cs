using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces;

namespace ExampleUsage
{
    public class SessionMiddleware: IMiddleware<ISession>
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public SessionMiddleware()
        {
            Dependencies = new List<IDependency>();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine("Session middleware invoked");

            context.SetFeature<ISession>(new Session());
            return next.Invoke();
        }

        private class Session: ISession
        {
        }
    }
}
