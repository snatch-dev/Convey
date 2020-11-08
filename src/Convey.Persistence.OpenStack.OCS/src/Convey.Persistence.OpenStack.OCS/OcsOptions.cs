namespace Convey.Persistence.OpenStack.OCS
{
    public class OcsOptions
    {
        public string StorageUrl { get; set; }
        public string AuthRelativeUrl { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string AuthMethod { get; set; }
        public string ProjectId { get; set; }
        public string ProjectRelativeUrl { get; set; }
        public string RootDirectory { get; set; }
        public string InternalHttpClientName { get; set; }
    }
}