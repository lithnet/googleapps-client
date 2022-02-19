using System;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps.Api
{
    public sealed class MakeAdminRequest : DirectoryBaseServiceRequest<UserAlias>
    {
        public MakeAdminRequest(IClientService service, bool makeAdmin, string userKey)
            : base(service)
        {
            this.UserKey = userKey;
            this.Body = new MakeAdminParameters() {Status = makeAdmin};
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

            this.RequestParameters.Add("userKey", parameter);
        }

        private MakeAdminParameters Body { get; set; }

        public override string HttpMethod => "POST";

        public override string MethodName => "makeAdmin";

        public override string RestPath => "admin/directory/v1/users/{userKey}/makeAdmin";

        [RequestParameter("userKey", RequestParameterType.Path)]
        public string UserKey { get; private set; }
    }
}
