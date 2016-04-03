using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Google.Apis.Services;
using System.Collections.Concurrent;

namespace Lithnet.GoogleApps
{
    public class BaseClientServiceWrapper<T> : IDisposable
        where T : BaseClientService
    {
        private T internalItem;

        private BaseClientServicePool<T> containingPool;

        public BaseClientServiceWrapper(BaseClientServicePool<T> pool, T item)
        {
            if (pool == null)
                throw new ArgumentNullException("pool");

            this.containingPool = pool;
            this.internalItem = item;
        }

        public void Dispose()
        {
            if (this.containingPool.IsDisposed)
            {
                this.internalItem.Dispose();
            }
            else
            {
                this.containingPool.Return(this);
            }
        }

        public T Client
        {
            get
            {
                return this.internalItem;
            }
        }
    }
}
