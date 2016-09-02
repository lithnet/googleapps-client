using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;

namespace Lithnet.GoogleApps.Api
{
    public sealed class DomainDeleteRequest : DirectoryBaseServiceRequest<string>
    {
        public DomainDeleteRequest(IClientService service, string domainName, string customer)
            : base(service)
        {
            this.DomainName = domainName;
            this.Customer = customer;
            this.InitParameters();
        }

        [RequestParameter("domainName", RequestParameterType.Path)]
        public string DomainName { get; set; }

        [RequestParameter("customer", RequestParameterType.Path)]
        public string Customer { get; set; }

        protected override void InitParameters()
        {
            base.InitParameters();
            Parameter parameter = new Parameter
            {
                Name = "domainName",
                IsRequired = true,
                ParameterType = "path",
                DefaultValue = null,
                Pattern = null
            };
            base.RequestParameters.Add(parameter.Name, parameter);

            parameter = new Parameter
            {
                Name = "customer",
                IsRequired = true,
                ParameterType = "path",
                DefaultValue = null,
                Pattern = null
            };
            base.RequestParameters.Add(parameter.Name, parameter);
        }

        public override string HttpMethod => "DELETE";

        public override string MethodName => "delete";

        public override string RestPath => "customer/{customer}/domains/{domainName}";
    }
}