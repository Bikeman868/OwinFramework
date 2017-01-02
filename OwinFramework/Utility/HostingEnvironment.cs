using System;
using System.IO;
using OwinFramework.Interfaces.Utility;

namespace OwinFramework.Utility
{
    /// <summary>
    /// Provides a MapPath method that uses AppDomain.CurrentDomain.SetupInformation.ApplicationBase
    /// which works for many different hosting envronments (but not all).
    /// </summary>
    public class HostingEnvironment: IHostingEnvironment
    {
        /// <summary>
        /// There is no own method that works for all environments. The safest way is to provide
        /// an implementation of this method within your application. The OWIN Framework will use
        /// your implementation wherever it needs to resolve a relative path into a physical file
        /// location.
        /// </summary>
        /// <seealso cref="http://stackoverflow.com/questions/24571258/how-do-you-resolve-a-virtual-path-to-a-file-under-an-owin-host"/>
        string IHostingEnvironment.MapPath(string path)
        {
            path = path.Replace("/", "\\");

            if (path.StartsWith("~\\"))
                path = path.Substring(2);

            if (Path.IsPathRooted(path))
                return path;

            return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
        }
    }
}
