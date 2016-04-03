using System;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps.Api
{
    public class UserDeleteRequest : DirectoryBaseServiceRequest<string>
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

        public override string HttpMethod
        {
            get
            {
                return "DELETE";
            }
        }

        public override string MethodName
        {
            get
            {
                return "delete";
            }
        }

        public override string RestPath
        {
            get
            {
                return "users/{userKey}";
            }
        }

        [RequestParameter("userKey", RequestParameterType.Path)]
        public string UserKey { get; private set; }
    }

}
