﻿using System;

namespace OwinFramework.Interfaces.Builder
{
    /// <summary>
    /// Defines an application configuration mechanism. The application can choose any available configuration
    /// mechanism (for example using the web.config file) or provide a custom implementation.
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Middleware components can call this to register for changes in the configuration data
        /// for the middleware component. Use this overload when you have a default configuration that 
        /// works if the application developer does not explicitly configure the middleware
        /// </summary>
        /// <typeparam name="T">The type of the class that configuration should be deserialized into. If the
        /// configuration data cannot be deserialized to this type then the default configuration will be
        /// used.</typeparam>
        /// <param name="path">The root location in the configuration file for this middleware components config. 
        /// This path looks like the path part of a URL, but maps onto the XML structure, JSON structure or other
        /// hierarchical structure used by the configuration system</param>
        /// <param name="onChangeAction">A Lambda expression that will be called immediately upon registration and
        /// again whenever the configuration changes</param>
        /// <param name="defaultValue">The default value to return when the configuration file does not have
        /// a configuration for this middleware component or the configuration cannot be deserialized as the
        /// requested type</param>
        /// <returns>A disposable object. Disposing of this object will stop any future onChangeAction events</returns>
        IDisposable Register<T>(string path, Action<T> onChangeAction, T defaultValue);

        /// <summary>
        /// Middleware components can call this to register for changes in the configuration data
        /// for the middleware component. Use this overload when the application developer must provide a
        /// configuration because there is no sensible default.
        /// </summary>
        /// <typeparam name="T">The type of the class that configuration should be deserialized into. If the
        /// configuration data can not be deserialized to this type then an exception is thrown by this method.
        /// If the configuration is initially valid then changes later to something that cannot be deserialized
        /// then an error will be logged and the onChangeAction will not be called.
        /// </typeparam>
        /// <param name="path">The root location in the configuration file for this middleware components config. 
        /// This path looks like the path part of a URL, but maps onto the XML structure, JSON structure or other
        /// hierarchical structure used by the configuration system</param>
        /// <param name="onChangeAction">A Lambda expression that will be called immediately upon registration and
        /// again whenever the configuration changes</param>
        /// <returns>A disposable object. Disposing of this object will stop any future onChangeAction events</returns>
        IDisposable Register<T>(string path, Action<T> onChangeAction);
    }
}
