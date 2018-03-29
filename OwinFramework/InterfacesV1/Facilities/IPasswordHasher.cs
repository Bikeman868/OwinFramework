using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwinFramework.InterfacesV1.Facilities
{
    /// <summary>
    /// This POCO is returned when the user chooses a new password and the
    /// password is checked to see if it meets the password complexity policy
    /// </summary>
    public class PasswordCheckResult
    {
        /// <summary>
        /// Contains true if the password is allowed under the password
        /// complexity policy
        /// </summary>
        public bool IsAllowed { get; set; }

        /// <summary>
        /// If the IsAllowed property is False, then this property contains
        /// a description of why the password is not acceptable. This 
        /// description must be suitably worded for display in the UI
        /// </summary>
        public string ValidationError { get; set; }

        /// <summary>
        /// Returns an HTML description of the password policy. This will
        /// be displayed in teh UI when the user changes their password.
        /// </summary>
        public string PasswordPolicy { get; set; }
    }

    /// <summary>
    /// You can implement this interface in your application to add a custom
    /// password hashing scheme for a specific version of stored passwords.
    /// This is particularly useful when migrating users from a prior system
    /// and you do not want to force all the users to reset their password.
    /// </summary>
    public interface IPasswordHashingScheme
    {
        /// <summary>
        /// Computes a hash for a password
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <param name="salt">If you pass salt in then it will be appended to
        /// the password prior to hashing. If you pass null for the salt, then
        /// the hashing scheme should generate random salt and return it.</param>
        /// <returns></returns>
        byte[] ComputeHash(string password, ref byte[] salt);
    }

    /// <summary>
    /// Defines a facility for hashing passwords so that they can be safely
    /// stored and checked later.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Tests a potential password to see if it meets the requirements for
        /// password complexity.
        /// </summary>
        /// <param name="identity">The identity of the user who is setting the
        /// password. This is passed as a parameter to enable identity specific
        /// password complexity rules - such as you can not use the same password
        /// again.</param>
        /// <param name="password">The password that the user wants to set</param>
        /// <returns>Results of checking their password against the policy</returns>
        PasswordCheckResult CheckPasswordAllowed(string identity, string password);

        /// <summary>
        /// Computes the hash for a password so that it can be stored as the new 
        /// password for the identity
        /// </summary>
        /// <param name="identity">The identity whos password is being set. This is passed in
        /// case the implementation uses different versions of the hashing scheme for different
        /// types of identity</param>
        /// <param name="password">The password that the user chose</param>
        /// <param name="version">You can pass in a version to use that hashing scheme or pass null
        /// for the most recent version. Outputs the version of the password hashing scheme that 
        /// was actually used, never returns null.</param>
        /// <param name="salt">Outputs the random salt that was added to the password before hashing</param>
        /// <param name="hash">Outputs the password hash that can be safely stored</param>
        void ComputeHash(string identity, string password, ref int? version, out byte[] salt, out byte[] hash);

        /// <summary>
        /// Computes the hash for a password so that it can be compared to the stored
        /// hash during the login process
        /// </summary>
        /// <param name="password">The login password supplied by the user</param>
        /// <param name="version">The version of the password hashing scheme that
        /// was employed when the password was originally hashed</param>
        /// <param name="salt">The random salt that was originally appended to the
        /// password before hashing</param>
        /// <param name="hash">Outputs a hash that can be compared to the stored
        /// hash to see if the user supplied the correct password.</param>
        void ComputeHash(string password, int version, byte[] salt, out byte[] hash);

        /// <summary>
        /// Sets the password hashing scheme for a specific version number.
        /// </summary>
        /// <param name="version">The version number that this scheme applies to</param>
        /// <param name="scheme">The hashing scheme</param>
        void SetHashingScheme(int version, IPasswordHashingScheme scheme);

        /// <summary>
        /// Gets a specific version of the password hashing scheme
        /// </summary>
        /// <param name="version">The version number to get</param>
        /// <returns>The hashing scheme used for this version of the hash</returns>
        IPasswordHashingScheme GetHashingScheme(int version);
    }
}
