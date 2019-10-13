using System.Collections.Generic;

namespace Convey.Metrics.AppMetrics.Builders
{
    internal sealed class MetricsOptionsBuilder : IMetricsOptionsBuilder
    {
        private readonly MetricsOptions _options = new MetricsOptions();
        
        public IMetricsOptionsBuilder Enable(bool enabled)
        {
            _options.Enabled = enabled;
            return this;
        }

        public IMetricsOptionsBuilder WithInfluxEnabled(bool influxEnabled)
        {
            _options.InfluxEnabled = influxEnabled;
            return this;
        }

        public IMetricsOptionsBuilder WithPrometheusEnabled(bool prometheusEnabled)
        {
            _options.PrometheusEnabled = prometheusEnabled;
            return this;
        }

        public IMetricsOptionsBuilder WithPrometheusFormatter(string prometheusFormatter)
        {
            _options.PrometheusFormatter = prometheusFormatter;
            return this;
        }

        public IMetricsOptionsBuilder WithInfluxUrl(string influxUrl)
        {
            _options.InfluxUrl = influxUrl;
            return this;
        }

        public IMetricsOptionsBuilder WithDatabase(string database)
        {
            _options.Database = database;
            return this;
        }

        public IMetricsOptionsBuilder WithInterval(int interval)
        {
            _options.Interval = interval;
            return this;
        }

        public IMetricsOptionsBuilder WithTags(IDictionary<string, string> tags)
        {
            _options.Tags = tags;
            return this;
        }

        public MetricsOptions Build()
            => _options;
    }
}