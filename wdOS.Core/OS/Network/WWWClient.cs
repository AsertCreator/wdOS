using System;
using System.Net;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.TCP;

namespace wdOS.Core.OS.Network
{
    // todo: implement network
    internal static class WWWClient
    {
        internal static TcpClient PingClient = new(80);
        internal static HTTPResponse SendHTTPRequest(HTTPRequest req)
        {
            return new HTTPResponse()
            {
                responsetext = ""
            };
        }
        internal static int PingServer(Address serveraddress, ushort port)
        {
            DateTime start = DateTime.Now;
            PingClient.Connect(serveraddress, port);
            return (DateTime.Now - start).Milliseconds;
        }
    }
    internal class HTTPRequest
    {
        internal string requesttext;
        internal string requesttype;
        internal string requestmethod;
        internal string serverurl;
        internal Address serverhost;
        internal ushort httpvermajor;
        internal ushort httpverminor;
    }
    internal class HTTPResponse
    {
        internal string responsetext;
        internal string responsetype;
        internal ushort httpcode;
        internal ushort httpvermajor;
        internal ushort httpverminor;
    }
}
