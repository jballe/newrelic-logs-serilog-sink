using Serilog.Configuration;

namespace Serilog.Sinks.NewRelic
{
    public static class SerilogExtensions
    {
        public static LoggerConfiguration NewRelic(this LoggerSinkConfiguration loggerSinkConfiguration,
            NewRelicSinkOptions options = null
            )
        {
            options = options ?? new NewRelicSinkOptions();
            var sink = new NewRelicLogsSink(options);
            return loggerSinkConfiguration.Sink(
                sink, 
                options.RestrictedToMinimumLevel, 
                options.LevelSwitch
                );
        }
    }
}
