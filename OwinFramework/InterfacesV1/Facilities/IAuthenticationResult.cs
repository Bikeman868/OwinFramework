using System;
using System.Collections.Generic;
using OwinFramework.InterfacesV1.Middleware;

namespace OwinFramework.InterfacesV1.Facilities
{
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
        /// A URL friendly string that uniquely identifies a consumer of this service (user).
        /// Other facilities and middleware should use this to associate other information
        /// with the caller. For example the Authorization middleware should associate
        /// group membership with this identity and a user store can use this to 
        /// associate real name, email address physical address and preferences with the
        /// identity of the caller.
        /// </summary>
        string Identity { get; }

        /// <summary>
        /// An opaque token that uniquely identifies this authentication result. If the
        /// application supports a 'Remember Me' feature where they store a cookie on
        /// the broswer to avoid the user having to log in each time, then this token
        /// is designed to be stored in that cookie.
        /// The implementation of this token can be a secure encryption of the Identity 
        /// and Purposes properties combined, or a random key that is a lookup for this
        /// information.
        /// </summary>
        string RememberMeToken { get; }

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
}
