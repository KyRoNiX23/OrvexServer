namespace Orvex.Server.Networking;

public static class NetworkEvents
{
    public static event Action<ClientConnection>? ClientConnected;

    public static event Action<ClientConnection>? ClientDisconnected;

    public static void OnClientConnected(ClientConnection connection)
    {
        ClientConnected?.Invoke(connection);
    }

    public static void OnClientDisconnected(ClientConnection connection)
    {
        ClientDisconnected?.Invoke(connection);
    }
}