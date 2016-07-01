namespace OwinFramework.InterfacesV1.Middleware
{
    /// <summary>
    /// Defines the functionallity exposed by the identification feature that
    /// can be used by other middleware compoennts.
    /// 
    /// Identification is the business of figuring out who made this request. The
    /// caller can be a user, an application, a trusted third party system etc.
    /// </summary>
    public interface IIdentification
    {
        /// <summary>
        /// Returns a unique identifier for the user that made the request.
        /// </summary>
        string Identity { get; }

        /// <summary>
        /// Returns true if the user could not be identified from the request. In this
        /// case the Identity property will still contain a unique value, but this value
        /// can change with subsequent requests from the same user.
        /// </summary>
        bool IsAnonymous { get; }
    }
}
