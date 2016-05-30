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

        private int created;

        private int poolSize;

        private Func<T> itemFactory;

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

            this.itemFactory = itemFactory;
            this.poolSize = poolSize;
            this.PoolEmptySleepInterval = 100;
            this.items = new ConcurrentBag<PoolItem<T>>();
        }

        public PoolItem<T> Take()
        {
            while (true)
            {
                PoolItem<T> item;

                if (!this.items.TryTake(out item))
                {
                    if (this.created < this.poolSize)
                    {
                        return this.CreateAndAddItem();
                    }
                    else
                    {
                        // Pool empty. Sleeping
                        Thread.Sleep(this.PoolEmptySleepInterval);
                    }
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

        private PoolItem<T> CreateAndAddItem()
        {
            T item = this.itemFactory();
            Interlocked.Increment(ref this.created);

            PoolItem<T> wrapper = new PoolItem<T>(this, item);
            this.items.Add(wrapper);
            return wrapper;
        }

        public int AvailableCount => this.items.Count;

        public bool IsDisposed => this.isDisposed;
    }
}