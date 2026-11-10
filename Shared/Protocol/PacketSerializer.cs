using System.Buffers.Binary;

namespace Orvex.Shared.Protocol;

public static class PacketSerializer
{
    public const int HeaderSize = 2;
    public const int MaxPacketSize = 1_048_576;

    public static byte[] Serialize(Packet packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        ArgumentNullException.ThrowIfNull(packet.Payload);

        if (!Enum.IsDefined(packet.Type))
            throw new InvalidDataException("Unknown packet type.");

        int bodyLength = checked(
            HeaderSize + packet.Payload.Length);

        if (bodyLength > MaxPacketSize)
            throw new InvalidDataException("Packet is too large.");

        byte[] buffer = new byte[4 + bodyLength];

        BinaryPrimitives.WriteInt32LittleEndian(
            buffer.AsSpan(0, 4),
            bodyLength);

        BinaryPrimitives.WriteUInt16LittleEndian(
            buffer.AsSpan(4, 2),
            (ushort)packet.Type);

        packet.Payload.CopyTo(buffer, 6);

        return buffer;
    }

    public static Packet Deserialize(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < HeaderSize ||
            buffer.Length > MaxPacketSize)
        {
            throw new InvalidDataException("Invalid packet size.");
        }

        ushort type = BinaryPrimitives.ReadUInt16LittleEndian(
            buffer[..2]);

        if (!Enum.IsDefined(typeof(PacketType), type))
            throw new InvalidDataException("Unknown packet type.");

        return new Packet(
            (PacketType)type,
            buffer[2..].ToArray());
    }
}