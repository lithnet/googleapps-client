using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps.Api
{
    public sealed class UserUpdateRequest : DirectoryBaseServiceRequest<User>
    {
        public UserUpdateRequest(IClientService service, User body, string userKey)
            : base(service)
        {
            this.UserKey = userKey;
            this.Body = body;
            this.InitParameters();
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
                Name = "userKey",
                IsRequired = true,
                ParameterType = "path",
                DefaultValue = null,
                Pattern = null
            };
            base.RequestParameters.Add("userKey", parameter);
        }

        private User Body { get; set; }

        public override string HttpMethod => "PUT";

        public override string MethodName => "update";

        public override string RestPath => "users/{userKey}";

        [RequestParameter("userKey", RequestParameterType.Path)]
        public string UserKey { get; private set; }
    }
}