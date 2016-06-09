using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Interfaces.Utility;

namespace OwinFramework.Interfaces.Builder
{
    /// <summary>
    /// All OWIN middleware should implement this interface. It provides a 
    /// way for code to pass a reference to a middleware building block
    /// without knowing what that component does.
    /// </summary>
    public interface IMiddleware
    {
        string Name { get; set; }
        IList<IDependency> Dependencies { get; }

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
