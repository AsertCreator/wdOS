using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using System;
using System.Linq;
using System.Text;
using wdOS.Core.Shell;

namespace wdOS.Core.Foundation.Network
{
    public static class NetworkManager
    {
        public static TcpClient MainClient;
        public static TcpClient PingClient;
        public static DHCPClient LANClient;
        public static string UserAgent = "curl/1.00.0";
        public static Encoding Encoding = Encoding.ASCII;
        public static HTTPResponse SendHTTPRequest(HTTPRequest req)
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
                    response.responsetext = string.Join("\n\n", data.Split("\n\n").Skip(1));
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
        public static int PingServer(Address serveraddress, ushort port)
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
    public struct HTTPRequest
    {
        public string requesttext;
        public string requesttype;
        public string requestmethod;
        public string serverdomain;
        public string pagelocation;
        public Address serverhost;
    }
    public struct HTTPResponse
    {
        public string responsetext;
        public string responsetype;
        public ushort httpcode;
        public ushort httpvermajor;
        public ushort httpverminor;
    }
}
