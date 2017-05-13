using System.Collections.Generic;
using System.Linq;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.InterfacesV1.Upstream;

namespace OwinFramework.MiddlewareHelpers.Identification
{
    /// <summary>
    /// You can use this class to implement IIdentification if you want. The
    /// main advantage of using this class is that if future versions of
    /// IIdentification have additional properties your code won't break because
    /// the new version of Identification will also have those properties
    /// </summary>
    public class Identification : IIdentification, IUpstreamIdentification
    {
        /// <summary>
        /// Gets or sets the unique identifer for this identity
        /// </summary>
        public string Identity { get; set; }

        /// <summary>
        /// Gets or sets the list of claims that this identity makes about itself
        /// </summary>
        public IList<IIdentityClaim> Claims { get; set; }

        /// <summary>
        /// Sets or sets the flag indicating if the current request is permitted
        /// for identities with no verified claims
        /// </summary>
        public bool AllowAnonymous { get; set; }

        /// <summary>
        /// Returns trus if the identity has no verified claims
        /// </summary>
        public bool IsAnonymous 
        {
            get { return Claims.All(c => c.Status != ClaimStatus.Verified); }
        }

        /// <summary>
        /// Constructs an instance that implements IIdentification
        /// </summary>
        public Identification(string identity, IEnumerable<IIdentityClaim> claims = null)
        {
            Identity = identity;
            Claims = claims == null ? new List<IIdentityClaim>() : claims.ToList();
        }

        /// <summary>
        /// Constructs an instance that implements IIdentification
        /// </summary>
        public Identification(IIdentification other)
        {
            Identity = other.Identity;
            Claims = other.Claims;
        }
    }
}
