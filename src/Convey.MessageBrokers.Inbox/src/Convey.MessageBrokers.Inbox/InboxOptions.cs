namespace Convey.MessageBrokers.Inbox
{
    public class InboxOptions
    {
        public bool Enabled { get; set; }
        public string Type { get; set; }
        public int ExpirySeconds { get; set; }
    }
}