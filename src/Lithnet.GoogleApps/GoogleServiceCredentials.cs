using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;

namespace Lithnet.GoogleApps
{
    public class GoogleServiceCredentials
    {
        public GoogleServiceCredentials(string serviceAccountEmailAddress, string impersonationUserEmailAddress, X509Certificate2 certificate)
        {
            this.ServiceAccountEmailAddress = serviceAccountEmailAddress;
            this.ImpersonationUserEmailAddress = impersonationUserEmailAddress;
            this.Certificate = certificate;
        }

        public string ServiceAccountEmailAddress { get; set; }

        public string ImpersonationUserEmailAddress { get; set; }

        public X509Certificate2 Certificate { get; set; }

        public ServiceAccountCredential.Initializer GetInitializer(string[] scopes)
        {
            return new ServiceAccountCredential.Initializer(this.ServiceAccountEmailAddress)
            {
                User = this.ImpersonationUserEmailAddress,
                Scopes = scopes
            }.FromCertificate(this.Certificate);
        }
    }
}
