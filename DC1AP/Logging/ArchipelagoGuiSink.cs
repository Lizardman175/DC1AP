using Serilog.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Text.Json;

namespace DC1AP.Logging
{
    public class ArchipelagoGuiSink : ILogEventSink
    {
        private LogEventLevel _logLevel;
        private Action<string, LogEventLevel> _outputEvent;
        private Action<APMessageModel, LogEventLevel> _archipelagoEventLogHandler;
        public ArchipelagoGuiSink(Action<string, LogEventLevel> outputEvent, Action<APMessageModel, LogEventLevel> archipelagoEventLogHandler, LogEventLevel level = LogEventLevel.Information)
        {
            _logLevel = level;
            _outputEvent = outputEvent;
            _archipelagoEventLogHandler = archipelagoEventLogHandler;
        }
        public void Emit(LogEvent logEvent)
        {
            try
            {
                var message = logEvent.MessageTemplate.Text;
                if (message.StartsWith('{') || message.StartsWith('['))
                {

                    var logMessage = JsonSerializer.Deserialize<APMessageModel>(message);


                    _archipelagoEventLogHandler?.Invoke(logMessage, logEvent.Level);

                    return;
                }
            }
            catch (Exception ex)
            {//not a json
            }
            if (logEvent.Level >= _logLevel)
            {
                var msg = logEvent.RenderMessage();
                _outputEvent?.Invoke(logEvent.RenderMessage(), logEvent.Level);
            }
        }
    }
    public static class ArchipelagoGuiSinkExtensions
    {
        public static LoggerConfiguration ArchipelagoGuiSink(
                  this LoggerSinkConfiguration loggerConfiguration, Action<string, LogEventLevel> outputEvent, Action<APMessageModel, LogEventLevel> archipelagoEventLogHandler, LogEventLevel level = LogEventLevel.Information)
        {
            return loggerConfiguration.Sink(new ArchipelagoGuiSink(outputEvent, archipelagoEventLogHandler, level));
        }
    }
}
