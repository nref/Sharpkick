using SharpKick.Core.Services;
using Typin;
using Typin.Attributes;
using Typin.Console;

namespace SharpKick.Cli.Commands;

[Command("watch")]
public class WatchCommand(IMonitorWatchService monitors) : ICommand
{
    private readonly IMonitorWatchService _monitors = monitors;

    [CommandOption(IsRequired = false, Description = "How often to poll for monitor changes")]
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(2);

    public async ValueTask ExecuteAsync(IConsole console) => await _monitors.RunAsync(PollInterval);
}
