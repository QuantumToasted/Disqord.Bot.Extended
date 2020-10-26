using System;
using System.Text.RegularExpressions;
using Disqord.Logging;

namespace Disqord.Bot.Extended
{
    // TODO: Naming
    public sealed class ExtendedSimpleLoggerConfiguration
    {
        /// <summary>
        /// Whether or not <see cref="LogSeverity.Trace"/> messages will be logged.
        /// <para>Defaults to <see langword="true"/>.</para>
        /// </summary>
        public bool EnableTraceLogSeverity { get; set; } = true;
        /// <summary>
        /// The color to set the console foreground (text) color when logging <see cref="LogSeverity.Trace"/> messages.
        /// <para>Defaults to <see cref="ConsoleColor.Cyan"/>.</para>
        /// </summary>
        public ConsoleColor TraceForegroundColor { get; set; } = ConsoleColor.Cyan;
        /// <summary>
        /// The color to set the console background color when logging <see cref="LogSeverity.Trace"/> messages.
        /// <para>Defaults to <see cref="DefaultBackgroundColor"/>.</para>
        /// </summary>
        public ConsoleColor TraceBackgroundColor { get; set; } = default;
        /// <summary>
        /// The text to use when logging <see cref="LogSeverity.Trace"/> messages.
        /// <para>Defaults to <c>Trace</c>.</para>
        /// </summary>
        public string TraceLevelText { get; set; } = "Trace";

        /// <summary>
        /// Whether or not <see cref="LogSeverity.Debug"/> messages will be logged.
        /// <para>Defaults to <see langword="true"/>.</para>
        /// </summary>
        public bool EnableDebugLogSeverity { get; set; } = true;
        /// <summary>
        /// The color to set the console foreground (text) color when logging <see cref="LogSeverity.Debug"/> messages.
        /// <para>Defaults to <see cref="ConsoleColor.DarkGray"/>.</para>
        /// </summary>
        public ConsoleColor DebugForegroundColor { get; set; } = ConsoleColor.DarkGray;
        /// <summary>
        /// The color to set the console background color when logging <see cref="LogSeverity.Debug"/> messages.
        /// <para>Defaults to <see cref="DefaultBackgroundColor"/>.</para>
        /// </summary>
        public ConsoleColor? DebugBackgroundColor { get; set; } = default;
        /// <summary>
        /// The text to use when logging <see cref="LogSeverity.Debug"/> messages.
        /// <para>Defaults to <c>Debug</c>.</para>
        /// </summary>
        public string DebugLevelText { get; set; } = "Debug";

        /// <summary>
        /// Whether or not <see cref="LogSeverity.Information"/> messages will be logged.
        /// <para>Defaults to <see langword="true"/>.</para>
        /// </summary>
        public bool EnableInformationLogSeverity { get; set; } = true;
        /// <summary>
        /// The color to set the console foreground (text) color when logging <see cref="LogSeverity.Information"/> messages.
        /// <para>Defaults to <see cref="ConsoleColor.Green"/>.</para>
        /// </summary>
        public ConsoleColor InformationForegroundColor { get; set; } = ConsoleColor.Green;
        /// <summary>
        /// The color to set the console background color when logging <see cref="LogSeverity.Information"/> messages.
        /// <para>Defaults to <see cref="DefaultBackgroundColor"/>.</para>
        /// </summary>
        public ConsoleColor? InformationBackgroundColor { get; set; } = default;
        /// <summary>
        /// The text to use when logging <see cref="LogSeverity.Information"/> messages.
        /// <para>Defaults to <c>Information</c>.</para>
        /// </summary>
        public string InformationLevelText { get; set; } = "Information";

        /// <summary>
        /// Whether or not <see cref="LogSeverity.Warning"/> messages will be logged.
        /// <para>Defaults to <see langword="true"/>.</para>
        /// </summary>
        public bool EnableWarningLogSeverity { get; set; } = true;
        /// <summary>
        /// The color to set the console foreground (text) color when logging <see cref="LogSeverity.Warning"/> messages.
        /// <para>Defaults to <see cref="ConsoleColor.Magenta"/>.</para>
        /// </summary>
        public ConsoleColor WarningForegroundColor { get; set; } = ConsoleColor.Magenta;
        /// <summary>
        /// The color to set the console background color when logging <see cref="LogSeverity.Warning"/> messages.
        /// <para>Defaults to <see cref="DefaultBackgroundColor"/>.</para>
        /// </summary>
        public ConsoleColor? WarningBackgroundColor { get; set; } = default;
        /// <summary>
        /// The text to use when logging <see cref="LogSeverity.Warning"/> messages.
        /// <para>Defaults to <c>Warning</c>.</para>
        /// </summary>
        public string WarningLevelText { get; set; } = "Warning";

