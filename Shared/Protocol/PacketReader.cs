using System.Buffers.Binary;

namespace Orvex.Shared.Protocol;

public static class PacketReader
{
    public static async Task<Packet?> ReadAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        byte[] lengthBuffer = new byte[4];

        int firstByte = await stream.ReadAsync(
            lengthBuffer.AsMemory(0, 1),
            cancellationToken);

        if (firstByte == 0)
            return null;

        await ReadExactlyAsync(
            stream,
            lengthBuffer.AsMemory(1, 3),
            cancellationToken);

        int length = BinaryPrimitives.ReadInt32LittleEndian(
            lengthBuffer);

        if (length < 2 || length > PacketSerializer.MaxPacketSize)
            throw new InvalidDataException(
                $"Invalid packet length: {length}");

        byte[] buffer = new byte[length];

        await ReadExactlyAsync(
            stream,
            buffer,
            cancellationToken);

        return PacketSerializer.Deserialize(buffer);
    }

    private static async Task ReadExactlyAsync(
        Stream stream,
        Memory<byte> buffer,
        CancellationToken cancellationToken)
    {
        int offset = 0;

        while (offset < buffer.Length)
        {
            int read = await stream.ReadAsync(
                buffer[offset..],
                cancellationToken);

            if (read == 0)
                throw new EndOfStreamException(
                    "Connection closed during packet reception.");

            offset += read;
        }
    }
}