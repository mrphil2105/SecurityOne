using System.Net;

namespace MiniProjectTwo.Shared;

public static class Constants
{
    public static IPAddress LocalAddress = IPAddress.Parse("127.0.0.1");
    public const int BasePort = 8888;

    public static EndPoint ServerEndPoint = new IPEndPoint(LocalAddress, BasePort);

    public static IReadOnlyDictionary<char, EndPoint> ClientEndPoints = new Dictionary<char, EndPoint>
    {
        { 'a', new IPEndPoint(LocalAddress, BasePort + 1) },
        { 'b', new IPEndPoint(LocalAddress, BasePort + 2) },
        { 'c', new IPEndPoint(LocalAddress, BasePort + 3) }
    };

    public static IReadOnlyDictionary<char, string> ClientNames = new Dictionary<char, string>
    {
        { 'a', "Alice" }, { 'b', "Bob" }, { 'c', "Charlie" }
    };
}
