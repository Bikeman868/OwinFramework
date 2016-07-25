using OwinFramework.Interfaces.Utility;

namespace OwinFramework.Utility
{
    /// <summary>
    /// Constructs instances that implement IDependencyGraph
    /// </summary>
    public class DependencyGraphFactory : IDependencyGraphFactory
    {
        /// <summary>
        /// Constructs an instance IDependencyGraph
        /// </summary>
        public IDependencyGraph<T> Create<T>() 
        {
            return new DependencyGraph<T>();
        }
    }
}
