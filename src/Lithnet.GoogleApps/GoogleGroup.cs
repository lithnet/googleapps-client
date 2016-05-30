using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps
{
    public class GoogleGroup
    {
        public GoogleGroup()
        {
            this.Errors = new List<Exception>();
            this.Group = new Group();
            this.Settings = new GroupSettings();
            this.Membership = new GroupMembership();
        }

        public GoogleGroup(Group group)
            :this()
        {
            this.Group = group;
        }

        public GroupSettings Settings { get; set; }

        public Group Group { get; set; }

        public GroupMembership Membership { get;set;}

        public List<Exception> Errors { get; private set; }
    }
}
