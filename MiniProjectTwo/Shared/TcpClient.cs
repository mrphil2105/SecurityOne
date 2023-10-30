using System.Buffers.Binary;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace MiniProjectTwo.Shared;

public class TcpClient : IDisposable
{
    private readonly Socket _socket;
    private readonly SslStream _sslStream;

    private TcpClient(Socket socket, SslStream sslStream)
    {
        _socket = socket;
        _sslStream = sslStream;
    }

    public static async Task<TcpClient> FromServerSocketAsync(Socket socket, X509Certificate certificate,
        CancellationToken cancellationToken = default)
    {
        var networkStream = new NetworkStream(socket);
        var sslStream = new SslStream(networkStream, false, ValidateCertificate);

        var authOptions = new SslServerAuthenticationOptions
        {
            ServerCertificate = certificate, ClientCertificateRequired = true
        };
        await sslStream.AuthenticateAsServerAsync(authOptions, cancellationToken);

        return new TcpClient(socket, sslStream);
    }

    public static async Task<TcpClient> ConnectToServerAsync(EndPoint endPoint, X509Certificate certificate,
        CancellationToken cancellationToken = default)
    {
        var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        await socket.ConnectAsync(endPoint, cancellationToken);

        var networkStream = new NetworkStream(socket);
        var sslStream = new SslStream(networkStream, false, ValidateCertificate);

        var authOptions = new SslClientAuthenticationOptions
        {
            ClientCertificates = new X509CertificateCollection { certificate }
        };
        await sslStream.AuthenticateAsClientAsync(authOptions, cancellationToken);

        return new TcpClient(socket, sslStream);
    }

    public async Task SendAsync(int value, CancellationToken cancellationToken = default)
    {
        var bytes = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);

        await _sslStream.WriteAsync(bytes, cancellationToken);
    }

    public async IAsyncEnumerable<int> ReceiveAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var bytes = new byte[sizeof(int)];
        var offset = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            while (offset < sizeof(int))
            {
                var memory = bytes.AsMemory(offset, sizeof(int) - offset);
                offset += await _sslStream.ReadAsync(memory, cancellationToken);
            }

            offset = 0;
            var value = BinaryPrimitives.ReadInt32LittleEndian(bytes);

            yield return value;
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    private static bool ValidateCertificate(object sender, X509Certificate? certificate, X509Chain? chain,
        SslPolicyErrors policyErrors)
    {
        // We blindly accept any certificate, as we do not want to fiddle with PKI.
        return true;
    }

    public void Dispose()
    {
        _sslStream.Dispose();
        _socket.Dispose();
    }
}
