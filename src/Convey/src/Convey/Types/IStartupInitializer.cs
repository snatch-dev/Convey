namespace Convey.Types;

public interface IStartupInitializer : IInitializer
{
    void AddInitializer(IInitializer initializer);
}