using System;

namespace OwinFramework.Utility
{
    /// <summary>
    /// This exception is thrown when the dependencies defined for
    /// middleware go around in a circle. For example if middleware A
    /// depends on middlewere B and middleware B depende on middleware C
    /// but middleware C depends on middleware A, then it is not possible
    /// to configure the OWIN pipeline so that dependant middleware is
    /// always executed before any middleware that depends on it.
    /// </summary>
    public class CircularDependencyException : Exception
    {
        /// <summary>
        /// Constructs a new CircularDependencyException
        /// </summary>
        public CircularDependencyException(string message)
            : base(message) { }
    }
}
