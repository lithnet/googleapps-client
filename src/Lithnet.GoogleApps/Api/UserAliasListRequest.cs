using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps.Api
{
    public sealed class UserAliasListRequest : DirectoryBaseServiceRequest<UserAliases>
    {
        public UserAliasListRequest(IClientService service, string userKey)
            : base(service)
        {
            this.UserKey = userKey;
            this.InitParameters();
        }

        protected override void InitParameters()
        {
            base.InitParameters();
            Parameter parameter = new Parameter
            {
                Name = "userKey",
                IsRequired = true,
                ParameterType = "path",
                DefaultValue = null,
                Pattern = null
            };
            base.RequestParameters.Add("userKey", parameter);
            Parameter parameter2 = new Parameter
            {
                Name = "event",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = null,
                Pattern = null
            };
            base.RequestParameters.Add("event", parameter2);
        }

        [RequestParameter("event", RequestParameterType.Query)]
        public EventEnum? Event { get; set; }

        public override string HttpMethod => "GET";

        public override string MethodName => "list";

        public override string RestPath => "users/{userKey}/aliases";

        [RequestParameter("userKey", RequestParameterType.Path)]
        public string UserKey { get; private set; }

        public enum EventEnum
        {
            [StringValue("add")]
            Add = 0,
            [StringValue("delete")]
            Delete = 1
        }
    }
}
