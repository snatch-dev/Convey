using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Convey.Secrets.Vault.Internals
{
    internal sealed class LeaseService : ILeaseService
    {
        private static readonly ConcurrentDictionary<string, LeaseData> Secrets =
            new ConcurrentDictionary<string, LeaseData>();

        public IReadOnlyDictionary<string, LeaseData> All => Secrets;

        public LeaseData Get(string key) => Secrets.TryGetValue(key, out var data) ? data : null;

        public void Set(string key, LeaseData data)
        {
            Secrets.TryRemove(key, out _);
            Secrets.TryAdd(key, data);
        }
    }
}