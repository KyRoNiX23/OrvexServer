using Orvex.Server.Core;
using Orvex.Server.Logging;

namespace Orvex.Server;

internal static class Program
{
    private static async Task Main()
    {
        Console.Title = "Orvex Dedicated Server";

        PrintBanner();

        ServerLogger.Initialize();

        ServerLogger.Info("Orvex Dedicated Server");
        ServerLogger.Info($"Version: 0.1.0");
        ServerLogger.Info($".NET Runtime: {Environment.Version}");

        await using var server = new ServerHost();

        await server.StartAsync();
    }

    private static void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;

        Console.WriteLine("""
          ____  _____  __       __  ______  __   __
         / __ \|  __ \ \ \     / / |  ____| \ \ / /
        | |  | | |__) | \ \   / /  | |__     \ V /
        | |  | |  _  /   \ \_/ /   |  __|     > <
        | |__| | | \ \    \   /    | |____   / . \
         \____/|_|  \_\    \_/     |______| /_/ \_\

                 ORVEX DEDICATED SERVER
        """);

        Console.ResetColor();
        Console.WriteLine();
    }
}