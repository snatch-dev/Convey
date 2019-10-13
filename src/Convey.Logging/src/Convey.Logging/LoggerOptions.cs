using System.Collections.Generic;
using Convey.Logging.Options;

namespace Convey.Logging
{
    public class LoggerOptions
    {
        public string ApplicationName { get; set; }
        public string ServiceId { get; set; }
        public string Level { get; set; }
        public ConsoleOptions Console { get; set; }
        public FileOptions File { get; set; }
        public ElkOptions Elk { get; set; }
        public SeqOptions Seq { get; set; }
        public IEnumerable<string> ExcludePaths { get; set; }
    }
}