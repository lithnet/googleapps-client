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
            Task<string> task = this.Credentials.GetAccessTokenForRequestAsync();
            task.Wait();

            if (task.IsFaulted)
            {
                if (task.Exception != null)
                {
                    throw task.Exception;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }

            if (this.lastToken == task.Result)
            {
                return;
            }

            this.CustomHeaders.RemoveAll(t => t.StartsWith("Authorization:"));
            this.CustomHeaders.Add($"Authorization: Bearer {task.Result}");
            this.lastToken = task.Result;
        }

        public override IGDataRequest CreateRequest(GDataRequestType type, Uri uriTarget)
        {
            this.RefreshToken();
            return base.CreateRequest(type, uriTarget);
        }
    }
}
