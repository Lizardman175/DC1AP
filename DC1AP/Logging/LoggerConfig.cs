using Serilog;
using Serilog.Events;
using System;

namespace DC1AP.Logging
{
    public static class LoggerConfig
    {
        private static ILogger _logger;
        private static Action<string, LogEventLevel> _outputAction;
        private static Action<APMessageModel, LogEventLevel> _archipelagoEventLogHandler;
        private static LogEventLevel _minimumLevel = LogEventLevel.Information;
        public static void Initialize(Action<string, LogEventLevel> mainFormWriter,Action<APMessageModel, LogEventLevel> archipelagoEventLogHandler)
        {
            _outputAction = mainFormWriter;
            _archipelagoEventLogHandler = archipelagoEventLogHandler;
            var loggerConfiguration = new LoggerConfiguration()
                .WriteTo.ArchipelagoGuiSink(_outputAction, archipelagoEventLogHandler);

            _logger = loggerConfiguration.CreateLogger();
            Log.Logger = _logger;
        }
        public static LogEventLevel GetMinimumLevel()
        {
            return _minimumLevel;
        }
        public static void SetLogLevel(LogEventLevel level)
        {
            _minimumLevel = level;
            var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .WriteTo.ArchipelagoGuiSink(_outputAction, _archipelagoEventLogHandler, level);
            _logger = loggerConfiguration.CreateLogger();
            Log.Logger = _logger;
        }
        public static LoggerConfiguration GetLoggerConfiguration(Action<string, LogEventLevel> mainFormWriter, Action<APMessageModel, LogEventLevel> archipelagoEventLogHandler)
        {
            _minimumLevel = LogEventLevel.Information;
            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.ArchipelagoGuiSink(mainFormWriter, archipelagoEventLogHandler);
        }
    }
}
