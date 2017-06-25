using System.Collections.Generic;

namespace OwinFramework.InterfacesV1.Middleware
{
    /// <summary>
    /// Defines the functionallity exposed by the identification feature that
    /// can be used by other middleware compoennts.
    /// 
    /// Identification is the business of figuring out who made this request. The
    /// caller can be a user, an application, a trusted third party system etc.
    /// </summary>
    public interface IIdentification
    {
        /// <summary>
        /// Returns a unique identifier for the user that made the request.
        /// </summary>
        string Identity { get; }

        /// <summary>
        /// Returns true if the user could not be identified from the request. In this
        /// case the Identity property will still contain a unique value, but this value
        /// can change with subsequent requests from the same user.
        /// </summary>
        bool IsAnonymous { get; }

        /// <summary>
        /// This is a list of claims made by the caller. For example the caller can
        /// claim to have an email address, a real name, an avatar etc. It is also possible
        /// to use claims to represent permissions, for example the user can claim to be
        /// an administrator. Each claim has a claim status that indicates if the claim
        /// was supported by evidence, and if that evidence has be verified.
        /// </summary>
        IList<IIdentityClaim> Claims { get; }

        /// <summary>
        /// Users can create additional methods of authenticating against their account, 
        /// for example by generating an API key which allows other applications or 
        /// services to access their account. These additional authentication methods
        /// can be restricted by passing a list of "purposes" that they can be used for.
        /// For example your application can define purposes of "read my messages" or
        /// "manage my calendar" then API tokens can be created that only allow the
        /// third party application to read my messages for example.
        /// When the user logs in using their own credentials this property will be
        /// null or an empty list. When a third party application logs in using a
        /// restricted access token, the list of things that they are allowed to do
        /// will be available in this property.
        /// </summary>
        IList<string> Purposes { get; }
    }

    /// <summary>
    /// Represents a claim made by the entity that is requesting access to a protected
    /// resource.
    /// </summary>
    public interface IIdentityClaim
    {

        /// <summary>
        /// This is the name of the claim the entity is making. This can be one of the
        /// constants defined in the ClaimNames class or any other name that is not 
        /// already defined here.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The value associated with this name. For example if the Name property
        /// contains ClaimNames.Email then this value property will contain the
        /// email address.
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Indicates if the identification system has verified this claim. Note that
        /// the claim can be verified at the point where the entity requests access, or
        /// it can be stored with the identity. For example a user can log in with a 
        /// username and password, so their username is a verified claim, then they
        /// can specify their email address, which is an unverified claim. The system
        /// can send an email to the email address and if the user clicks a link in
        /// the email and verifies their username and password, we can now flag the
        /// email address as verified. Next time the user identifies via username and
        /// password, the identiity middleware can include a verified email claim because
        /// it knows that the verified email address is associated with that username.
        /// </summary>
        ClaimStatus Status { get; }
    }

    /// <summary>
    /// Specifies the wll known claim names
    /// </summary>
    public static class ClaimNames
    {
        /// <summary>
        /// The email address of the identity
        /// </summary>
        public const string Email = "email";

        /// <summary>
        /// The username or display name of the identity
        /// </summary>
        public const string Username = "username";

        /// <summary>
        /// The domain name that the user was logged into. For example 
        /// for active directory intrgration if the user logged into
        /// a domain controller and presented a kerberos token as evidence
        /// then this would be a verified claim to be a member of that
        /// active directory domain. If the request is from another
        /// trusted organization in a B2B transaction, this can also be
        /// the dmain name of the organization requesting access
        /// </summary>
        public const string Domain = "domain";

        /// <summary>
        /// The last name of a real person
        /// </summary>
        public const string Surname = "surname";

        /// <summary>
        /// The first name of a real person
        /// </summary>
        public const string FirstName = "firstname";

        /// <summary>
        /// The phone number of a real person
        /// </summary>
        public const string PhoneNumber = "phone";

        /// <summary>
        /// The year in which the person was born
        /// </summary>
        public const string BirthYear = "birth-year";

        /// <summary>
        /// The month in which the person was born
        /// </summary>
        public const string BirthMonth = "birth-month";

        /// <summary>
        /// The day in which the person was born
        /// </summary>
        public const string BirthDay = "birth-day";

        /// <summary>
        /// The name of the software that is requesting access
        /// </summary>
        public const string Application = "application";

        /// <summary>
        /// The machine name of the server that is requesting access
        /// </summary>
        public const string Machine = "machine";

        /// <summary>
        /// The V4 IP address of the identity
        /// </summary>
        public const string IpV4 = "ip-v4";
    
        /// <summary>
        /// The V6 IP address of the identity
        /// </summary>
        public const string IpV6 = "ip-v6";
    }

    /// <summary>
    /// When an entity submits a request to a secure resource, it makes claims about its
    /// identity and supports those claims with evidence. For example a user might claim
    /// to have a certain username, and support that claim by providing the password as
    /// evidence. If the evidence is accepted then the user's claim is verfied. The user 
    /// might also make a claim about their email address, but we can't verify this claim
    /// until we send an email to this address and have the user click a link in the email
    /// and type in their password.
    /// </summary>
    public enum ClaimStatus 
    {  
        /// <summary>
        /// This means that claim verification is pending, od the software does not
        /// have a mechanism to verify the claim.
        /// </summary>
        Unknown, 

        /// <summary>
        /// This status means that the software took steps to verify the claim, and it
        /// can be trusted by the application.
        /// </summary>
        Verified, 

        /// <summary>
        /// This means that the claim has no evidence to support it, or the claim
        /// verification failed. The claim can not be trusted.
        /// </summary>
        Unverified 
    }

}
