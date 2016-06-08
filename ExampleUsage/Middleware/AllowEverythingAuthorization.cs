using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces;
using OwinFramework.Interfaces.Builder;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This middleware example demonstrates the following techniques:
    /// * It injects the IAuthorization feature into the OWIN context
    /// </summary>
    public class AllowEverythingAuthorization : IMiddleware<IAuthorization>, IAuthorization
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public AllowEverythingAuthorization()
        {
            Dependencies = new List<IDependency>();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine("Allow everything authorization middleware invoked");

            context.SetFeature<IAuthorization>(this);

            return next.Invoke();
        }

        public bool IsInRole(string roleName)
        {
            return true;
        }

        public bool HasPermission(string permissionName)
        {
            return true;
        }
    }
}
