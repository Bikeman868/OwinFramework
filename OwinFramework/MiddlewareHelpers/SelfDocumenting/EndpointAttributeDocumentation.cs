using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.MiddlewareHelpers.SelfDocumenting
{
    /// <summary>
    /// Concrete implementatation of IEndpointDocumentation
    /// </summary>
    public class EndpointAttributeDocumentation: IEndpointAttributeDocumentation
    {
        /// <summary>
        /// The kind of attribute you are describing, can be a path segment, query
        /// string parameter, header etc
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The name of the attribute you are describing. For example if the Type
        /// property is 'Header' then this would be the name of the header
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A description of the accepted values for this attribue. Can inclue
        /// simple HTML for formatting
        /// </summary>
        public string Description { get; set; }
    }
}
