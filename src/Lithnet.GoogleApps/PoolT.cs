using System;
using System.Threading;
using System.Collections.Concurrent;

namespace Lithnet.GoogleApps
{
    public class Pool<T> : IDisposable
    {
        public int PoolEmptySleepInterval { get; set; }

        private readonly ConcurrentBag<PoolItem<T>> items;

        private int created;

        private readonly int poolSize;

        private readonly Func<T> itemFactory;

        public Pool(int poolSize, Func<T> itemFactory)
        {
            if (poolSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(poolSize), poolSize, "Pool size must be greater than zero");
            }

            this.itemFactory = itemFactory ?? throw new ArgumentNullException(nameof(itemFactory));
            this.poolSize = poolSize;
            this.PoolEmptySleepInterval = 100;
            this.items = new ConcurrentBag<PoolItem<T>>();
        }

        public PoolItem<T> Take()
        {
            while (true)
            {
                if (!this.items.TryTake(out PoolItem<T> item))
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
            if (this.IsDisposed)
            {
                return;
            }

            this.IsDisposed = true;

            if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
            {
                while (this.items.TryTake(out PoolItem<T> item))
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

        public bool IsDisposed { get; private set; }
    }
}