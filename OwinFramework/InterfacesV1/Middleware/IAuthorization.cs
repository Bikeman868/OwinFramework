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
        /// </summary>
        bool IsInRole(string roleName);

        /// <summary>
        /// Tests if the user that made this request has the specified permission.
        /// </summary>
        bool HasPermission(string permissionName);
    }
}
