using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Mike.Vmware.Connect.Api
{
    internal class TrustAllCertificatePolicy
    {
        public static bool Validate(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}