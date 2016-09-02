using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Services;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps.Api
{
    public sealed class InsertRequest : DirectoryBaseServiceRequest<User>
    {
        public InsertRequest(IClientService service, User body)
            : base(service)
        {
            this.Body = body;
            this.InitParameters();
        }

        protected override object GetBody()
        {
            return this.Body;
        }

        private User Body { get; set; }

        public override string HttpMethod => "POST";

        public override string MethodName => "insert";

        public override string RestPath => "users";
    }
}