        /// <summary>
        /// Whether or not <see cref="LogSeverity.Error"/> messages will be logged.
        /// <para>Defaults to <see langword="true"/>.</para>
        /// </summary>
        public bool EnableErrorLogSeverity { get; set; } = true;
        /// <summary>
        /// The color to set the console foreground (text) color when logging <see cref="LogSeverity.Error"/> messages.
        /// <para>Defaults to <see cref="ConsoleColor.Yellow"/>.</para>
        /// </summary>
        public ConsoleColor ErrorForegroundColor { get; set; } = ConsoleColor.Yellow;
        /// <summary>
        /// The color to set the console background color when logging <see cref="LogSeverity.Error"/> messages.
        /// <para>Defaults to <see cref="DefaultBackgroundColor"/>.</para>
        /// </summary>
        public ConsoleColor? ErrorBackgroundColor { get; set; } = default;
        /// <summary>
        /// The text to use when logging <see cref="LogSeverity.Error"/> messages.
        /// <para>Defaults to <c>Error</c>.</para>
        /// </summary>
        public string ErrorLevelText { get; set; } = "Error";

        /// <summary>
        /// Whether or not <see cref="LogSeverity.Critical"/> messages will be logged.
        /// <para>Defaults to <see langword="true"/>.</para>
        /// </summary>
        public bool EnableCriticalLogSeverity { get; set; } = true;
        /// <summary>
        /// The color to set the console foreground (text) color when logging <see cref="LogSeverity.Critical"/> messages.
        /// <para>Defaults to <see cref="ConsoleColor.Red"/>.</para>
        /// </summary>
        public ConsoleColor CriticalForegroundColor { get; set; } = ConsoleColor.Red;
        /// <summary>
        /// The color to set the console background color when logging <see cref="LogSeverity.Critical"/> messages.
        /// <para>Defaults to <see cref="DefaultBackgroundColor"/>.</para>
        /// </summary>
        public ConsoleColor? CriticalBackgroundColor { get; set; } = default;
        /// <summary>
        /// The text to use when logging <see cref="LogSeverity.Critical"/> messages.
        /// <para>Defaults to <c>Critical</c>.</para>
        /// </summary>
        public string CriticalLevelText { get; set; } = "Critical";

        /// <summary>
        /// The directory to store log files in. Set to <see langword="null"/> to disable file logging.
        /// <para>Defaults to <c>./Logs/</c>.</para>
        /// </summary>
        public string LogDirectory { get; set; } = "./Logs/";

        /// <summary>
        /// The <see cref="Regex"/> to filter messages with. Messages matching this <see cref="Regex"/> will not be logged.
        /// <para>Defaults to <see langword="null"/> (no filter).</para>
        /// </summary>
        public Regex MessageFilterRegex { get; set; } = null;

        /// <summary>
        /// Whether to log messages in the same color as their respective <see cref="LogSeverity"/>'s color.
        /// <para>Defaults to <see langword="false"/>.</para>
        /// </summary>
        public bool LogMessagesInColor { get; set; } = false;

        /// <summary>
        /// Whether to split messages by newlines (<c>\n</c>), specifying the log severity and source before each line.
        /// <para>Purely a cosmetic option. Defaults to <see langword="true"/>.</para>
        /// </summary>
        public bool LogNewlinesSeparately { get; set; } = true;

        /// <summary>
        /// The color to set the console background color when a <see cref="LogSeverity"/>'s background color is otherwise not specified.
        /// <para>Defaults to <see cref="ConsoleColor.Black"/>.</para>
        /// </summary>
        public ConsoleColor DefaultBackgroundColor { get; set; } = ConsoleColor.Black;
    }
}