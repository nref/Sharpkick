namespace SharpKick.Infra.Adapters.Gigabyte;

public class PropertySerializer()
{
    public byte[] Serialize(Property prop, byte val)
    {
        if (val < prop.Min || val > prop.Max)
        {
            throw new ArgumentException($"Value {val} for property {prop} is not within range: {prop.Min}-{prop.Max}");
        }

        var buf = new byte[193];
        buf[0] = 0; // Report ID
        buf[1] = 0x40;
        buf[2] = 0xC6;

        buf[7] = 0x20;
        buf[8] = 0x0;
        buf[9] = 0x6E;
        buf[10] = 0x0;
        buf[11] = 0x80;

        List<byte> msg = [];

        // Append property ID.
        {
            ushort propId = prop.Id;

            // Handle 2-byte ID
            if (propId > 0xff)
            {
                msg.Add((byte)(propId >> 8));
                propId &= 0xff;
            }

            msg.Add((byte)propId);
        }

        msg.Add(0);
        msg.Add(val);

        byte[] preamble = [0x51, (byte)(0x81 + msg.Count), 0x03]; // 0x01 is read, 0x03 is write
        preamble.Concat(msg).ToArray().CopyTo(buf, 1 + 0x40);

        return buf;
    }
}
