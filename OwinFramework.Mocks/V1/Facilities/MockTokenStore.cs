using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Moq.Modules;
using OwinFramework.InterfacesV1.Facilities;

namespace OwinFramework.Mocks.V1.Facilities
{
    public class MockTokenStore : ConcreteImplementationProvider<ITokenStore>
    {
        private readonly TestTokenStore _tokenStore = new TestTokenStore();

        protected override ITokenStore GetImplementation(IMockProducer mockProducer)
        {
            return _tokenStore;
        }

        private class TestTokenStore: ITokenStore
        {
            private readonly IDictionary<string, StoredToken> _tokens = new ConcurrentDictionary<string, StoredToken>();

            string ITokenStore.CreateToken(string tokenType, string purpose, string identity)
            {
                return ((ITokenStore) this).CreateToken(tokenType, new List<string>{purpose}, identity);
            }

            string ITokenStore.CreateToken(string tokenType, IEnumerable<string> purpose, string identity)
            {
                var token = new StoredToken 
                {
                    Value = Guid.NewGuid().ToString("N"),
                    TokenType = tokenType,
                    Purpose = purpose == null ? new List<string>() : purpose.ToList(),
                    Identity = identity
                };
                _tokens.Add(token.Value, token);
                return token.Value;
            }

            bool ITokenStore.DeleteToken(string token)
            {
                return _tokens.Remove(token);
            }

            IToken ITokenStore.GetToken(string tokenType, string token, string purpose, string identity)
            {
                var result = new Token
                {
                    Value = token,
                    Identity = identity,
                    Purpose = purpose,
                    Status = TokenStatus.Allowed
                };

                StoredToken storedToken;
                if (_tokens.TryGetValue(token, out storedToken))
                {
                    if (storedToken.TokenType != tokenType)
                        result.Status = TokenStatus.NotAllowed;

                   if (!string.IsNullOrEmpty(storedToken.Identity))
                   {
                       if (!string.Equals(identity, storedToken.Identity, StringComparison.InvariantCultureIgnoreCase))
                           result.Status = TokenStatus.NotAllowed;
                   }

                   if (storedToken.Purpose != null && storedToken.Purpose.Count > 0)
                   {
                       if (!storedToken.Purpose.Any(p => string.Equals(purpose, p, StringComparison.InvariantCultureIgnoreCase)))
                           result.Status = TokenStatus.NotAllowed;
                   }
                }
                else
                {
                    result.Status = TokenStatus.Invalid;
                }

                return result;
            }

            private class StoredToken
            {
                public string Value;
                public string TokenType;
                public List<string> Purpose;
                public string Identity;
            }

            private class Token : IToken
            {
                public string Identity { get; set; }
                public string Purpose { get; set; }
                public TokenStatus Status { get; set; }
                public string Value { get; set; }
            }
        }
    }
}
