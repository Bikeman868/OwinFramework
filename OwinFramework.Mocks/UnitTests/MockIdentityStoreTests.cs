using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using OwinFramework.InterfacesV1.Facilities;
using OwinFramework.Mocks.V1.Facilities;

namespace OwinFramework.Mocks.UnitTests
{
    /// <summary>
    /// Unit tests for mocks in a bit further than most people like to take it
    /// but there is no harm, right.
    /// </summary>
    [TestFixture]
    public class MockIdentityStoreTests
    {
        private MockIdentityStore _mockIdentityStore;
        private IIdentityStore _identityStore;

        [SetUp]
        public void Setup()
        {
            _mockIdentityStore = new MockIdentityStore();
            _identityStore = _mockIdentityStore.GetImplementation<IIdentityStore>(null);
        }

        [Test]
        public void Should_provide_certificate_based_identitfication()
        {
            var lifetime = TimeSpan.FromMilliseconds(100);
            var purpose = new[] { "api" };

            var identity = _identityStore.CreateIdentity();
            var cert = _identityStore.AddCertificate(identity, lifetime, purpose);

            var result = _identityStore.AuthenticateWithCertificate(cert);

            Assert.IsNotNull(result);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(purpose.Length, result.Purposes.Count);
            Assert.AreEqual(purpose[0], result.Purposes[0]);
            Assert.AreEqual(AuthenticationStatus.Authenticated, result.Status);

            result = _identityStore.AuthenticateWithCertificate(Guid.NewGuid().ToByteArray());
            Assert.AreNotEqual(AuthenticationStatus.Authenticated, result.Status);

            Thread.Sleep(500);

            result = _identityStore.AuthenticateWithCertificate(cert);
            Assert.AreNotEqual(AuthenticationStatus.Authenticated, result.Status);
        }

        [Test]
        public void Should_provide_shared_secret_based_identification()
        {
            const string name = "Facebook access to API";
            var purpose = new[] { "api" };

            var identity = _identityStore.CreateIdentity();
            var secret = _identityStore.AddSharedSecret(identity, name, purpose);

            var result = _identityStore.AuthenticateWithSharedSecret(secret);

            Assert.IsNotNull(result);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(purpose.Length, result.Purposes.Count);
            Assert.AreEqual(purpose[0], result.Purposes[0]);
            Assert.AreEqual(AuthenticationStatus.Authenticated, result.Status);

            result = _identityStore.AuthenticateWithSharedSecret(Guid.NewGuid().ToString());
            Assert.AreNotEqual(AuthenticationStatus.Authenticated, result.Status);

            _identityStore.DeleteSharedSecret(secret);

            result = _identityStore.AuthenticateWithSharedSecret(Guid.NewGuid().ToString());
            Assert.AreNotEqual(AuthenticationStatus.Authenticated, result.Status);
        }

        [Test]
        public void Should_provide_credentials_based_identification()
        {
            const string userName = "martin@gmail.com";
            const string password = "somethingHardT0Gu3$$";

            var identity = _identityStore.CreateIdentity();
            var success = _identityStore.AddCredentials(identity, userName, password);

            Assert.IsTrue(success);

            var result = _identityStore.AuthenticateWithCredentials(userName, password);

            Assert.IsNotNull(result);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(AuthenticationStatus.Authenticated, result.Status);

            result = _identityStore.AuthenticateWithCredentials(userName, "wrong password");

            Assert.IsNotNull(result);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(AuthenticationStatus.InvalidCredentials, result.Status);

            result = _identityStore.AuthenticateWithCredentials("wrong username", password);

            Assert.IsNotNull(result);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(AuthenticationStatus.NotFound, result.Status);
        }

        [Test]
        public void Should_provide_social_login()
        {
            const string userName = "martin@gmail.com";

            var socialServices = _identityStore.SocialServices;

            Assert.IsNotNull(socialServices);
            Assert.IsTrue(socialServices.Count > 0);

            var identity = _identityStore.CreateIdentity();
            var success = _identityStore.AddSocial(identity, userName, socialServices[0]);

            Assert.IsTrue(success);

            var result = _identityStore.AuthenticateWithSocial(userName, socialServices[0], Guid.NewGuid().ToString());

            Assert.IsNotNull(result);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(AuthenticationStatus.Authenticated, result.Status);

            result = _identityStore.AuthenticateWithSocial(userName, "wrong social", Guid.NewGuid().ToString());

            Assert.IsNotNull(result);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreNotEqual(AuthenticationStatus.Authenticated, result.Status);

            result = _identityStore.AuthenticateWithSocial("wrong username", socialServices[0], Guid.NewGuid().ToString());

            Assert.IsNotNull(result);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreNotEqual(AuthenticationStatus.Authenticated, result.Status);
        }
    }
}
