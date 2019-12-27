using System;
using System.Net;
using Convey.WebApi.Exceptions;

namespace Conveyor.Services.Deliveries
{
    public class ExceptionToResponseMapper : IExceptionToResponseMapper
    {
        public ExceptionResponse Map(Exception exception)
            => new ExceptionResponse(new {code = "error", message = exception.Message}, HttpStatusCode.BadRequest);
    }
}