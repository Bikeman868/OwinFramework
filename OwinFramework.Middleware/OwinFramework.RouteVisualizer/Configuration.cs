using System;

namespace OwinFramework.RouteVisualizer
{
    [Serializable]
    public class Configuration
    {
        public string Path { get; set; }

        public Configuration()
        {
            Path = "/routes.svg";
        }
    }
}
