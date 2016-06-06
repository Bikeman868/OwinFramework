using System;

namespace OwinFramework.Interfaces
{
    /// <summary>
    /// Contains information about a dependency on another OWIN
    /// middleware component.
    /// </summary>
    public interface IDependency
    {
        /// <summary>
        /// The type of middleware functionallity that this component
        /// depends on.
        /// </summary>
        Type DependentType { get; }

        /// <summary>
        /// If there are multiple OWIN components that provide the same
        /// functionallity this name will identify which of those it
        /// refers to. If there is only OWIN component implementing the
        /// specified type then this property should be null
        /// </summary>
        string Name { get; }

        /// <summary>
        /// This should be true if it is an error to have the dependency
        /// missing from the configuration
        /// </summary>
        bool Required { get; }
    }

    /// <summary>
    /// Adds type information to the dependency
    /// </summary>
    /// <typeparam name="T">The type of middleware that is dependent on</typeparam>
    public interface IDependency<T> : IDependency
    {
    }
}
