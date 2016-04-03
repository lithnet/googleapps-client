﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lithnet.GoogleApps.ManagedObjects;
using Google.Apis.Groupssettings.v1;
using Google.Apis.Discovery;
using Google.Apis.Services;
using Google.Apis.Util;

namespace Lithnet.GoogleApps.Api
{
    public class GroupSettingsGetRequest : GroupssettingsBaseServiceRequest<GroupSettings>
    {
        public GroupSettingsGetRequest(IClientService service, string groupUniqueId)
            : base(service)
        {
            this.GroupUniqueId = groupUniqueId;
            this.InitParameters();
            this.Alt = AltEnum.Json;
        }

        protected override void InitParameters()
        {
            base.InitParameters();
            Parameter parameter = new Parameter
            {
                Name = "groupUniqueId",
                IsRequired = true,
                ParameterType = "path",
                DefaultValue = null,
                Pattern = null
            };
            base.RequestParameters.Add("groupUniqueId", parameter);
        }

        [RequestParameter("groupUniqueId", RequestParameterType.Path)]
        public string GroupUniqueId { get; private set; }

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

        public override string RestPath
        {
            get
            {
                return "{groupUniqueId}";
            }
        }
    }
}
