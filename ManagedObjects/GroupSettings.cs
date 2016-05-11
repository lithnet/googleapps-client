namespace Lithnet.GoogleApps.ManagedObjects
{
    using Google.Apis.Requests;
    using Newtonsoft.Json;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    public class GroupSettings : IDirectResponseSchema, ISerializable
    {
        [JsonProperty("allowExternalMembers")]
        public bool? AllowExternalMembers { get; set; }

        [JsonProperty("allowGoogleCommunication")]
        public bool? AllowGoogleCommunication { get; set; }

        [JsonProperty("allowWebPosting")]
        public bool? AllowWebPosting { get; set; }

        [JsonProperty("archiveOnly")]
        public bool? ArchiveOnly { get; set; }

        [JsonProperty("customReplyTo"), JsonConverter(typeof(JsonNullStringConverter))]
        public string CustomReplyTo { get; set; }

        [JsonProperty("defaultMessageDenyNotificationText"), JsonConverter(typeof(JsonNullStringConverter))]
        public string DefaultMessageDenyNotificationText { get; set; }

        [JsonProperty("description"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Description { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        public string ETag { get; set; }

        [JsonProperty("includeInGlobalAddressList")]
        public bool? IncludeInGlobalAddressList { get; set; }

        [JsonProperty("isArchived")]
        public bool? IsArchived { get; set; }

        [JsonProperty("kind"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Kind { get; set; }

        [JsonProperty("maxMessageBytes")]
        public int? MaxMessageBytes { get; set; }

        [JsonProperty("membersCanPostAsTheGroup")]
        public bool? MembersCanPostAsTheGroup { get; set; }

        [JsonProperty("messageDisplayFont"), JsonConverter(typeof(JsonNullStringConverter))]
        public string MessageDisplayFont { get; set; }

        [JsonProperty("messageModerationLevel"), JsonConverter(typeof(JsonNullStringConverter))]
        public string MessageModerationLevel { get; set; }

        [JsonProperty("name"), JsonConverter(typeof(JsonNullStringConverter))]
        public string Name { get; set; }

        [JsonProperty("primaryLanguage"), JsonConverter(typeof(JsonNullStringConverter))]
        public string PrimaryLanguage { get; set; }

        [JsonProperty("replyTo"), JsonConverter(typeof(JsonNullStringConverter))]
        public string ReplyTo { get; set; }

        [JsonProperty("sendMessageDenyNotification")]
        public bool? SendMessageDenyNotification { get; set; }

        [JsonProperty("showInGroupDirectory")]
        public bool? ShowInGroupDirectory { get; set; }

        [JsonProperty("spamModerationLevel")]
        public string SpamModerationLevel { get; set; }

        [JsonProperty("whoCanContactOwner")]
        public string WhoCanContactOwner { get; set; }

        [JsonProperty("whoCanInvite")]
        public string WhoCanInvite { get; set; }

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

                    case "allowGoogleCommunication":
                        this.AllowGoogleCommunication = info.GetString(entry.Name).ToNullableBool();
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

                    case "messageDisplayFont":
                        this.MessageDisplayFont = info.GetString(entry.Name);
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

                    case "whoCanInvite":
                        this.WhoCanInvite = info.GetString(entry.Name);
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

                    case "isArchived":
                        this.IsArchived = info.GetString(entry.Name).ToNullableBool();
                        break;

                    case "maxMessageBytes":
                        this.MaxMessageBytes = info.GetInt32(entry.Name);
                        break;

                    case "membersCanPostAsTheGroup":
                        this.MembersCanPostAsTheGroup = info.GetString(entry.Name).ToNullableBool();
                        break;

                    case "sendMessageDenyNotification":
                        this.SendMessageDenyNotification = info.GetString(entry.Name).ToNullableBool();
                        break;

                    case "showInGroupDirectory":
                        this.ShowInGroupDirectory = info.GetString(entry.Name).ToNullableBool();
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

            if (this.AllowGoogleCommunication != null)
            {
                info.AddValue("allowGoogleCommunication", this.AllowGoogleCommunication.Value.ToLowerString());
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

            if (this.ShowInGroupDirectory != null)
            {
                info.AddValue("showInGroupDirectory", this.ShowInGroupDirectory.Value.ToLowerString());
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

            if (this.MaxMessageBytes != null)
            {
                info.AddValue("maxMessageBytes", this.MaxMessageBytes.Value);
            }

            if (this.MessageDisplayFont != null)
            {
                info.AddValue("messageDisplayFont", this.MessageDisplayFont);
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

            if (this.WhoCanInvite != null)
            {
                info.AddValue("whoCanInvite", this.WhoCanInvite);
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
        }
    }
}
