using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace OwinFramework.Interfaces
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
    /// This interface defines an OWIN middleware component of a specific
    /// type.
    /// </summary>
    /// <typeparam name="T">Defines the type of middleware component that
    /// is referenced. This should almost always be an interface type
    /// so that different implementations of the same interface can
    /// be swapped for each other. Within an application if the
    /// application developer wants to use concrete types this is
    /// OK, but not best practice.</typeparam>
    public interface IMiddleware<T>: IMiddleware
    {
    }
}
