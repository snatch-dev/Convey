using System;
using System.Net;
using Convey.WebApi.Exceptions;

namespace Conveyor.Services.Pricing;

public class ExceptionToResponseMapper : IExceptionToResponseMapper
{
    public ExceptionResponse Map(Exception exception)
        => new(new {code = "error", message = exception.Message}, HttpStatusCode.BadRequest);
}