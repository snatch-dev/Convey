namespace Convey.Persistence.Fs.Seaweed.Infrastructure.Builders
{
    public interface IRequestBuilder<out TMessage>
    {
        TMessage Build();
    }
}