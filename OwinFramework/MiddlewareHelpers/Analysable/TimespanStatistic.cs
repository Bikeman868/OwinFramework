using System;
using System.Globalization;
using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.MiddlewareHelpers.Analysable
{
    /// <summary>
    /// Implements IStatistic for TimeSpan statistic values
    /// </summary>
    public class TimeSpanStatistic : Statistic
    {
        private readonly Func<TimeSpan> _getValue;

        public TimeSpanStatistic(Func<TimeSpan> getValue)
        {
            _getValue = getValue;
        }

        public override IStatistic Refresh()
        {
            var timeSpan = _getValue();

            Value = (float)timeSpan.TotalSeconds;
            Denominator = 1;

            if (timeSpan.TotalDays >= 2) Formatted = ((int)timeSpan.TotalDays) + " days";
            else if (timeSpan.TotalHours >= 2) Formatted = ((int)timeSpan.TotalHours) + " hours";
            else if (timeSpan.TotalMinutes >= 2) Formatted = ((int) timeSpan.TotalMinutes) + " minutes";
            else
            {
                var seconds = (int) timeSpan.TotalSeconds;
                if (seconds == 0)
                {
                    var milliSeconds = timeSpan.TotalSeconds * 1e3;
                    if (milliSeconds < 1)
                        Formatted = (milliSeconds * 1e3).ToString("g3", CultureInfo.InvariantCulture) + "us";
                    else if (milliSeconds < 10)
                        Formatted = milliSeconds.ToString("f1", CultureInfo.InvariantCulture) + "ms";
                    else
                        Formatted = ((int) milliSeconds) + "ms";
                }
                else if (seconds == 1) Formatted = "~1s";
                else Formatted = seconds + "s";
            }

            return this;
        }
    }
}
