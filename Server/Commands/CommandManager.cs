using Orvex.Server.Core;
using Orvex.Server.Logging;

namespace Orvex.Server.Commands;

public sealed class CommandManager
{
    private readonly Dictionary<string, Action<string[]>> _commands = new(
        StringComparer.OrdinalIgnoreCase);

    public CommandManager(ServerHost host)
    {
        Register("help", _ => Help());
        Register("stop", _ => host.Stop());
        Register("info", _ => Info(host));
        Register("list", _ => List(host));
        Register("save", _ => Save());
    }

    private void Register(string name, Action<string[]> action)
    {
        _commands[name] = action;
    }

    public void Execute(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        string[] parts = input.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries);

        string name = parts[0];
        string[] args = parts.Skip(1).ToArray();

        if (_commands.TryGetValue(name, out var command))
        {
            try
            {
                command(args);
            }
            catch (Exception ex)
            {
                ServerLogger.Error($"Command error: {ex.Message}");
            }

            return;
        }

        ServerLogger.Warning($"Unknown command: {name}");
    }

    private void Help()
    {
        ServerLogger.Info("Available commands:");

        Console.WriteLine("  help  - Show commands");
        Console.WriteLine("  stop  - Stop server");
        Console.WriteLine("  info  - Server information");
        Console.WriteLine("  list  - Connected players");
        Console.WriteLine("  save  - Save world");
    }

    private void Info(ServerHost host)
    {
        ServerLogger.Info("Server information:");
        Console.WriteLine($"  Name: {host.Config.Server.Name}");
        Console.WriteLine($"  Version: {host.Config.Server.Version}");
        Console.WriteLine($"  State: {host.Lifecycle.State}");
        Console.WriteLine($"  Port: {host.Config.Server.Port}");
        Console.WriteLine($"  Max players: {host.Config.Server.MaxPlayers}");
    }

    private void List(ServerHost host)
    {
        var connections = host.Network.Connections.Connections;

        ServerLogger.Info(
            $"Connected players: {connections.Count}/{host.Config.Server.MaxPlayers}");

        foreach (var connection in connections)
        {
            Console.WriteLine(
                $"  {connection.Id} | {connection.Address}");
        }
    }

    private void Save()
    {
        ServerLogger.Warning("World saving is not implemented yet.");
    }
}