using System;
using System.Net;
using Mike.Vmware.Connect.Security;
using Vim25Api;

namespace Mike.Vmware.Connect.Api
{
    internal class ClientConnection
    {
        private const string Protocol = "https";
        private const int Port = 443;

        internal VimService VimService { get; set; }
        internal ServiceContent Sic { get; set; }
        internal ManagedObjectReference SvcRef { get; set; }


        public string BuildServerUrl(string protocol, string serverName, int port, string path)
        {
            var uriBuilder = new UriBuilder(protocol, serverName, port, path);
            return uriBuilder.Uri.ToString();
        }


        private bool ConnectVim(string serverEndPoint, int port, string protocol)
        {
            string serviceUrl = BuildServerUrl(protocol, serverEndPoint, port, "/sdk");

            SvcRef = new ManagedObjectReference {type = "ServiceInstance", Value = "ServiceInstance"};

            VimService = new VimService {Url = serviceUrl, CookieContainer = new CookieContainer(), Timeout = 216000};

            Sic = VimService.RetrieveServiceContent(SvcRef);

            return Sic != null;
        }

        internal void Connect(string serverEndPoint, string username, string password)
        {
            InitializeServicePointManager();

            bool serverIsRunning = ConnectVim(serverEndPoint, Port, Protocol);

            if (serverIsRunning == false)
                throw new ApiException("Service Unavailable");

            VimService.Login(Sic.sessionManager, username, password, null);
        }


        /// <summary>
        /// </summary>
        /// <param name="serverEndPoint"></param>
        /// <param name="connectToVCenter"></param>
        /// <param name="serviceSpn"></param>
        internal void ConnectBySspi(string serverEndPoint, string serviceSpn = null)
        {
            InitializeServicePointManager();

            bool serverIsRunning = ConnectVim(serverEndPoint, Port, Protocol);

            if (serverIsRunning == false)
                throw new ApiException("Service Unavailable");

            LoginBySspiPackage(SspiPackageType.Negotiate, serviceSpn);
        }

        internal void Disconnect()
        {
            if (VimService != null)
                VimService.Logout(Sic.sessionManager);
        }

        private UserSession LoginBySspiPackage(SspiPackageType sspiPackage, string serviceSpn)
        {
            var sspiClient = new SspiClient(serviceSpn, sspiPackage);


            bool serverNotReady = true;
            UserSession session = null;

            while (serverNotReady)
            {
                try
                {
                    session = VimService.LoginBySSPI(Sic.sessionManager, Convert.ToBase64String(sspiClient.Token), "en");

                    serverNotReady = false; // Connected!  

                    VimService.CurrentTime(SvcRef);
                }
                catch (Exception e)
                {
                    MethodFault fault = FaultConverter.CreateMethodFault(e);

                    if (fault is SSPIChallenge)
                    {
                        var sspiChallenge = fault as SSPIChallenge;
                        try
                        {
                            sspiClient.Initialize(Convert.FromBase64String(sspiChallenge.base64Token));

                            VimService.CurrentTime(SvcRef);

                            serverNotReady = false; // Connected!  
                        }
                        catch (Exception)
                        {
                            serverNotReady = true;
                        }
                    }
                    else if (fault is InvalidLogin)
                    {
                        throw new InvalidLoginException(e.Message);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return session;
        }

        private void InitializeServicePointManager()
        {
            ServicePointManager.ServerCertificateValidationCallback = TrustAllCertificatePolicy.Validate;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }
    }
}