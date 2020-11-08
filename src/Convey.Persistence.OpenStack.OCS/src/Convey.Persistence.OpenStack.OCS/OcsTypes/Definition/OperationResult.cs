namespace Convey.Persistence.OpenStack.OCS.OcsTypes.Definition
{
    public class OperationResult : IOperationResult
    {
        public OperationStatus Status { get; }

        public OperationResult(OperationStatus status)
        {
            Status = status;
        }
    }

    public class OperationResult<T> : OperationResult, IOperationResult<T> where T : class
    {
        public T Result { get; }

        public OperationResult(OperationStatus status, T result = null) : base(status)
        {
            Result = result;
        }
    }
}