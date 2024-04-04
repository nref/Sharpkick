using SharpKick.Core.Model;

namespace SharpKick.Infra.Adapters.Gigabyte;

public class Property
{
    public required PropertyType Type { get; set; }
    public byte Min { get; set; }
    public byte Max { get; set; }
    public ushort Id { get; set; }
    public string Description { get; set; } = string.Empty;
}