using System.Collections.Generic;

namespace OwinFramework.Interfaces.Facilities
{
    public enum TokenStatus
    {
        /// <summary>
        /// This token is allowed to be used for the specified purpose
        /// </summary>
        Allowed = 1,

        /// <summary>
        /// This token is not allowed to be used for the specified
        /// purpose at this time but it could be valid in the future 
        /// for this purpose or for other purposes.
        /// When this status is returned it could be that the token has expired,
        /// been used too many times, has been used too frequently, or
        /// it was not created for this purpose
        /// </summary>
        NotAllowed = 2,

        /// <summary>
        /// This token is no longer valid and any future attempts to check
        /// the status of this token will also result in this same status result.
        /// </summary>
        Invalid = 3
    }

    /// <summary>
    /// An object of this type is returned by the token facility
    /// </summary>
    public interface IToken
    {
        /// <summary>
        /// The URL friendly unique identifier for this token
        /// </summary>
        string Value { get; }

        /// <summary>
        /// The current status of the token for the requested purpose
        /// </summary>
        TokenStatus Status { get; }

        /// <summary>
        /// An optional identity associated with the token
        /// </summary>
        string Identity { get; }

        /// <summary>
        /// The purpose for which this token was checked
        /// </summary>
        string Purpose { get; }
    }

    /// <summary>
    /// Defines a token management facility. Any middleware that needs to use
    /// tokens can add a dependency on this interface. The application developer
    /// must implement this interface or include a package in thier application 
    /// that provides it.
    /// 
    /// Example usage:
    /// The user forgot their password and requested a password reset email. 
    /// The email needs to contain a url that the user can click that brings 
    /// the user to a page where they can reset their password. The url needs
    /// to be opaque (ie no user id in plain text). This url should 
    /// only be valid for a specific user, should only work once, and
    /// should only be valid for a limited time. This can be achieved by creating
    /// a token whose types is configured for single use with validity period and
    /// contains the user's identity.
    /// </summary>
    public interface ITokenStore
    {
        /// <summary>
        /// Creates a URL friendly token of the specified type and with a list of
        /// allowed purposes.
        /// </summary>
        /// <param name="tokenType">The application can define whatever token types
        /// make sense to the application. It is expected that implementations of
        /// this facility will allow configuration of token types with things
        /// like period of validity, number of times they can be used etc.</param>
        /// <param name="purpose">Optional list of purposes that this token is
        /// valid for. For example an application can create a token that
        /// represents a user but can only be used for viewing certain kinds
        /// of data and can not change anything.</param>
        /// <param name="identity">Optional identity associated with the token. This
        /// can be thought of as a user id except that it's not always a user, it
        /// could just as easily be a service, machine etc.</param>
        /// <returns>A unique short string containing with none of the url reserved characters</returns>
        string CreateToken(string tokenType, IEnumerable<string> purpose = null, string identity = null);

        /// <summary>
        /// Creates a URL friendly token of the specified type and a specific purpose
        /// </summary>
        /// <param name="tokenType">The application can define whatever token types
        /// make sense to the application. It is expected that implementations of
        /// this facility will allow configuration of token types with things
        /// like period of validity, number of times they can be used etc.</param>
        /// <param name="purpose">The only purpose that is allowed with this token</param>
        /// <param name="identity">Optional identity associated with the token. This
        /// can be thought of as a user id except that it's not always a user, it
        /// could just as easily be a service, machine etc.</param>
        /// <returns>A unique short string containing with none of the url reserved characters</returns>
        string CreateToken(string tokenType, string purpose, string identity = null);

        /// <summary>
        /// Deletes a token making it invalid. This is how you would invalidate an 
        /// authentication token when a user logs out
        /// </summary>
        /// <param name="token">A token that was returned by one of the CreateToken overrides</param>
        /// <returns>True if the token was deleted and False if the  token did not exist</returns>
        bool DeleteToken(string token);

        /// <summary>
        /// Checks if a token is valid for the specified purpose. The token will be invalid
        /// if the token has expired or been used too many times etc. The token will only
        /// be valid if the purpose passed to this method is one of the purposes that was 
        /// passed when the token was created.
        /// </summary>
        /// <param name="tokenType">The application can define whatever token types
        /// make sense to the application. It is expected that implementations of
        /// this facility will allow configuration of token types with things
        /// like period of validity, number of times they can be used etc.</param>
        /// <param name="token">A token that was returned by one of the CreateToken overrides</param>
        /// <param name="purpose">The purpose that this token is being used for</param>
        /// <param name="identity">An optional identity. If you pass this then the token
        /// store will check that this is the identity associated with the token. If you
        /// pass null then this check will be skipped</param>
        /// <returns>Information about the validity of this token</returns>
        IToken GetToken(string tokenType, string token, string purpose = null, string identity = null);
    }
}
