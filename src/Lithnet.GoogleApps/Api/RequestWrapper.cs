using System.Collections.Generic;
using System.Linq;
using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Discovery;
using Google.Apis.Requests;
using Google.Apis.Services;

namespace Lithnet.GoogleApps.Api
{
    public abstract class RequestWrapper<TResponse, TWrappedRequest, TWrappedRequestResponse> : DirectoryBaseServiceRequest<TResponse> where TWrappedRequest : ClientServiceRequest<TWrappedRequestResponse>
    {
        private TWrappedRequest internalRequest;

        protected RequestWrapper(IClientService service, TWrappedRequest request)
            : base(service)
        {
            this.internalRequest = request;
            this.InitParameters();
        }

        protected sealed override void InitParameters()
        {
            base.InitParameters();

            var newParameters = this.internalRequest.RequestParameters.Except(base.RequestParameters, new ParameterComparer());

            foreach (var parameter in newParameters)
            {
                base.RequestParameters.Add(parameter);
            }
        }

        public sealed override string HttpMethod => this.internalRequest.HttpMethod;

        public sealed override string MethodName => this.internalRequest.MethodName;

        public sealed override string RestPath => this.internalRequest.RestPath;
    }

    public class ParameterComparer : IEqualityComparer<KeyValuePair<string, IParameter>>
    {
        public bool Equals(KeyValuePair<string, IParameter> x, KeyValuePair<string, IParameter> y)
        {
            return x.Key == y.Key;
        }

        public int GetHashCode(KeyValuePair<string, IParameter> obj)
        {
            return obj.Key.GetHashCode();
        }
    }
}

