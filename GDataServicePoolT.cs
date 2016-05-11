using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Google.Apis.Services;
using System.Collections.Concurrent;

namespace Lithnet.GoogleApps
{
    using Google.GData.Client;

    public class GDataServicePool<T> : IDisposable where T : Service
    {
        public int PoolEmptySleepInterval { get; set; }

        private bool isDisposed;

        private Func<string, T> itemFactory;

        public GDataServicePool(int poolSize, Func<string, T> itemFactory)
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
            this.PoolEmptySleepInterval = 100;
        }

        public GDataServiceWrapper<T> Take(string domain)
        {
            return new GDataServiceWrapper<T>(this, this.itemFactory(domain));
        }

        public void Return(GDataServiceWrapper<T> item)
        {
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
        }

        public bool IsDisposed
        {
            get
            {
                return this.isDisposed;
            }
        }
    }
}