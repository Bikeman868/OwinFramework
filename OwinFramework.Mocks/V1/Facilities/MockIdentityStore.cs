using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq.Modules;
using OwinFramework.Builder;
using OwinFramework.InterfacesV1.Facilities;

namespace OwinFramework.Mocks.V1.Facilities
{
    public class MockIdentityStore : ConcreteImplementationProvider<IIdentityStore>, IIdentityStore
    {
        public MockIdentityStore()
        {
            SocialServices = new List<string> {"Facebook", "Google+", "LinkedIn", "Twitter"};
        }

        protected override IIdentityStore GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        private readonly IDictionary<string, TestIdentity> _identities = new ConcurrentDictionary<string, TestIdentity>();
        private readonly IList<TestSharedSecret> _sharedSecrets = new List<TestSharedSecret>();
        private IList<TestCredential> _credentials = new List<TestCredential>();
        private IList<TestSocial> _socialIds = new List<TestSocial>();

        #region Methods to control mock behaviour in unit tests

        public void SetIdentityName(string identity, string name)
        {
            _identities[identity].Name = name;
        }

        public string GetIdentityName(string identity)
        {
            return _identities[identity].Name;
        }

        public bool SupportsCertificates { get; set; }
        public bool SupportsCredentials { get; set; }
        public bool SupportsSharedSecrets { get; set; }
        public IList<string> SocialServices { get; set; }

        #endregion

        string IIdentityStore.CreateIdentity()
        {
            var identity = new TestIdentity {Identity = Guid.NewGuid().ToString("N")};
            _identities.Add(identity.Identity, identity);
            return identity.Identity;
        }

        public IAuthenticationResult RememberMe(string rememberMeToken)
        {
            if (string.IsNullOrEmpty(rememberMeToken))
                return new AuthenticationResult
                {
                    Status = AuthenticationStatus.InvalidCredentials
                };

            var colonIndex = rememberMeToken.IndexOf(":", StringComparison.Ordinal);
            if (colonIndex < 1)
                return new AuthenticationResult
                {
                    Status = AuthenticationStatus.InvalidCredentials
                };

            return new AuthenticationResult
            {
                Status = AuthenticationStatus.Authenticated,
                RememberMeToken = rememberMeToken,
                Identity = rememberMeToken.Substring(0, colonIndex),
                Purposes = colonIndex < rememberMeToken.Length 
                    ? rememberMeToken.Substring(colonIndex + 1).Split(',').ToList() 
                    : new List<string>()
            };
        }

        #region Certificates

        public byte[] AddCertificate(string identity, TimeSpan? lifetime, IEnumerable<string> purposes)
        {
            var id = Guid.NewGuid().ToString("N");
            var certificateText = identity + ":" + id;
            var certificate = new TestCertificate
            {
                Identity = identity,
                Id = id,
                Certificate = Encoding.UTF8.GetBytes(certificateText),
                Expiry = lifetime.HasValue ? DateTime.UtcNow + lifetime : null,
                Purposes = purposes == null ? new List<string>() : purposes.Where(p => !string.IsNullOrEmpty(p)).ToList()
            };
            _identities[identity].Certificates.Add(certificate);
            return certificate.Certificate;
        }

        public IAuthenticationResult AuthenticateWithCertificate(byte[] certificate)
        {
            var result = new AuthenticationResult
            {
                Status = AuthenticationStatus.NotFound
            };

            TestIdentity identity;
            TestCertificate testCertificate;
            if (!FindCertificate(certificate, out identity, out testCertificate)) return result;

            result.Identity = identity.Identity;
            result.Purposes = testCertificate.Purposes;
            result.Status = testCertificate.Expiry.HasValue && (DateTime.UtcNow > testCertificate.Expiry)
                ? AuthenticationStatus.InvalidCredentials
                : AuthenticationStatus.Authenticated;

            return result;
        }

