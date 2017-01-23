using System;
using System.Globalization;
using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.MiddlewareHelpers.Analysable
{
    /// <summary>
    /// Implements IStatistic when measuring the number of events that occured
    /// within a given time span
    /// </summary>
    public abstract class CountPerTimeSpanStatistic : Statistic
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

            Denominator = (float)timeSpan.TotalSeconds;
            Value = count / Denominator;
            Formatted = Value.ToString("g3", CultureInfo.InvariantCulture) + "/s";

            return this;
        }
    }
}
