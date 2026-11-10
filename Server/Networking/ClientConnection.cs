using System.Net.Sockets;

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
}