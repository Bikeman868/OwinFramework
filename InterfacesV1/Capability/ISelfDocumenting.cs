using System;

namespace OwinFramework.InterfacesV1.Capability
{
    /// <summary>
    /// Defaines the types of documentation that Owin middleware can provide
    /// to the application developer.
    /// </summary>
    public enum DocumentationTypes
    {
        Overview,
        GettingStarted,
        SampleCode,
        Configuration,
        TechnicalDetails
    }
 
    /// <summary>
    /// If you implement this interface in your middleware then tools that 
    /// generate documentation for the application developer will be able 
    /// to include more detail about your middleware.
    /// 
    /// Implementing this interface is optional.
    /// </summary>
    public interface ISelfDocumenting
    {
        /// <summary>
        /// Returns a short one line description of the middleware. Is
        /// typically used to label drawings and populate drop-down lists
        /// </summary>
        string ShortDescription { get; }

        /// <summary>
        /// This is a longer plain text description that can contain
        /// line breaks. This is typically used in roll-over popups
        /// that provide a little more detail about the middleware
        /// </summary>
        string LongDescription { get; }

        /// <summary>
        /// Retrieves the URL of a type of documentation. The middleware
        /// can return the same URL for all types of documentation if
        /// it likes, or it can return null for documentation types
        /// that it does not support.
        /// This will typically be used to create hyperlinks that the
        /// application developer can click on to find out more about
        /// the middleware.
        /// Middleware can return a relative URL and serve the
        /// documentation itself, i.e. the URL can point to a
        /// location that is handled by this middleware, or the
        /// URL can be the absolute URL of an external web page.
        /// </summary>
        Uri GetDocumentation(DocumentationTypes documentationType);
    }
}
