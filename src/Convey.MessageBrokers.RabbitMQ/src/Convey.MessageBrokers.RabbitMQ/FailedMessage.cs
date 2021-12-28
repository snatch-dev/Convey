using System.ComponentModel;

namespace Convey.MessageBrokers.RabbitMQ;

public class FailedMessage
{
    public object Message { get; }
    public bool ShouldRetry { get; }
        
    [Description("This will only work if 'deadLetter' is enabled in RabbitMQ options." +
                 "For more information, see https://www.rabbitmq.com/dlx.html")]
    public bool MoveToDeadLetter { get; }

    public FailedMessage(bool shouldRetry = true, bool moveToDeadLetter = true) : this(null, shouldRetry,
        moveToDeadLetter)
    {
    }

    public FailedMessage(object message, bool shouldRetry = true, bool moveToDeadLetter = true)
    {
        Message = message;
        ShouldRetry = shouldRetry;
        MoveToDeadLetter = moveToDeadLetter;
    }
}