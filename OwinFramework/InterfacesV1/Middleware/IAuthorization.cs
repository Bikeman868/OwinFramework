namespace OwinFramework.InterfacesV1.Middleware
{
    /// <summary>
    /// Defines the functionallity exposed by the authorization feature that
    /// can be used by other middleware compoennts.
    /// 
    /// Authorization is the business of deciding who it allowed to do what.
    /// Middleware that provides this feature will typically have a dependency
    /// on middleware that provides the IIdentification feature.
    /// 
    /// In most authorization systems users can be assigned to a role which
    /// makes them a member of several groups and these groups have permissions
    /// assigned to them. For example a user might have the role of "Developer"
    /// which adds them to the "Developers" group and the "IIS Admins" group.
    /// These groups in turn will grant specific permissions within certain
    /// applications.
    /// 
    /// Within a specific application, all the application usually cares about
    /// is whether the user has permission to perform a certain action.
    /// </summary>
    public interface IAuthorization
    {
        /// <summary>
        /// Tests if the user that made this request is in the specified role.
        /// Each user can have many roles. The role defines what type of user 
        /// this is.
        /// The expected usage of this function is that different types of user
        /// might have different user experiences, or have access to different
        /// areas of functionallity, for example only show the 'Developer Tools'
        /// menu if the user has the role of 'Developer'.
        /// </summary>
        bool IsInRole(string roleName);

        /// <summary>
        /// Tests if the user that made this request has the specified permission.
        /// Each authorization provider is free to define how the permission name
        /// and resource name is interpreted. Read the authentication provider
        /// documentation to know how to pass these parameters.
        /// Permissions can be granted for specific operations on all resources
        /// or limited to a subset of resources.
        /// </summary>
        /// <param name="permissionName">The name of the permission to test. We
        /// recommend that you use some structure within your permission names.
        /// Our suggestion is 'service:operation' for example 'cart:order.delete'</param>
        /// <param name="resource">Optionally specifies the resource on which the
        /// permission is being tested. We recommend using a heirachical notation
        /// for resources so that for example having permission on the 'user:{self}'
        /// resource implies permission on sub-resources such as 'user:{self}.profile'
        /// which also implies permission on it's sub-resources such as 
        /// 'user:{self}.profile.picture'.</param>
        bool HasPermission(string permissionName, string resource);
    }
}
