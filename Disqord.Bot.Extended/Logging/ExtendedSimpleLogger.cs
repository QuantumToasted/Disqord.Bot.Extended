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

        public virtual void Log(object sender, LogEventArgs e)
        {
            if (_configuration.MessageFilterRegex is { } && _configuration.MessageFilterRegex.IsMatch(e.Message))
                return;

            switch (e.Severity)
            {
                case LogSeverity.Trace when !_configuration.EnableTraceLogSeverity:
                    return;
                case LogSeverity.Debug when !_configuration.EnableDebugLogSeverity:
                    return;
                case LogSeverity.Information when !_configuration.EnableInformationLogSeverity:
                    return;
                case LogSeverity.Warning when !_configuration.EnableWarningLogSeverity:
                    return;
                case LogSeverity.Error when !_configuration.EnableErrorLogSeverity:
                    return;
                case LogSeverity.Critical when !_configuration.EnableCriticalLogSeverity:
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

        private void LogLine(string source, LogSeverity severity, string message)
        {
            (ConsoleColor? background, ConsoleColor? foreground) = severity switch
            {
                LogSeverity.Trace => (_configuration.TraceBackgroundColor, _configuration.TraceForegroundColor),
                LogSeverity.Debug => (_configuration.DebugBackgroundColor, _configuration.DebugForegroundColor),
                LogSeverity.Information => (_configuration.InformationBackgroundColor, _configuration.InformationForegroundColor),
                LogSeverity.Warning => (_configuration.WarningBackgroundColor, _configuration.WarningForegroundColor),
                LogSeverity.Error => (_configuration.ErrorBackgroundColor, _configuration.ErrorForegroundColor),
                LogSeverity.Critical => (_configuration.CriticalBackgroundColor, _configuration.CriticalForegroundColor),
                _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
            };

            var level = severity switch
            {
                LogSeverity.Trace => _configuration.TraceLevelText ?? "Trace",
                LogSeverity.Debug => _configuration.DebugLevelText ?? "Debug",
                LogSeverity.Information => _configuration.InformationLevelText ?? "Information",
                LogSeverity.Warning => _configuration.WarningLevelText ?? "Warning",
                LogSeverity.Error => _configuration.ErrorLevelText ?? "Error",
                LogSeverity.Critical => _configuration.CriticalLevelText ?? "Critical",
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

        private void LogToFile(string source, LogSeverity severity, string message)
        {
            var now = DateTimeOffset.Now;
            var path = Path.Join(_configuration.LogDirectory, $"{now:MMddyyyy}.log");
            using var writer = File.AppendText(path);

            var level = severity switch
            {
                LogSeverity.Trace => _configuration.TraceLevelText ?? "Trace",
                LogSeverity.Debug => _configuration.DebugLevelText ?? "Debug",
                LogSeverity.Information => _configuration.InformationLevelText ?? "Information",
                LogSeverity.Warning => _configuration.WarningLevelText ?? "Warning",
                LogSeverity.Error => _configuration.ErrorLevelText ?? "Error",
                LogSeverity.Critical => _configuration.CriticalLevelText ?? "Critical",
                _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
            };

            writer.WriteLine($"[{now:g}|{level}|{source}] {message}");
        }

        // Hide(?) the event, as it is not used by this logger at all.
        event EventHandler<LogEventArgs> ILogger.Logged
        {
            add => throw new NotSupportedException("To use ExtendedSimpleLogger, set your bot's Logger instead of using the event.");
            remove => throw new NotSupportedException();
        }

        void IDisposable.Dispose()
        { }
    }
}