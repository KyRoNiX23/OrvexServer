using System.Net;
using System.Net.Sockets;
using Orvex.Server.Logging;

namespace Orvex.Server.Networking;

public sealed class NetworkServer : IAsyncDisposable
{
    private TcpListener? _listener;
    private CancellationTokenSource? _cancellation;
    private Task? _acceptTask;

    public ConnectionManager Connections { get; } = new();

    public bool IsRunning { get; private set; }

    private int _maxPlayers;

    public Task StartAsync(int port, int maxPlayers)
    {
        if (IsRunning)
            return Task.CompletedTask;

        _maxPlayers = maxPlayers;

        _cancellation = new CancellationTokenSource();

        _listener = new TcpListener(IPAddress.Any, port);

        _listener.Start();

        IsRunning = true;

        ServerLogger.Success($"TCP server listening on port {port}");

        _acceptTask = AcceptLoopAsync(_cancellation.Token);

        return Task.CompletedTask;
    }

    private async Task AcceptLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient client = await _listener!
                    .AcceptTcpClientAsync(token);

                if (Connections.Count >= _maxPlayers)
                {
                    ServerLogger.Warning(
                        "Connection rejected: server is full.");

                    client.Close();
                    continue;
                }

                _ = HandleClientAsync(client, token);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
        catch (ObjectDisposedException)
        {
            // Listener stopped.
        }
        catch (SocketException) when (token.IsCancellationRequested)
        {
            // Listener stopped.
        }
        catch (Exception ex)
        {
            ServerLogger.Error($"Network error: {ex.Message}");
        }
    }

    private async Task HandleClientAsync(
        TcpClient client,
        CancellationToken token)
    {
        var connection = new ClientConnection(client);

        if (!Connections.Add(connection))
        {
            await connection.DisposeAsync();
            return;
        }

        NetworkEvents.OnClientConnected(connection);

        ServerLogger.Success(
            $"Client connected: {connection.Address} [{connection.Id}]");

        try
        {
            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[4096];

            while (!token.IsCancellationRequested)
            {
                int received = await stream.ReadAsync(
                    buffer.AsMemory(),
                    token);

                if (received == 0)
                    break;

                ServerLogger.Debug(
                    $"Received {received} bytes from {connection.Address}");
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
        catch (IOException)
        {
            // Client disconnected.
        }
        catch (SocketException)
        {
            // Connection interrupted.
        }
        catch (Exception ex)
        {
            ServerLogger.Error(
                $"Client error ({connection.Address}): {ex.Message}");
        }
        finally
        {
            Connections.Remove(connection.Id, out _);

            NetworkEvents.OnClientDisconnected(connection);

            await connection.DisposeAsync();

            ServerLogger.Warning(
                $"Client disconnected: {connection.Address}");
        }
    }

    public async Task StopAsync()
    {
        if (!IsRunning)
            return;

        IsRunning = false;

        _cancellation?.Cancel();

        _listener?.Stop();

        if (_acceptTask != null)
        {
            try
            {
                await _acceptTask;
            }
            catch (OperationCanceledException)
            {
                // Expected.
            }
        }

        Connections.Clear();

        _cancellation?.Dispose();
        _cancellation = null;

        _listener = null;
        _acceptTask = null;

        ServerLogger.Info("TCP server stopped.");
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}