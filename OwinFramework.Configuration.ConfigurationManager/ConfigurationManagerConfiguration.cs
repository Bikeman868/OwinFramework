using System;
using System.Configuration;
using OwinFramework.Interfaces.Builder;

namespace OwinFramework.Configuration.ConfiurationManager
{
    /// <summary>
    /// This class implements the IConfiguration interface using the standard .Net ConfigurationManager
    /// class. This allows you to store configuration values in your web.config or app.config file.
    /// </summary>
    /// <remarks>The .Net ConfigurationManager does not allow the config file to change
    /// while the application is running. If you use this configuration method you will have to restart
    /// your application for configuration changes to be effective.
    /// </remarks>
    /// <remarks>The Microsoft ConfigurationManager is extremely inflexible which means that when
    /// you configure the 'path' of the configuration data for your middleware this must
    /// by the name of a custom section in the config file. Furthermore the Microsoft
    /// ConfigurationManager requires each custom section to be backed by a class that conforms
    /// to strict patterns. These classes are very onerous to write so this framework does
    /// not require middleware authors to write them, and this means that it falls to your
    /// application to do this. Additionally the class that backs the custom section will not be
    /// the class that the middleware component is expecting to contain its configuration
    /// options, so this implementation uses AutoMapper to map from the custom section class
    /// to the middleware configuration class. Your application must configure AutpMapper
    /// to perform this mapping. If all of this sounds like too much trouble then consider using
    /// another more flexible configuration mechanism (such as Urchin)</remarks>
    public class ConfigurationManagerConfiguration: IConfiguration
    {
        public IDisposable Register<T>(string path, Action<T> onChangeAction, T defaultValue)
        {
            var value = defaultValue;

            var section = ConfigurationManager.GetSection(path);
            if (section != null)
                value = AutoMapper.Mapper.Map<T>(section);

            onChangeAction(value);

            return new ChangeRegistration();
        }
    
        private class ChangeRegistration : IDisposable
        {
            public void Dispose() { }
        }
    }
}
