using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps.Api
{
    public sealed class DomainListRequest : DirectoryBaseServiceRequest<DomainList>
    {
        public DomainListRequest(IClientService service, string customer)
            : base(service)
        {
            this.Customer = customer;
            this.InitParameters();
        }

        [RequestParameter("customer", RequestParameterType.Path)]
        public string Customer { get; set; }

        protected override void InitParameters()
        {
            base.InitParameters();
            Parameter parameter = new Parameter
            {
                Name = "customer",
                IsRequired = true,
                ParameterType = "path",
                DefaultValue = null,
                Pattern = null
            };
            base.RequestParameters.Add(parameter.Name, parameter);
        }

        public override string HttpMethod => "GET";

        public override string MethodName => "list";

        public override string RestPath => "customer/{customer}/domains";
    }
}