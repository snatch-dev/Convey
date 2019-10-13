namespace Convey.LoadBalancing.Fabio.Builders
{
    public class FabioOptionsBuilder : IFabioOptionsBuilder
    {
        private FabioOptions _options = new FabioOptions();

        public IFabioOptionsBuilder Enable(bool enabled)
        {
            _options.Enabled = enabled;
            return this;
        }

        public IFabioOptionsBuilder WithUrl(string url)
        {
            _options.Url = url;
            return this;
        }

        public IFabioOptionsBuilder WithService(string service)
        {
            _options.Service = service;
            return this;
        }

        public FabioOptions Build() => _options;
    }
}