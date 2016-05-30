using System;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps.Api
{
    public class UserListRequest : DirectoryBaseServiceRequest<UserList>
    {
        public UserListRequest(IClientService service)
            : base(service)
        {
            this.InitParameters();
        }

        protected override void InitParameters()
        {
            base.InitParameters();
            Parameter parameter = new Parameter
            {
                Name = "customer",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = null,
                Pattern = null
            };

            base.RequestParameters.Add("customer", parameter);
            Parameter parameter2 = new Parameter
            {
                Name = "orderBy",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = null,
                Pattern = null
            };

            base.RequestParameters.Add("orderBy", parameter2);
            Parameter parameter3 = new Parameter
            {
                Name = "domain",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = null,
                Pattern = null
            };

            base.RequestParameters.Add("domain", parameter3);
            Parameter parameter4 = new Parameter
            {
                Name = "projection",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = "basic",
                Pattern = null
            };
            base.RequestParameters.Add("projection", parameter4);
            Parameter parameter5 = new Parameter
            {
                Name = "showDeleted",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = null,
                Pattern = null
            };

            base.RequestParameters.Add("showDeleted", parameter5);
            Parameter parameter6 = new Parameter
            {
                Name = "customFieldMask",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = null,
                Pattern = null
            };

            base.RequestParameters.Add("customFieldMask", parameter6);
            Parameter parameter7 = new Parameter
            {
                Name = "maxResults",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = null,
                Pattern = null
            };
            base.RequestParameters.Add("maxResults", parameter7);
            Parameter parameter8 = new Parameter
            {
                Name = "pageToken",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = null,
                Pattern = null
            };

            base.RequestParameters.Add("pageToken", parameter8);
            Parameter parameter9 = new Parameter
            {
                Name = "sortOrder",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = null,
                Pattern = null
            };
            base.RequestParameters.Add("sortOrder", parameter9);
            Parameter parameter10 = new Parameter
            {
                Name = "query",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = null,
                Pattern = null
            };

            base.RequestParameters.Add("query", parameter10);
            Parameter parameter11 = new Parameter
            {
                Name = "viewType",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = "admin_view",
                Pattern = null
            };

            base.RequestParameters.Add("viewType", parameter11);
            Parameter parameter12 = new Parameter
            {
                Name = "event",
                IsRequired = false,
                ParameterType = "query",
                DefaultValue = null,
                Pattern = null
            };

            base.RequestParameters.Add("event", parameter12);
        }

        [RequestParameter("customer", RequestParameterType.Query)]
        public virtual string Customer { get; set; }

        [RequestParameter("customFieldMask", RequestParameterType.Query)]
        public virtual string CustomFieldMask { get; set; }

        [RequestParameter("domain", RequestParameterType.Query)]
        public virtual string Domain { get; set; }

        [RequestParameter("event", RequestParameterType.Query)]
        public virtual EventEnum? Event { get; set; }

        public override string HttpMethod
        {
            get
            {
                return "GET";
            }
        }

        [RequestParameter("maxResults", RequestParameterType.Query)]
        public virtual int? MaxResults { get; set; }

        public override string MethodName
        {
            get
            {
                return "list";
            }
        }

        [RequestParameter("orderBy", RequestParameterType.Query)]
        public virtual OrderByEnum? OrderBy { get; set; }

        [RequestParameter("pageToken", RequestParameterType.Query)]
        public virtual string PageToken { get; set; }

        [RequestParameter("projection", RequestParameterType.Query)]
        public virtual ProjectionEnum? Projection { get; set; }

        [RequestParameter("query", RequestParameterType.Query)]
        public virtual string Query { get; set; }

        public override string RestPath
        {
            get
            {
                return "users";
            }
        }

        [RequestParameter("showDeleted", RequestParameterType.Query)]
        public virtual string ShowDeleted { get; set; }

        [RequestParameter("sortOrder", RequestParameterType.Query)]
        public virtual SortOrderEnum? SortOrder { get; set; }

        [RequestParameter("viewType", RequestParameterType.Query)]
        public virtual ViewTypeEnum? ViewType { get; set; }

        public enum EventEnum
        {
            [StringValue("add")]
            Add = 0,
            [StringValue("delete")]
            Delete = 1,
            [StringValue("makeAdmin")]
            MakeAdmin = 2,
            [StringValue("undelete")]
            Undelete = 3,
            [StringValue("update")]
            Update = 4
        }

        public enum OrderByEnum
        {
            [StringValue("email")]
            Email = 0,
            [StringValue("familyName")]
            FamilyName = 1,
            [StringValue("givenName")]
            GivenName = 2
        }

        public enum ProjectionEnum
        {
            [StringValue("basic")]
            Basic = 0,
            [StringValue("custom")]
            Custom = 1,
            [StringValue("full")]
            Full = 2
        }

        public enum SortOrderEnum
        {
            [StringValue("ASCENDING")]
            ASCENDING = 0,
            [StringValue("DESCENDING")]
            DESCENDING = 1
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
