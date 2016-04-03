using System;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps.Api
{
    public class UserAliasDeleteRequest : DirectoryBaseServiceRequest<string>
    {
        public UserAliasDeleteRequest(IClientService service, string userKey, string alias)
            : base(service)
        {
            this.UserKey = userKey;
            this.Alias = alias;
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
                Name = "alias",
                IsRequired = true,
                ParameterType = "path",
                DefaultValue = null,
                Pattern = null
            };
            base.RequestParameters.Add("alias", parameter2);
        }

        [RequestParameter("alias", RequestParameterType.Path)]
        public string Alias { get; private set; }

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
                return "users/{userKey}/aliases/{alias}";
            }
        }

        [RequestParameter("userKey", RequestParameterType.Path)]
        public string UserKey { get; private set; }
    }
}
