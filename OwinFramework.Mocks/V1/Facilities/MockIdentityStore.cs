using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq.Modules;
using OwinFramework.InterfacesV1.Facilities;

namespace OwinFramework.Mocks.V1.Facilities
{
    public class MockIdentityStore : ConcreteImplementationProvider<IIdentityStore>, IIdentityStore
    {
        protected override IIdentityStore GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        private readonly IDictionary<string, TestIdentity> _identities = new ConcurrentDictionary<string, TestIdentity>();
        private readonly IList<TestSharedSecret> _sharedSecrets = new List<TestSharedSecret>();

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

        #endregion

        string IIdentityStore.CreateIdentity()
        {
            var identity = new TestIdentity {Identity = Guid.NewGuid().ToString("N")};
            _identities.Add(identity.Identity, identity);
            return identity.Identity;
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

        public bool AddCredentials(string identity, string userName, string password, bool replaceExisting, IList<string> purposes)
        {
            throw new NotImplementedException();
        }

        public IAuthenticationResult AuthenticateWithCredentials(string userName, string password)
        {
            throw new NotImplementedException();
        }

        public bool SupportsCredentials
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Shared secrets

        public string AddSharedSecret(string identity, string name, IList<string> purposes)
        {
            throw new NotImplementedException();
        }

        public IAuthenticationResult AuthenticateWithSharedSecret(string sharedSecret)
        {
            throw new NotImplementedException();
        }

        public bool DeleteSharedSecret(string sharedSecret)
        {
            throw new NotImplementedException();
        }

        public IList<ISharedSecret> GetAllSharedSecrets(string identity)
        {
            throw new NotImplementedException();
        }

        public bool SupportsSharedSecrets
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region Social

        public bool AddSocial(string identity, string userName, string socialService, IList<string> purposes)
        {
            throw new NotImplementedException();
        }

        public IAuthenticationResult AuthenticateWithSocial(string userName, string socialService, string authenticationToken)
        {
            throw new NotImplementedException();
        }

        public bool DeleteAllSocial(string identity)
        {
            throw new NotImplementedException();
        }

        public bool DeleteSocial(string identity, string userName, string socialService)
        {
            throw new NotImplementedException();
        }

        public IList<string> SocialServices
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        private class TestIdentity
        {
            public string Identity;
            public string Name;
            public readonly IList<TestCertificate> Certificates = new List<TestCertificate>();
        }

        private class TestCredential
        {

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

        }

        private class TestSocial
        {

        }

        private class AuthenticationResult : IAuthenticationResult
        {
            public string Identity { get; set; }
            public IList<string> Purposes { get; set; }
            public AuthenticationStatus Status { get; set; }
        }
    }
}
