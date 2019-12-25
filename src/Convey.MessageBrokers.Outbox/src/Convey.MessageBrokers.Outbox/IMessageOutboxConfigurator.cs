namespace Convey.MessageBrokers.Outbox
{
    public interface IMessageOutboxConfigurator
    {
        IConveyBuilder Builder { get; }
        OutboxOptions Options { get; }
    }
}