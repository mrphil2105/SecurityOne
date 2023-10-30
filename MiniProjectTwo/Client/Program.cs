using CommandLine;
using MiniProjectTwo.Shared;
using Serilog;

namespace MiniProjectTwo.Client;

public static class Program
{
    public static async Task Main(string[] args)
    {
        await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(RunProgramAsync);
    }

    private static async Task RunProgramAsync(Options options)
    {
        var clientName = Constants.ClientNames[options.Identity];
        var clientEndPoint = Constants.ClientEndPoints[options.Identity];

        const string outputTemplate =
            "[{ClientName:u} {Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration().Enrich.WithProperty("ClientName", clientName)
            .WriteTo.Console(outputTemplate: outputTemplate)
            .WriteTo.File($"{clientName}.log")
            .CreateLogger();

        var peer = new Peer(clientName, clientEndPoint);
        await peer.RunAsync();
    }
}
