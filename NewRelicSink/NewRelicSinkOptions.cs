using System;
using System.Collections.Generic;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.NewRelic
{
    public class NewRelicSinkOptions
    {
        public string Endpoint { get; set; } = "https://log-api.eu.newrelic.com/log/v1";
        public int BatchSize { get; set; } = 25;
        public TimeSpan BatchPeriod { get; set; } = TimeSpan.FromMinutes(1);
        public string LicenseKey { get; set; }
        public string InsertKey { get; set; }
        public Dictionary<string, string> CommonAttributes { get; set; }
        public LogEventLevel RestrictedToMinimumLevel { get; set; } = LogEventLevel.Verbose;
        public LoggingLevelSwitch LevelSwitch { get; set; }
        public ITextFormatter CustomFormatter { get; set; }

        public string ServiceName
        {
            get => CommonAttributes.TryGetValue("service", out var value) ? value : null;
            set => CommonAttributes["service"] = value;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Endpoint))
            {
                throw new ArgumentException("Endpoint must be specified, see https://docs.newrelic.com/docs/logs/new-relic-logs/log-api/introduction-log-api", nameof(Endpoint));
            }

            if (string.IsNullOrEmpty(InsertKey) && string.IsNullOrEmpty(LicenseKey))
            {
                throw new ArgumentException($"Either {nameof(InsertKey)} or {nameof(LicenseKey)} must be specified");
            }
        }
    }
}