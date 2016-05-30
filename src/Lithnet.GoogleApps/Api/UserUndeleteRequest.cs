using System;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps.Api
{
    public class UserUndeleteRequest : DirectoryBaseServiceRequest<string>
    {
        public UserUndeleteRequest(IClientService service, string userKey, string orgUnitPath = "/")
            : base(service)
        {
            this.UserKey = userKey;
            this.Body = new UserUndeleteRequestParameters(orgUnitPath);
            this.InitParameters();
        }

        protected override object GetBody()
        {
            return this.Body;
        }

        private UserUndeleteRequestParameters Body { get; set; }


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
                return "POST";
            }
        }

        public override string MethodName
        {
            get
            {
                return "undelete";
            }
        }

        public override string RestPath
        {
            get
            {
                return "users/{userKey}/undelete";
            }
        }

        [RequestParameter("userKey", RequestParameterType.Path)]
        public string UserKey { get; set; }
    }

}
