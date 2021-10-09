using Convey.Persistence.Fs.Seaweed.Infrastructure;
using System.Threading.Tasks;

namespace Convey.Persistence.Fs.Seaweed.Operations.Abstractions
{
    public interface IOperation<in TOperator> where TOperator : IOperator
    {
        
    }
}