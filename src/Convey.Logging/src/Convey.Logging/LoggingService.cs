namespace Convey.Logging
{
    public interface ILoggingService
    {
        public void SetLoggingLevel(string logEventLevel)
            => Extensions.LoggingLevelSwitch.MinimumLevel = Extensions.GetLogEventLevel(logEventLevel);
    }
    public class LoggingService : ILoggingService {}
}
