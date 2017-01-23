using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.MiddlewareHelpers.Analysable
{
    /// <summary>
    /// An abstract base class which makes it easier to implement IStatistic
    /// </summary>
    public abstract class Statistic : IStatistic
    {
        public float Value { get; protected set; }
        public float Denominator { get; protected set; }
        public string Formatted { get; protected set; }

        public abstract IStatistic Refresh();
    }
}
