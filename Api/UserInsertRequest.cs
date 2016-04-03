using System;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;
using Lithnet.GoogleApps.ManagedObjects;

namespace Lithnet.GoogleApps.Api
{
    public class InsertRequest : DirectoryBaseServiceRequest<User>
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

        protected override void InitParameters()
        {
            base.InitParameters();
        }

        private User Body { get; set; }

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
                return "insert";
            }
        }

        public override string RestPath
        {
            get
            {
                return "users";
            }
        }
    }
}