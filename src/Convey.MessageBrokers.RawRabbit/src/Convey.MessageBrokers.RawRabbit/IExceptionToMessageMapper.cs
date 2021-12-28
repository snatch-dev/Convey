using System;

namespace Convey.MessageBrokers.RawRabbit;

public interface IExceptionToMessageMapper
{
    object Map(Exception exception, object message);
}