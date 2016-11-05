using System;
using System.Collections.Generic;

namespace OwinFramework.InterfacesV1.Facilities
{
    /// <summary>
    /// This is returned by authentication methods
    /// </summary>
    public enum AuthenticationStatus
    {
        /// <summary>
        /// The requestor sucesfully authenticated and should be given access
        /// </summary>
        Authenticated,

        /// <summary>
        /// The information provided by the requestor was not valid
        /// </summary>
        InvalidCredentials,

        /// <summary>
        /// The supplied information did not match any identity
        /// </summary>
        NotFound,

        /// <summary>
        /// The requested authentication method is not supported by this identity store
        /// </summary>
        Unsupported
    }

    /// <summary>
    /// The result of an authentication attempt
    /// </summary>
    public interface IAuthenticationResult
    {
        /// <summary>
        /// See definition of enum values
        /// </summary>
        AuthenticationStatus Status { get; }

        /// <summary>
        /// A URL friendly string that uniquely identifies a consumer of this service.
        /// Other facilities and middleware should use this to associate other information
        /// with the caller. For example the Authorization middleware should associate
        /// group membership with this identity and a user store can use this to 
        /// associate real name, email address physical address and preferences with the
        /// identity of the caller.
        /// </summary>
        string Identity { get; }

        /// <summary>
        /// Returns a list of optional purposes associated with this login. 
        /// If this is null then the authentication is good for all purposes.
        /// For example a user might want to create an API key that uses the shared
        /// secret method of authentication. This API key should be associated with
        /// the user but give only partial access, i.e. you can't do everything with
        /// that user's account using the API key.
        /// </summary>
        IList<string> Purposes { get; }
    }

    /// <summary>
    /// When users create shared secrets that provide access to their account, they
    /// need to be able to go back later and delete or deactivate these secrets, hence
    /// these have to be given names.
    /// </summary>
    public interface ISharedSecret
    {
        /// <summary>
        /// The name of this shared secret
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The secret that can be shared to provide access to the system
        /// </summary>
        string SharedSecret { get; }

        /// <summary>
        /// Contains the purposes that this shared secret can be used for
        /// </summary>
        IList<string> Purposes { get; }
    }

    /// <summary>
    /// Defines a facility that stores information about identities and provides
    /// methods to verify the identity of the entity making the request.
    /// </summary>
    public interface IIdentityStore
    {
        /// <summary>
        /// Creates a new identity in the system. You must associate the identity with
        /// some type of evidence to make it useful (for example you have to add a
        /// username and password or certificate or something).
        /// </summary>
        /// <returns>A unique url friendly identifier for a new identity</returns>
        string CreateIdentity();

        #region Credentials

        /// <summary>
        /// Returns true if this identity store can work with usernames and passwords
        /// </summary>
        bool SupportsCredentials { get; }

        /// <summary>
        /// Adds username/password credentials to an identity so that the identity can
        /// log in using these credentials
        /// </summary>
        /// <param name="identity">A URL friendly string that uniquely identifies an identity</param>
        /// <param name="userName">The username that they will use to login</param>
        /// <param name="password">The password that they will use to login</param>
        /// <param name="replaceExisting">True to delete all existing credentials. This
        /// will not delete any secret keys, certificates etc. False to add this as a
        /// new login but keep the old credentials still active, this allows different
        /// credentials to have different purposes on the same account.</param>
        /// <param name="purposes">Optional list of purposes to restrict what is allowed
        /// when a user logs in with these credentials. If this is null then the 
        /// login is unrestricted</param>
        /// <returns>True if sucessful. Returns false if the identity was not found or the
        /// password does not meet requirements for password complexity</returns>
        bool AddCredentials(string identity, string userName, string password, bool replaceExisting, IList<string> purposes);

        /// <summary>
        /// Checks user supplied credentials and returns the identity of the user
        /// </summary>
        /// <param name="userName">The user id for this user (usually email address)</param>
        /// <param name="password">The user's password</param>
        /// <returns>The results of checking the user's credentials</returns>
        IAuthenticationResult AuthenticateWithCredentials(string userName, string password);

        #endregion

        #region Certificates

        /// <summary>
        /// Returns true if this identity store can work with certificates
        /// </summary>
        bool SupportsCertificates { get; }

