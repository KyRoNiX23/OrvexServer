namespace Orvex.Server.Configuration;

public sealed class ServerConfig
{
    public ServerSettings Server { get; set; } = new();
    public NetworkSettings Network { get; set; } = new();
    public WorldSettings World { get; set; } = new();
}

public sealed class ServerSettings
{
    public string Name { get; set; } = "Orvex Server";
    public string Motd { get; set; } = "Welcome to Orvex!";
    public string Version { get; set; } = "0.1.0";
    public int Port { get; set; } = 25565;
    public int MaxPlayers { get; set; } = 50;
}

public sealed class NetworkSettings
{
    public bool Tcp { get; set; } = true;
    public bool Udp { get; set; } = true;
    public int Timeout { get; set; } = 30;
}

public sealed class WorldSettings
{
    public string Name { get; set; } = "World";
    public int ViewDistance { get; set; } = 8;
}