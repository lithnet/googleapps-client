using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Google.Apis.Services;
using System.Collections.Concurrent;
using Google.GData.Client;

namespace Lithnet.GoogleApps
{

    public class GDataServiceWrapper<T> : IDisposable
        where T : Service
    {
        private GDataServicePool<T> containingPool;

        public GDataServiceWrapper(GDataServicePool<T> pool, T item)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            this.containingPool = pool;
            this.Client = item;
        }

        public void Dispose()
        {
            if (!this.containingPool.IsDisposed)
            {
                this.containingPool.Return(this);
            }
        }

        public T Client { get; }
    }
}
