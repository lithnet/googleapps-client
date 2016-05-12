using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Google.Apis.Services;
using System.Collections.Concurrent;

namespace Lithnet.GoogleApps
{
    public class Pool<T> : IDisposable 
    {
        public int PoolEmptySleepInterval { get; set; }

        private bool isDisposed;

        private ConcurrentBag<PoolItem<T>> items;

        public Pool(int poolSize, Func<T> itemFactory)
        {
            if (poolSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(poolSize), poolSize, "Pool size must be greater than zero");
            }

            if (itemFactory == null)
            {
                throw new ArgumentNullException(nameof(itemFactory));
            }

            this.PoolEmptySleepInterval = 100;
            this.items = new ConcurrentBag<PoolItem<T>>();
            this.LoadItems(poolSize, itemFactory);
        }

        public PoolItem<T> Take()
        {
            while (true)
            {
                PoolItem<T> item;

                if (!this.items.TryTake(out item))
                {
                    // Pool empty. Sleeping
                    Thread.Sleep(this.PoolEmptySleepInterval);
                }
                else
                {
                    return item;
                }
            }
        }

        public void Return(PoolItem<T> item)
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
                PoolItem<T> item;

                while (this.items.TryTake(out item))
                {
                    item.Dispose();
                }
            }
        }

        private void LoadItems(int count, Func<T> itemFactory)
        {
            for (int i = 0; i < count; i++)
            {
                T item = itemFactory();
                PoolItem<T> wrapper = new PoolItem<T>(this, item);
                this.items.Add(wrapper);
            }
        }

        public int AvailableCount => this.items.Count;

        public bool IsDisposed => this.isDisposed;
    }
}