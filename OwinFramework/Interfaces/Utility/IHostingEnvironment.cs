namespace OwinFramework.Interfaces.Utility
{
    /// <summary>
    /// This interface provides a way for the application to supply the OWIN
    /// Framework with information about the hosting environment. Unfortunately
    /// OWIN itself does not provide a mechanism that works for all types of hosting.
    /// </summary>
    public interface IHostingEnvironment
    {
        /// <summary>
        /// Maps a relative path within the web site to a physical file path.
        /// There is no way to write code that works for every hosting environment,
        /// so you might need to provide your own implementation of this in your
        /// application.
        /// </summary>
        /// <param name="path">The path to a file within your web site</param>
        /// <returns>The fully qualified file name in the file system for this file</returns>
        string MapPath(string path);
    }
}
