using System;

namespace OwinFramework.Utility
{
    /// <summary>
    /// This exception is thrown when the application developer
    /// configures two or more middleware with the same name.
    /// </summary>
    public class DuplicateKeyException : Exception
    {
        /// <summary>
        /// Constructs a new DuplicateKeyException
        /// </summary>
        public DuplicateKeyException(string message)
            : base(message) { }
    }
}
