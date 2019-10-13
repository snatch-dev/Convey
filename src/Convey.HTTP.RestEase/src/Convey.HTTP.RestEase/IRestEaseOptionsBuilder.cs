using System;

namespace Convey.HTTP.RestEase
{
    public interface IRestEaseOptionsBuilder
    {
        IRestEaseOptionsBuilder WithLoadBalancer(string loadBalancer);
        IRestEaseOptionsBuilder WithService(Func<IRestEaseServiceBuilder, IRestEaseServiceBuilder> buildService);
        RestEaseOptions Build();
    }
}