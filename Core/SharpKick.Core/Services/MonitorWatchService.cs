using Microsoft.Extensions.Logging;
using SharpKick.Core.Adapters;
using SharpKick.Core.Extensions;
using SharpKick.Core.Model;

namespace SharpKick.Core.Services;

public interface IMonitorWatchService
{
    Task RunAsync(TimeSpan pollInterval, CancellationToken ct = default);
}

public class MonitorWatchService : IMonitorWatchService
{
    private readonly IEventService _events;
    private readonly IMonitorAdapter _monitors; // Need a reference to prevent GC
    private readonly ILogger<MonitorWatchService> _log;

    public MonitorWatchService(
        IEventService events, 
        IMonitorAdapter monitors, // Need to request this service for it to be instantiated
        ILogger<MonitorWatchService> log
     )
    {
        _events = events;
        _monitors = monitors;
        _log = log;
    }

    public async Task RunAsync(TimeSpan pollInterval, CancellationToken ct = default)
    {
        _events.Subscribe<string>(EventKey.HidDeviceAdded, HandleDeviceAdded);
        _events.Subscribe<string>(EventKey.HidDeviceRemoved, HandleDeviceRemoved);

        _ = Task.Run(async () => await _monitors.RunAsync(pollInterval, ct), ct);
        _log.LogInformation($"Listening for monitors");
        await ct.WaitForCancelAsync();
    }

    private void HandleDeviceAdded(string devicePath) => _log.LogInformation($"Added: {devicePath}");
    private void HandleDeviceRemoved(string devicePath) => _log.LogInformation($"Removed: {devicePath}");
}
