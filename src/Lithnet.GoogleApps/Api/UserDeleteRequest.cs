using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;

namespace Lithnet.GoogleApps.Api
{
    public sealed class UserDeleteRequest : DirectoryBaseServiceRequest<string>
    {
        public UserDeleteRequest(IClientService service, string userKey)
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
        }

        public override string HttpMethod => "DELETE";

        public override string MethodName => "delete";

        public override string RestPath => "users/{userKey}";

        [RequestParameter("userKey", RequestParameterType.Path)]
        public string UserKey { get; private set; }
    }

}
