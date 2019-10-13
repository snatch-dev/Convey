namespace Convey.Types
{
    public interface IIdentifiable<out T>
    {
        T Id { get; }
    }
}