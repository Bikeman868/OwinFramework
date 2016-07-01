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
        /// depends on or null if the dependency is on name only.
        /// </summary>
        Type DependentType { get; set; }

        /// <summary>
        /// If the DependentType property is set and theer are multiple OWIN 
        /// components that provide the dependant type functionallity this 
        /// name will identify which of those it refers to.
        /// If there is only one OWIN component implementing the
        /// specified type then this property should be null.
        /// If the DependentType property is null then this Name can not be 
        /// null or empty.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// This should be true if it is an error to have the dependency
        /// missing from the configuration. When this property is false the
        /// dependency is only used to defene execution order.
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
