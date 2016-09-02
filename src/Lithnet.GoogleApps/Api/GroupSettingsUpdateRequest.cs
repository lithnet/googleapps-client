using Lithnet.GoogleApps.ManagedObjects;
using Google.Apis.Groupssettings.v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;

namespace Lithnet.GoogleApps.Api
{
    public sealed class GroupSettingsUpdateRequest : GroupssettingsBaseServiceRequest<GroupSettings>
    {
        public GroupSettingsUpdateRequest(IClientService service, GroupSettings body, string groupUniqueId)
            : base(service)
        {
            this.GroupUniqueId = groupUniqueId;
            this.Body = body;
            this.InitParameters();
            this.Alt = AltEnum.Json;
        }

        protected override object GetBody()
        {
            return this.Body;
        }

        protected override void InitParameters()
        {
            base.InitParameters();
            Parameter parameter = new Parameter
            {
                Name = "groupUniqueId",
                IsRequired = true,
                ParameterType = "path",
                DefaultValue = null,
                Pattern = null
            };
            base.RequestParameters.Add("groupUniqueId", parameter);
        }

        private GroupSettings Body { get; set; }

        [RequestParameter("groupUniqueId", RequestParameterType.Path)]
        public string GroupUniqueId { get; private set; }

        public override string HttpMethod => "PUT";

        public override string MethodName => "update";

        public override string RestPath => "{groupUniqueId}";
    }
}
