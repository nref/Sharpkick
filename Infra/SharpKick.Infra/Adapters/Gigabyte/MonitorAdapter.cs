using HidSharp;
using SharpKick.Core.Adapters;
using SharpKick.Core.Model;
using SharpKick.Core.Services;

namespace SharpKick.Infra.Adapters.Gigabyte;

public class MonitorAdapter : IMonitorAdapter
{
    private readonly PropertySerializer _sez = new();
    private readonly PropertyMap _map = new();
    private readonly int _hid;
    private readonly int _pid;
    private readonly IEventService _events;

    public MonitorAdapter(int hid, int pid, IEventService events)
    {
        _hid = hid;
        _pid = pid;
        _events = events;
    }

    public async Task RunAsync(TimeSpan pollInterval, CancellationToken ct = default)
    {
        Dictionary<string, HidDevice> lastDevices = new();

        while (!ct.IsCancellationRequested)
        {
            var devices = GetDevices();

            foreach (var dev in devices)
            {
                if (!lastDevices.ContainsKey(dev.DevicePath))
                {
                    _events.Publish(EventKey.HidDeviceAdded, dev.DevicePath);
                }
            }

            foreach (var dev in lastDevices)
            {
                if (!devices.Any(d => d.DevicePath == dev.Value.DevicePath))
                {
                    _events.Publish(EventKey.HidDeviceRemoved, dev.Value.DevicePath);
                }
            }

            lastDevices = devices.ToDictionary(d => d.DevicePath, d => d);

            await Task.Delay(pollInterval, ct);
        }
    }

    public async Task<WriteResult> WriteMessageAsync(string devicePath, PropertyType prop, byte val, CancellationToken ct = default) => 
        await WriteMessageAsync(devicePath, _sez.Serialize(_map[prop], val), ct);

    public async Task<WriteResult> WriteMessageAsync(string devicePath, byte[] message, CancellationToken ct = default)
    {
        List<HidDevice> devices = GetDevices(devicePath);

        if (!devices.Any())
        {
            return WriteResult.NoDevice;
        }

        foreach (HidDevice device in devices)
        {
            if (!device.TryOpen(out var stream))
            {
                return WriteResult.Error;
            }

            //await stream.WriteAsync(message, ct); // Crashes
            stream.WriteAsync(message, ct).GetAwaiter().GetResult(); // Does not crash
            await Task.CompletedTask;
        }

        return WriteResult.Success;
    }

    private List<HidDevice> GetDevices() => DeviceList.Local
        .GetHidDevices()
        .Where(d => d.VendorID == _hid && d.ProductID == _pid)
        .OrderBy(d => d.DevicePath)
        .ToList();

    private List<HidDevice> GetDevices(string devicePath) => GetDevices()
        .Where(d => d.DevicePath == devicePath)
        .ToList();
}
