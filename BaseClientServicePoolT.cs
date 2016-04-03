using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Google.Apis.Services;
using System.Collections.Concurrent;

namespace Lithnet.GoogleApps
{
    public class BaseClientServicePool<T> : IDisposable where T : BaseClientService
    {
        public int PoolEmptySleepInterval { get; set; }
        
        private bool isDisposed;

        private ConcurrentBag<BaseClientServiceWrapper<T>> items;

        public BaseClientServicePool(int poolSize, Func<int, T> itemFactory)
        {
            if (poolSize <= 0)
            {
                throw new ArgumentOutOfRangeException("poolSize", poolSize, "Pool size must be greater than zero");
            }

            if (itemFactory == null)
            {
                throw new ArgumentNullException("itemFactory");
            }

            this.PoolEmptySleepInterval = 100;
            this.items = new ConcurrentBag<BaseClientServiceWrapper<T>>();
            this.LoadItems(poolSize, itemFactory);
        }

        public BaseClientServiceWrapper<T> Take(Newtonsoft.Json.NullValueHandling nullValueHandling)
        {
            BaseClientServiceWrapper<T> item;

            while (true)
            {
                if (!this.items.TryTake(out item))
                {
                    // Pool empty. Sleeping
                    Thread.Sleep(this.PoolEmptySleepInterval);
                }
                else
                {
                    ((GoogleJsonSerializer)item.Client.Serializer).NullValueHandling = nullValueHandling;
                    return item;
                }
            }
        }

        public void Return(BaseClientServiceWrapper<T> item)
        {
            this.items.Add(item);
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
            {
                BaseClientServiceWrapper<T> item;

                while (this.items.TryTake(out item))
                {
                    item.Dispose();
                }
            }
        }

        private void LoadItems(int count, Func<int, T> itemFactory)
        {
            for (int i = 0; i < count; i++)
            {
                T item = itemFactory(i);
                var wrapper = new BaseClientServiceWrapper<T>(this, item);
                this.items.Add(wrapper);
            }
        }

        public int AvailableCount
        {
            get
            {
                return this.items.Count;
            }
        }

        public bool IsDisposed
        {
            get
            {
                return isDisposed;
            }
        }
    }
}