using System;
using System.Globalization;
using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.MiddlewareHelpers.Analysable
{
    /// <summary>
    /// Implements IStatistic when measuring the total time taken to
    /// complete a specific number of events
    /// </summary>
    public abstract class TimeSpanPerCountStatistic : Statistic
    {
        /// <summary>
        /// You must override this in a derrived class and provide the mechanism for
        /// retrieving the values to record
        /// </summary>
        protected abstract void GetValue(out TimeSpan time, out int count);

        /// <summary>
        /// Updates the properties with the latest statistic
        /// </summary>
        /// <returns>this for fluid syntax</returns>
        public override IStatistic Refresh()
        {
            TimeSpan timeSpan;
            int count;
            GetValue(out timeSpan, out count);

            Denominator = count;
            Value = (float)(timeSpan.TotalSeconds / Denominator);
            Formatted = Value.ToString("g3", CultureInfo.InvariantCulture) + "s";

            return this;
        }
    }
}
