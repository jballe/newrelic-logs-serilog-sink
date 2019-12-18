using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using NewRelic.LogEnrichers.Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.NewRelic
{
    public class NewRelicLogsSink : PeriodicBatchingSink
    {
        public NewRelicSinkOptions Options { get; }
        public ITextFormatter Formatter { get; }
        public HttpClient HttpClient { get; } = new HttpClient();

        public NewRelicLogsSink(NewRelicSinkOptions options) : base(options.BatchSize, options.BatchPeriod)
        {
            options.Validate();
            Options = options;
            Formatter = Options.CustomFormatter ?? new NewRelicFormatter();
        }

        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            var collection = events?.ToList();
            try
            {
                EmitBatchChecked(collection);
            }
            catch (Exception ex)
            {
                HandleException(ex, collection);
            }
        }

        private void EmitBatchChecked(ICollection<LogEvent> events)
        {
            if (events == null || !events.Any())
            {
                return;
            }

            string payload;
            using (var writer = new StringWriter())
            {
                writer.Write("[");
                var added = false;
                foreach (var evt in events)
                {
                    if (added) writer.Write(",");
                    Formatter.Format(evt, writer);
                    added = true;
                }
                writer.Write("]");
                payload = writer.ToString();
            }


            var result = HttpClient
                .SendAsync(CreateRequest(payload), HttpCompletionOption.ResponseContentRead)
                .GetAwaiter().GetResult();

            if (!result.IsSuccessStatusCode)
            {
                var req = result.RequestMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var content = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                SelfLog.WriteLine("Posting logs to NewRelic gave status code: {0} Payload: {1} Exception: {2}", result.StatusCode, req, content);
            }
        }

        private HttpRequestMessage CreateRequest(string payload)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, Options.Endpoint);
            if (!string.IsNullOrEmpty(Options.InsertKey))
            {
                req.Headers.Add("X-Insert-Key", Options.InsertKey);
            }
            else if (!string.IsNullOrEmpty(Options.LicenseKey))
            {
                req.Headers.Add("X-License-Key", Options.LicenseKey);
            }

            req.Content = new StringContent(payload, Encoding.UTF8)
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/json")
                }
            };
            return req;
        }

        /// <summary>
        /// Handles the exceptions.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="events"></param>
        protected virtual void HandleException(Exception ex, IEnumerable<LogEvent> events)
        {
            //if (Options.EmitEventFailure.HasFlag(EmitEventFailureHandling.WriteToSelfLog))
            {
                // ES reports an error, output the error to the selflog
                SelfLog.WriteLine("Caught exception while preforming bulk operation to NewRelic: {0}", ex);
            }
            /*
            if (_state.Options.EmitEventFailure.HasFlag(EmitEventFailureHandling.WriteToFailureSink) &&
                _state.Options.FailureSink != null)
            {
                // Send to a failure sink
                try
                {
                    foreach (var e in events)
                    {
                        _state.Options.FailureSink.Emit(e);
                    }
                }
                catch (Exception exSink)
                {
                    // We do not let this fail too
                    SelfLog.WriteLine("Caught exception while emitting to sink {1}: {0}", exSink,
                        _state.Options.FailureSink);
                }
            }
            if (_state.Options.EmitEventFailure.HasFlag(EmitEventFailureHandling.RaiseCallback) &&
                       _state.Options.FailureCallback != null)
            {
                // Send to a failure callback
                try
                {
                    foreach (var e in events)
                    {
                        _state.Options.FailureCallback(e);
                    }
                }
                catch (Exception exCallback)
                {
                    // We do not let this fail too
                    SelfLog.WriteLine("Caught exception while emitting to callback {1}: {0}", exCallback,
                        _state.Options.FailureCallback);
                }
            }
            if (_state.Options.EmitEventFailure.HasFlag(EmitEventFailureHandling.ThrowException))
                throw ex;
            */
        }
    }
}
