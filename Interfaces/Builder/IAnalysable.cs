using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwinFramework.Interfaces.Builder
{
    /// <summary>
    /// Middleware that implements this interface gather statictics at
    /// runtime about the requests that have been handled, and expose
    /// this to allow other middleware to provide a 'dashboard' type
    /// functionallity for the application developer or support
    /// engineer to figure out issues with the way the system is
    /// performing.
    /// </summary>
    public interface IAnalysable
    {
        /// <summary>
        /// Returns a list of the statistics that this middleware provides for inclusion
        /// on a dashboard. Typically the user will choose statistics to incluse on
        /// the dashboard from this list
        /// </summary>
        IList<IStatisticInformation> AvailableStatistics { get; }

        /// <summary>
        /// Returns a statistic by its ID.
        /// </summary>
        IStatistic GetStatistic(string id);
    }

    public interface IStatisticInformation
    {
        /// <summary>
        /// Persistent identifier for this statstic. This can be stored in dashboard
        /// applications persistently to refer to this statistic. Once your middleware
        /// is published you must support the old ID values in the future.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// A short lingle line name of this statistic. This is used in drop-down lists
        /// when the user is asked to choose a statistic
        /// </summary>
        /// <see cref="https://www.vicimediainc.com/google-analytics-cheat-sheet-2/"/>
        string Name { get; }

        /// <summary>
        /// The units of measure for this statistic, For example seconds or
        /// bytes per second.
        /// </summary>
        string Units { get; }

        /// <summary>
        /// A longer plain text description that can be mutiple lines
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Returns a detailed explanation of how to interpret the numbers
        /// </summary>
        string Explanation { get; }
    }

    public interface IStatistic
    {
        /// <summary>
        /// The numerical value of this statistic. This is provided so that
        /// a dashboard application can graph the results or calculate
        /// trends.
        /// </summary>
        float Value { get; }

        /// <summary>
        /// When the value property is a ratio then this property contains
        /// the denominator of that ratio. This allows the ratio calculation
        /// to be reversed back into the original two values.
        /// For example if the Value is in bytes/sec then this property
        /// must contain the number of seconds so that Value*Deniminator
        /// will result in the total number of bytes.
        /// </summary>
        float Denominator { get; }

        /// <summary>
        /// Returns the value formatted with units, and scaled for human
        /// readability.
        /// </summary>
        string Formatted { get; }

        /// <summary>
        /// Updates all of the properties atomically with new values. If no
        /// new values are available then the values will remain unchanged.
        /// </summary>
        IStatistic Refresh();
    }
}
