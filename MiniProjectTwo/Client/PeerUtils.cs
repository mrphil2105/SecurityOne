using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using MiniProjectTwo.Shared;
using Serilog;
using TcpClient = MiniProjectTwo.Shared.TcpClient;

namespace MiniProjectTwo.Client;

public static class PeerUtils
{
    public static async Task<TcpClient> ConnectToPeerAsync(char peerIdentity, X509Certificate certificate)
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
