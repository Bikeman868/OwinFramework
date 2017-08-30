using System;
using System.Collections.Generic;
using OwinFramework.InterfacesV1.Middleware;

namespace OwinFramework.InterfacesV1.Facilities
{
    /// <summary>
    /// This is returned by authentication methods
    /// </summary>
    public enum AuthenticationStatus
    {
        /// <summary>
        /// The evidence provided represents the anonymous user
        /// </summary>
        Anonymous = 0,

        /// <summary>
        /// The requestor sucesfully authenticated and should be given access
        /// </summary>
        Authenticated = 1,

        /// <summary>
        /// The information provided by the requestor was not valid. This could be
        /// incorrect password, invalid shared secret, invalid cerificate signature etc.
        /// </summary>
        InvalidCredentials = 2,

        /// <summary>
        /// The supplied information did not match any identity
        /// </summary>
        NotFound = 3,

        /// <summary>
        /// This result is returned after too many failed login attempts. The
        /// implementor of this factiity can decide the business rules around
        /// locking and unlocking users because of failed logins. Generic middleware
        /// should provide configuration options to allow the application developer
        /// to choose the behavour they want. This should be considered a temporary
        /// failure, trying again later might succeed.
        /// </summary>
        Locked = 4,

        /// <summary>
        /// Some implementations might have time-limited user accounts, secret keys etc.
        /// Certificates also have an expiration date. In all of these circumstances the
        /// Expired result is returned. This should be considered a permenant failure,
        /// ie trying again later will produce the same result.
        /// </summary>
        Expired = 5,

        /// <summary>
        /// The requested authentication method is not supported by this identity store
        /// </summary>
        Unsupported = 6
    }
}
