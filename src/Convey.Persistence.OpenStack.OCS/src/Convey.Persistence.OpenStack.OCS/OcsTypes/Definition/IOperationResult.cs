namespace Convey.Persistence.OpenStack.OCS.OcsTypes.Definition
{
    public interface IOperationResult
    {
        OperationStatus Status { get; }
    }

    public interface IOperationResult<out T> : IOperationResult
    {
        T Result { get; }
    }
}
