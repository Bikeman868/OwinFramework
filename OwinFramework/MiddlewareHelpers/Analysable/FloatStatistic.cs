using System;
using System.Globalization;
using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.MiddlewareHelpers.Analysable
{
    /// <summary>
    /// Implements IStatistic for float statistic values
    /// </summary>
    public class FloatStatistic : Statistic
    {
        private readonly Func<float> _getValue;

        public FloatStatistic(Func<float> getValue)
        {
            _getValue = getValue;
        }

        public override IStatistic Refresh()
        {
            Value = _getValue();
            Denominator = 1;

            if (Value < float.Epsilon)
                Formatted = "0";
            else if (Value < 1e-14f)
                Formatted = "~0";
            else if (Value < 1e-11f)
                Formatted = (Value * 1e12f).ToString("g3", CultureInfo.InvariantCulture) + "p";
            else if (Value < 1e-8f)
                Formatted = (Value * 1e9f).ToString("f1", CultureInfo.InvariantCulture) + "n";
            else if (Value < 1e-5f)
                Formatted = (Value * 1e6f).ToString("f1", CultureInfo.InvariantCulture) + "u";
            else if (Value < 1e-2f)
                Formatted = (Value * 1e3f).ToString("f1", CultureInfo.InvariantCulture) + "m";            
            else if (Value < 2e3f)
                Formatted = Value.ToString("g3", CultureInfo.InvariantCulture);
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
