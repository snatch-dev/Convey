namespace Convey.MessageBrokers.Outbox.Configurators
{
    internal sealed class MessageOutboxConfigurator : IMessageOutboxConfigurator
    {
        public IConveyBuilder Builder { get; }
        public OutboxOptions Options { get; }

        public MessageOutboxConfigurator(IConveyBuilder builder, OutboxOptions options)
        {
            Builder = builder;
            Options = options;
        }
    }
}