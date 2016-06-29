using System;

namespace OwinFramework.AnalysisReporter
{
    [Serializable]
    public class Configuration
    {
        public string Path { get; set; }
        public bool Enabled { get; set; }
        public string RequiredPermission { get; set; }
        public string DefaultFormat { get; set; }

        public Configuration()
        {
            Path = "/owin/analysis";
            Enabled = true;
            DefaultFormat = "application/json";
        }
    }
}
