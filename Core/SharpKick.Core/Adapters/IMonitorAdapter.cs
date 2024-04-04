using SharpKick.Core.Model;

namespace SharpKick.Core.Adapters;

public interface IMonitorAdapter
{
    Task RunAsync(TimeSpan pollInterval, CancellationToken ct = default);
    Task<WriteResult> WriteMessageAsync(string devicePath, PropertyType prop, byte val, CancellationToken ct = default);
}
