using SharpKick.Core.Model;

namespace SharpKick.Infra.Adapters.Gigabyte;

public class PropertyMap : Dictionary<PropertyType, Property>
{
    private readonly IReadOnlyList<Property> properties_ =
    [
        new() { Type = PropertyType.Brightness, Min = 0, Max = 100, Id = 0x10 },
        new() { Type = PropertyType.Contrast, Min = 0, Max = 100, Id = 0x12 },
        new() { Type = PropertyType.Sharpness, Min = 0, Max = 10, Id = 0x87 },
        new() { Type = PropertyType.Volume, Min = 0, Max = 100, Id = 0x62 },
        new() {
            Type = PropertyType.LowBlueLight,
            Description = "Blue light reduction. 0 means no reduction.",
            Min = 0,
            Max = 10,
            Id = 0xe00b
        },
        new() {
            Type = PropertyType.KvmSwitch,
            Description = "Switch KVM to device 0 or 1",
            Min = 0,
            Max = 1,
            Id = 0xe069
        },
        new() {
            Type = PropertyType.ColorMode,
            Description = "0 is cool, 1 is normal, 2 is warm, 3 is user-defined.",
            Min = 0,
            Max = 3,
            Id = 0xe003
        },
        new() {
            Type = PropertyType.RgbRed,
            Description = "Red value -- only works if colour-mode is set to 3",
            Min = 0,
            Max = 100,
            Id = 0xe004
        },
        new() {
            Type = PropertyType.RgbGreen,
            Description = "Green value -- only works if colour-mode is set to 3",
            Min = 0,
            Max = 100,
            Id = 0xe005
        },
        new() {
            Type = PropertyType.RgbBlue,
            Description = "Blue value -- only works if colour-mode is set to 3",
            Min = 0,
            Max = 100,
            Id = 0xe006
        },
    ];

    public PropertyMap()
    {
        foreach (var prop in properties_)
        {
            this[prop.Type] = prop;
        }
    }
}
