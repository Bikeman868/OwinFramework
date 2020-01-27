using System;
using System.Collections.Generic;
using Moq.Modules;
using OwinFramework.Interfaces.Builder;

namespace OwinFramework.Mocks.Builder
{
    public class MockConfiguration : ConcreteImplementationProvider<IConfiguration>, IConfiguration, IDisposable
    {
        private readonly List<Registration> _registrations = new List<Registration>();
        private readonly Dictionary<string, object> _configurations = new Dictionary<string, object>();

        protected override IConfiguration GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        public IDisposable Register<T>(string path, Action<T> onChangeAction, T defaultValue)
        {
            _registrations.Add(new Registration<T>().Initialize(path, onChangeAction));

            object config;
            if (_configurations.TryGetValue(path.ToLower(), out config))
                onChangeAction((T)config);
            else
                onChangeAction(defaultValue);

            return this;
        }

        public IDisposable Register<T>(string path, Action<T> onChangeAction)
        {
            _registrations.Add(new Registration<T>().Initialize(path, onChangeAction));

            object config;
            if (_configurations.TryGetValue(path.ToLower(), out config))
                onChangeAction((T)config);
            return this;
        }

        public void CancelRegistrations()
        {
            _registrations.Clear();
        }

        public void Clear()
        {
            _configurations.Clear();
        }

        public void Dispose()
        {
            CancelRegistrations();
            Clear();
        }

        public void SetConfiguration<T>(string path, T newValue)
        {
            _configurations[path.ToLower()] = newValue;

            foreach (var registration in _registrations)
                if (registration.IsMatch<T>(path))
                    registration.Changed(newValue);
        }

        private abstract class Registration
        {
            private string _path;
            private Type _type;

            protected Registration Initialize(string path, Type type)
            {
                _path = path.ToLower();
                _type = type;
                return this;
            }

            public bool IsMatch<T>(string path)
            {
                return 
                    string.Equals(path, _path, StringComparison.InvariantCultureIgnoreCase) && 
                    _type == typeof(T);
            }

            public abstract void Changed(object newValue);
        }

        private class Registration<T> : Registration
        {
            private Action<T> _onChangeAction;

            public Registration Initialize(string path, Action<T> onChangeAction)
            {
                _onChangeAction = onChangeAction;

                return base.Initialize(path, typeof(T));
            }

            public override void Changed(object newValue)
            {
                _onChangeAction((T)newValue);
            }
        }
    }
}
