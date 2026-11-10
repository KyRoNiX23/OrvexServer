using Orvex.Server.Commands;
using Orvex.Server.Configuration;
using Orvex.Server.Logging;

namespace Orvex.Server.Core;
using Orvex.Server.Networking;

public sealed class ServerHost : IAsyncDisposable
{
    public ServerConfig Config { get; private set; } = new();

    public ServerLifecycle Lifecycle { get; } = new();

    private readonly CancellationTokenSource _cancellation = new();
    public NetworkServer Network { get; } = new();

    private CommandManager? _commandManager;

    public async Task StartAsync()
    {
        if (Lifecycle.State != ServerState.Stopped)
            return;

        Lifecycle.SetState(ServerState.Starting);

        try
        {
            ServerLogger.Initialize();

            ServerLogger.Info("Loading configuration...");
            Config = ConfigManager.Load();

            ServerLogger.Info("Initializing Server Core...");

            _commandManager = new CommandManager(this);

            if (Config.Network.Tcp)
            {
                await Network.StartAsync(
                    Config.Server.Port,
                    Config.Server.MaxPlayers);
            }

            Lifecycle.SetState(ServerState.Running);

            ServerLogger.Success("Server started successfully.");
            ServerLogger.Info("Type 'help' to see available commands.");



            await RunConsoleAsync(_cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
        catch (Exception ex)
        {
            ServerLogger.Error($"Server startup/runtime error: {ex}");
        }
        finally
        {
            Lifecycle.SetState(ServerState.Stopping);

            ServerLogger.Info("Stopping server...");

            await Network.StopAsync();

            Lifecycle.SetState(ServerState.Stopped);

            ServerLogger.Success("Server stopped.");

        }
    }

    private async Task RunConsoleAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            string? input = await Console.In.ReadLineAsync(cancellationToken);

            if (input is null)
            {
                Stop();
                break;
            }

            _commandManager?.Execute(input);
        }
    }

    public void Stop()
    {
        if (Lifecycle.State != ServerState.Running)
            return;

        _cancellation.Cancel();
    }

    public async ValueTask DisposeAsync()
    {
        Stop();
        _cancellation.Dispose();

        await ValueTask.CompletedTask;
    }
}