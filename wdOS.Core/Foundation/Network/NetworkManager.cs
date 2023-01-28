using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using System;
using System.Text;
using wdOS.Core.Shell;

namespace wdOS.Core.Foundation.Network
{
    internal static class NetworkManager
    {
        internal static TcpClient MainClient;
        internal static TcpClient PingClient;
        internal static DHCPClient LANClient;
        internal static string UserAgent = "curl/1.00.0";
        internal static Encoding Encoding = Encoding.ASCII;
        internal static HTTPResponse SendHTTPRequest(HTTPRequest req)
        {
            try
            {
                HTTPResponse response = new();
                string requesttext = $"{req.requestmethod} {req.pagelocation} HTTP/1.1\n";
                requesttext += "Accept-Charset: ascii\n";
                requesttext += "Accept-Encoding: identity\n";
                requesttext += "Connection: close\n";
                requesttext += $"User-Agent: {UserAgent}\n";
                requesttext += "\n";
                MainClient.Connect(req.serverhost, 80);
                MainClient.Send(Encoding.GetBytes(requesttext));
                var endpoint = MainClient.RemoteEndPoint;
                string data = Encoding.GetString(MainClient.Receive(ref endpoint));
                {
                    string[] toplevel = data.Split('\n')[0].Split(' ');
                    response.httpcode = (ushort)int.Parse(toplevel[1]);
                    response.httpvermajor = 1;
                    response.httpvermajor = 1;
                    response.responsetext = string.Join("\n\n", Utilities.SkipArray(data.Split("\n\n"), 1));
                    response.responsetype = "text/plain";
                }
                return response;
            }
            catch
            {
                HTTPResponse response = new()
                {
                    httpcode = 2000,
                    httpvermajor = 0
                };
                response.httpvermajor = 0;
                response.responsetext = "Cant access specified server";
                response.responsetype = "text/plain";
                return response;
            }
        }
        internal static int PingServer(Address serveraddress, ushort port)
        {
            try
            {
                DateTime start = DateTime.Now;
                PingClient.Connect(serveraddress, port);
                return (DateTime.Now - start).Milliseconds;
            }
            catch { return -1; }
        }
    }
    internal struct HTTPRequest
    {
        internal string requesttext;
        internal string requesttype;
        internal string requestmethod;
        internal string serverdomain;
        internal string pagelocation;
        internal Address serverhost;
    }
    internal struct HTTPResponse
    {
        internal string responsetext;
        internal string responsetype;
        internal ushort httpcode;
        internal ushort httpvermajor;
        internal ushort httpverminor;
    }
}
