using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using Tetrifact.Core;
using Tetrifact.Web.Porter_Packages.MadScience_SimpleDI;

namespace Tetrifact.Web
{
    public class LogFactory : ISimpleDIFactory
    {
        private readonly ISettings _settings;
        
        public LogFactory(ISettings settings)
        {
            _settings = settings;        
        }
        
        public object Resolve<T>()
        {
            return this.Resolve(typeof(T));
        }
        
        public object Resolve(Type service)
        {
            Serilog.Core.Logger fileLogger = new LoggerConfiguration()
                //.MinimumLevel.Override("Tetrifact", LogEventLevel.Information)
                .MinimumLevel.Is((LogEventLevel)Enum.Parse(typeof(LogEventLevel), _settings.LogLevel.ToString()))
                .WriteTo
                .File(_settings.LogPath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            ILoggerFactory loggerFactory = new LoggerFactory().AddSerilog(fileLogger);
            Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger(service);

            return logger;
        }
    }    
}

