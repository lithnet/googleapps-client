using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Http;
using Google.Apis.Services;
using Delegate = Google.Apis.Gmail.v1.Data.Delegate;

namespace Lithnet.GoogleApps
{
    public class GmailServiceRequestFactory
    {
        private readonly X509Certificate2 x509Certificate;

        private readonly string serviceAccountID;

        private readonly string[] scopes;

        private const int CacheSize = 100;

        private readonly OrderedDictionary cache = new OrderedDictionary(CacheSize);

        public int RetryCount { get; set; } = 12;

        public GmailServiceRequestFactory(string serviceAccountID, X509Certificate2 x509Certificate, string[] scopes)
        {
            this.x509Certificate = x509Certificate;
            this.serviceAccountID = serviceAccountID;
            this.scopes = scopes;
        }

        private GmailService GetService(string user)
        {
            return this.GetService(user, Settings.DefaultTimeout);
        }

        private GmailService GetService(string user, TimeSpan timeout)
        {
            lock (this.cache)
            {
                if (!this.cache.Contains(user))
                {
                    ServiceAccountCredential.Initializer initializerInstance = new ServiceAccountCredential.Initializer(this.serviceAccountID)
                    {
                        User = user,
                        Scopes = this.scopes
                    }.FromCertificate(this.x509Certificate);

                    GmailService x = new GmailService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = new ServiceAccountCredential(initializerInstance),
                        ApplicationName = "LithnetGoogleAppsLibrary",
                        GZipEnabled = !Settings.DisableGzip,
                        Serializer = new GoogleJsonSerializer(),
                        DefaultExponentialBackOffPolicy = ExponentialBackOffPolicy.None,
                    });

                    x.HttpClient.Timeout = timeout;
                    this.cache.Add(user, x);

                    if (this.cache.Count > CacheSize)
                    {
                        this.cache.RemoveAt(0);
                    }
                }

