using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public interface IPrimaryCandidateObject
    {
        bool? Primary { get; set; }

        bool IsPrimary { get; }
    }
}
