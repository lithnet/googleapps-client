using System;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps.Api
{
    public class UserAliasInsertRequest : DirectoryBaseServiceRequest<UserAlias>
    {
        public UserAliasInsertRequest(IClientService service, UserAlias body, string userKey)
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

        private UserAlias Body { get; set; }

        public override string HttpMethod
        {
            get
            {
                return "POST";
            }
        }

        public override string MethodName
        {
            get
            {
                return "insert";
            }
        }

        public override string RestPath
        {
            get
            {
                return "users/{userKey}/aliases";
            }
        }

        [RequestParameter("userKey", RequestParameterType.Path)]
        public string UserKey { get; private set; }
    }

}
