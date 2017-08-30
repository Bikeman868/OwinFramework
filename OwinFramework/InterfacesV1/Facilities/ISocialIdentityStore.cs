using System;
using System.Collections.Generic;
using OwinFramework.InterfacesV1.Middleware;

namespace OwinFramework.InterfacesV1.Facilities
{
    /// <summary>
    /// Used to provide information about a sucesful login to a social network site
    /// such as Google, Facebook Twitter etc
    /// </summary>
    public interface ISocialAuthentication
    {
        /// <summary>
        /// A URL friendly string that uniquely identifies a consumer of this service.
        /// Other facilities and middleware should use this to associate other information
        /// with the caller. For example the Authorization middleware should associate
        /// group membership with this identity and a user store can use this to 
        /// associate real name, email address physical address and preferences with the
        /// identity of the caller.
        /// </summary>
        string Identity { get; }

        /// <summary>
        /// Returns a list of optional purposes associated with this login. 
        /// If this is null then the authentication is good for all purposes.
        /// For example a user might want to create an API key that uses the shared
        /// secret method of authentication. This API key should be associated with
        /// the user but give only partial access, i.e. you can't do everything with
        /// that user's account using the API key.
        /// </summary>
        IList<string> Purposes { get; }

        /// <summary>
        /// Contains the authentication token that was received from the social service
        /// when the user successfullly logged in to that service. This token can be used
        /// to request access tokens from the social service.
        /// </summary>
        string AuthenticationToken { get; }
    }

    /// <summary>
    /// Defines a facility that stores social login tokens that identify
    /// users that registered using a 3rd party authentication such as Facebook, LinkedIn etc
    /// </summary>
    public interface ISocialIdentityStore
    {
        /// <summary>
        /// Returns a list of the domain names for social services that can be used to 
        /// authenticate through this identity store. If the identity store dows not support
        /// social login then this list will be empty.
        /// </summary>
        IList<string> SocialServices { get; }
        
        /// <summary>
        /// Associates a sucessfull login to a social account (on Google, Facebook etc) with an identity.
        /// Call this method only after the user has successfully authenticated with the social service.
        /// </summary>
        /// <param name="identity">The identity to associate</param>
        /// <param name="userId">An identifier from the social login site that identifies this user on that site</param>
        /// <param name="socialService">The domain name of the social service</param>
        /// <param name="authenticationToken">An identification token received from the social service for this 
        /// specific user. These are sometimes referred to as refresh tokens in social login APIs. This
        /// token will be used later to obtain an access token for the social site</param>
        /// <param name="purposes">Optional list of purposes to limit the scope of this login</param>
        /// <param name="replaceExisting">Pass true to delete other social logins for the same identity on the
        /// same social service</param>
        /// <returns>True if this is a new social login and false if an existing one was updated</returns>
        bool AddSocial(
            string identity, 
            string userId, 
            string socialService,
            string authenticationToken,
            IEnumerable<string> purposes = null, 
            bool replaceExisting = true);
 
        /// <summary>
        /// Removes a social login account from an identity preventing login with this social account
        /// </summary>
        /// <returns>True if the social login was deleted and False if it did not exist</returns>
        bool DeleteSocial(string identity, string socialService);

        /// <summary>
        /// Removes all social login account from an identity preventing login with these social accounts
        /// </summary>
        bool DeleteAllSocial(string identity);

        /// <summary>
        /// Each time a new session is established for an identity and the application needs to 
        /// communicate with a social service API it should call this method. This method will
        /// return the authentication token obtained from the social service when the user logged in.
        /// You can pass this authentication token along with the user ID to the social service to get
        ///  an access token and you can store the access token in session to gain access to social
        /// site apis.
        /// </summary>
        /// <param name="userId">An identifier from the social login site that identifies this user on that site</param>
        /// <param name="socialService">The domain name of the social service that was used</param>
        ISocialAuthentication GetSocialAuthentication(string userId, string socialService);
    }
}
