using System;

namespace Convey.Secrets.Vault
{
    public class LeaseData
    {
        public string Type { get; }
        public string Id { get; }
        public int Duration { get; }
        public bool AutoRenewal { get; }
        public DateTime CreatedAt { get; }
        public DateTime ExpiryAt { get; private set; }
        public object Data { get; }

        public LeaseData(string type, string id, int duration, bool autoRenewal, DateTime createdAt, object data)
        {
            Type = type;
            Id = id;
            Duration = duration;
            AutoRenewal = autoRenewal;
            CreatedAt = createdAt;
            ExpiryAt = CreatedAt.AddSeconds(duration);
            Data = data;
        }

        public void Refresh(int duration)
        {
            ExpiryAt = ExpiryAt.AddSeconds(duration);
        }
    }
}