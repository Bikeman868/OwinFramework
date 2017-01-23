using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.MiddlewareHelpers.Analysable
{
    /// <summary>
    /// An abstract base class which makes it easier to implement IStatistic
    /// </summary>
    public abstract class Statistic : IStatistic
    {
        /// <summary>
        /// The value that gets graphed and displayed of this statistic
        /// </summary>
        public float Value { get; protected set; }

        /// <summary>
        /// If this statistic is a ratio then this is the denominator. You can get back
        /// the numerator of the ratio by multiplying the Value and the Denominator
        /// </summary>
        public float Denominator { get; protected set; }

        /// <summary>
        /// Human readable format of the Value property
        /// </summary>
        public string Formatted { get; protected set; }

        /// <summary>
        /// When this is called the properties of this object should be all updated
        /// </summary>
        /// <returns>this for fluid syntax</returns>
        public abstract IStatistic Refresh();
    }
}
