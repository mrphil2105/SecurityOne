using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using MiniProjectTwo.Shared;
using Serilog;

namespace MiniProjectTwo.Client;

public class Peer
{
    private readonly string _name;
    private readonly EndPoint _endPoint;
    private readonly X509Certificate _certificate;

    private readonly IReadOnlyList<int> _ownShares;
    private readonly ConcurrentBag<int> _receivedShares;

    public Peer(string name, EndPoint endPoint)
    {
        _name = name;
        _endPoint = endPoint;
        _certificate = CryptoUtils.CreateOrOpenCertificate(name);

        _ownShares = Enumerable.Range(0, 3).Select(_ => RandomNumberGenerator.GetInt32(Constants.Prime)).ToList();
        _receivedShares = new ConcurrentBag<int>();
    }

    public async Task RunAsync()
    {
        Log.Information("Running client as {Client}.", _name);

        var serverTask = RunServerAsync();

        var peerIdentities = Constants.ClientNames.Where(kvp => kvp.Value != _name).Select(kvp => kvp.Key);
        var tasks = peerIdentities.Select((peerIdentity, index) => SendToPeerAsync(_ownShares[index + 1], peerIdentity))
            .ToList();
        tasks.Add(serverTask);

        await Task.WhenAll(tasks);
    }

    private async Task RunServerAsync()
    {
        var tcpServer = new TcpServer(_endPoint, _certificate);

        tcpServer.ClientConnected +=
            (_, peerEndPoint) => Log.Information("Peer connected from {EndPoint}.", peerEndPoint);
        tcpServer.ValueReceived += OnValueReceived;

        var cancelCts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, _) =>
        {
            Log.Information("Shutting down server.");
            cancelCts.Cancel();
        };

        Log.Information("Running server on {EndPoint}.", _endPoint);

        await tcpServer.AcceptClientsAsync(cancelCts.Token);
    }

    private async void OnValueReceived(object? sender, (EndPoint EndPoint, int Value) args)
    {
        Log.Information("Share {Share} received from {EndPoint}.", args.Value, args.EndPoint);

        _receivedShares.Add(args.Value);

        if (_receivedShares.Count != _ownShares.Count - 1)
        {
            return;
        }

        var ownShare = _ownShares[0];
        var sum = ownShare + _receivedShares.Sum();
        await SendToHospitalAsync(sum % Constants.Prime);
    }

    private async Task SendToPeerAsync(int share, char peerIdentity)
    {
        var peerName = Constants.ClientNames[peerIdentity];

        Log.Information("Attempting to send {Share} to {Peer}.", share, peerName);

        var tcpClient = await PeerUtils.ConnectToPeerAsync(peerIdentity, _certificate);
        await tcpClient.SendAsync(share);
    }

    private async Task SendToHospitalAsync(int share)
    {
        Log.Information("Attempting to send {Share} to hospital.", share);

        var tcpClient = await TcpClient.ConnectToServerAsync(Constants.ServerEndPoint, _certificate);
        await tcpClient.SendAsync(share);
    }
}
