using System;
using System.Collections.Generic;

namespace Lithnet.GoogleApps
{
    public class AggregateGroupUpdateException : Exception
    {
        public IEnumerable<string> FailedMembers;

        public IList<Exception> Exceptions;

        public AggregateGroupUpdateException()
            : base()
        {
        }

        public AggregateGroupUpdateException(string groupId, IEnumerable<string> memberKeys)
            : base($"The group member update operation failed. Group ID {groupId}")
        {
            this.FailedMembers = memberKeys;
        }

        public AggregateGroupUpdateException(string groupId, IEnumerable<string> memberKeys, IList<Exception> exceptions)
            : this(groupId, memberKeys)
        {
            this.Exceptions = exceptions;
        }
    }
}
