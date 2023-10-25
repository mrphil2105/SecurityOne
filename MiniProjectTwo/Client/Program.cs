using System.Net;
using System.Security.Cryptography.X509Certificates;
using CommandLine;
using MiniProjectTwo.Shared;
using Serilog;
using SocketException = System.Net.Sockets.SocketException;

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
        var certificate = CryptoUtils.CreateOrOpenCertificate(clientName);

        Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File($"{clientName}.log").CreateLogger();
        Log.Information("Running client as {Client}.", clientName);

        var serverTask = RunServerAsync(clientEndPoint, certificate);

        var peerIdentities = Constants.ClientNames.Keys.Where(i => i != options.Identity);
        var tasks = peerIdentities.Select(peerIdentity => SendToPeerAsync(peerIdentity, certificate)).ToList();
        tasks.Add(serverTask);

        await Task.WhenAll(tasks);
    }

    private static async Task RunServerAsync(EndPoint endPoint, X509Certificate certificate)
    {
        var tcpServer = new TcpServer(endPoint, certificate);

        tcpServer.ClientConnected +=
            (_, peerEndPoint) => Log.Information("Peer connected from {EndPoint}.", peerEndPoint);
        tcpServer.ValueReceived += (_, args) =>
            Log.Information("Number {Value} received from {EndPoint}.", args.Value, args.EndPoint);

        var cancelCts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, _) =>
        {
            Log.Information("Shutting down server.");
            cancelCts.Cancel();
        };

        Log.Information("Running server on {EndPoint}.", endPoint);

        await tcpServer.AcceptClientsAsync(cancelCts.Token);
    }

    private static async Task SendToPeerAsync(char peerIdentity, X509Certificate certificate)
    {
        var peerName = Constants.ClientNames[peerIdentity];

        Log.Information("Attempting to send {Share} to {Peer}.", null, peerName);

        var tcpClient = await ConnectToPeerAsync(peerIdentity, certificate);
        await tcpClient.SendAsync(1000);
    }

    private static async Task<TcpClient> ConnectToPeerAsync(char peerIdentity, X509Certificate certificate)
    {
        var peerName = Constants.ClientNames[peerIdentity];
        var peerEndPoint = Constants.ClientEndPoints[peerIdentity];

        while (true)
        {
            try
            {
                return await TcpClient.ConnectToServerAsync(peerEndPoint, certificate);
            }
            catch (SocketException)
            {
                Log.Warning("Connection to {Peer} failed. Trying again in 3 seconds.", peerName);
                await Task.Delay(3000);
            }
        }
    }
}
