namespace OwinFramework.InterfacesV1.Facilities
{
    /// <summary>
    /// Defines a facility that stores information about identities and provides
    /// methods to verify the identity of the entity making the request.
    /// </summary>
    public interface IIdentityStore : ICredentialStore, ISharedSecretStore, ICertificateStore, ISocialIdentityStore, IIdentityDirectory
    {
    }
}
