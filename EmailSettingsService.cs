using System;
using Google.GData.Apps.GoogleMailSettings;

namespace Lithnet.GoogleApps
{

    public class EmailSettingsService : GoogleMailSettingsService
    {
        public EmailSettingsService(string applicationName) : base(string.Empty, applicationName)
        {
        }

        public void SetDomain(string domain)
        {
            this.domain = domain;
        }
    }

}
