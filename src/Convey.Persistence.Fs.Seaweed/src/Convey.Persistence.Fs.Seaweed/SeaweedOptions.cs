namespace Convey.Persistence.Fs.Seaweed
{
    public class SeaweedOptions
    {
        public bool Enabled { get; set; }
        public string MasterUrl { get; set; }
        public string MasterHttpClientName { get; set; }
        public string FilerUrl { get; set; }
        public string FilerHttpClientName { get; set; }
    }
}