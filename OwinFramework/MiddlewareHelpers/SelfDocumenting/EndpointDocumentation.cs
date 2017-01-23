using System.Collections.Generic;
using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.MiddlewareHelpers.SelfDocumenting
{
    /// <summary>
    /// Concrete implementatation of IEndpointDocumentation
    /// </summary>
    public class EndpointDocumentation : IEndpointDocumentation
    {
        /// <summary>
        /// The URL of an endpoint within this website
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// A description of what this endpoint is for, Can basic include HTML
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Example URLs showing examples of how to invoke this endpoint properly with
        /// explananation. Can include simple HTML
        /// </summary>
        public string Examples { get; set; }

        /// <summary>
        /// Optional and required characteristics of this endpoint. You can document
        /// variable path elements, query string parameters, http methods, request headers
        /// and the request body.
        /// </summary>
        public IList<IEndpointAttributeDocumentation> Attributes { get; set; }
    }
}
