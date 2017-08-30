using System;
using System.Collections.Generic;
using OwinFramework.InterfacesV1.Middleware;

namespace OwinFramework.InterfacesV1.Facilities
{
    /// <summary>
    /// Encapsulates the information stored about a username/password combination
    /// </summary>
    public interface ICredential
    {
        /// <summary>
        /// The unique identifier for the identity this credential belongs to
        /// </summary>
        string Identity { get; }

        /// <summary>
        /// The username used to sign in with this credential
        /// </summary>
        string Username { get; }

        /// <summary>
        /// The actions that are permitted on the identity identified by this credential
        /// </summary>
        List<string> Purposes { get; }
    }

    /// <summary>
    /// Defines a facility that stores username and password credentials that identify
    /// users making requests to the system
    /// </summary>
    public interface ICredentialStore
    {
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
        bool AddCredentials(string identity, string userName, string password, bool replaceExisting = true, IEnumerable<string> purposes = null);

        /// <summary>
        /// Checks user supplied credentials and returns the identity of the user
        /// </summary>
        /// <param name="userName">The user id for this user (usually email address)</param>
        /// <param name="password">The user's password</param>
        /// <returns>The results of checking the user's credentials</returns>
        IAuthenticationResult AuthenticateWithCredentials(string userName, string password);

        /// <summary>
        /// Logs the user in using a stored Remember Me Token. This token
        /// can be obtained from a full login with credentials or a secret key
        /// </summary>
        /// <param name="rememberMeToken">The remember me token from a succesful
        /// login</param>
        /// <returns>Details about the user and purposes permitted by this login</returns>
        IAuthenticationResult RememberMe(string rememberMeToken);

        /// <summary>
        /// Retrieves the username that was used to log in using credentials
        /// </summary>
        /// <param name="rememberMeToken">A token returned from a sucessful login</param>
        /// <returns>Credentials if this login was a creddentials login, or null
        /// if the user identified in some other way (for example with a cert)</returns>
        ICredential GetRememberMeCredential(string rememberMeToken);

        /// <summary>
        /// Retrieves the username that was used to log in using credentials
        /// </summary>
        /// <param name="username">A username that is used to login to the system</param>
        /// <returns>Credentials if this username exists in the system, or null
        /// if there is no such user</returns>
        ICredential GetUsernameCredential(string username);

        /// <summary>
        /// Retrieves a list of the credentials associated with an identity
        /// </summary>
        /// <param name="identity">The unique identifier for the identity</param>
        /// <returns>A list of credentials</returns>
        IEnumerable<ICredential> GetCredentials(string identity);

        /// <summary>
        /// Deletes a user credential from the system preventing any further login
        /// attempts with that username.
        /// </summary>
        /// <param name="credential">The credential to delete</param>
        /// <returns>True if the deletion was sucessful and false if not found</returns>
        bool DeleteCredential(ICredential credential);

        /// <summary>
        /// Changes the password for a credential
        /// </summary>
        /// <param name="credential"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        bool ChangePassword(ICredential credential, string newPassword);
    }
}
