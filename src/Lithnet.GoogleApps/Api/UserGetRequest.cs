using System;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps.Api
{
    public class UserGetRequest : DirectoryBaseServiceRequest<User>
    {
        public UserGetRequest(IClientService service, string userKey)
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
                Name = "viewType",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = "admin_view",
                Pattern = null
            };
            base.RequestParameters.Add("viewType", parameter2);
            Parameter parameter3 = new Parameter
            {
                Name = "customFieldMask",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = null,
                Pattern = null
            };
            base.RequestParameters.Add("customFieldMask", parameter3);
            Parameter parameter4 = new Parameter
            {
                Name = "projection",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = "basic",
                Pattern = null
            };
            base.RequestParameters.Add("projection", parameter4);
        }

        [RequestParameter("customFieldMask", RequestParameterType.Query)]
        public virtual string CustomFieldMask { get; set; }

        public override string HttpMethod
        {
            get
            {
                return "GET";
            }
        }

        public override string MethodName
        {
            get
            {
                return "get";
            }
        }

        [RequestParameter("projection", RequestParameterType.Query)]
        public virtual ProjectionEnum? Projection { get; set; }

        public override string RestPath
        {
            get
            {
                return "users/{userKey}";
            }
        }

        [RequestParameter("userKey", RequestParameterType.Path)]
        public string UserKey { get; private set; }

        [RequestParameter("viewType", RequestParameterType.Query)]
        public virtual ViewTypeEnum? ViewType { get; set; }

        public enum ProjectionEnum
        {
            [StringValue("basic")]
            Basic = 0,
            [StringValue("custom")]
            Custom = 1,
            [StringValue("full")]
            Full = 2
        }

        public enum ViewTypeEnum
        {
            [StringValue("admin_view")]
            AdminView = 0,
            [StringValue("domain_public")]
            DomainPublic = 1
        }
    }
}