        public bool DeleteCertificate(byte[] certificate)
        {
            TestIdentity identity;
            TestCertificate testCertificate;
            if (!FindCertificate(certificate, out identity, out testCertificate)) return false;

            for (var i = 0; i < identity.Certificates.Count; i++)
            {
                if (identity.Certificates[i].Id == testCertificate.Id)
                {
                    identity.Certificates.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public int DeleteCertificates(string identity)
        {
            var certificates = _identities[identity].Certificates;
            var result = certificates.Count;
            certificates.Clear();
            return result;
        }

        private bool FindCertificate(byte[] certificate, out TestIdentity identity, out TestCertificate testCertificate)
        {
            identity = null;
            testCertificate = null;
            if (certificate == null) return false;

            var certificateText = Encoding.UTF8.GetString(certificate);
            if (string.IsNullOrEmpty(certificateText)) return false;

            var colonIndex = certificateText.IndexOf(':');
            if (colonIndex < 1) return false;

            var identityToken = certificateText.Substring(0, colonIndex);
            if (!_identities.TryGetValue(identityToken, out identity)) return false;

            foreach (var identityCertificate in identity.Certificates)
            {
                if (identityCertificate.Certificate.Length == certificate.Length)
                {
                    var matching = true;
                    for (var i = 0; i < certificate.Length; i++)
                    {
                        if (identityCertificate.Certificate[i] != certificate[i])
                        {
                            matching = false;
                            break;
                        }
                    }
                    if (matching)
                    {
                        testCertificate = identityCertificate;
                    }
                }
            }
            return testCertificate != null;
        }


        #endregion

        #region Credentials

        public bool AddCredentials(string identity, string userName, string password, bool replaceExisting, IEnumerable<string> purposes)
        {
            var purposeList = purposes == null
                        ? new List<string>()
                        : purposes.Where(p => !string.IsNullOrEmpty(p)).ToList();

            var existing = _credentials.FirstOrDefault(c => string.Equals(c.UserName, userName, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                if (string.Equals(identity, existing.Identity, StringComparison.OrdinalIgnoreCase))
                {
                    existing.Password = password;
                    existing.Purposes = purposeList;
                }
                else
                {
                    return false;
                }
            }

            if (replaceExisting)
            {
                _credentials = _credentials
                    .Where(c => !string.Equals(c.Identity, identity, StringComparison.OrdinalIgnoreCase) 
                        || ReferenceEquals(c, existing))
                    .ToList();
            }

            if (existing == null)
            {
                var newCredential = new TestCredential
                {
                    Identity = identity,
                    UserName = userName,
                    Password = password,
                    Purposes = purposeList
                };
                _credentials.Add(newCredential);
            }
            return true;
        }

        public IAuthenticationResult AuthenticateWithCredentials(string userName, string password)
        {
            var result = new AuthenticationResult
            {
                Status = AuthenticationStatus.NotFound
            };

            var existing = _credentials.FirstOrDefault(c => string.Equals(c.UserName, userName, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
                return result;

            result.Identity = existing.Identity;
            result.Purposes = existing.Purposes;
            result.RememberMeToken = existing.Identity + ":" + string.Join(",", existing.Purposes ?? new List<string>());

            result.Status = password == existing.Password
                ? AuthenticationStatus.Authenticated
                : AuthenticationStatus.InvalidCredentials;
            return result;
        }

        #endregion

        #region Shared secrets

        public string AddSharedSecret(string identity, string name, IList<string> purposes)
        {
            var purposeList = purposes == null
                ? new List<string>()
                : purposes.Where(p => !string.IsNullOrEmpty(p)).ToList();

            var secret = Guid.NewGuid().ToShortString();
            _sharedSecrets.Add(
                new TestSharedSecret
                {
                    Identity = identity,
                    Name = name,
                    Secret = secret,
                    Purposes = purposeList
                });
            return secret;
        }

        public IAuthenticationResult AuthenticateWithSharedSecret(string sharedSecret)
        {
            var result = new AuthenticationResult
            {
                Status = AuthenticationStatus.NotFound
            };

            var secret = _sharedSecrets
                .FirstOrDefault(s => string.Equals(s.Secret, sharedSecret, StringComparison.Ordinal));
            if (secret == null)
                return result;

            result.Identity = secret.Identity;
            result.Purposes = secret.Purposes;
            result.Status = AuthenticationStatus.Authenticated;
            return result;
        }

        public bool DeleteSharedSecret(string sharedSecret)
        {
            var index = -1;
            for (var i = 0; i < _sharedSecrets.Count; i++)
                if (string.Equals(_sharedSecrets[i].Secret, sharedSecret, StringComparison.Ordinal))
                    index = i;

            if (index >= 0)
            {
                _sharedSecrets.RemoveAt(index);
                return true;
            }

            return false;
        }

        public IList<ISharedSecret> GetAllSharedSecrets(string identity)
        {
            return _sharedSecrets
                .Where(s => string.Equals(s.Identity, identity, StringComparison.OrdinalIgnoreCase))
                .Select(s => 
                    new SharedSecret
                    {
                        Name = s.Name,
                        Secret = s.Secret,
                        Purposes = s.Purposes
                    })
                .Cast<ISharedSecret>()
                .ToList();
        }

        #endregion

        #region Social

        public bool AddSocial(
            string identity,
            string userId,
            string socialService,
            string authenticationToken,
            IEnumerable<string> purposes = null,
            bool replaceExisting = true)
        {
            var purposeList = purposes == null
                ? new List<string>()
                : purposes.Where(p => !string.IsNullOrEmpty(p)).ToList();

            var existing = _socialIds
                .FirstOrDefault(s =>
                    string.Equals(s.SocialService, socialService, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(s.UserId, userId, StringComparison.Ordinal));

            if (existing != null)
            {
                existing.Identity = identity;
                existing.Purposes = purposeList;
                existing.AuthenticationToken = authenticationToken;
            }

            if (replaceExisting)
            {
                _socialIds = _socialIds
                    .Where(s => 
                        !string.Equals(identity, s.Identity, StringComparison.OrdinalIgnoreCase)
                        || ReferenceEquals(s, existing)
                        || !string.Equals(socialService, s.SocialService, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (existing == null)
            {
                var social = new TestSocial
                {
                    Identity = identity,
                    SocialService = socialService,
                    UserId = userId,
                    Purposes = purposeList,
                    AuthenticationToken = authenticationToken
                };
                _socialIds.Add(social);
                return true;
            }
            return false;
        }

        public ISocialAuthentication GetSocialAuthentication(string userId, string socialService)
        {
            var existing = _socialIds
                .FirstOrDefault(s =>
                    string.Equals(s.SocialService, socialService, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(s.UserId, userId, StringComparison.Ordinal));

            return existing == null 
                ? null 
                : new SocialAuthentication 
                {
                    Identity = existing.Identity,
                    Purposes = existing.Purposes,
                    AuthenticationToken = existing.AuthenticationToken
                };
        }

        public bool DeleteAllSocial(string identity)
        {
            _socialIds = _socialIds
                .Where(s => !string.Equals(identity, s.Identity, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return true;
        }

        public bool DeleteSocial(string identity, string socialService)
        {
            var index = -1;
            for (var i = 0; i < _socialIds.Count; i++)
            {
                if (string.Equals(_socialIds[i].SocialService, socialService, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(_socialIds[i].Identity, identity, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                _socialIds.RemoveAt(index);
                return true;
            }
            return false;
        }

        #endregion

        private class TestIdentity
        {
            public string Identity;
            public string Name;
            public readonly IList<TestCertificate> Certificates = new List<TestCertificate>();
            public readonly IList<TestCredential> Credentials = new List<TestCredential>();
        }

        private class TestCredential
        {
            public string Identity;
            public string UserName;
            public string Password;
            public IList<string> Purposes;
        }

        private class TestCertificate
        {
            public string Identity;
            public string Id;
            public byte[] Certificate;
            public DateTime? Expiry;
            public IList<string> Purposes;
        }

        private class TestSharedSecret
        {
            public string Identity;
            public string Name;
            public string Secret;
            public IList<string> Purposes;
        }

        private class TestSocial
        {
            public string Identity;
            public string SocialService;
            public string UserId;
            public string AuthenticationToken;
            public IList<string> Purposes;
        }

        private class AuthenticationResult : IAuthenticationResult
        {
            public string Identity { get; set; }
            public IList<string> Purposes { get; set; }
            public AuthenticationStatus Status { get; set; }
            public string RememberMeToken { get; set; }
        }

        private class SocialAuthentication: ISocialAuthentication
        {
            public string Identity { get; set; }
            public IList<string> Purposes { get; set; }
            public string AuthenticationToken { get; set; }
        }

        private class SharedSecret: ISharedSecret
        {
            public string Name { get; set; }
            public string Secret { get; set; }
            public IList<string> Purposes { get; set; }
        }


    }
}
