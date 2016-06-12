using System;

namespace OwinFramework.Interfaces.Builder
{
    // Note that the order of this enumeration is important because
    // middleware components are sorted by position and added to the
    // pipeline in that order.
    public enum PipelinePosition { Front, Middle, Back }


    /// <summary>
    /// Contains information about a dependency on another OWIN
    /// middleware component.
    /// </summary>
    public interface IDependency
    {
        /// <summary>
        /// Specifies which part of the pipeline this middleware should
        /// run in.
        /// </summary>
        PipelinePosition Position { get; }

        /// <summary>
        /// The type of middleware functionallity that this component
        /// depends on.
        /// </summary>
        Type DependentType { get; }

        /// <summary>
        /// If there are multiple OWIN components that provide the same
        /// functionallity this name will identify which of those it
        /// refers to. If there is only one OWIN component implementing the
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
