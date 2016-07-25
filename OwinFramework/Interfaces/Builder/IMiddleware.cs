using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace OwinFramework.Interfaces.Builder
{
    /// <summary>
    /// All OWIN middleware should implement this interface. It provides a 
    /// way for code to pass a reference to a middleware building block
    /// without knowing what that component does.
    /// </summary>
    public interface IMiddleware
    {
        /// <summary>
        /// A unique name for this middleware instance
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// A list of the other middleware that this one directly depends on
        /// </summary>
        IList<IDependency> Dependencies { get; }

        /// <summary>
        /// Standard OWIN function for invoking middleware
        /// </summary>
        /// <param name="context">The context of this request</param>
        /// <param name="next">A function pointer that will execute the next
        /// middleware in the OWIN pipeline</param>
        /// <returns></returns>
        Task Invoke(IOwinContext context, Func<Task> next);
    }

    /// <summary>
    /// This interface defines an OWIN middleware component that provides
    /// a specific feature.
    /// </summary>
    /// <typeparam name="T">Defines the feature that this middleware component
    /// provides. This design deliberately restricts middleware components to
    /// implementing only one feature. For example this type can be ISession,
    /// IAuthorization etc. You are not limited to the interfaces defined in this
    /// project, you can use any interface you like.</typeparam>
    public interface IMiddleware<T>: IMiddleware
    {
    }
}