                return (GmailService) this.cache[user];
            }
        }

        public IEnumerable<string> GetDelegates(string id)
        {
            id.ThrowIfNotEmailAddress();

            GmailService service = this.GetService(id);
            UsersResource.SettingsResource.DelegatesResource.ListRequest request = service.Users.Settings.Delegates.List(id);
            ListDelegatesResponse result = request.ExecuteWithRetry(RetryEvents.BackoffOAuthNotFound, this.RetryCount);
            return result.Delegates?.Select(t => t.DelegateEmail) ?? new List<string>();
        }

        public IEnumerable<string> GetSendAsAddresses(string id)
        {
            try
            {
                id.ThrowIfNotEmailAddress();

                GmailService service = this.GetService(id);
                UsersResource.SettingsResource.SendAsResource.ListRequest request = service.Users.Settings.SendAs.List(id);
                ListSendAsResponse result = request.ExecuteWithRetry(RetryEvents.BackoffOAuthNotFound | RetryEvents.Timeout, this.RetryCount);
                return result.SendAs?.Select(t => t.SendAsEmail) ?? new List<string>();
            }
            catch(GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.BadRequest && ex.Message != null && ex.Message.IndexOf("Mail service not enabled", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return new List<string>();
                }
                else
                {
                    throw;
                }
            }
        }

        public IEnumerable<SendAs> GetSendAs(string id)
        {
            id.ThrowIfNotEmailAddress();

            try
            {
                GmailService service = this.GetService(id);
                UsersResource.SettingsResource.SendAsResource.ListRequest request = service.Users.Settings.SendAs.List(id);
                ListSendAsResponse result = request.ExecuteWithRetry(RetryEvents.BackoffOAuthNotFound | RetryEvents.Timeout, this.RetryCount);
                return result.SendAs ?? new List<SendAs>();
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.BadRequest && ex.Message != null && (ex.Message.IndexOf("Mail service not enabled", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    ex.Message.IndexOf("failedPrecondition", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return new List<SendAs>();
                }
                else
                {
                    throw;
                }
            }
        }

        public void RemoveDelegate(string id, string @delegate)
        {
            id.ThrowIfNotEmailAddress();

            GmailService service = this.GetService(id);
            UsersResource.SettingsResource.DelegatesResource.DeleteRequest request = service.Users.Settings.Delegates.Delete(id, @delegate);
            request.ExecuteWithRetry(RetryEvents.BackoffOAuthNotFound, this.RetryCount, 5);
        }

        public void RemoveSendAs(string id, string sendas)
        {
            id.ThrowIfNotEmailAddress();

            GmailService service = this.GetService(id);
            UsersResource.SettingsResource.SendAsResource.DeleteRequest request = service.Users.Settings.SendAs.Delete(id, sendas);
            request.ExecuteWithRetry(RetryEvents.BackoffOAuth, this.RetryCount, 5);
        }

        public void RemoveDelegate(string id)
        {
            id.ThrowIfNotEmailAddress();

            GmailService service = this.GetService(id);
            ListDelegatesResponse result = service.Users.Settings.Delegates.List(id).ExecuteWithRetry(RetryEvents.BackoffOAuthNotFound, this.RetryCount);

            if (result.Delegates != null)
            {
                foreach (Delegate item in result.Delegates)
                {
                    service.Users.Settings.Delegates.Delete(id, item.DelegateEmail).ExecuteWithRetry(RetryEvents.BackoffOAuthNotFound, this.RetryCount, 5);
                }
            }
        }

        public void RemoveSendAs(string id)
        {
            id.ThrowIfNotEmailAddress();

            GmailService service = this.GetService(id);
            ListSendAsResponse result = service.Users.Settings.SendAs.List(id).ExecuteWithRetry(RetryEvents.BackoffOAuth, this.RetryCount);

            if (result.SendAs != null)
            {
                foreach (SendAs item in result.SendAs.Where(t => !t.IsPrimary ?? false))
                {
                    service.Users.Settings.SendAs.Delete(id, item.SendAsEmail).ExecuteWithRetry(RetryEvents.BackoffOAuth, this.RetryCount, 5);
                }
            }
        }

        public void AddDelegate(string id, string @delegate)
        {
            id.ThrowIfNotEmailAddress();

            GmailService service = this.GetService(id);
            service.Users.Settings.Delegates.Create(new Delegate { DelegateEmail = @delegate }, id).ExecuteWithRetry(RetryEvents.BackoffOAuthNotFound, this.RetryCount, 100);
        }

        public void AddSendAs(string id, string sendAs)
        {
            id.ThrowIfNotEmailAddress();

            GmailService service = this.GetService(id);
            service.Users.Settings.SendAs.Create(new SendAs { SendAsEmail = sendAs }, id).ExecuteWithRetry(RetryEvents.BackoffOAuthNotFound, this.RetryCount, 100);
        }

        public void AddSendAs(string id, SendAs sendAs)
        {
            id.ThrowIfNotEmailAddress();

            GmailService service = this.GetService(id);
            service.Users.Settings.SendAs.Create(sendAs, id).ExecuteWithRetry(RetryEvents.BackoffOAuthNotFound, this.RetryCount, 100);
        }

        public void AddSendAs(string id, IEnumerable<string> sendAs)
        {
            id.ThrowIfNotEmailAddress();

            GmailService service = this.GetService(id);
            foreach (string item in sendAs)
            {
                service.Users.Settings.SendAs.Create(new SendAs { SendAsEmail = item }, id).ExecuteWithRetry(RetryEvents.BackoffOAuthNotFound, this.RetryCount, 100);
            }
        }

        public void AddSendAs(string id, IEnumerable<SendAs> sendAs)
        {
            id.ThrowIfNotEmailAddress();

            GmailService service = this.GetService(id);
            foreach (SendAs item in sendAs)
            {
                service.Users.Settings.SendAs.Create(item, id).ExecuteWithRetry(RetryEvents.BackoffOAuthNotFound, this.RetryCount, 100);
            }
        }

        public void AddDelegate(string id, IEnumerable<string> delegates)
        {
            id.ThrowIfNotEmailAddress();

            GmailService service = this.GetService(id);
            foreach (string item in delegates)
            {
                service.Users.Settings.Delegates.Create(new Delegate { DelegateEmail = item }, id).ExecuteWithRetry(RetryEvents.Backoff | RetryEvents.OAuthImpersonationError | RetryEvents.NotFound, this.RetryCount, 100);
            }
        }
    }
}
