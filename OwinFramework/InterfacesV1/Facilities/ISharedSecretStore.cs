using System;
using System.Collections.Generic;
using OwinFramework.InterfacesV1.Middleware;

namespace OwinFramework.InterfacesV1.Facilities
{
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
        string Secret { get; }

        /// <summary>
        /// Contains the purposes that this shared secret can be used for
        /// </summary>
        IList<string> Purposes { get; }
    }

    /// <summary>
    /// Defines a facility that stores shared secrets that third-party systems
    /// can use when identifying themselves to your APIs
    /// </summary>
    public interface ISharedSecretStore
    {
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
    }
}
