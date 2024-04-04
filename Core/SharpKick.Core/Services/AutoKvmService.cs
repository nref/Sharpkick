using Microsoft.Extensions.Logging;
using SharpKick.Core.Adapters;
using SharpKick.Core.Extensions;
using SharpKick.Core.Model;

namespace SharpKick.Core.Services;

public interface IAutoKvmService
{
    Task RunAsync(
        IList<string>? devicePaths,
        TimeSpan pollInterval,
        TimeSpan debounceInterval,
        byte kvmToggle,
        CancellationToken ct = default);
}

public class AutoKvmService : IAutoKvmService
{
    private readonly IEventService _events;
    private readonly IMonitorAdapter _monitors;
    private readonly ILogger<AutoKvmService> _log;
    private CancellationTokenSource _cts = new();

    private readonly Dictionary<string, DateTime> _lastSwitch = new();

    public AutoKvmService(IEventService events, IMonitorAdapter monitors, ILogger<AutoKvmService> log)
    {
        _events = events;
        _monitors = monitors;
        _log = log;
    }

    public async Task RunAsync(
        IList<string>? devicePaths,
        TimeSpan pollInterval,
        TimeSpan debounceInterval,
        byte kvmToggle,
        CancellationToken ct = default)
    {
        _cts = new();

        var paths = new HashSet<string>(devicePaths ?? []);

        _events.Subscribe<string>(EventKey.HidDeviceAdded, devicePath => HandleDeviceAdded(paths, devicePath, debounceInterval, kvmToggle));
        _events.Subscribe<string>(EventKey.HidDeviceRemoved, HandleDeviceRemoved);

        _log.LogInformation($"Ready to autoswitch monitors.");

        if (paths.Any())
        {
            _log.LogInformation($"Listening for:\n  {string.Join("\n  ", paths)}"); 
        }

        _ = Task.Run(async () => await _monitors.RunAsync(pollInterval, ct), ct);
        await ct.WaitForCancelAsync();

        _cts.Cancel();
    }

    private void HandleDeviceRemoved(string devicePath)
    {
        _log.LogInformation($"Removed: {devicePath}");
    }

    private void HandleDeviceAdded(HashSet<string> devicePaths, string devicePath, TimeSpan debounceInterval, byte kvmToggle)
    {
        _log.LogInformation($"Added: {devicePath}");

        if (_monitors is null) { return; }

        if (Ignored(devicePaths, devicePath))
        {
            return;
        }

        if (Debounce(devicePath, debounceInterval))
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            _log.LogInformation($"  Debouncing for {debounceInterval.TotalSeconds}s...");
            await Task.Delay(debounceInterval, _cts.Token);

            _log.LogInformation($"  Switching now");
            await _monitors.WriteMessageAsync(devicePath, PropertyType.KvmSwitch, kvmToggle, _cts.Token);
        });
    }

    private bool Ignored(ICollection<string> devicePaths, string devicePath)
    {
        if (!devicePaths.Any()) { return false; }

        if (!devicePaths.Contains(devicePath))
        {
            _log.LogInformation("  Is ignored. No action");
            return true;
        }

        return false;
    }

    private bool Debounce(string devicePath, TimeSpan debounce)
    {
        var now = DateTime.UtcNow;

        if (!_lastSwitch.ContainsKey(devicePath))
        {
            _lastSwitch[devicePath] = new DateTime();
        }

        if (now - _lastSwitch[devicePath] < debounce)
        {
            _log.LogInformation($"  Debounced. No action");
            return true;
        }

        _lastSwitch[devicePath] = now;
        return false;
    }
}