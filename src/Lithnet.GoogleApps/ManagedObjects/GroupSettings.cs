using Google.Apis.Requests;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class GroupSettings : IDirectResponseSchema, ISerializable
    {
        [JsonProperty("allowExternalMembers")]
        public bool? AllowExternalMembers { get; set; }

        [JsonProperty("allowWebPosting")]
        public bool? AllowWebPosting { get; set; }

        [JsonProperty("archiveOnly")]
        public bool? ArchiveOnly { get; set; }

        [JsonProperty("customReplyTo"), JsonConverter(typeof(JsonNullStringConverter))]
        public string CustomReplyTo { get; set; }

        [JsonProperty("customFooterText"), JsonConverter(typeof(JsonNullStringConverter))]
        public string CustomFooterText { get; set; }

        [JsonProperty("defaultMessageDenyNotificationText"), JsonConverter(typeof(JsonNullStringConverter))]
        public string DefaultMessageDenyNotificationText { get; set; }

        [JsonProperty("description"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Description { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        public string ETag { get; set; }

        [JsonProperty("includeCustomFooter")]
        public bool? IncludeCustomFooter { get; set; }

        [JsonProperty("includeInGlobalAddressList")]
        public bool? IncludeInGlobalAddressList { get; set; }

        [JsonProperty("isArchived")]
        public bool? IsArchived { get; set; }

        [JsonProperty("kind"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Kind { get; set; }

        [JsonProperty("membersCanPostAsTheGroup")]
        public bool? MembersCanPostAsTheGroup { get; set; }

        [JsonProperty("messageModerationLevel"), JsonConverter(typeof(JsonNullStringConverter))]
        public string MessageModerationLevel { get; set; }

        [JsonProperty("name"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Name { get; set; }

        [JsonProperty("primaryLanguage", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(JsonNullStringConverter))]
        public string PrimaryLanguage { get; set; }

        [JsonProperty("replyTo"), JsonConverter(typeof(JsonNullStringConverter))]
        public string ReplyTo { get; set; }

        [JsonProperty("sendMessageDenyNotification")]
        public bool? SendMessageDenyNotification { get; set; }

        [JsonProperty("spamModerationLevel")]
        public string SpamModerationLevel { get; set; }

        [JsonProperty("whoCanContactOwner")]
        public string WhoCanContactOwner { get; set; }

        [JsonProperty("whoCanJoin")]
        public string WhoCanJoin { get; set; }

        [JsonProperty("whoCanLeaveGroup")]
        public string WhoCanLeaveGroup { get; set; }

        [JsonProperty("whoCanPostMessage")]
        public string WhoCanPostMessage { get; set; }

        [JsonProperty("whoCanViewGroup")]
        public string WhoCanViewGroup { get; set; }

        [JsonProperty("whoCanViewMembership")]
        public string WhoCanViewMembership { get; set; }

        [JsonProperty("whoCanModerateMembers")]
        public string WhoCanModerateMembers { get; set; }

        [JsonProperty("whoCanModerateContent")]
        public string WhoCanModerateContent { get; set; }

        [JsonProperty("whoCanAssistContent")]
        public string WhoCanAssistContent { get; set; }

        [JsonProperty("whoCanDiscoverGroup")]
        public string WhoCanDiscoverGroup { get; set; }

        [JsonProperty("enableCollaborativeInbox")]
        public bool? EnableCollaborativeInbox { get; set; }

        public GroupSettings()
        {
        }

        protected GroupSettings(SerializationInfo info, StreamingContext context)
        {
            foreach (SerializationEntry entry in info)
            {
                switch (entry.Name)
                {
                    case "allowExternalMembers":
                        this.AllowExternalMembers = info.GetString(entry.Name).ToNullableBool();
                        break;

                    case "enableCollaborativeInbox":
                        this.EnableCollaborativeInbox = info.GetString(entry.Name).ToNullableBool();
                        break;

                    case "allowWebPosting":
                        this.AllowWebPosting = info.GetString(entry.Name).ToNullableBool();
                        break;

                    case "archiveOnly":
                        this.ArchiveOnly = info.GetString(entry.Name).ToNullableBool();
                        break;

                    case "customReplyTo":
                        string customReplyTo = info.GetString(entry.Name);
                        if (string.IsNullOrEmpty(customReplyTo))
                        {
                            this.CustomReplyTo = null;
                        }
                        else
                        {
                            this.CustomReplyTo = customReplyTo;
                        }
                        break;

                    case "customFooterText":
                        string customFooterText = info.GetString(entry.Name);
                        if (string.IsNullOrEmpty(customFooterText))
                        {
                            this.CustomFooterText = null;
                        }
                        else
                        {
                            this.CustomFooterText = customFooterText;
                        }
                        break;

                    case "defaultMessageDenyNotificationText":
                        string defaultMessageDenyNotificationText = info.GetString(entry.Name);
                        if (string.IsNullOrEmpty(defaultMessageDenyNotificationText))
                        {
                            this.DefaultMessageDenyNotificationText = null;
                        }
                        else
                        {
                            this.DefaultMessageDenyNotificationText = defaultMessageDenyNotificationText;
                        }
                        break;

                    case "description":
                        this.Description = info.GetString(entry.Name);
                        break;

                    case "email":
                        this.Email = info.GetString(entry.Name);
                        break;

                    case "kind":
                        this.Kind = info.GetString(entry.Name);
                        break;

                    case "messageModerationLevel":
                        this.MessageModerationLevel = info.GetString(entry.Name);
                        break;

                    case "name":
                        this.Name = info.GetString(entry.Name);
                        break;

                    case "primaryLanguage":
                        this.PrimaryLanguage = info.GetString(entry.Name);
                        break;

                    case "replyTo":
                        this.ReplyTo = info.GetString(entry.Name);
                        break;

                    case "spamModerationLevel":
                        this.SpamModerationLevel = info.GetString(entry.Name);
                        break;

                    case "whoCanContactOwner":
                        this.WhoCanContactOwner = info.GetString(entry.Name);
                        break;

                    case "whoCanJoin":
                        this.WhoCanJoin = info.GetString(entry.Name);
                        break;

                    case "whoCanLeaveGroup":
                        this.WhoCanLeaveGroup = info.GetString(entry.Name);
                        break;

                    case "whoCanPostMessage":
                        this.WhoCanPostMessage = info.GetString(entry.Name);
                        break;

                    case "whoCanViewGroup":
                        this.WhoCanViewGroup = info.GetString(entry.Name);
                        break;

                    case "whoCanViewMembership":
                        this.WhoCanViewMembership = info.GetString(entry.Name);
                        break;

                    case "includeInGlobalAddressList":
                        this.IncludeInGlobalAddressList = info.GetString(entry.Name).ToNullableBool();
                        break;

                    case "includeCustomFooter":
                        this.IncludeCustomFooter = info.GetString(entry.Name).ToNullableBool();
                        break;

                    case "isArchived":
                        this.IsArchived = info.GetString(entry.Name).ToNullableBool();
                        break;

                    case "membersCanPostAsTheGroup":
                        this.MembersCanPostAsTheGroup = info.GetString(entry.Name).ToNullableBool();
                        break;

                    case "sendMessageDenyNotification":
                        this.SendMessageDenyNotification = info.GetString(entry.Name).ToNullableBool();
                        break;

                    case "whoCanModerateMembers":
                        this.WhoCanModerateMembers = info.GetString(entry.Name);
                        break;

                    case "whoCanModerateContent":
                        this.WhoCanModerateContent = info.GetString(entry.Name);
                        break;

                    case "whoCanAssistContent":
                        this.WhoCanAssistContent = info.GetString(entry.Name);
                        break;

                    case "whoCanDiscoverGroup":
                        this.WhoCanDiscoverGroup = info.GetString(entry.Name);
                        break;
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (this.AllowExternalMembers != null)
            {
                info.AddValue("allowExternalMembers", this.AllowExternalMembers.Value.ToLowerString());
            }

            if (this.EnableCollaborativeInbox != null)
            {
                info.AddValue("enableCollaborativeInbox", this.EnableCollaborativeInbox.Value.ToLowerString());
            }

            if (this.AllowWebPosting != null)
            {
                info.AddValue("allowWebPosting", this.AllowWebPosting.Value.ToLowerString());
            }

            if (this.ArchiveOnly != null)
            {
                info.AddValue("archiveOnly", this.ArchiveOnly.Value.ToLowerString());
            }

            if (this.IncludeInGlobalAddressList != null)
            {
                info.AddValue("includeInGlobalAddressList", this.IncludeInGlobalAddressList.Value.ToLowerString());
            }

            if (this.IncludeCustomFooter != null)
            {
                info.AddValue("includeCustomFooter", this.IncludeCustomFooter.Value.ToLowerString());
            }

            if (this.IsArchived != null)
            {
                info.AddValue("isArchived", this.IsArchived.Value.ToLowerString());
            }

            if (this.MembersCanPostAsTheGroup != null)
            {
                info.AddValue("membersCanPostAsTheGroup", this.MembersCanPostAsTheGroup.Value.ToLowerString());
            }

            if (this.SendMessageDenyNotification != null)
            {
                info.AddValue("sendMessageDenyNotification", this.SendMessageDenyNotification.Value.ToLowerString());
            }

            if (this.CustomReplyTo != null)
            {
                if (this.CustomReplyTo == Constants.NullValuePlaceholder)
                {
                    info.AddValue("customReplyTo", string.Empty);
                }
                else
                {
                    info.AddValue("customReplyTo", this.CustomReplyTo);
                }
            }

            if (this.CustomFooterText != null)
            {
                if (this.CustomFooterText == Constants.NullValuePlaceholder)
                {
                    info.AddValue("customFooterText", string.Empty);
                }
                else
                {
                    info.AddValue("customFooterText", this.CustomFooterText);
                }
            }

            if (this.DefaultMessageDenyNotificationText != null)
            {
                if (this.DefaultMessageDenyNotificationText == Constants.NullValuePlaceholder)
                {
                    info.AddValue("defaultMessageDenyNotificationText", string.Empty);
                }
                else
                {
                    info.AddValue("defaultMessageDenyNotificationText", this.DefaultMessageDenyNotificationText);
                }
            }

            if (this.Description != null)
            {
                info.AddValue("description", this.Description);
            }

            if (this.MessageModerationLevel != null)
            {
                info.AddValue("messageModerationLevel", this.MessageModerationLevel);
            }

            if (this.Name != null)
            {
                info.AddValue("name", this.Name);
            }

            if (this.PrimaryLanguage != null)
            {
                info.AddValue("primaryLanguage", this.PrimaryLanguage);
            }

            if (this.ReplyTo != null)
            {
                info.AddValue("replyTo", this.ReplyTo);
            }

            if (this.SpamModerationLevel != null)
            {
                info.AddValue("spamModerationLevel", this.SpamModerationLevel);
            }

            if (this.WhoCanContactOwner != null)
            {
                info.AddValue("whoCanContactOwner", this.WhoCanContactOwner);
            }

            if (this.WhoCanJoin != null)
            {
                info.AddValue("whoCanJoin", this.WhoCanJoin);
            }

            if (this.WhoCanLeaveGroup != null)
            {
                info.AddValue("whoCanLeaveGroup", this.WhoCanLeaveGroup);
            }

            if (this.WhoCanPostMessage != null)
            {
                info.AddValue("whoCanPostMessage", this.WhoCanPostMessage);
            }

            if (this.WhoCanViewGroup != null)
            {
                info.AddValue("whoCanViewGroup", this.WhoCanViewGroup);
            }

            if (this.WhoCanViewMembership != null)
            {
                info.AddValue("whoCanViewMembership", this.WhoCanViewMembership);
            }

            if (this.WhoCanModerateMembers != null)
            {
                info.AddValue("whoCanModerateMembers", this.WhoCanModerateMembers);
            }

            if (this.WhoCanModerateContent != null)
            {
                info.AddValue("whoCanModerateContent", this.WhoCanModerateContent);
            }

            if (this.WhoCanAssistContent != null)
            {
                info.AddValue("whoCanAssistContent", this.WhoCanAssistContent);
            }

            if (this.WhoCanDiscoverGroup != null)
            {
                info.AddValue("whoCanDiscoverGroup", this.WhoCanDiscoverGroup);
            }
        }
    }
}