using Microsoft.Extensions.Primitives;

namespace Convey.Secrets.Vault.Internals
{
    internal interface IVaultPeriodicalWatcher
    {
        void Dispose();
        IChangeToken Watch();
    }
}