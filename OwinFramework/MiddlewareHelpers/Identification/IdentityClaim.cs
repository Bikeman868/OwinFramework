using OwinFramework.InterfacesV1.Middleware;

namespace OwinFramework.MiddlewareHelpers.Identification
{
    /// <summary>
    /// You can use this class to implement IIdentityClaim if you want. The
    /// main advantage of using this class is that if future versions of
    /// IIdentityClaim have additional properties your code won't break because
    /// the new version of IdentityClaim will also have those properties
    /// </summary>
    public class IdentityClaim: IIdentityClaim
    {
        /// <summary>
        /// Gets or sets the name of this claim
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of this claim
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets of sets the status of this claim
        /// </summary>
        public ClaimStatus Status { get; set; }

        /// <summary>
        /// Default public constructor required for serialization
        /// </summary>
        public IdentityClaim()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public IdentityClaim(IIdentityClaim other)
        {
            Name = other.Name;
            Value = other.Value;
            Status = other.Status;
        }

        /// <summary>
        /// Constructs an identity claim
        /// </summary>
        public IdentityClaim(string name, string value, ClaimStatus status = ClaimStatus.Unknown)
        {
            Name = name;
            Value = value;
            Status = status;
        }
    }
}
