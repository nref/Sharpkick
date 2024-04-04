using Lamar;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SharpKick.Core.Adapters;
using SharpKick.Core.Services;
using SharpKick.Infra.Adapters.Gigabyte;

namespace SharpKick.Infra;

public class CompositionRoot
{
    /// <summary>
    /// Inject services into the given collection.
    /// </summary>
    public void ApplyTo(IServiceCollection services) => services.AddLamar(GetRegistry());

    private ServiceRegistry GetRegistry()
    {
        IConfigurationRoot config = GetConfig();
        ILoggerFactory factory = GetLogger(config);

        var interval = config.GetValue("pollInterval", TimeSpan.FromSeconds(1));

        ServiceRegistry reg = new();
        reg.For<ILoggerFactory>().Use(factory);
        reg.For<IEventService>().Use<EventService>().Singleton();
        reg.For<IMonitorAdapter>().Use<GigabyteMonitorAdapter>();
        reg.For<IMonitorWatchService>().Use<MonitorWatchService>();
        reg.For<IAutoKvmService>().Use<AutoKvmService>();

        return reg;
    }

    private static IConfigurationRoot GetConfig()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
         .SetBasePath(AppContext.BaseDirectory) // exe directory
         .AddJsonFile("appsettings.json", false)
         .Build();

        return config;
    }

    private static ILoggerFactory GetLogger(IConfiguration config)
    {
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .Enrich.FromLogContext()
            .CreateLogger();

        return new LoggerFactory().AddSerilog(logger);
    }
}
