using System;

namespace Convey.MessageBrokers.RabbitMQ;

public interface IExceptionToFailedMessageMapper
{
    FailedMessage Map(Exception exception, object message);
}