using System;
using System.Collections.Generic;
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
            : this()
        {
            this.Group = group;
        }

        public GroupSettings Settings { get; internal set; }

        public Group Group { get; set; }

        public GroupMembership Membership { get; internal set; }

        public List<Exception> Errors { get; private set; }

        internal bool IsComplete
        {
            get
            {
                lock (this)
                {
                    return this.LoadedMembers == this.RequiresMembers && this.LoadedSettings == this.RequiresSettings;
                }
            }
        }

        internal bool LoadedSettings { get; set; }

        internal bool LoadedMembers { get; set; }

        internal bool RequiresSettings { get; set; }

        internal bool RequiresMembers { get; set; }
    }
}
