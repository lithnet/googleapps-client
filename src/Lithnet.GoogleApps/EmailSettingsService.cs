using System;
using Google.GData.Apps.GoogleMailSettings;

namespace Lithnet.GoogleApps
{
    using System.Reflection;

    public class EmailSettingsService : GoogleMailSettingsService
    {
        public EmailSettingsService(string applicationName) : base(string.Empty, applicationName)
        {
        }

        public void SetDomain(string domain)
        {
            this.domain = domain;

            if (this.Domain != domain)
            {
                FieldInfo fi = typeof(GoogleMailSettingsService).GetField("domain", BindingFlags.NonPublic | BindingFlags.Instance);
                fi?.SetValue(this, domain);
            }
        }
    }

}
