using System.Net;
using Mike.Vmware.Connect.Api;

namespace Mike.Vmware.Connect
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string hostSpn = CreateServiceSpn("esxi001.mike.lab", "host");

            var hostClient = new ClientConnection();
            hostClient.ConnectBySspi("esxi001.mike.lab", hostSpn);

            var vSphereClient = new ClientConnection();
            vSphereClient.ConnectBySspi("esxi001.mike.lab", "");
        }


        private static string CreateServiceSpn(string serverName, string serviceSpnName)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(serverName);
            return serviceSpnName + "/" + hostEntry.HostName;
        }
    }
}