        /// <summary>
        /// Generates a certificate and associates it with an identity
        /// </summary>
        /// <param name="identity">The identity to associate the certificate with</param>
        /// <param name="lifetime">How long is this certificate valid for</param>
        /// <param name="purposes">Optional list of purposes to limit the scope of this certificate</param>
        /// <returns>A certificate that a 3rd party can store on their system and use to access
        /// services for specific purposes</returns>
        byte[] AddCertificate(string identity, TimeSpan lifetime, IList<string> purposes);

        /// <summary>
        /// Deletes a specific certificate from the identity store
        /// </summary>
        bool DeleteCertificate(byte[] cerificate);

        /// <summary>
        /// Deletes all of the certificates associated with an identity
        /// </summary>
        int DeleteCertificates(string identity);
        
        /// <summary>
        /// Checks the supplied certifcate and returns status of the identity associated with
        /// that certificate. This mechanism is useful when you want to issue certificates to
        /// trusted external systems and be able to identify those systems by the certificate
        /// that they present.
        /// </summary>
        IAuthenticationResult AuthenticateWithCertificate(byte[] certificate);

        #endregion

        #region Social login

        /// <summary>
        /// Returns a list of the domain names for social services that can be used to 
        /// authenticate through this identity store. If the identity store dows not support
        /// social login then this list will be empty.
        /// </summary>
        IList<string> SocialServices { get; }
        
        /// <summary>
        /// Associates a social account (on Google, Facebook etc) with an identity.
        /// </summary>
        /// <param name="identity">The identity to associate</param>
        /// <param name="userName">The username on the social login site</param>
        /// <param name="socialService">The domain name of the social service</param>
        /// <param name="purposes">Optional list of purposes to limit the scope of this login</param>
        /// <returns>True if the social account was succesfully associated</returns>
        bool AddSocial(string identity, string userName, string socialService, IList<string> purposes);
 
        /// <summary>
        /// Removes a social login account from an identity preventing login with this social account
        /// </summary>
        bool DeleteSocial(string identity, string userName, string socialService);

        /// <summary>
        /// Removes all social login account from an identity preventing login with these social accounts
        /// </summary>
        bool DeleteAllSocial(string identity);

        /// <summary>
        /// After a user has signed in with a social login (for example Google or Facebook)
        /// they can call this method to pass the authentication token received from the 
        /// social service to gain access to this system.
        /// </summary>
        /// <param name="userName">The user name (email address) used to log in</param>
        /// <param name="socialService">The domain name of the social service that was used</param>
        /// <param name="authenticationToken">The authentication token received. The
        /// identity store needs to reach out to the social login site to validate this
        /// token before giving access to the account</param>
        /// <returns></returns>
        IAuthenticationResult AuthenticateWithSocial(string userName, string socialService, string authenticationToken);

        #endregion

        #region Shared secrets

        /// <summary>
        /// Returns true if this identity store can work with shared secrets
        /// </summary>
        bool SupportsSharedSecrets { get; }

        /// <summary>
        /// Creates a shared secret that can be used to authenticate as an identity
        /// </summary>
        /// <param name="identity">The identity to associate</param>
        /// <param name="name">When users create shared keys to give access to their account they
        /// can give a name to each one so that they can manage them later</param>
        /// <param name="purposes">Optional list of purposes to limit the scope of this login</param>
        /// <returns>A short unique url friendly string that can be shared with a third party to give them
        /// the ability to authenticate as this identity</returns>
        string AddSharedSecret(string identity, string name, IList<string> purposes);

        /// <summary>
        /// Removes a shared secret from an identity preventing login with this shared secret in future
        /// </summary>
        bool DeleteSharedSecret(string sharedSecret);

        /// <summary>
        /// Returns a list of all the shared secrets associated with an identity
        /// </summary>
        IList<ISharedSecret> GetAllSharedSecrets(string identity);

        /// <summary>
        /// Provides shared secret authentication. The shared secret should be send securely
        /// to the other party, and they must logon through a secure connection.
        /// </summary>
        /// <param name="sharedSecret">A secret key that was provided to a 3rd party</param>
        IAuthenticationResult AuthenticateWithSharedSecret(string sharedSecret);

        #endregion
    }
}
