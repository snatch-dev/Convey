namespace Convey.MessageBrokers.RabbitMQ
{
    public class FailedMessage
    {
        public object Message { get; }
        public bool ShouldRetry { get; }
        
        public FailedMessage(bool shouldRetry = true) : this(null, shouldRetry)
        {
            ShouldRetry = shouldRetry;
        }

        public FailedMessage(object message, bool shouldRetry = true)
        {
            Message = message;
            ShouldRetry = shouldRetry;
        }
    }
}