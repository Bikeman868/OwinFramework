using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class MockTokenStoreTests
    {
        private MockTokenStore _mockTokenStore;
        private ITokenStore _tokenStore;

        [SetUp]
        public void Setup()
        {
            _mockTokenStore = new MockTokenStore();
            _tokenStore = _mockTokenStore.GetImplementation<ITokenStore>(null);
        }

        [Test]
        public void Should_allow_default_tokens_for_any_identity_and_purpose()
        {
            const string tokenType = "session";

            var sessionToken = _tokenStore.CreateToken(tokenType);
            var token = _tokenStore.GetToken(tokenType,  sessionToken);

            Assert.IsNotNull(token);
            Assert.AreEqual(sessionToken, token.Value);
            Assert.IsTrue(string.IsNullOrEmpty(token.Identity));
            Assert.IsTrue(string.IsNullOrEmpty(token.Purpose));
            Assert.AreEqual(TokenStatus.Allowed, token.Status);

            const string identity = "urn:user:431";
            const string purpose = "login";
            token = _tokenStore.GetToken(tokenType, sessionToken, purpose, identity);

            Assert.IsNotNull(token);
            Assert.AreEqual(sessionToken, token.Value);
            Assert.AreEqual(purpose, token.Purpose);
            Assert.AreEqual(identity, token.Identity);
            Assert.AreEqual(TokenStatus.Allowed, token.Status);
        }

        [Test]
        public void Should_check_token_type()
        {
            var sessionToken = _tokenStore.CreateToken("session");
            var token = _tokenStore.GetToken("authentication", sessionToken);

            Assert.IsNotNull(token);
            Assert.AreEqual(TokenStatus.NotAllowed, token.Status);
        }

        [Test]
        public void Should_check_identity()
        {
            const string tokenType = "session";
            const string identity = "urn:user:1234";

            var tokenId = _tokenStore.CreateToken(tokenType, "", identity);
            var token = _tokenStore.GetToken(tokenType, tokenId, "login", identity);

            Assert.IsNotNull(token);
            Assert.AreEqual(TokenStatus.Allowed, token.Status);

            token = _tokenStore.GetToken(tokenType, tokenId, "", identity);

            Assert.IsNotNull(token);
            Assert.AreEqual(TokenStatus.Allowed, token.Status);

            token = _tokenStore.GetToken(tokenType, tokenId, "login", identity + "00");

            Assert.IsNotNull(token);
            Assert.AreEqual(TokenStatus.NotAllowed, token.Status);

            token = _tokenStore.GetToken(tokenType, tokenId, "login");

            Assert.IsNotNull(token);
            Assert.AreEqual(TokenStatus.NotAllowed, token.Status);
        }

        [Test]
        public void Should_check_purpose()
        {
            const string tokenType = "session";
            const string purpose = "login";

            var tokenId = _tokenStore.CreateToken(tokenType, purpose);
            var token = _tokenStore.GetToken(tokenType, tokenId, purpose);

            Assert.IsNotNull(token);
            Assert.AreEqual(TokenStatus.Allowed, token.Status);

            token = _tokenStore.GetToken(tokenType, tokenId);

            Assert.IsNotNull(token);
            Assert.AreEqual(TokenStatus.NotAllowed, token.Status);

            token = _tokenStore.GetToken(tokenType, tokenId, "wrong purpose");

            Assert.IsNotNull(token);
            Assert.AreEqual(TokenStatus.NotAllowed, token.Status);
        }

        [Test]
        public void Should_support_milti_purpose_tokens()
        {
            const string tokenType = "session";
            var purpose = new[] { "login", "logout" };

            var tokenId = _tokenStore.CreateToken(tokenType, purpose);

            var token = _tokenStore.GetToken(tokenType, tokenId, purpose[0]);
            Assert.IsNotNull(token);
            Assert.AreEqual(TokenStatus.Allowed, token.Status);

            token = _tokenStore.GetToken(tokenType, tokenId, purpose[1]);
            Assert.IsNotNull(token);
            Assert.AreEqual(TokenStatus.Allowed, token.Status);

            token = _tokenStore.GetToken(tokenType, tokenId, "wrong purpose");
            Assert.IsNotNull(token);
            Assert.AreEqual(TokenStatus.NotAllowed, token.Status);

            token = _tokenStore.GetToken(tokenType, tokenId);
            Assert.IsNotNull(token);
            Assert.AreEqual(TokenStatus.NotAllowed, token.Status);
        }
    }
}
