using Convey.Persistence.Fs.Seaweed.Infrastructure;
using System.Threading.Tasks;

namespace Convey.Persistence.Fs.Seaweed.Operations.Abstractions
{
    public interface IFilerOperation<TResult> : IOperation<IFiler>
    {
        Task<TResult> Execute(IFiler filer);
    }
}