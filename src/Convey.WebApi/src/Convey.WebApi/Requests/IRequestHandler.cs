using System.Threading;
using System.Threading.Tasks;

namespace Convey.WebApi.Requests;

public interface IRequestHandler<in TRequest, TResult> where TRequest : class, IRequest 
{
    Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}