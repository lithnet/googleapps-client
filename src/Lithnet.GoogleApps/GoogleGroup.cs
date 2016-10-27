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

        internal GoogleGroup(Group group, bool getSettings, bool getMembers)
            : this(group)
        {
            if (getSettings)
            {
                this.GetSettings();
            }

            if (getMembers)
            {
                this.GetMembership();
            }
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

        internal void GetSettings()
        {
            lock (this)
            {
                try
                {
                    this.Settings = GroupSettingsRequestFactory.Get(this.Group.Email);
                }
                catch (Exception ex)
                {
                    this.Errors.Add(ex);
                }
                finally
                {
                    this.LoadedSettings = true;
                }
            }
        }

        internal void GetMembership()
        {
            lock (this)
            {
                try
                {
                    this.Membership = GroupMemberRequestFactory.GetMembership(this.Group.Email);
                }
                catch (Exception ex)
                {
                    this.Errors.Add(ex);
                }
                finally
                {
                    this.LoadedMembers = true;
                }
            }
        }

        internal bool RequiresSettings { get; set; }

        internal bool RequiresMembers { get; set; }

    }
}
