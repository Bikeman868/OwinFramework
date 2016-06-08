using System;
using Microsoft.Owin;
using OwinFramework.Interfaces.Builder;

namespace OwinFramework.Interfaces.Routing
{
    public interface IRouter : IMiddleware<IRoute>, IRoutingProcessor
    {
        IRouter Add(string routeName, Func<IOwinContext, bool> filterExpression);
    }
}
