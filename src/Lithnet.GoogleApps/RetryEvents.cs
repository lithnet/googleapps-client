using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lithnet.GoogleApps
{
    [Flags]
    public enum RetryEvents
    {
        None = 0,
        Backoff = 1,
        OAuthImpersonationError = 2,
        NotFound = 4,
        BadRequest = 8,
        Timeout = 16,
        Aborted = 32,

        BackoffOAuth = Backoff | OAuthImpersonationError,
        BackoffNotFound = Backoff | NotFound,
        BackoffOAuthNotFound = Backoff | OAuthImpersonationError | NotFound,

    }
}
