using System;
using System.Collections.Generic;
using OwinFramework.InterfacesV1.Middleware;

namespace OwinFramework.InterfacesV1.Facilities
{
    /// <summary>
    /// This structure is returned when a request is made to find identities
    /// </summary>
    public interface IIdentitySearchResult
    {
        /// <summary>
        /// This token can be passed back to the search method to retrieve the
        /// next page of results. To make this useful implementors should assume
        /// that this token will be passed in a URL query string and therefore
        /// should not use characters that are illegal in that context.
        /// </summary>
        string PagerToken { get; }

        /// <summary>
        /// This is are one page of search results
        /// </summary>
        IList<IMatchingIdentity> Identities { get; }
    }
    
    /// <summary>
    /// Represents an identity that matched the search phrase
    /// </summary>
    public interface IMatchingIdentity
    {
        /// <summary>
        /// The string that represents this identity. The format of this string
        /// is implementation specific, but should always be something that you
        /// could include in a URL with encoding for it to be useful.
        /// </summary>
        string Identity { get; }

        /// <summary>
        /// These are the claims made by the identity. Claims include things like
        /// email address, real name etc.
        /// </summary>
        IList<IIdentityClaim> Claims { get; }
    }

    /// <summary>
    /// Defines a facility that stores information about identities.
    /// </summary>
    public interface IIdentityDirectory 
    {
        /// <summary>
        /// Creates a new identity in the system. You must associate the identity with
        /// some type of evidence to make it useful (for example you have to add a
        /// username and password or certificate or something).
        /// </summary>
        /// <returns>A unique url friendly identifier for a new identity</returns>
        string CreateIdentity();

        /// <summary>
        /// Returna a list of the claims made by this identity and the status of
        /// each of thsose claims
        /// </summary>
        IList<IIdentityClaim> GetClaims(string identity);

        /// <summary>
        /// Adds or updates a claim for an identity. Claims are things like the user's
        /// email address, real name, date of birth etc. Each claim has a status that
        /// indicates if it has been verified.
        /// </summary>
        string UpdateClaim(string identity, IIdentityClaim claim);

        /// <summary>
        /// Removes a claim from an identity. This might be appropriate for example
        /// if a certificate expires, or a user changes their email address
        /// </summary>
        string DeleteClaim(string identity, string claimName);

        /// <summary>
        /// Searches for matching identities. This is useful in administration UIs where
        /// system administrators need to find users by name or email etc to reset their
        /// password, change permissions etc
        /// </summary>
        /// <param name="searchText">The text that the user typed into the search box</param>
        /// <param name="pagerToken">Pass the token from a prior search result to return the 
        /// next page of results or null to start from the beginning</param>
        /// <param name="maxResultCount">The maximum number of results to return</param>
        /// <param name="claimName">Restricts the search to one claim only. When this
        /// parameter is null all claims and the identity string will be searched</param>
        /// <returns></returns>
        IIdentitySearchResult Search(string searchText, string pagerToken = null, int maxResultCount = 20, string claimName = null);
    }
}
