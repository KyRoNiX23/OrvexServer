using System.Text.Json;
using System.Text.Json.Serialization;

namespace Orvex.Server.Configuration;

public static class ConfigManager
{
    private const string ConfigFile = "server.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static ServerConfig Load()
    {
        if (!File.Exists(ConfigFile))
        {
            var config = new ServerConfig();
            Save(config);
            return config;
        }

        string json = File.ReadAllText(ConfigFile);

        return JsonSerializer.Deserialize<ServerConfig>(json, Options)
               ?? throw new InvalidDataException("Invalid server configuration.");
    }

    public static void Save(ServerConfig config)
    {
        string json = JsonSerializer.Serialize(config, Options);
        File.WriteAllText(ConfigFile, json);
    }
}