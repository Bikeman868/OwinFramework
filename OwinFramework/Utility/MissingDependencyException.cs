using System;

namespace OwinFramework.Utility
{
    /// <summary>
    /// This exception is thrown when a middleware is configured that has a
    /// mandatory dependency on another middleware that was not configured by
    /// the application developer.
    /// </summary>
    public class MissingDependencyException : Exception
    {
        /// <summary>
        /// Constructs a new MissingDependencyException
        /// </summary>
        public MissingDependencyException(string message)
            : base(message) { }
    }
}
