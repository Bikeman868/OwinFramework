namespace OwinFramework.Interfaces.Utility
{
    /// <summary>
    /// Constructs instances of IDependencyGraph and supplies dependencies
    /// </summary>
    public interface IDependencyGraphFactory
    {
        /// <summary>
        /// Creates and initializes an instance of IDependencyGraph
        /// </summary>
        IDependencyGraph<T> Create<T>();
    }
}
