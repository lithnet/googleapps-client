using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps.Api
{
    public sealed class UserListRequest : DirectoryBaseServiceRequest<UserList>
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
        public string Customer { get; set; }

        [RequestParameter("customFieldMask", RequestParameterType.Query)]
        public string CustomFieldMask { get; set; }

        [RequestParameter("domain", RequestParameterType.Query)]
        public string Domain { get; set; }

        [RequestParameter("event", RequestParameterType.Query)]
        public EventEnum? Event { get; set; }

        public override string HttpMethod => "GET";

        [RequestParameter("maxResults", RequestParameterType.Query)]
        public int? MaxResults { get; set; }

        public override string MethodName => "list";

        [RequestParameter("orderBy", RequestParameterType.Query)]
        public OrderByEnum? OrderBy { get; set; }

        [RequestParameter("pageToken", RequestParameterType.Query)]
        public string PageToken { get; set; }

        [RequestParameter("projection", RequestParameterType.Query)]
        public ProjectionEnum? Projection { get; set; }

        [RequestParameter("query", RequestParameterType.Query)]
        public string Query { get; set; }

        public override string RestPath => "admin/directory/v1/users";

        [RequestParameter("showDeleted", RequestParameterType.Query)]
        public string ShowDeleted { get; set; }

        [RequestParameter("sortOrder", RequestParameterType.Query)]
        public SortOrderEnum? SortOrder { get; set; }

        [RequestParameter("viewType", RequestParameterType.Query)]
        public ViewTypeEnum? ViewType { get; set; }
    }
}
