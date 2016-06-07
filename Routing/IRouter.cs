using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Interfaces;

namespace OwinFramework.Routing
{
    public interface IRouter : IMiddleware<IRoute>
    {
        IRouter Add(string routeName, Func<IOwinContext, bool> filterExpression);
    }
}
