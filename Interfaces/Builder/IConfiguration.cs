using System;

namespace OwinFramework.Interfaces.Builder
{
    public interface IConfiguration
    {
        /// <summary>
        /// Middleware components can call this to register for changes in the configuration data
        /// for the middleware component.
        /// </summary>
        /// <typeparam name="T">The type of the class that configuration should be deserialized into</typeparam>
        /// <param name="path">The root location in the configuration file for this middleware components config. 
        /// This path looks like the path part of a URL, but maps onto the XML structure, JSON structure or other
        /// structure used by the configuration system</param>
        /// <param name="onChangeAction">A Lambda expression that will be called immediately upon registration and
        /// again whenever the configuration changes</param>
        /// <param name="defaultValue">The default value to return when the configuration file does not have
        /// a configuration for this middleware component</param>
        /// <returns>A disposable object. Disposing of this object will stop any further change events</returns>
        IDisposable Register<T>(string path, Action<T> onChangeAction, T defaultValue = default(T));
    }
}
