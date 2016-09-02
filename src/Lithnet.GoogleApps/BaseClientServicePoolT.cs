using System;
using Google.Apis.Services;

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