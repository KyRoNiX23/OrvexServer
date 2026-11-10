namespace Orvex.Shared.Protocol;

public sealed class Packet
{
    public PacketType Type { get; set; }

    public byte[] Payload { get; set; } = [];

    public Packet()
    {
    }

    public Packet(PacketType type, byte[] payload)
    {
        Type = type;
        Payload = payload;
    }
}