using System;
using System.Globalization;
using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.MiddlewareHelpers.Analysable
{
    /// <summary>
    /// Implements IStatistic for long statistic values
    /// </summary>
    public class LongStatistic : Statistic
    {
        private readonly Func<long> _getValue;

        public LongStatistic(Func<long> getValue)
        {
            _getValue = getValue;
        }

        public override IStatistic Refresh()
        {
            Value = _getValue();
            Denominator = 1;

            if (Value < 2e3f)
                Formatted = Value.ToString(CultureInfo.InvariantCulture);
            else if (Value < 2e6f)
                Formatted = (Value / 1e3).ToString("f1", CultureInfo.InvariantCulture) + "K";
            else if (Value < 2e9f)
                Formatted = (Value / 1e6f).ToString("f1", CultureInfo.InvariantCulture) + "M";
            else if (Value < 2e12f)
                Formatted = (Value / 1e9f).ToString("f1", CultureInfo.InvariantCulture) + "G";
            else
                Formatted = (Value / 1e12f).ToString("f1", CultureInfo.InvariantCulture) + "T";

            return this;
        }
    }
}
