using System;
using System.IO;
using Disqord.Logging;

namespace Disqord.Bot.Extended
{
    // TODO: Naming
    public class ExtendedSimpleLogger : ILogger
    {
        private readonly object _lock;
        private readonly ExtendedSimpleLoggerConfiguration _configuration;

        public ExtendedSimpleLogger(ExtendedSimpleLoggerConfiguration configuration = default)
        {
            _lock = new object();
            _configuration = configuration ?? new ExtendedSimpleLoggerConfiguration();

            if (!string.IsNullOrWhiteSpace(_configuration.LogDirectory))
                Directory.CreateDirectory(_configuration.LogDirectory);
        }

        public virtual void Log(object sender, MessageLoggedEventArgs e)
        {
            if (_configuration.MessageFilterRegex is { } && _configuration.MessageFilterRegex.IsMatch(e.Message))
                return;

            switch (e.Severity)
            {
                case LogMessageSeverity.Trace when !_configuration.EnableTraceLogSeverity:
                    return;
                case LogMessageSeverity.Debug when !_configuration.EnableDebugLogSeverity:
                    return;
                case LogMessageSeverity.Information when !_configuration.EnableInformationLogSeverity:
                    return;
                case LogMessageSeverity.Warning when !_configuration.EnableWarningLogSeverity:
                    return;
                case LogMessageSeverity.Error when !_configuration.EnableErrorLogSeverity:
                    return;
                case LogMessageSeverity.Critical when !_configuration.EnableCriticalLogSeverity:
                    return;
            }

            var message = e.Message;
            if (e.Exception is { })
            {
                message += $"\n{e.Exception}";
            }

            if (string.IsNullOrWhiteSpace(e.Message)) return;

            lock (_lock)
            {
                if (_configuration.LogNewlinesSeparately)
                {
                    foreach (var line in message.Split("\n", StringSplitOptions.RemoveEmptyEntries))
                    {
                        LogLine(e.Source, e.Severity, line);
                        if (!string.IsNullOrWhiteSpace(_configuration.LogDirectory))
                        {
                            LogToFile(e.Source, e.Severity, line);
                        }
                    }

                    return;
                }

                LogLine(e.Source, e.Severity, message);
                if (!string.IsNullOrWhiteSpace(_configuration.LogDirectory))
                {
                    LogToFile(e.Source, e.Severity, message);
                }
            }
        }

        private void LogLine(string source, LogMessageSeverity severity, string message)
        {
            (ConsoleColor? background, ConsoleColor? foreground) = severity switch
            {
                LogMessageSeverity.Trace => (_configuration.TraceBackgroundColor, _configuration.TraceForegroundColor),
                LogMessageSeverity.Debug => (_configuration.DebugBackgroundColor, _configuration.DebugForegroundColor),
                LogMessageSeverity.Information => (_configuration.InformationBackgroundColor, _configuration.InformationForegroundColor),
                LogMessageSeverity.Warning => (_configuration.WarningBackgroundColor, _configuration.WarningForegroundColor),
                LogMessageSeverity.Error => (_configuration.ErrorBackgroundColor, _configuration.ErrorForegroundColor),
                LogMessageSeverity.Critical => (_configuration.CriticalBackgroundColor, _configuration.CriticalForegroundColor),
                _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
            };

            var level = severity switch
            {
                LogMessageSeverity.Trace => _configuration.TraceLevelText ?? "Trace",
                LogMessageSeverity.Debug => _configuration.DebugLevelText ?? "Debug",
                LogMessageSeverity.Information => _configuration.InformationLevelText ?? "Information",
                LogMessageSeverity.Warning => _configuration.WarningLevelText ?? "Warning",
                LogMessageSeverity.Error => _configuration.ErrorLevelText ?? "Error",
                LogMessageSeverity.Critical => _configuration.CriticalLevelText ?? "Critical",
                _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
            };

            Console.Write('[');
            Console.BackgroundColor = background ?? _configuration.DefaultBackgroundColor;
            Console.ForegroundColor = foreground ?? ConsoleColor.Gray;
            Console.Write(level);
            Console.ResetColor();

            if (!_configuration.LogMessagesInColor)
            {
                Console.WriteLine($"|{source}] {message}");
                return;
            }

            Console.Write($"|{source}] ");
            Console.BackgroundColor = background ?? _configuration.DefaultBackgroundColor;
            Console.ForegroundColor = foreground ?? ConsoleColor.Gray;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private void LogToFile(string source, LogMessageSeverity severity, string message)
        {
            var now = DateTimeOffset.Now;
            var path = Path.Join(_configuration.LogDirectory, $"{now:MMddyyyy}.log");
            using var writer = File.AppendText(path);

            var level = severity switch
            {
                LogMessageSeverity.Trace => _configuration.TraceLevelText ?? "Trace",
                LogMessageSeverity.Debug => _configuration.DebugLevelText ?? "Debug",
                LogMessageSeverity.Information => _configuration.InformationLevelText ?? "Information",
                LogMessageSeverity.Warning => _configuration.WarningLevelText ?? "Warning",
                LogMessageSeverity.Error => _configuration.ErrorLevelText ?? "Error",
                LogMessageSeverity.Critical => _configuration.CriticalLevelText ?? "Critical",
                _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
            };

            writer.WriteLine($"[{now:g}|{level}|{source}] {message}");
        }

        // Hide(?) the event, as it is not used by this logger at all.
        // TODO: Copy DefaultLogger from DQ and implement it in Log, instead of hiding this event.
        event EventHandler<MessageLoggedEventArgs> ILogger.MessageLogged
        {
            add => throw new NotSupportedException("Subscribing to the MessageLogged event is not supported, as it will never be invoked internally.");
            remove => throw new NotSupportedException();
        }
    }
}