namespace SharpKick.Core.Extensions;

public static class CancellationTokenExtensions
{
    public static async Task WaitForCancelAsync(this CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(1000, ct);
        }
    }
}