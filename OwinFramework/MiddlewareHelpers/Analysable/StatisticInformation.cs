using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.MiddlewareHelpers.Analysable
{
    /// <summary>
    /// Implemennts IStatisticInformation with read/write properties
    /// </summary>
    public class StatisticInformation : IStatisticInformation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Explanation { get; set; }
        public string Units { get; set; }
    }
}
