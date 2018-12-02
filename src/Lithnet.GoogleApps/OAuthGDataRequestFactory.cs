using System;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.GData.Client;

namespace Lithnet.GoogleApps
{
    public class OAuthGDataRequestFactory : GDataRequestFactory
    {
        public ServiceAccountCredential Credentials { get; set; }

        private string lastToken;

        public OAuthGDataRequestFactory(string userAgent, ServiceAccountCredential credentials) : base(userAgent)
        {
            this.Credentials = credentials;
        }

        public void RefreshToken()
        {
            string token = this.Credentials.GetAccessTokenForRequestAsync().GetAwaiter().GetResult();

            if (this.lastToken == token)
            {
                return;
            }

            this.CustomHeaders.RemoveAll(t => t.StartsWith("Authorization:"));
            this.CustomHeaders.Add($"Authorization: Bearer {token}");
            this.lastToken = token;
        }

        public override IGDataRequest CreateRequest(GDataRequestType type, Uri uriTarget)
        {
            this.RefreshToken();
            return base.CreateRequest(type, uriTarget);
        }
    }
}
