using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;
using Serilog;

namespace Convey.Logging.Options
{
    public class ApplicationInsightsOptions
    {
        public bool Enabled { get; set; }
        public string Converter { get; set; } = "traces";

        public ITelemetryConverter GetTelemetryConverter()
            => Converter.ToLower() == "events" ? TelemetryConverter.Events : TelemetryConverter.Traces;
    }
}