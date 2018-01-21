using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Moq.Modules;
using OwinFramework.InterfacesV1.Facilities;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.MiddlewareHelpers.Identification;

namespace OwinFramework.Mocks.V1.Facilities
{
    public class MockIdentityDirectory : ConcreteImplementationProvider<IIdentityDirectory>, IIdentityDirectory
    {
        protected override IIdentityDirectory GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        private readonly IDictionary<string, TestIdentity> _identities = new ConcurrentDictionary<string, TestIdentity>();

        #region Methods to control mock behaviour in unit tests

        public void SetIdentityName(string identity, string name)
        {
            _identities[identity].Name = name;
        }

        public string GetIdentityName(string identity)
        {
            return _identities[identity].Name;
        }

        #endregion

        public string CreateIdentity()
        {
            var identity = new TestIdentity {Identity = Guid.NewGuid().ToString("N")};
            _identities.Add(identity.Identity, identity);
            return identity.Identity;
        }

        public IList<IIdentityClaim> GetClaims(string identity)
        {
            TestIdentity testIdentity;
            if (!_identities.TryGetValue(identity, out testIdentity))
                return new List<IIdentityClaim>();
            return testIdentity.Claims.Cast<IIdentityClaim>().ToList();
        }

        public string UpdateClaim(string identity, IIdentityClaim claim)
        {
            TestIdentity testIdentity;
            if (!_identities.TryGetValue(identity, out testIdentity)) return identity;

            var existingClaim = testIdentity.Claims.FirstOrDefault(c => c.Name == claim.Name);
            if (existingClaim == null)
            {
                testIdentity.Claims.Add(new IdentityClaim(claim));
            }
            else
            {
                existingClaim.Value = claim.Value;
                existingClaim.Status = claim.Status;
            }

            return identity;
        }

        public string DeleteClaim(string identity, string claimName)
        {
            TestIdentity testIdentity;
            if (!_identities.TryGetValue(identity, out testIdentity)) return identity;

            var claimsToKeep = testIdentity.Claims.Where(c => c.Name != claimName).ToList();

            testIdentity.Claims.Clear();
            foreach (var claim in claimsToKeep)
                testIdentity.Claims.Add(claim);

            return identity;
        }

        public IIdentitySearchResult Search(string searchText, string pagerToken = null, int maxResultCount = 20, string claimName = null)
        {
            return new IdentitySearchResult
            {
                PagerToken = string.Empty,
                Identities = _identities.Values
                    .Where(i =>
                        (i.Identity.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (i.Claims.Any(
                            c =>
                                c.Status == ClaimStatus.Verified &&
                                c.Value.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)))
                    .Select(i =>
                        new MatchingIdentity
                        {
                            Identity = i.Identity,
                            Claims = i.Claims.Cast<IIdentityClaim>().ToList()
                        } as IMatchingIdentity)
                    .ToList()
            };
        }

        private class TestIdentity
        {
            public string Identity;
            public string Name;
            public readonly IList<IdentityClaim> Claims = new List<IdentityClaim>();
        }
        private class IdentitySearchResult: IIdentitySearchResult
        {
            public string PagerToken { get; set; }
            public IList<IMatchingIdentity> Identities { get; set; }
        }

        private class MatchingIdentity: IMatchingIdentity
        {
            public string Identity { get; set; }
            public IList<IIdentityClaim> Claims { get; set; }
        }
    }
}
