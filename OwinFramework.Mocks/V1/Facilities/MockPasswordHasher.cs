using System;
using System.Collections.Generic;
using System.Text;
using Moq.Modules;
using OwinFramework.InterfacesV1.Facilities;

namespace OwinFramework.Mocks.V1.Facilities
{
    public class MockPasswordHasher : ConcreteImplementationProvider<IPasswordHasher>, IPasswordHasher
    {
        private readonly IDictionary<int, IPasswordHashingScheme> _schemes = new Dictionary<int, IPasswordHashingScheme>();
        private int _latestVersion = 1;

        protected override IPasswordHasher GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        public PasswordCheckResult CheckPasswordAllowed(string identity, string password)
        {
            return new PasswordCheckResult {IsAllowed = true};
        }

        public void ComputeHash(string identity, string password, ref int? version, out byte[] salt, out byte[] hash)
        {
            version = version ?? _latestVersion;

            IPasswordHashingScheme scheme;
            if (_schemes.TryGetValue(version.Value, out scheme))
            {
                salt = null;
                hash = scheme.ComputeHash(password, ref salt);
            }
            else
            {
                salt = Guid.NewGuid().ToByteArray();
                hash = Encoding.ASCII.GetBytes(password);
            }
        }

        public void ComputeHash(string password, int version, byte[] salt, out byte[] hash)
        {
            IPasswordHashingScheme scheme;
            if (_schemes.TryGetValue(version, out scheme))
            {
                hash = scheme.ComputeHash(password, ref salt);
            }
            else
            {
                hash = Encoding.ASCII.GetBytes(password);
            }
        }

        public void SetHashingScheme(int version, IPasswordHashingScheme scheme)
        {
            _schemes[version] = scheme;

            if (version > _latestVersion) _latestVersion = version;
        }

        public IPasswordHashingScheme GetHashingScheme(int version)
        {
            IPasswordHashingScheme scheme;
            return _schemes.TryGetValue(version, out scheme) ? scheme : null;
        }
    }
}
