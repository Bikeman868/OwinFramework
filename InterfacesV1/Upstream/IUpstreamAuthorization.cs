namespace OwinFramework.InterfacesV1.Upstream
{
    /// <summary>
    /// Allows middleware that is further down the pipeline to communicate upstream to
    /// the authorization middleware. This allows request processing to be short-circuited
    /// by the authorization module when the required permissions are not granted to the
    /// user making the request.
    /// 
    /// If the downstream middleware does not specify any permissions here, the permissions
    /// can also be checked later, it is just less efficient because a lot of other work
    /// was done that was not needed.
    /// 
    /// More complex permission checks can be carried out downstream, or an authorization
    /// middleware can be built that knows how to do these checks itself.
    /// </summary>
    public interface IUpstreamAuthorization
    {
        /// <summary>
        /// Adds a required role to the request. If the user who made the request
        /// does not have this role the authorization middleware should end the request
        /// and return a not-authorized response to the caller.
        /// </summary>
        void AddRequiredRole(string roleName);

        /// <summary>
        /// Adds a required permission to the request. If the user who made the request
        /// does not have this permission the authorization middleware should end the request
        /// and return a not-authorized response to the caller.
        /// </summary>
        void AddRequiredPermission(string permissionName);
    }
}
