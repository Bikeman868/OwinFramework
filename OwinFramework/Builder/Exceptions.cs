using System;

namespace OwinFramework.Builder
{
    /// <summary>
    /// This exception is thrown when the builder encounters a fatal error
    /// in the pipeline configuration that prevents it from building the
    /// OWIN pipeline.
    /// </summary>
    public class BuilderException: Exception
    {
        /// <summary>
        /// Constructs a new BuilderException
        /// </summary>
        public BuilderException() { }

        /// <summary>
        /// Constructs a new BuilderException with an error message
        /// </summary>
        public BuilderException(string message) : base(message) { }
    }
}
