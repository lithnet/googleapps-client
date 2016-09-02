using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps.Api
{
    public sealed class DomainInsertRequest : DirectoryBaseServiceRequest<Domain>
    {
        public DomainInsertRequest(IClientService service, string customer, Domain domain)
            : base(service)
        {
            this.Body = domain;
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

        private Domain Body { get; set; }

        protected override object GetBody()
        {
            return this.Body;
        }

        public override string HttpMethod => "POST";

        public override string MethodName => "insert";

        public override string RestPath => "customer/{customer}/domains";
    }
}