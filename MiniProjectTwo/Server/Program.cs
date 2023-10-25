using System.Collections.Concurrent;
using MiniProjectTwo.Shared;
using Serilog;

namespace MiniProjectTwo.Server;

public static class Program
{
    private static readonly ConcurrentBag<int> _numbers = new();

    public static async Task Main()
    {
        var certificate = CryptoUtils.CreateOrOpenCertificate("Hospital");
        var tcpServer = new TcpServer(Constants.ServerEndPoint, certificate);

        tcpServer.ClientConnected += (_, endPoint) => Log.Information("Client connected from {EndPoint}.", endPoint);
        tcpServer.ValueReceived += (_, args) =>
        {
            _numbers.Add(args.Value);
            Log.Information("Number {Value} received from {EndPoint}.", args.Value, args.EndPoint);
        };

        var cancelCts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, _) =>
        {
            Log.Information("Shutting down server.");
            cancelCts.Cancel();
        };

        await tcpServer.AcceptClientsAsync(cancelCts.Token);
    }
}
