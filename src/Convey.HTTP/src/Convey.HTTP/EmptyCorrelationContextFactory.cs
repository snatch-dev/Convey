namespace Convey.HTTP
{
    internal class EmptyCorrelationContextFactory : ICorrelationContextFactory
    {
        public string Create() => default;
    }
}