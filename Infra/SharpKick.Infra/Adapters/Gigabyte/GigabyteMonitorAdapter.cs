using SharpKick.Core.Services;

namespace SharpKick.Infra.Adapters.Gigabyte;

public class GigabyteMonitorAdapter : MonitorAdapter
{
    private const int GIGABYTE = 0x0bda;
    private const int M32U = 0x1100;

    public GigabyteMonitorAdapter(IEventService events) : base(GIGABYTE, M32U, events)
    {
    }
}
