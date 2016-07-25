using OwinFramework.Interfaces.Builder;

namespace OwinFramework.InterfacesV1.Capability
{
    /// <summary>
    /// Middleware can choose to implement this interface if they can be configured.
    /// If there is nothing to configure then the middleware does not need to implement this interface
    /// </summary>
    public interface IConfigurable
    {
        /// <summary>
        /// The pipeline builder will call this method of the middleware once at startup to give it
        /// a chance grab configuration data.
        /// </summary>
        /// <param name="configuration">The configuration mechanism used by the application</param>
        /// <param name="path">The path to this middleware's configuration in the configuration file</param>
        void Configure(IConfiguration configuration, string path);
    }
}
