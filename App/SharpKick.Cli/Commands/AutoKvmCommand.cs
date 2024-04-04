using SharpKick.Core.Services;
using Typin;
using Typin.Attributes;
using Typin.Console;

namespace SharpKick.Cli.Commands;

[Command("autokvm")]
public class AutoKvmCommand(IAutoKvmService autoKvm) : ICommand
{
    [CommandOption(IsRequired = true, Description = "Which KVM setting to toggle. 0 or 1. 0 means the first device. 1 means the second device.")]
    public byte Toggle { get; set; }

    /// <summary>
    /// Which monitors to listen for. If null, listen for any Gigabyte monitors.
    /// 
    /// <para/>
    /// Use this to ignore USB-C device paths that connect after USB-A.
    /// </summary>
    [CommandOption(IsRequired = false, Description = "Which monitors to listen for")]
    public string[]? DevicePaths { get; set; }

    [CommandOption(IsRequired = false, Description = "How often to poll for monitor changes")]
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(2);

    [CommandOption(IsRequired = false, Description = "How often to wait after a monitor is added before toggling its KVM")]
    public TimeSpan DebounceInterval { get; set; } = TimeSpan.FromSeconds(4);

    private readonly IAutoKvmService _autoKvm = autoKvm;

    public async ValueTask ExecuteAsync(IConsole console) => await _autoKvm.RunAsync(DevicePaths, PollInterval, DebounceInterval, Toggle);
}