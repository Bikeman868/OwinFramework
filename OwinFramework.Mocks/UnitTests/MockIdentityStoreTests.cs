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
            Assert.AreEqual(AuthenticationStatus.NotFound, result.Status);

            _identityStore.DeleteSharedSecret(secret);
            result = _identityStore.AuthenticateWithSharedSecret(secret);
            Assert.AreEqual(AuthenticationStatus.NotFound, result.Status);
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
            Assert.AreEqual(AuthenticationStatus.NotFound, result.Status);
        }

        [Test]
        public void Should_remember_me()
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

            var result2 = _identityStore.RememberMe(result.RememberMeToken);

            Assert.IsNotNull(result2);
            Assert.AreEqual(identity, result2.Identity);
            Assert.AreEqual(AuthenticationStatus.Authenticated, result2.Status);
        }

        [Test]
        public void Should_remember_third_party()
        {
            const string userName = "martin@gmail.com";
            const string password = "somethingHardT0Gu3$$";

            const string delegateUserName = "fred@gmail.com";
            const string delegatePassword = "NotSoHard";
            var delegatePurposes = new List<string> {"ManageContacts"};

            var identity = _identityStore.CreateIdentity();
            Assert.IsTrue(_identityStore.AddCredentials(identity, userName, password));
            Assert.IsTrue(_identityStore.AddCredentials(identity, delegateUserName, delegatePassword, false, delegatePurposes));

            var result = _identityStore.AuthenticateWithCredentials(delegateUserName, delegatePassword);

            Assert.IsNotNull(result);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(AuthenticationStatus.Authenticated, result.Status);
            Assert.AreEqual(delegatePurposes[0], result.Purposes[0]);

            var result2 = _identityStore.RememberMe(result.RememberMeToken);

            Assert.IsNotNull(result2);
            Assert.AreEqual(identity, result2.Identity);
            Assert.AreEqual(AuthenticationStatus.Authenticated, result2.Status);
            Assert.AreEqual(delegatePurposes[0], result2.Purposes[0]);
        }

        [Test]
        public void Should_allow_password_change()
        {
            const string userName = "martin@gmail.com";
            const string oldPassword = "somethingHardT0Gu3$$";
            const string newPassword = "evenHarderT0Gu3$$2016";

            var identity = _identityStore.CreateIdentity();
            _identityStore.AddCredentials(identity, userName, oldPassword);

            var result = _identityStore.AuthenticateWithCredentials(userName, oldPassword);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(AuthenticationStatus.Authenticated, result.Status);

            _identityStore.AddCredentials(identity, userName, newPassword);

            result = _identityStore.AuthenticateWithCredentials(userName, newPassword);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(AuthenticationStatus.Authenticated, result.Status);

            result = _identityStore.AuthenticateWithCredentials(userName, oldPassword);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(AuthenticationStatus.InvalidCredentials, result.Status);
        }

        [Test]
        public void Should_allow_change_username()
        {
            const string oldUserName = "martin@gmail.com";
            const string newUserName = "martin@hotmail.com";
            const string password = "somethingHardT0Gu3$$";

            var identity = _identityStore.CreateIdentity();
            _identityStore.AddCredentials(identity, oldUserName, password);

            var result = _identityStore.AuthenticateWithCredentials(oldUserName, password);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(AuthenticationStatus.Authenticated, result.Status);

            _identityStore.AddCredentials(identity, newUserName, password);

            result = _identityStore.AuthenticateWithCredentials(newUserName, password);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(AuthenticationStatus.Authenticated, result.Status);

            result = _identityStore.AuthenticateWithCredentials(oldUserName, password);
            Assert.AreEqual(AuthenticationStatus.NotFound, result.Status);
        }

        [Test]
        public void Should_allow_multiple_usernames()
        {
            const string fullAccessUserName = "martin@gmail.com";
            const string restrictedUserName = "martin@hotmail.com";
            const string fullAccessPassword = "somethingHardT0Gu3$$";
            const string restrictedAccessPassword = "EasyToGuess";

            var identity = _identityStore.CreateIdentity();
            _identityStore.AddCredentials(identity, fullAccessUserName, fullAccessPassword);
            _identityStore.AddCredentials(identity, restrictedUserName, restrictedAccessPassword, false, new[]{"view", "report"});

            var result = _identityStore.AuthenticateWithCredentials(fullAccessUserName, fullAccessPassword);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(0, result.Purposes.Count);
            Assert.AreEqual(AuthenticationStatus.Authenticated, result.Status);

            result = _identityStore.AuthenticateWithCredentials(restrictedUserName, restrictedAccessPassword);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(2, result.Purposes.Count);
            Assert.AreEqual(AuthenticationStatus.Authenticated, result.Status);
        }

        [Test]
        public void Should_provide_social_login()
        {
            var socialServices = _identityStore.SocialServices;

            Assert.IsNotNull(socialServices);
            Assert.IsTrue(socialServices.Count > 0);

            var socialService = socialServices[0];
            const string userId = "martin@gmail.com";
            var authenticationToken = Guid.NewGuid().ToString();

            var identity = _identityStore.CreateIdentity();
            var isNew = _identityStore.AddSocial(identity, userId, socialService, authenticationToken);

            Assert.IsTrue(isNew);

            var result = _identityStore.GetSocialAuthentication(userId, socialService);

            Assert.IsNotNull(result);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(authenticationToken, result.AuthenticationToken);

            result = _identityStore.GetSocialAuthentication("invalid user id", socialService);

            Assert.IsNull(result);

            result = _identityStore.GetSocialAuthentication(userId, socialServices[1]);

            Assert.IsNull(result);

            var deleted = _identityStore.DeleteSocial(identity, socialService);

            Assert.IsTrue(deleted);

            result = _identityStore.GetSocialAuthentication(userId, socialService);

            Assert.IsNull(result);
        }

        [Test]
        public void Should_allow_multiple_identification_mechanisms()
        {
            const string userName = "martin@gmail.com";
            const string password = "somethingHardT0Gu3$$";

            var identity = _identityStore.CreateIdentity();
            _identityStore.AddCredentials(identity, userName, password);
            var sharedSecret = _identityStore.AddSharedSecret(identity, "3rd party API access", new[] { "api" });

            var result = _identityStore.AuthenticateWithCredentials(userName, password);

            Assert.IsNotNull(result);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(0, result.Purposes.Count);
            Assert.AreEqual(AuthenticationStatus.Authenticated, result.Status);

            result = _identityStore.AuthenticateWithSharedSecret(sharedSecret);

            Assert.IsNotNull(result);
            Assert.AreEqual(identity, result.Identity);
            Assert.AreEqual(1, result.Purposes.Count);
            Assert.AreEqual("api", result.Purposes[0]);
            Assert.AreEqual(AuthenticationStatus.Authenticated, result.Status);
        }
    }
}
