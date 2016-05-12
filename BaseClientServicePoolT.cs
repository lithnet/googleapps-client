using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Google.Apis.Services;
using System.Collections.Concurrent;

namespace Lithnet.GoogleApps
{
    public class BaseClientServicePool<T> : Pool<T> where T : BaseClientService
    {
        public BaseClientServicePool(int poolSize, Func<T> itemFactory)
            : base(poolSize, itemFactory)
        {
        }

        public PoolItem<T> Take(Newtonsoft.Json.NullValueHandling nullValueHandling)
        {
            PoolItem<T> item = base.Take();
            ((GoogleJsonSerializer)item.Item.Serializer).NullValueHandling = nullValueHandling;
            return item;
        }
    }
}