namespace OwinFramework.Interfaces.Builder
{
    /// <summary>
    /// Middleware can choose to implement this interface if they can be configured.
    /// If there is nothing to configure then the middleware does not need to implement this interface
    /// </summary>
    public interface IConfigurable
    {
        // The pipeline builder will call this method of the middleware once at startup to give it
        // a chance grab configuration data.
        void Configure(IConfiguration configuration, string path);
    }
}
