using System.Collections.Concurrent;
using MiniProjectTwo.Shared;
using Serilog;

namespace MiniProjectTwo.Server;

public static class Program
{
    private static readonly ConcurrentBag<int> _shares = new();

    public static async Task Main()
    {
        Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File("Hospital.log").CreateLogger();
        Log.Information("Running hospital server on {EndPoint}.", Constants.ServerEndPoint);

        var certificate = CryptoUtils.CreateOrOpenCertificate("Hospital");
        var tcpServer = new TcpServer(Constants.ServerEndPoint, certificate);

        tcpServer.ClientConnected += (_, endPoint) => Log.Information("Client connected from {EndPoint}.", endPoint);
        tcpServer.ValueReceived += OnValueReceived;

        var cancelCts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, _) =>
        {
            Log.Information("Shutting down server.");
            cancelCts.Cancel();
        };

        await tcpServer.AcceptClientsAsync(cancelCts.Token);
    }

    private static void OnValueReceived(object? sender, (System.Net.EndPoint EndPoint, int Value) args)
    {
        Log.Information("Share {Share} received from {EndPoint}.", args.Value, args.EndPoint);

        _shares.Add(args.Value);

        if (_shares.Count != 3)
        {
            return;
        }

        var sum = _shares.Sum();
        var aggregate = sum % Constants.Prime;

        Log.Information("All shares received. Final aggregated value is {Aggregate}.", aggregate);
    }
}
