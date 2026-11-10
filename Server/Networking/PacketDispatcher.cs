using Orvex.Server.Logging;
using Orvex.Shared.Protocol;
using System.Net.Sockets;
using System.Text;

namespace Orvex.Server.Networking;

public sealed class PacketDispatcher
{
    public async Task HandleAsync(
        ClientConnection connection,
        Packet packet,
        CancellationToken cancellationToken)
    {
        switch (packet.Type)
        {
            case PacketType.Handshake:
                await HandleHandshakeAsync(
                    connection,
                    packet,
                    cancellationToken);
                break;

            case PacketType.KeepAliveResponse:
                ServerLogger.Debug(
                    $"KeepAlive response: {connection.Id}");
                break;

            case PacketType.Disconnect:
                ServerLogger.Info(
                    $"Client requested disconnect: {connection.Id}");
                break;

            default:
                ServerLogger.Warning(
                    $"Unexpected packet {packet.Type} from {connection.Id}");
                break;
        }
    }

    private async Task HandleHandshakeAsync(
        ClientConnection connection,
        Packet packet,
        CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(packet.Payload);
        using var reader = new BinaryReader(stream, Encoding.UTF8);

        if (stream.Length < sizeof(int))
            throw new InvalidDataException("Invalid handshake.");

        int version = reader.ReadInt32();

        bool compatible = version == ProtocolVersion.Current;

        byte[] payload;

        using (var output = new MemoryStream())
        using (var writer = new BinaryWriter(output, Encoding.UTF8))
        {
            writer.Write(compatible);
            writer.Write(ProtocolVersion.Current);
            writer.Write(ProtocolVersion.GameVersion);

            payload = output.ToArray();
        }

        await connection.SendPacketAsync(
            new Packet(PacketType.HandshakeResponse, payload),
            cancellationToken);

        if (compatible)
        {
            ServerLogger.Success(
                $"Protocol handshake accepted: {connection.Id}");
        }
        else
        {
            ServerLogger.Warning(
                $"Protocol mismatch: {connection.Id}");
        }
    }
}