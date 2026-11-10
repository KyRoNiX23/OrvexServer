using System.Net.Sockets;
using Orvex.Shared.Protocol;

namespace Orvex.Server.Networking;

public sealed class ClientConnection : IAsyncDisposable
{
    public Guid Id { get; } = Guid.NewGuid();

    public TcpClient Client { get; }

    public string Address { get; }

    public DateTime ConnectedAt { get; } = DateTime.UtcNow;

    public bool IsConnected => Client.Connected;

    public ClientConnection(TcpClient client)
    {
        Client = client;

        Address = client.Client.RemoteEndPoint?.ToString()
                  ?? "Unknown";
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            Client.Close();
            await ValueTask.CompletedTask;
        }
        catch
        {
            // Connection already closed.
        }
    }

    public async Task SendPacketAsync(
    Packet packet,
    CancellationToken cancellationToken = default)
    {
        byte[] data = PacketSerializer.Serialize(packet);

        NetworkStream stream = Client.GetStream();

        await stream.WriteAsync(
            data.AsMemory(),
            cancellationToken);
    }
}