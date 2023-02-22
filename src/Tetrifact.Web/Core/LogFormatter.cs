using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace Tetrifact.Web
{
    public static class ConsoleLoggerExtensions
    {
        public static ILoggingBuilder AddCustomFormatter(
            this ILoggingBuilder builder) =>
            builder.AddConsole(options => options.FormatterName = nameof(CustomLoggingFormatter))
                .AddConsoleFormatter<CustomLoggingFormatter, ConsoleFormatterOptions>();
    }

    public sealed class CustomLoggingFormatter : ConsoleFormatter, IDisposable
    {
        private readonly IDisposable _optionsReloadToken;

        private ConsoleFormatterOptions _formatterOptions;

        public CustomLoggingFormatter(IOptionsMonitor<ConsoleFormatterOptions> options) : base(nameof(CustomLoggingFormatter)) =>
        (_optionsReloadToken, _formatterOptions) = (options.OnChange(ReloadLoggerOptions), options.CurrentValue);
        
        private void ReloadLoggerOptions(ConsoleFormatterOptions options) => _formatterOptions = options;

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
        {
            string? message = logEntry.Formatter?.Invoke(
                    logEntry.State, logEntry.Exception);

            if (message is null)
                return;

            string ex = logEntry.Exception == null ? string.Empty : $" {logEntry.Exception}";

            textWriter.WriteLine($"{logEntry.LogLevel} : {message}{ex}");

        }

        public void Dispose() => _optionsReloadToken?.Dispose();
    }
}
