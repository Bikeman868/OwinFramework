using System;
using OwinFramework.Interfaces.Builder;
using Urchin.Client.Interfaces;

namespace OwinFramework.Configuration.Urchin
{
    /// <summary>
    /// This class implements the Owin Framework IConfiguration interface using the Urchin client.
    /// If you choose to use this method of configuring your middleware you must add the Urchin.Client
    /// NuGet package to your application and initialize it. See Urchin documentation for more detail.
    /// </summary>
    public class UrchinConfiguration: IConfiguration
    {
        private readonly IConfigurationStore _configurationStore;

        public UrchinConfiguration(IConfigurationStore configurationStore)
        {
            _configurationStore = configurationStore;
        }

        public IDisposable Register<T>(string path, Action<T> onChangeAction, T defaultValue)
        {
            return _configurationStore.Register(path, onChangeAction, defaultValue);
        }
    }
}
