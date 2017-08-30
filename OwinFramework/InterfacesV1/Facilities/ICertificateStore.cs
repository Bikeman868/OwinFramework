using System;
using System.Collections.Generic;
using OwinFramework.InterfacesV1.Middleware;

namespace OwinFramework.InterfacesV1.Facilities
{
    /// <summary>
    /// Defines a facility that stores certificates that identify identities
    /// making requests to the system
    /// </summary>
    public interface ICertificateStore
    {
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
        byte[] AddCertificate(string identity, TimeSpan? lifetime = null, IEnumerable<string> purposes = null);

        /// <summary>
        /// Deletes a specific certificate from the identity store
        /// </summary>
        bool DeleteCertificate(byte[] certificate);

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
    }
}
