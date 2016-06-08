using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Interfaces;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;

namespace OwinFramework.Routing
{
    public class Router : IRouter
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public Router()
        {
            Dependencies = new List<IDependency>();
        }

        public IRouter Add(string routeName, Func<IOwinContext, bool> filterExpression)
        {
            return this;
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            return next.Invoke();
        }
    }
}
