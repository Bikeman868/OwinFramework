using System;
using System.Collections.Generic;

namespace OwinFramework.InterfacesV1.Capability
{
    /// <summary>
    /// Defaines the types of documentation that Owin middleware can provide
    /// to the application developer.
    /// </summary>
    public enum DocumentationTypes
    {
        /// <summary>
        /// A high level description of why this middleware exists, the 
        /// circumstances where it is useful and what it depends on.
        /// </summary>
        Overview,

        /// <summary>
        /// Step by step instructions for downloading, installing and 
        /// configuring this middleware for a simple 'hello world' type
        /// application.
        /// </summary>
        GettingStarted,

        /// <summary>
        /// Source code recipies for some typical situations.
        /// </summary>
        SampleCode,

        /// <summary>
        /// Reference information for all configuration options provided
        /// by this middleware
        /// </summary>
        Configuration,

        /// <summary>
        /// Descriptions and diagrams explaining how this middleware works
        /// and exactly what it does.
        /// </summary>
        TechnicalDetails,

        /// <summary>
        /// If this middleware is open source then this should point
        /// to the web site where the source code can be obtained
        /// </summary>
        SourceCode
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

        /// <summary>
        /// Optionally returns API documentation for application 
        /// developers or null if no documentation is available.
        /// </summary>
        IList<IEndpointDocumentation> Endpoints { get; }
    }

    /// <summary>
    /// Provides application developer with information about an
    /// endpoint that is supported by your middleware. The longer
    /// descriptive properties support simple HTML. You can use:
    /// &lt;p&gt;&lt;/p&gt;
    /// &lt;b&gt;&lt;/b&gt;
    /// &lt;i&gt;&lt;/i&gt;
    /// &lt;span style=""&gt;&lt;/span&gt;
    /// &lt;div stype=""&gt;&lt;/div&gt;
    /// &lt;ul&gt;&lt;li&gt;&lt;/li&gt;&lt;/ul&gt;
    /// </summary>
    public interface IEndpointDocumentation
    {
        /// <summary>
        /// The path of this endpoint in the web site. If the path
        /// contains variable data such as the ID or name of something
        /// then put the name of this variable in {}, for example
        ///   /customer/{customerId}/address/{index}
        /// Document the variable elements in curly brackets in the
        /// Attributes property.
        /// </summary>
        string RelativePath { get; }

        /// <summary>
        /// A description of what this endpoint does and how/when to 
        /// use it. You can use simple HTML markup here
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Some examples of paths with a description of how these
        /// will be interpreted by the endpoint. You can use simple 
        /// HTML markup here
        /// </summary>
        string Examples { get; }

        /// <summary>
        /// A list of the optional parts of the request including
        /// supported methods, variable path elements, HTTP headers
        /// etc.
        /// </summary>
        IList<IEndpointAttributeDocumentation> Attributes { get; }
    }

    /// <summary>
    /// Describes an attribute of an endpoint, for example a
    /// supported HTTP method, query string parameter or header
    /// </summary>
    public interface IEndpointAttributeDocumentation
    {
        /// <summary>
        /// The type of attribute. Can be 'Query string', 'Header',
        /// 'Method' or any other type appropriate to your endpoint
        /// </summary>
        string Type { get; }

        /// <summary>
        /// The name of the attribute. If Type is 'Method' for example
        /// then this should be the name of the method, for example 'POST'.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// An explanation of when this attribute applies, what effect 
        /// it has on the response, the possible values etc. You can use
        /// simple HTML markup here
        /// </summary>
        string Description { get; }
    }
}
