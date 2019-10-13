using System.Collections.Generic;

namespace Convey.Metrics.AppMetrics
{
    public interface IMetricsOptionsBuilder
    {
        IMetricsOptionsBuilder Enable(bool enabled);
        IMetricsOptionsBuilder WithInfluxEnabled(bool influxEnabled);
        IMetricsOptionsBuilder WithPrometheusEnabled(bool prometheusEnabled);
        IMetricsOptionsBuilder WithPrometheusFormatter(string prometheusFormatter);
        IMetricsOptionsBuilder WithInfluxUrl(string influxUrl);
        IMetricsOptionsBuilder WithDatabase(string database);
        IMetricsOptionsBuilder WithInterval(int interval);
        IMetricsOptionsBuilder WithTags(IDictionary<string, string> tags);
        MetricsOptions Build();
    }
}