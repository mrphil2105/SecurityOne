using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace MiniProjectTwo.Shared;

public class TcpServer
{
    private readonly EndPoint _endPoint;
    private readonly X509Certificate _certificate;

    private readonly Socket _socket;
    private readonly List<TcpClient> _clients;

    public TcpServer(EndPoint endPoint, X509Certificate certificate)
    {
        _endPoint = endPoint;
        _certificate = certificate;

        _socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _clients = new List<TcpClient>();
    }

    public event EventHandler<EndPoint>? ClientConnected;

    public event EventHandler<(EndPoint EndPoint, int Value)>? ValueReceived;

    public async Task AcceptClientsAsync(CancellationToken cancellationToken = default)
    {
        _socket.Bind(_endPoint);
        _socket.Listen(10);

        cancellationToken.Register(() => _clients.ForEach(c => c.Dispose()));

        while (!cancellationToken.IsCancellationRequested)
        {
            var clientSocket = await _socket.AcceptAsync(cancellationToken);
            var tcpClient = await TcpClient.FromServerSocketAsync(clientSocket, _certificate, cancellationToken);
            _clients.Add(tcpClient);

            _ = HandleClientAsync(clientSocket, tcpClient, cancellationToken);

            ClientConnected?.Invoke(this, clientSocket.RemoteEndPoint!);
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    private async Task HandleClientAsync(Socket clientSocket, TcpClient tcpClient, CancellationToken cancellationToken)
    {
        await foreach (var value in tcpClient.ReceiveAsync(cancellationToken))
        {
            ValueReceived?.Invoke(this, (clientSocket.RemoteEndPoint!, value));
        }
    }
}
