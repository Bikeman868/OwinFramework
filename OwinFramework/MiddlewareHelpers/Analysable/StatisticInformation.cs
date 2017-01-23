using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.MiddlewareHelpers.Analysable
{
    /// <summary>
    /// Implemennts IStatisticInformation with read/write properties
    /// </summary>
    public class StatisticInformation : IStatisticInformation
    {
        /// <summary>
        /// Any unique ID that identifies this statistic within your middleware
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// A short name to display in labels and drop-down lists
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Longer description, can include HTML
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Technical explanation of how this staticstic is measured and calculated
        /// </summary>
        public string Explanation { get; set; }

        /// <summary>
        /// Basic units of measure (for example 's' for seconds)
        /// </summary>
        public string Units { get; set; }
    }
}
